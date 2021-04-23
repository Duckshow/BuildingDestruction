using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VoxelGrid), typeof(Rigidbody))]
public class VoxelController : MonoBehaviour {

	[SerializeField] private Transform meshTransform;
	[SerializeField, HideInInspector] private bool isOriginal = true;

	private VoxelGrid voxelGrid;
	private new Rigidbody rigidbody;

	private bool isStatic;
	private Queue<int> dirtyVoxels = new Queue<int>();
	private Queue<int> dirtyBins = new Queue<int>();

	private const float UPDATE_LATENCY = 0.1f;
	private float timeToUpdate;

    private void Awake() {
		rigidbody = GetComponent<Rigidbody>();
		voxelGrid = GetComponent<VoxelGrid>();
    }

    private void Start() {
		//meshTransform = TryGetNewMeshTransform(transform);
		voxelGrid.SubscribeToOnVoxelUpdate(OnVoxelUpdated);

		if(isOriginal) {
			Vector3Int dimensions = new Vector3Int(16, 16, 16);

            // this just ensures that the initial building will be in the same spot as it was placed in the editor - a bit ugly, but I haven't figured out anything better yet
            meshTransform.localPosition = new Vector3(-(dimensions.x / 2f), 0.5f, -(dimensions.z / 2f));

            Voxel[] voxels = new Voxel[dimensions.x * dimensions.y * dimensions.z];
            for(int i = 0; i < voxels.Length; i++) {
				voxels[i] = new Voxel(i, VoxelGrid.IndexToCoords(i, dimensions));
            }

			ApplySettings(voxels, bins: null, dimensions, offset: Vector3Int.zero, isStatic: true, isOriginalSetup: true);

			for(int i = 0; i < voxels.Length; i++) {
				voxelGrid.SetVoxelIsFilled(i, true);
			}
		}
	}

	//private static Transform TryGetNewMeshTransform(Transform root) {
	//	Transform meshTransform;
        
	//	if(root.childCount > 0) {
 //           meshTransform = root.GetChild(0);
 //       }
 //       else {
	//		Debug.Log("Made my own meshtransform!");
 //           GameObject child = new GameObject();
 //           child.transform.parent = root;
 //           child.transform.localPosition = Vector3.zero;
 //           meshTransform = child.transform;
 //       }

	//	Debug.AssertFormat(meshTransform.GetComponents<Component>() == null, "Tried to use {0} as MeshTransform!", meshTransform.name);
	//	meshTransform.name = "MeshTransform";
		
	//	return meshTransform;
 //   }

	private void Update() {
		rigidbody.isKinematic = isStatic;

		if(voxelGrid == null) {
			return;
		}

        if(Input.GetKeyDown(KeyCode.Space)) {
            for(int z = 0; z < voxelGrid.GetVoxelGridDimensions().z; z++) {
                for(int y = 0; y < voxelGrid.GetVoxelGridDimensions().y; y++) {
					Vector3Int coords = new Vector3Int(voxelGrid.GetVoxelGridDimensions().x / 2, y, z);
					int index = VoxelGrid.CoordsToIndex(coords, voxelGrid.GetVoxelGridDimensions());
					
					voxelGrid.SetVoxelIsFilled(index, false);
                }
            }
        }

        if(Input.GetMouseButton(0)) {
			FireBeam(isInstant: true);
        }
        else if(Input.GetMouseButton(1)) {
			FireBeam(isInstant: false);
        }
	}

