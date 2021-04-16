using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VoxelController : MonoBehaviour {

	private static bool DEBUG = false;

	[SerializeField] private GameObject meshObjectPrefab;
	[SerializeField] private Material material;
	[SerializeField] private Texture2D[] textures;

	[SerializeField, HideInInspector] private bool isOriginal = true;
	
	private Transform meshTransform;

	private VoxelGrid voxelGrid;
	private bool isStatic;
	private Color color;
	private new Rigidbody rigidbody;
	private Queue<int> dirtyVoxels = new Queue<int>();

	private const float UPDATE_LATENCY = 0.1f;
	private float timeToUpdate;

    private void Awake() {
		VoxelBuilder.SetMeshObjectPrefab(meshObjectPrefab); // TODO: handle dependencies such as this in a better way, this makes no sense
	}

	private void Start() {
		TrySetupDependencies();

		if(isOriginal) {
			Vector3Int dimensions = new Vector3Int(16, 16, 16);

            // this just ensures that the initial building will be in the same spot as it was placed in the editor - a bit ugly, but I haven't figured out anything better yet
            meshTransform.localPosition = new Vector3(-(dimensions.x / 2f), 0.5f, -(dimensions.z / 2f));

            Voxel[] voxels = new Voxel[dimensions.x * dimensions.y * dimensions.z];
            for(int i = 0; i < voxels.Length; i++) {
				voxels[i] = new Voxel(i);
            }

			ApplySettings(voxels, dimensions, offset: Vector3Int.zero, isStatic: true, isOriginalSetup: true);

			for(int i = 0; i < voxels.Length; i++) {
				voxelGrid.SetVoxelIsFilled(i, true);
			}
		}
	}

	private void TrySetupDependencies() {
        if(rigidbody == null) {
            rigidbody = GetComponent<Rigidbody>();
        }

        if(meshTransform == null) {
            if(transform.childCount > 0) {
                meshTransform = transform.GetChild(0);
            }
            else {
				Debug.Log("Made my own meshtransform!");
                GameObject child = new GameObject();
                child.transform.parent = transform;
                child.transform.localPosition = Vector3.zero;
                meshTransform = child.transform;
            }

            meshTransform.name = "MeshTransform";
        }

        if(voxelGrid == null) {
            voxelGrid = new VoxelGrid(this, OnVoxelUpdated);
        }
    }

	private void Update() {
		rigidbody.isKinematic = isStatic;

		if(voxelGrid == null) {
			return;
		}

        if(Input.GetKeyDown(KeyCode.Space)) {
            for(int z = 0; z < voxelGrid.GetVoxelGridDimensions().z; z++) {
                for(int y = 0; y < voxelGrid.GetVoxelGridDimensions().y; y++) {
					voxelGrid.SetVoxelIsFilled(voxelGrid.GetVoxelGridDimensions().x / 2, y, z, isFilled: false);

					int index = VoxelGrid.CoordsToIndex(voxelGrid.GetVoxelGridDimensions().x / 2, y, z, voxelGrid.GetVoxelGridDimensions());
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
			Voxel voxel = voxelGrid.GetVoxel(i);

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
		Vector3Int coords = VoxelGrid.IndexToCoords(index, voxelGrid.GetVoxelGridDimensions());

		TrySetVoxelDirty(coords);
		TrySetVoxelDirty(coords + Vector3Int.right);
		TrySetVoxelDirty(coords + Vector3Int.left);
		TrySetVoxelDirty(coords + Vector3Int.up);
		TrySetVoxelDirty(coords + Vector3Int.down);
		TrySetVoxelDirty(coords + Vector3Int.forward);
		TrySetVoxelDirty(coords + Vector3Int.back);
	}

	private void TrySetVoxelDirty(Vector3Int coords) {
		Voxel v;
        if(!Voxel.TryGetVoxel(coords, voxelGrid, out v)) {
			return;
        }

		if(!v.IsFilled) {
			return;
		}

		int index = VoxelGrid.CoordsToIndex(coords, voxelGrid.GetVoxelGridDimensions());

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

		List<VoxelCluster> clusters = voxelGrid.FindVoxelClusters(startingPoints: dirtyVoxels);
		ApplyClustersToVoxelControllers(clusters, this);

		Debug.Assert(dirtyVoxels.Count == 0);
	}

	private static void ApplyClustersToVoxelControllers(List<VoxelCluster> clusters, VoxelController caller) { // TODO: very strange having a static method that just gets the instance as a parameter - like why even be static then? figure out something better.
		Debug.Assert(clusters.Count > 0);

        if(DEBUG) {
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
			voxelController.TrySetupDependencies();

			VoxelCluster cluster = clusters[i0];
			voxelController.ApplySettings(cluster.Voxels, cluster.Dimensions, cluster.Offset, ShouldClusterBeStatic(caller.isStatic, cluster.Offset));
		}

		VoxelCluster biggestCluster = clusters[biggestClusterIndex];
		caller.ApplySettings(biggestCluster.Voxels, biggestCluster.Dimensions, biggestCluster.Offset, ShouldClusterBeStatic(caller.isStatic, biggestCluster.Offset)); // TODO: currently all clusters will discard all meshobjects every time and rebuild from scratch - might be nice to have the biggest cluster use the old approach of only discarding what we don't need
	}

	private static bool ShouldClusterBeStatic(bool wasOriginallyStatic, Vector3Int offset) {
		return wasOriginallyStatic && offset.y == 0;
	}

	private void ApplySettings(Voxel[] voxels, Vector3Int dimensions, Vector3Int offset, bool isStatic, bool isOriginalSetup = false) {
		this.isStatic = isStatic;

		Vector3 pivot = GetPivot(voxels, dimensions, isStatic);

		Vector3 GetLocalPosWithWorldRotation(Vector3 localPos, Transform t) {
			return (t.TransformPoint(localPos) - t.position);
		}

        meshTransform.position += GetLocalPosWithWorldRotation(offset, meshTransform);
        meshTransform.parent = null;
        transform.position = meshTransform.position + GetLocalPosWithWorldRotation(pivot, meshTransform);
        meshTransform.parent = transform;

        voxelGrid.ApplySettings(voxels, dimensions, offset, isOriginalSetup);
    }

	private static Vector3 GetPivot(Voxel[] voxels, Vector3Int dimensions, bool isStatic) {
		Vector3 pivot = Vector3.zero;
		float divisor = 0f;

		for(int i = 0; i < voxels.Length; i++) {
			Voxel v = voxels[i];
			if(!v.IsFilled) {
				continue;
			}

			Vector3Int coords = VoxelGrid.IndexToCoords(v.Index, dimensions);

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

	public Vector3 GetVoxelLocalPos(int index) {
		return VoxelGrid.IndexToCoords(index, voxelGrid.GetVoxelGridDimensions());
	}

	public Vector3 GetVoxelWorldPos(int index) {
		return meshTransform.TransformPoint(GetVoxelLocalPos(index));
	}

	public Material GetMaterial() {
		return material;
	}

	public Color GetColor() {
		return color;
	}

	public Transform GetMeshTransform() {
		return meshTransform;
	}

	public static void RunTests() {
		TestGetPivot();
	}

	private static void TestGetPivot() {
		Vector3Int dimensions = new Vector3Int(8, 8, 8);
		Voxel[] cluster = new Voxel[dimensions.x * dimensions.y * dimensions.z];

		for(int z = 0; z < dimensions.z; z++) {
			for(int y = 0; y < dimensions.y; y++) {
				for(int x = 0; x < dimensions.x; x++) {
					int i = VoxelGrid.CoordsToIndex(x, y, z, dimensions);
					bool isFilled = x == 0 || y == 0 || z == 0 || x == dimensions.x - 1 || y == dimensions.y - 1 || z == dimensions.z - 1;
					cluster[i] = new Voxel(i, isFilled, false, false, false, false, false, false);
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
