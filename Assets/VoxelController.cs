using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VoxelController : MonoBehaviour {

	[SerializeField] private VoxelBuilder voxelBuilder;
	[SerializeField] private Texture2D[] textures;

	[SerializeField, HideInInspector] private bool isStatic;
	[SerializeField, HideInInspector] private bool isDescendant;

	private VoxelGrid voxelGrid;
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
			isStatic = true;

			voxelGrid = new VoxelGrid(new Vector3Int(32, 32, 32));

			voxelBuilder.transform.localPosition = new Vector3(-(voxelGrid.GetDimensions().x / 2), 0.5f, -(voxelGrid.GetDimensions().z / 2));

			isDirty = true;
		}
    }

    private void Update() {
		rigidbody.isKinematic = isStatic;

		if(voxelGrid == null) {
			return;
		}

        if(Input.GetKeyDown(KeyCode.Space)) {
            for(int z = 0; z < voxelGrid.GetDimensions().z; z++) {
                for(int y = 0; y < voxelGrid.GetDimensions().y; y++) {
					voxelGrid.ModifyVoxel(voxelGrid.GetDimensions().x / 2, y, z, isFilled: false);
                }
            }

			isDirty = true;
        }

		//return;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		List<int> hitIndexes = new List<int>();

		//RaycastHit hit;
  //      if(Physics.Raycast(ray, out hit)) {
		//	Debug.Log(hit.point);
  //      }

		Bounds b = new Bounds(Vector3.zero, Vector3.one);
		for(int i = 0; i < voxelGrid.GetVoxelCount(); i++) {
			Voxel voxel = voxelGrid.GetVoxel(i);

			if(!voxel.IsFilled) {
				continue;
			}

            if(voxel.HasNeighborRight && voxel.HasNeighborLeft && voxel.HasNeighborUp && voxel.HasNeighborDown && voxel.HasNeighborFore && voxel.HasNeighborBack) {
				continue;
            }

			b.center = GetVoxelWorldPos(voxel.Index);
			if(b.IntersectRay(ray)) {
				hitIndexes.Add(voxel.Index);
			}
		}

		float closestHit = Mathf.Infinity;
		int closestHitIndex = -1;
		for(int i = 0; i < hitIndexes.Count; i++) {
			int hitIndex = hitIndexes[i];
			float dist = Vector3.Distance(Camera.main.transform.position, GetVoxelWorldPos(hitIndex));

			if(dist < closestHit) {
				closestHit = dist;
				closestHitIndex = i;
			}
		}

		if(closestHitIndex >= 0) {
			voxelGrid.ModifyVoxel(hitIndexes[closestHitIndex], isFilled: false);
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

		List<VoxelCluster> voxelClusters = new List<VoxelCluster>(); // TODO: maybe replace this with a queue, but low prio
		int voxelCount = voxelGrid.GetVoxelCount();
		bool[] visitedVoxels = new bool[voxelCount];

		for(int i = 0; i < voxelCount; i++) {
			if(visitedVoxels[i]) {
				continue;
			}

			Voxel voxel = voxelGrid.GetVoxel(i);
			if(!voxel.IsFilled) {
				continue;
			}

			voxelClusters.Add(new VoxelCluster(voxel, voxelGrid, isStatic, visitedVoxels));
		}

		for(int i = voxelClusters.Count - 1; i >= 0; i--) {
			if(i == 0) {
				ApplyVoxelCluster(this, voxelClusters[i]);
			}
			else {
				GameObject go = Instantiate(gameObject, transform.parent);
				VoxelController vc = go.GetComponent<VoxelController>();
				vc.isDescendant = true;
				ApplyVoxelCluster(vc, voxelClusters[i]);
			}
		}
	}

	private static void ApplyVoxelCluster(VoxelController controller, VoxelCluster cluster) {
		controller.isStatic = cluster.IsStatic;

		Vector3 GetLocalPosWithWorldRotation(Vector3 localPos, Transform t) {
			return (t.TransformPoint(localPos) - t.position);
		}

		controller.voxelBuilder.transform.position += GetLocalPosWithWorldRotation(cluster.Offset, controller.voxelBuilder.transform);

		if(controller.isDescendant) {
			controller.voxelBuilder.transform.parent = null;
			controller.transform.position = controller.voxelBuilder.transform.position + GetLocalPosWithWorldRotation(cluster.Pivot, controller.voxelBuilder.transform);
			controller.voxelBuilder.transform.parent = controller.transform;
		}

		controller.ApplyNewVoxels(cluster);
	}

	private void ApplyNewVoxels(VoxelCluster cluster) {
		voxelGrid = new VoxelGrid(cluster.Dimensions, cluster.Voxels);
		voxelBuilder.Build(voxelGrid);
	}

	//private Voxel GetVoxel(int x, int y, int z) {
	//	return voxels[Voxel.GetIndex(x, y, z, dimensions)];
	//}

	public Vector3 GetVoxelLocalPos(int index) {
		return VoxelGrid.IndexToCoords(index, voxelGrid.GetDimensions());
	}

	public Vector3 GetVoxelWorldPos(int index) {
		return voxelBuilder.transform.TransformPoint(GetVoxelLocalPos(index));
	}

	private class VoxelCluster {
		public Voxel[] Voxels { get; private set; }
		public Vector3 Pivot { get; private set; }
		public Vector3Int Offset { get; private set; }
		public Vector3Int Dimensions { get; private set; }
		public bool IsStatic { get; private set; }

		public VoxelCluster(Voxel startVoxel, VoxelGrid grid, bool wasStatic, bool[] visitedVoxels) {
			Vector3Int dimensions;
			Vector3Int offset;
			bool isTouchingGround;
			Voxels = FindVoxels(startVoxel, grid, visitedVoxels, out dimensions, out offset, out isTouchingGround);

			Offset = offset;
			Dimensions = dimensions;
			IsStatic = wasStatic && isTouchingGround;

			Pivot = GetClusterPivot(Voxels, dimensions, IsStatic);
		}

		private static Voxel[] FindVoxels(Voxel startVoxel, VoxelGrid grid, bool[] visitedVoxels, out Vector3Int newDimensions, out Vector3Int offset, out bool isTouchingGround) {
			Vector3Int minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
			Vector3Int maxCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

			Queue<Voxel> q1 = new Queue<Voxel>();
			Queue<Voxel> q2 = new Queue<Voxel>();

			q1.Enqueue(startVoxel);

			while(q1.Count > 0) {
				Voxel v = q1.Dequeue();

				if(!v.IsFilled) {
					continue;
				}

				if(visitedVoxels[v.Index]) {
					continue;
				}

				visitedVoxels[v.Index] = true;

				Vector3Int coords = VoxelGrid.IndexToCoords(v.Index, grid.GetDimensions());
				minCoord.x = Mathf.Min(minCoord.x, coords.x);
				minCoord.y = Mathf.Min(minCoord.y, coords.y);
				minCoord.z = Mathf.Min(minCoord.z, coords.z);
				maxCoord.x = Mathf.Max(maxCoord.x, coords.x);
				maxCoord.y = Mathf.Max(maxCoord.y, coords.y);
				maxCoord.z = Mathf.Max(maxCoord.z, coords.z);

				Voxel vRight, vLeft, vUp, vDown, vFore, vBack;
				bool hasNeighborRight = grid.TryGetVoxel(coords.x + 1, coords.y, coords.z, out vRight) && vRight.IsFilled;
				bool hasNeighborLeft = grid.TryGetVoxel(coords.x - 1, coords.y, coords.z, out vLeft) && vLeft.IsFilled;
				bool hasNeighborUp = grid.TryGetVoxel(coords.x, coords.y + 1, coords.z, out vUp) && vUp.IsFilled;
				bool hasNeighborDown = grid.TryGetVoxel(coords.x, coords.y - 1, coords.z, out vDown) && vDown.IsFilled;
				bool hasNeighborFore = grid.TryGetVoxel(coords.x, coords.y, coords.z + 1, out vFore) && vFore.IsFilled;
				bool hasNeighborBack = grid.TryGetVoxel(coords.x, coords.y, coords.z - 1, out vBack) && vBack.IsFilled;

				q2.Enqueue(Voxel.GetChangedVoxel(v, hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack));

				if(hasNeighborRight)
					q1.Enqueue(vRight);
				if(hasNeighborLeft)
					q1.Enqueue(vLeft);
				if(hasNeighborUp)
					q1.Enqueue(vUp);
				if(hasNeighborDown)
					q1.Enqueue(vDown);
				if(hasNeighborFore)
					q1.Enqueue(vFore);
				if(hasNeighborBack)
					q1.Enqueue(vBack);
			}

			newDimensions = maxCoord - minCoord + Vector3Int.one;
			Voxel[] newVoxels = new Voxel[newDimensions.x * newDimensions.y * newDimensions.z];
			Color newVoxelColor = new Color(Random.value, Random.value, Random.value, 1f);

			while(q2.Count > 0) {
				Voxel v = q2.Dequeue();

				Vector3Int oldCoords = VoxelGrid.IndexToCoords(v.Index, grid.GetDimensions());
				Vector3Int newCoords = oldCoords - minCoord;
				int newIndex = VoxelGrid.CoordsToIndex(newCoords, newDimensions);

				newVoxels[newIndex] = Voxel.GetChangedVoxel(v, newIndex, newVoxelColor);
			}

			offset = minCoord;
			isTouchingGround = minCoord.y == 0;

			return newVoxels;
		}

		private static Vector3 GetClusterPivot(Voxel[] cluster, Vector3Int clusterDimensions, bool isStatic) {
			Vector3 pivot = Vector3.zero;
			float divisor = 0f;

			for(int i = 0; i < cluster.Length; i++) {
				Voxel v = cluster[i];
				if(!v.IsFilled) {
					continue;
				}

				Vector3Int coords = VoxelGrid.IndexToCoords(v.Index, clusterDimensions);

				if(!isStatic || coords.y == 0) {
					pivot += coords;
					divisor++;
				}
			}

			pivot /= divisor;
			if(isStatic) {
				pivot.y = -0.5f;
			}

			return pivot;
		}
	}
}