	private void FireBeam(bool isInstant) {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		List<int> hitIndexes = new List<int>();

		Bounds b = new Bounds(Vector3.zero, Vector3.one);
		for(int i = 0; i < voxelGrid.GetVoxelCount(); i++) {
			Voxel voxel;

			Vector3Int coords = VoxelGrid.IndexToCoords(i, voxelGrid.GetVoxelGridDimensions());
			if(!voxelGrid.TryGetVoxel(coords.x, coords.y, coords.z, out voxel)) {
				Debug.LogError("Something went wrong!");
				continue;
            }

			if(!voxel.IsFilled) {
				continue;
			}

			b.center = GetVoxelWorldPos(voxel.Index);
			if(b.IntersectRay(ray)) {
				hitIndexes.Add(voxel.Index);
			}
		}

        if(isInstant) {
			for(int i = 0; i < hitIndexes.Count; i++) {
				voxelGrid.SetVoxelIsFilled(hitIndexes[i], false);
			}
		}
        else {
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
				voxelGrid.SetVoxelIsFilled(hitIndexes[closestHitIndex], false);
			}
		}
    }

	private void OnVoxelUpdated(int index) {
		Vector3Int dimensions = voxelGrid.GetVoxelGridDimensions();
		Vector3Int coords = VoxelGrid.IndexToCoords(index, dimensions);

		TrySetVoxelDirty(coords.x + 1, coords.y, coords.z);
		TrySetVoxelDirty(coords.x - 1, coords.y, coords.z);
		TrySetVoxelDirty(coords.x, coords.y + 1, coords.z);
		TrySetVoxelDirty(coords.x, coords.y - 1, coords.z);
		TrySetVoxelDirty(coords.x, coords.y, coords.z + 1);
		TrySetVoxelDirty(coords.x, coords.y, coords.z - 1);
	}

	private void TrySetVoxelDirty(int x, int y, int z) {
		Voxel v;
        if(!voxelGrid.TryGetVoxel(x, y, z, out v)) {
			return;
        }

		if(!v.IsFilled) {
			return;
		}

		int index = VoxelGrid.CoordsToIndex(x, y, z, voxelGrid.GetVoxelGridDimensions());
		if(dirtyVoxels.Contains(index)) {
			return;
		}

		dirtyVoxels.Enqueue(index);
	}

	private void LateUpdate() {
		if(dirtyVoxels.Count == 0) {
			return;
		}

		if(Time.time < timeToUpdate) {
            return;
        }
        else {
            timeToUpdate = Time.time + UPDATE_LATENCY;
        }

		UpdateDirtyVoxels();
		UpdateDirtyBins();
	}

	private void UpdateDirtyVoxels() {
		while(dirtyVoxels.Count > 0) {
			int index = dirtyVoxels.Dequeue();

			int binIndex = VoxelGrid.VoxelToBinIndex(index, voxelGrid.GetVoxelGridDimensions());
			if(!dirtyBins.Contains(binIndex)) {
				dirtyBins.Enqueue(binIndex);
			}

			voxelGrid.RefreshVoxelHasNeighborValues(index);
		}

		Debug.Assert(dirtyVoxels.Count == 0);
	}

	private void UpdateDirtyBins() {
		List<VoxelCluster> clusters = new List<VoxelCluster>();
		bool[] visitedBins = new bool[voxelGrid.GetVoxelCount()]; // TODO: if I cached how many voxels have been visited, we could just break the loop when that number is reached, thus potentially saving a bunch of iterating

		Queue<int> dirtyBinsLoop2 = new Queue<int>();
		Queue<int> dirtyBinsLoop3 = new Queue<int>();

		while(dirtyBins.Count > 0) {
			int binIndex = dirtyBins.Dequeue();
			dirtyBinsLoop2.Enqueue(binIndex);

			voxelGrid.RefreshBinHasVoxelValues(binIndex);
		}

        while(dirtyBinsLoop2.Count > 0) {
			int binIndex = dirtyBinsLoop2.Dequeue();
			dirtyBinsLoop3.Enqueue(binIndex);
		
			voxelGrid.RefreshBinHasConnectionValues(binIndex);
		}

		while(dirtyBinsLoop3.Count > 0) {
			int binIndex = dirtyBinsLoop3.Dequeue();

			VoxelCluster cluster;
			if(voxelGrid.TryFindVoxelCluster(binIndex, visitedBins, out cluster)) {
				clusters.Add(cluster);
			}
		}

		ApplyClustersToVoxelControllers(clusters, this);

		Debug.Assert(dirtyBins.Count == 0);
	}

	private static void ApplyClustersToVoxelControllers(List<VoxelCluster> clusters, VoxelController caller) { // TODO: very strange having a static method that just gets the instance as a parameter - like why even be static then? figure out something better.
		Debug.Assert(clusters.Count > 0);

		if(clusters.Count > 1) {
			Debug.LogFormat("==========SPLIT: {0}==========", clusters.Count);
			for(int i = 0; i < clusters.Count; i++) {
				VoxelCluster cluster = clusters[i];
				Vector3Int voxelGridDimensions = cluster.Dimensions;
				Vector3Int binGridDimensions = VoxelGrid.CalculateBinGridDimensions(voxelGridDimensions);

				Debug.LogFormat("Cluster #{0}: Voxels: {1}, Bins: {2}, Offset: {3}", i, voxelGridDimensions, binGridDimensions, cluster.Offset);
			}
			Debug.LogFormat("==============================");
		}

		int biggestClusterIndex = VoxelGrid.GetBiggestVoxelClusterIndex(clusters);

        for(int i0 = 0; i0 < clusters.Count; i0++) {
            if(i0 == biggestClusterIndex) {
				continue;
			}

			Transform[] meshObjects = caller.meshTransform.GetComponentsInChildren<Transform>(includeInactive: true);
            for(int i1 = 1; i1 < meshObjects.Length; i1++) {
				meshObjects[i1].parent = null;
            }

			GameObject go = Instantiate(caller.gameObject, caller.transform.parent);
			
			for(int i1 = 0; i1 < meshObjects.Length; i1++) {
				meshObjects[i1].parent = caller.meshTransform;
			}

			go.name = caller.name + " (Cluster)";
			
			VoxelController voxelController = go.GetComponent<VoxelController>();
			voxelController.isOriginal = false;
			//voxelController.meshTransform = TryGetNewMeshTransform(voxelController.transform);

			VoxelCluster cluster = clusters[i0];
			voxelController.ApplySettings(cluster.Voxels, cluster.Dimensions, cluster.Offset, ShouldClusterBeStatic(caller.isStatic, cluster.Offset));
		}

		VoxelCluster biggestCluster = clusters[biggestClusterIndex];
		caller.ApplySettings(biggestCluster.Voxels, biggestCluster.Dimensions, biggestCluster.Offset, ShouldClusterBeStatic(caller.isStatic, biggestCluster.Offset));
	}

	private static bool ShouldClusterBeStatic(bool wasOriginallyStatic, Vector3Int offset) {
		return wasOriginallyStatic && offset.y == 0;
	}

	private void ApplySettings(Voxel[] voxels, Bin[] bins, Vector3Int voxelGridDimensions, Vector3Int offset, bool isStatic, bool isOriginalSetup = false) {
		this.isStatic = isStatic;

		Vector3 pivot = GetPivot(voxels, voxelGridDimensions, isStatic);

		Vector3 GetLocalPosWithWorldRotation(Vector3 localPos, Transform t) {
			return (t.TransformPoint(localPos) - t.position);
		}

        meshTransform.position += GetLocalPosWithWorldRotation(offset, meshTransform);
        meshTransform.parent = null;
        transform.position = meshTransform.position + GetLocalPosWithWorldRotation(pivot, meshTransform);
        meshTransform.parent = transform;

        voxelGrid.ApplySettings(voxels, bins, voxelGridDimensions, isOriginalSetup);
    }

	private static Vector3 GetPivot(Voxel[] voxels, Vector3Int dimensions, bool isStatic) {
		Vector3 pivot = Vector3.zero;
		float divisor = 0f;

		for(int i = 0; i < voxels.Length; i++) {
			Voxel v = voxels[i];
			if(!v.IsFilled) {
				continue;
			}

			Vector3Int coords = v.Coords;// VoxelGrid.IndexToCoords(v.Index, dimensions);

			if(!isStatic || coords.y == 0) {
				pivot += coords;
				divisor++;
			}
		}

        if(Mathf.Approximately(divisor, 0f)) {
			return Vector3.zero;
        }

		pivot /= divisor;
		if(isStatic) {
			pivot.y = -0.5f;
		}

		return pivot;
	}

	public Vector3 GetVoxelWorldPos(int index) {
		Vector3 localPos = VoxelGrid.IndexToCoords(index, voxelGrid.GetVoxelGridDimensions());
		Vector3 worldPos = meshTransform.TransformPoint(localPos);
		return worldPos;
	}

	public static void RunTests() {
		TestGetPivot();
		Debug.Log("Tests done.");
	}

	private static void TestGetPivot() {
		Vector3Int dimensions = new Vector3Int(8, 8, 8);
		Voxel[] cluster = new Voxel[dimensions.x * dimensions.y * dimensions.z];

		for(int z = 0; z < dimensions.z; z++) {
			for(int y = 0; y < dimensions.y; y++) {
				for(int x = 0; x < dimensions.x; x++) {
					Vector3Int coords = new Vector3Int(x, y, z);
					int i = VoxelGrid.CoordsToIndex(coords, dimensions);
					bool isFilled = x == 0 || y == 0 || z == 0 || x == dimensions.x - 1 || y == dimensions.y - 1 || z == dimensions.z - 1;
					cluster[i] = new Voxel(i, coords, isFilled, false, false, false, false, false, false);
				}
			}
		}

		bool isStatic;
		Vector3 pivot;

		isStatic = false;
		pivot = GetPivot(cluster, dimensions, isStatic);
		Debug.LogFormat("GetClusterPivot Result = {0} in hollow cube measuring {1}, isStatic: {2}", pivot, dimensions, isStatic);

		isStatic = true;
		pivot = GetPivot(cluster, dimensions, true);
		Debug.LogFormat("GetClusterPivot Result = {0} in hollow cube measuring {1}, isStatic: {2}", pivot, dimensions, isStatic);
	}
}
