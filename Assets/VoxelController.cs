using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VoxelController : MonoBehaviour {

	public class Voxel {
		public int Index;
		public bool IsFilled;
		public Color Color;

		public bool HasBeenTouchedByBucketFill;
		public bool HasNeighborUp;
		public bool HasNeighborDown;
		public bool HasNeighborLeft;
		public bool HasNeighborRight;
		public bool HasNeighborFore;
		public bool HasNeighborBack;



		public Voxel(int i, bool isFilled) {
			Index = i;
			IsFilled = isFilled;
			HasBeenTouchedByBucketFill = false;
		}

		public void CopyProperties(Voxel otherVoxel) {
			IsFilled = otherVoxel.IsFilled;
			Color = otherVoxel.Color;
		}

		

		public static int GetIndex(int x, int y, int z, Vector3Int dimensions) {
			return x + dimensions.x * (y + dimensions.y * z);
		}

		public static Vector3Int GetCoordinates(int index, Vector3Int dimensions) {
			return new Vector3Int(index % dimensions.x, (index / dimensions.x) % dimensions.y, index / (dimensions.x * dimensions.y));
		}

		public static bool TryGet(int x, int y, int z, Voxel[] voxels, Vector3Int dimensions, out Voxel voxel) {
			if(x < 0 || y < 0 || z < 0 || x >= dimensions.x || y >= dimensions.y || z >= dimensions.z) {
				voxel = null;
				return false;
			}

			voxel = voxels[GetIndex(x, y, z, dimensions)];
			return true;
		}
	}

	//public readonly struct Voxel {
	//	public readonly int x;
	//	public readonly int y;
	//	public readonly int z;
	//	public readonly bool IsFilled;
	//	public readonly Color Color;

	//	public readonly bool HasNeighborUp;
	//	public readonly bool HasNeighborDown;
	//	public readonly bool HasNeighborLeft;
	//	public readonly bool HasNeighborRight;
	//	public readonly bool HasNeighborFore;
	//	public readonly bool HasNeighborBack;

	//	public Voxel(int x, int y, int z, bool isFilled, Color color, bool hasNeighborUp, bool hasNeighborDown, bool hasNeighborLeft, bool hasNeighborRight, bool hasNeighborFore, bool hasNeighborBack) {
	//		this.x = x;
	//		this.y = y;
	//		this.z = z;
	//		IsFilled = isFilled;
	//		Color = color;
	//		HasNeighborUp = hasNeighborUp;
	//		HasNeighborDown = hasNeighborDown;
	//		HasNeighborLeft = hasNeighborLeft;
	//		HasNeighborRight = hasNeighborRight;
	//		HasNeighborFore = hasNeighborFore;
	//		HasNeighborBack = hasNeighborBack;
	//	}

	//	public static Voxel GetChangedVoxel(Voxel v, bool isFilled) {
	//		return new Voxel(v.x, v.y, v.z, isFilled, v.Color, v.HasNeighborUp, v.HasNeighborDown, v.HasNeighborLeft, v.HasNeighborRight, v.HasNeighborFore, v.HasNeighborBack);
	//	}

	//	public static Voxel GetChangedVoxel(Voxel v, Color color) {
	//		return new Voxel(v.x, v.y, v.z, v.IsFilled, color, v.HasNeighborUp, v.HasNeighborDown, v.HasNeighborLeft, v.HasNeighborRight, v.HasNeighborFore, v.HasNeighborBack);
	//	}

	//	public static Voxel GetChangedVoxel(Voxel v, bool hasNeighborUp, bool hasNeighborDown, bool hasNeighborLeft, bool hasNeighborRight, bool hasNeighborFore, bool hasNeighborBack) {
	//		return new Voxel(v.x, v.y, v.z, v.IsFilled, v.Color, hasNeighborUp, hasNeighborDown, hasNeighborLeft, hasNeighborRight, hasNeighborFore, hasNeighborBack);
	//	}

	//	public static Vector3 GetWorldPos(int x, int y, int z, Transform t) {
	//		return t.TransformPoint(new Vector3(x, y, z));
	//	}

	//	public static int GetIndex(int x, int y, int z, Vector3Int dimensions) {
	//		return x + dimensions.x * (y + dimensions.y * z);
	//	}
	//}
	 
	private class VoxelCluster {
		private Voxel[] newVoxels;
		private List<Voxel> newVoxelsList;
		private int minX, minY, minZ, maxX, maxY, maxZ;
		private Vector3 pivot;
		private bool isStatic;
		private Vector3Int newDimensions;
		public Color color;

		public void Fill(Voxel[] oldVoxels, Vector3Int oldDimensions, Voxel startVoxel, bool wasStatic) {
			newVoxelsList = new List<Voxel>();
			TryRecursiveAdd(startVoxel, newVoxelsList, oldVoxels, oldDimensions);

			color = new Color(Random.value, Random.value, Random.value, 1);
			foreach(var foundVoxel in newVoxelsList) {
				foundVoxel.Color = color;
			}

			minX = int.MaxValue;
			minY = int.MaxValue;
			minZ = int.MaxValue;
			maxX = int.MinValue;
			maxY = int.MinValue;
			maxZ = int.MinValue;

			foreach(var v in newVoxelsList) {
				Vector3Int coords = Voxel.GetCoordinates(v.Index, oldDimensions);

				minX = Mathf.Min(minX, coords.x);
				minY = Mathf.Min(minY, coords.y);
				minZ = Mathf.Min(minZ, coords.z);

				maxX = Mathf.Max(maxX, coords.x);
				maxY = Mathf.Max(maxY, coords.y);
				maxZ = Mathf.Max(maxZ, coords.z);
			}

			newDimensions = new Vector3Int(maxX - minX + 1, maxY - minY + 1, maxZ - minZ + 1);
			newVoxels = new Voxel[newDimensions.x * newDimensions.y * newDimensions.z];
		
            for(int i = 0; i < newVoxels.Length; i++) {
				newVoxels[i] = new Voxel(i, isFilled: false);
			}

			isStatic = wasStatic && minY == 0;
			pivot = Vector3.zero;
			float divisor = 0f;

			foreach(var v in newVoxelsList) {

				Vector3Int oldCoords = Voxel.GetCoordinates(v.Index, oldDimensions);
				
				int newX = oldCoords.x - minX;
				int newY = oldCoords.y - minY;
				int newZ = oldCoords.z - minZ;

				int newIndex = Voxel.GetIndex(newX, newY, newZ, newDimensions);

				// hack until newVoxelsList isn't needed
				v.Index = newIndex;
				//

				newVoxels[newIndex].CopyProperties(v);
				

				Vector3Int coords = Voxel.GetCoordinates(newIndex, newDimensions);
                if(!isStatic || coords.y == 0) {
					pivot += new Vector3(coords.x, coords.y, coords.z);
					divisor++;
                }
			}

			pivot /= divisor;
            if(isStatic) {
				pivot.y = -0.5f;
            }
		}

		private static void TryRecursiveAdd(Voxel voxel, List<Voxel> foundVoxels, Voxel[] grid, Vector3Int dimensions) {
            void TryGoToNeighbor(int x, int y, int z, out bool doesNeighborExist) {
                Voxel neighbor;

				doesNeighborExist = Voxel.TryGet(x, y, z, grid, dimensions, out neighbor) && neighbor.IsFilled;

                if(!doesNeighborExist || neighbor.HasBeenTouchedByBucketFill) {
					return;
                }

                TryRecursiveAdd(neighbor, foundVoxels, grid, dimensions);
            }

            voxel.HasBeenTouchedByBucketFill = true;

			Vector3Int coords = Voxel.GetCoordinates(voxel.Index, dimensions);
            TryGoToNeighbor(coords.x + 1, coords.y, coords.z, out voxel.HasNeighborRight);
            TryGoToNeighbor(coords.x - 1, coords.y, coords.z, out voxel.HasNeighborLeft);
            TryGoToNeighbor(coords.x, coords.y + 1, coords.z, out voxel.HasNeighborUp);
            TryGoToNeighbor(coords.x, coords.y - 1, coords.z, out voxel.HasNeighborDown);
            TryGoToNeighbor(coords.x, coords.y, coords.z + 1, out voxel.HasNeighborFore);
            TryGoToNeighbor(coords.x, coords.y, coords.z - 1, out voxel.HasNeighborBack);

			// OPTIMIZATION: this could save a lot of iterating, but might not be necessary after we Job-ify this
            //if(!voxel.HasNeighborRight || !voxel.HasNeighborLeft || !voxel.HasNeighborUp || !voxel.HasNeighborDown || !voxel.HasNeighborFore || !voxel.HasNeighborBack) {
                foundVoxels.Add(voxel);
            //}
        }

		public Voxel[] GetVoxels() {
			return newVoxels;
		}

		public List<Voxel> GetVoxelsAsList() {
			return newVoxelsList;
		}

		public Vector3Int GetDimensions() {
			return newDimensions;
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
	[SerializeField, HideInInspector] private Vector3Int dimensions;

	private Voxel[] voxels;
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
			dimensions = new Vector3Int(10, 10, 10);
			voxels = new Voxel[dimensions.x * dimensions.y * dimensions.z];

            for(int i = 0; i < voxels.Length; i++) {
				voxels[i] = new Voxel(i, true);
			}


			voxelBuilder.transform.localPosition = new Vector3(-(dimensions.x / 2), 0.5f, -(dimensions.z / 2));

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
        
		for(int i = 0; i < voxels.Length; i++) {
			Voxel voxel = voxels[i];

			if(!voxel.IsFilled) {
				continue;
			}

			Bounds b = new Bounds(GetVoxelWorldPos(voxel.Index), Vector3.one);
			if(b.IntersectRay(ray)) {
				hits.Add(voxel);
			}
		}

		float closestHit = Mathf.Infinity;
		int closestHitIndex = -1;
		for(int i = 0; i < hits.Count; i++) {
			Voxel hit = hits[i];
			float dist = Vector3.Distance(Camera.main.transform.position, GetVoxelWorldPos(hit.Index));

			if(dist < closestHit) {
				closestHit = dist;
				closestHitIndex = i;
			}
		}

		if(closestHitIndex >= 0) {
			Voxel hit = hits[closestHitIndex];
			voxels[hit.Index].IsFilled = false;
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
        for(int i = 0; i < voxels.Length; i++) {
			Voxel voxel = voxels[i];

			if(!voxel.IsFilled) {
				continue;
			}
			if(voxel.HasBeenTouchedByBucketFill) {
				continue;
			}

			VoxelCluster newCluster = new VoxelCluster();
			newCluster.Fill(voxels, dimensions, voxel, isStatic);
			voxelClusters.Add(newCluster);
		}

        for(int i = 0; i < voxels.Length; i++) {
			voxels[i].HasBeenTouchedByBucketFill = false;
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

			voxelController.ApplyNewVoxels(cluster);
        }
	}

	private void ApplyNewVoxels(VoxelCluster cluster) {
		voxels = cluster.GetVoxels();
		dimensions = cluster.GetDimensions();
		voxelBuilder.Build(cluster.GetVoxelsAsList().ToArray(), dimensions);
	}

	//private Voxel GetVoxel(int x, int y, int z) {
	//	return voxels[Voxel.GetIndex(x, y, z, dimensions)];
	//}

	public Vector3 GetVoxelLocalPos(int index) {
		return Voxel.GetCoordinates(index, dimensions);
	}

	public Vector3 GetVoxelWorldPos(int index) {
		return voxelBuilder.transform.TransformPoint(GetVoxelLocalPos(index));
	}
}
