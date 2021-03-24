using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VoxelController : MonoBehaviour {

	public class Voxel {
		public int X;
		public int Y;
		public int Z;
		public bool IsFilled;
		public Color Color;
		public bool HasBeenTouchedByBucketFill;

		public Voxel(int x, int y, int z, bool isFilled) {
			X = x;
			Y = y;
			Z = z;
			IsFilled = isFilled;
			HasBeenTouchedByBucketFill = false;
		}

		public void CopyProperties(Voxel otherVoxel) {
			IsFilled = otherVoxel.IsFilled;
			Color = otherVoxel.Color;
		}

		public Vector3 GetLocalPos() {
			return new Vector3(X, Y, Z);
		}

		public Vector3 GetWorldPos(VoxelBuilder owner) {
			return owner.transform.TransformPoint(GetLocalPos());
		}
	}

	private class VoxelCluster {
		private Voxel[,,] clusterVoxels;
		private int minX, minY, minZ, maxX, maxY, maxZ;
		private Vector3 pivot;
		private bool isStatic;

		public Color color;

		public void Fill(Voxel[,,] voxels, Voxel startVoxel, bool wasStatic) {
			List<Voxel> foundVoxels = new List<Voxel>();
			TryRecursiveAdd(startVoxel, foundVoxels, voxels);

			color = new Color(Random.value, Random.value, Random.value, 1);
			foreach(var foundVoxel in foundVoxels) {
				foundVoxel.Color = color;
			}

			minX = int.MaxValue;
			minY = int.MaxValue;
			minZ = int.MaxValue;
			maxX = int.MinValue;
			maxY = int.MinValue;
			maxZ = int.MinValue;

			foreach(var v in foundVoxels) {
				minX = Mathf.Min(minX, v.X);
				minY = Mathf.Min(minY, v.Y);
				minZ = Mathf.Min(minZ, v.Z);

				maxX = Mathf.Max(maxX, v.X);
				maxY = Mathf.Max(maxY, v.Y);
				maxZ = Mathf.Max(maxZ, v.Z);
			}


			Vector3Int size = new Vector3Int(maxX - minX + 1, maxY - minY + 1, maxZ - minZ + 1);
			clusterVoxels = new Voxel[size.x, size.y, size.z];
			for(int z = 0; z < size.z; z++) {
				for(int y = 0; y < size.y; y++) {
					for(int x = 0; x < size.x; x++) {
						clusterVoxels[x, y, z] = new Voxel(x, y, z, isFilled: false);
					}
				}
			}

			isStatic = wasStatic && minY == 0;
			pivot = Vector3.zero;
			float divisor = 0f;

			foreach(var v in foundVoxels) {
				int newX = v.X - minX;
				int newY = v.Y - minY;
				int newZ = v.Z - minZ;

				clusterVoxels[newX, newY, newZ].CopyProperties(v);

                if(!isStatic || newY == 0) {
					pivot += new Vector3(newX, newY, newZ);
					divisor++;
                }
			}

			pivot /= divisor;
            if(isStatic) {
				pivot.y = -0.5f;
            }
		}

		private static void TryRecursiveAdd(Voxel voxel, List<Voxel> foundVoxels, Voxel[,,] grid) {
			void TryAddNeighbor(int x, int y, int z) {
				Voxel neighbor;
				if(!TryGetVoxelAt(x, y, z, grid, out neighbor) || !neighbor.IsFilled || neighbor.HasBeenTouchedByBucketFill) {
					return;
				}

				TryRecursiveAdd(neighbor, foundVoxels, grid);
			}

			voxel.HasBeenTouchedByBucketFill = true;
			foundVoxels.Add(voxel);

			TryAddNeighbor(voxel.X + 1, voxel.Y, voxel.Z);
			TryAddNeighbor(voxel.X - 1, voxel.Y, voxel.Z);
			TryAddNeighbor(voxel.X, voxel.Y + 1, voxel.Z);
			TryAddNeighbor(voxel.X, voxel.Y - 1, voxel.Z);
			TryAddNeighbor(voxel.X, voxel.Y, voxel.Z + 1);
			TryAddNeighbor(voxel.X, voxel.Y, voxel.Z - 1);
		}

		public Voxel[,,] GetVoxels() {
			return clusterVoxels;
		}

		public Vector3 GetPivot() {
			return pivot;
		}

		public Vector3Int GetOffset() {
			return new Vector3Int(minX, minY, minZ);
		}

		public bool GetIsStatic() {
			return isStatic;
		}
	}

	[SerializeField] private VoxelBuilder voxelBuilder;
	[SerializeField] private Texture2D[] textures;

	[SerializeField, HideInInspector] private bool isStatic;
	[SerializeField, HideInInspector] private bool isDescendant;
	[SerializeField, HideInInspector] private Vector3Int size;

	private Voxel[,,] voxels;
	private new Rigidbody rigidbody;

	private bool isDirty;
	private const float UPDATE_LATENCY = 0.1f;
	private float timeToUpdate;

	private void OnValidate() {
		voxelBuilder = GetComponentInChildren<VoxelBuilder>();
        if(voxelBuilder == null) {
			GameObject child = new GameObject();
			child.transform.name = "VoxelBuilder";
			child.transform.parent = transform;
			child.transform.localPosition = Vector3.zero;
			voxelBuilder = child.AddComponent<VoxelBuilder>();
        }
    }

    private void Awake() {
		rigidbody = GetComponent<Rigidbody>();
	}

    private void Start() {
        if(!isDescendant) {
			voxels = new Voxel[size.x, size.y, size.z];

			for(int z = 0; z < size.z; z++) {
				for(int y = 0; y < size.y; y++) {
					for(int x = 0; x < size.x; x++) {
						voxels[x, y, z] = new Voxel(x, y, z, textures[y].GetPixel(x, z).r > 0);
					}
				}
			}

			voxelBuilder.transform.localPosition = new Vector3(-(size.x / 2), 0.5f, -(size.z / 2));

			isDirty = true;
		}
    }

    private void Update() {
		rigidbody.isKinematic = isStatic;

		if(voxels == null) {
			return;
		}

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		List<Voxel> hits = new List<Voxel>();
		for(int z = 0; z < size.z; z++) {
			for(int y = 0; y < size.y; y++) {
				for(int x = 0; x < size.x; x++) {
					Voxel voxel = voxels[x, y, z];

					if(!voxel.IsFilled) {
						continue;
					}

					Bounds b = new Bounds(voxel.GetWorldPos(voxelBuilder), Vector3.one);
					if(b.IntersectRay(ray)) {
						hits.Add(voxel);
					}
				}
			}
		}

		float closestHit = Mathf.Infinity;
		int closestHitIndex = -1;
		for(int i = 0; i < hits.Count; i++) {
			Voxel hit = hits[i];
			float dist = Vector3.Distance(Camera.main.transform.position, hit.GetWorldPos(voxelBuilder));

			if(dist < closestHit) {
				closestHit = dist;
				closestHitIndex = i;
			}
		}

		if(closestHitIndex >= 0) {
			Voxel hit = hits[closestHitIndex];
			voxels[hit.X, hit.Y, hit.Z].IsFilled = false;
			isDirty = true;
		}
	}

	private void LateUpdate() {
		if(Time.time < timeToUpdate) {
			return;
		}
		else {
			timeToUpdate = Time.time + UPDATE_LATENCY;
		}

		if(!isDirty) {
			return;
		}
		else {
			isDirty = false;
		}

		List<VoxelCluster> voxelClusters = new List<VoxelCluster>();
		for(int z = 0; z < size.z; z++) {
			for(int y = 0; y < size.y; y++) {
				for(int x = 0; x < size.x; x++) {
					Voxel voxel = voxels[x, y, z];
					
					if(!voxel.IsFilled) {
						continue;
					}
					if(voxel.HasBeenTouchedByBucketFill) {
						continue;
					}

					VoxelCluster newCluster = new VoxelCluster();
					newCluster.Fill(voxels, voxel, isStatic);
					voxelClusters.Add(newCluster);
				}
			}
		}

		for(int z = 0; z < size.z; z++) {
			for(int y = 0; y < size.y; y++) {
				for(int x = 0; x < size.x; x++) {
					voxels[x, y, z].HasBeenTouchedByBucketFill = false;
				}
			}
		}

		for(int i = voxelClusters.Count - 1; i >= 0; i--) {
			VoxelController voxelController;
			if(i == 0) {
				voxelController = this;
			}
			else {
				GameObject go = Instantiate(gameObject, transform.parent);
				voxelController = go.GetComponent<VoxelController>();
				voxelController.isDescendant = true;
			}

			VoxelCluster cluster = voxelClusters[i];

			voxelController.isStatic = cluster.GetIsStatic();

			Vector3 GetLocalPosWithWorldRotation(Vector3 localPos, Transform t) {
				return (t.TransformPoint(localPos) - t.position);
			}

			voxelController.voxelBuilder.transform.position += GetLocalPosWithWorldRotation(cluster.GetOffset(), voxelController.voxelBuilder.transform);

			if(voxelController.isDescendant) {
				voxelController.voxelBuilder.transform.parent = null;
				voxelController.transform.position = voxelController.voxelBuilder.transform.position + GetLocalPosWithWorldRotation(cluster.GetPivot(), voxelController.voxelBuilder.transform);
				voxelController.voxelBuilder.transform.parent = voxelController.transform;
			}

			voxelController.ApplyNewVoxels(cluster.GetVoxels());
        }
	}

	private void ApplyNewVoxels(Voxel[,,] newVoxels) {
		voxels = newVoxels;
		size = new Vector3Int(newVoxels.GetLength(0), newVoxels.GetLength(1), newVoxels.GetLength(2));
		voxelBuilder.Build(voxels);
	}

	public static bool TryGetVoxelAt(int x, int y, int z, Voxel[,,] voxels, out Voxel voxel) {
		if(x < 0 || y < 0 || z < 0 || x >= voxels.GetLength(0) || y >= voxels.GetLength(1) || z >= voxels.GetLength(2)) {
			voxel = null;
			return false;
		}

		voxel = voxels[x, y, z];
		return true;
	}
}
