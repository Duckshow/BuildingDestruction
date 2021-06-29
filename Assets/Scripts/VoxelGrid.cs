using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EditMode")] [RequireComponent(typeof(Rigidbody))]
public partial class VoxelGrid : MonoBehaviour {

    [SerializeField] private Vector3Int startDimensions;
    [SerializeField] private Transform meshTransform;
    [SerializeField, HideInInspector] private bool isOriginal = true; // TODO: this should be removed once we have a more permanent way of saving and loading buildings

    private VoxelBuilder voxelBuilder;
    private new Rigidbody rigidbody;

    private VoxelCluster voxels;
    
    private bool isStatic;

    private void Awake() {
        rigidbody = GetComponent<Rigidbody>();
        voxelBuilder = new VoxelBuilder(this);
    }

    private void Start() {
        if(isOriginal) {
            // this just ensures that the initial building will be in the same spot as it was placed in the editor - a bit ugly, but I haven't figured out anything better yet
            meshTransform.localPosition = new Vector3(-(startDimensions.x * Bin.WIDTH) / 2f, 0f, -(startDimensions.z * Bin.WIDTH) / 2f);

            ApplyCluster(new VoxelCluster(dimensions: startDimensions, voxelBlockStartValue: byte.MaxValue));
        }
    }

    private void LateUpdate() {
        rigidbody.isKinematic = isStatic;
    }

    private VoxelGrid[] CreateMoreVoxelGridsForNewClusters(int clusterCount) {
        Debug.Assert(clusterCount > 0);

        if(clusterCount == 1) {
            return null;
        }

        VoxelGrid[] newVoxelGrids = new VoxelGrid[clusterCount - 1];
        Transform[] originalMeshObjects = meshTransform.GetComponentsInChildren<Transform>(includeInactive: true);

        for(int i = 1; i < originalMeshObjects.Length; i++) {
            originalMeshObjects[i].parent = null;
        }

        for(int i = 1; i < clusterCount; i++) {
            GameObject go = Instantiate(gameObject, transform.parent);
            go.name = transform.name + " (Cluster)";

            VoxelGrid newVoxelGrid = go.GetComponent<VoxelGrid>();
            newVoxelGrid.MarkAsCopy();

            newVoxelGrids[i] = newVoxelGrid;
        }

        for(int i = 1; i < originalMeshObjects.Length; i++) {
            originalMeshObjects[i].parent = meshTransform;
        }

        return newVoxelGrids;
    }

    public void ApplyCluster(VoxelCluster voxelCluster) {
        isStatic = isOriginal ? true : voxelCluster.ShouldBeStatic(isStatic);

        Vector3 GetLocalPosWithWorldRotation(Vector3 localPos, Transform t) {
            return (t.TransformPoint(localPos) - t.position);
        }

        Vector3 newMeshTransformPos = meshTransform.position + GetLocalPosWithWorldRotation(voxelCluster.VoxelOffset, meshTransform);
        Vector3 pivot = GetPivot(voxelCluster, isStatic);

        transform.position = meshTransform.position + GetLocalPosWithWorldRotation(pivot, meshTransform);
        meshTransform.position = newMeshTransformPos;

        voxels = voxelCluster;
        voxels.SubscribeToOnFinishedUpdate(OnClusterFinishedUpdate);

        voxelBuilder.Refresh();
    }

    private void OnClusterFinishedUpdate(List<VoxelCluster> newClusters) {
        if(newClusters.Count == 1) {
            ApplyCluster(newClusters[0]);
            return;
        }

        VoxelGrid[] splitVoxelGrids = CreateMoreVoxelGridsForNewClusters(newClusters.Count);

        Debug.Assert(splitVoxelGrids.Length + 1 == newClusters.Count);

        int biggestClusterIndex = GetBiggestVoxelClusterIndex(newClusters);

        int voxelGridIndex = 0;
        for(int i = 0; i < newClusters.Count; i++) {
            if(i == biggestClusterIndex) {
                continue;
            }

            splitVoxelGrids[voxelGridIndex].ApplyCluster(newClusters[i]);
            voxelGridIndex++;
        }

        ApplyCluster(newClusters[biggestClusterIndex]);
    }

    public void MarkAsCopy() {
        isOriginal = false;
    }

    public bool IsStatic() {
        return isStatic;
    }

    public Transform GetMeshTransform() {
        return meshTransform;
    }

    internal static int GetBiggestVoxelClusterIndex(List<VoxelCluster> clusters) {
        int biggestClusterIndex = -1;
        int biggestClusterSize = int.MinValue;
        for(int i = 0; i < clusters.Count; i++) {
            if(clusters[i].GetVoxelBlockCount() > biggestClusterSize) {
                biggestClusterSize = clusters[i].GetVoxelBlockCount();
                biggestClusterIndex = i;
            }
        }

        Debug.Assert(biggestClusterIndex >= 0);
        Debug.Assert(biggestClusterIndex < clusters.Count);
        return biggestClusterIndex;
    }

    public Vector3Int GetVoxelCoordsFromWorldPos(Vector3 worldPos) {
        Vector3 targetLocalPos = meshTransform.InverseTransformPoint(worldPos);
        
        return new Vector3Int(
            Mathf.FloorToInt(targetLocalPos.x + 0.5f), 
            Mathf.FloorToInt(targetLocalPos.y + 0.5f), 
            Mathf.FloorToInt(targetLocalPos.z + 0.5f)
        );
    }

    internal static Vector3 GetPivot(VoxelCluster voxelCluster, bool isStatic) {
        Vector3 pivot = Vector3.zero;
        Vector3Int divisors = Vector3Int.zero;

        for(int voxelBlockIndex = 0; voxelBlockIndex < voxelCluster.GetVoxelBlockCount(); voxelBlockIndex++) {
            if(!voxelCluster.TryGetVoxelBlock(voxelBlockIndex, out Bin voxelBlock)) {
                continue;
            }

            if(voxelBlock.IsExterior && voxelBlock.IsWholeBinEmpty()) {
                continue;
            }

            if(voxelBlock.IsInterior || voxelBlock.IsWholeBinFilled()) {
                TryAddToPivot(voxelBlock.Coords * Bin.WIDTH + Vector3Int.one, isStatic, ref pivot, ref divisors);
                continue;
            }

            for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {
                if(!voxelBlock.GetVoxelExists(localVoxelIndex)) {
                    continue;
                }

                TryAddToPivot(Bin.GetVoxelGlobalCoords(voxelBlockIndex, localVoxelIndex, voxelCluster.Dimensions), isStatic, ref pivot, ref divisors);
            }
        }

        static void TryAddToPivot(Vector3 coords, bool isStatic, ref Vector3 pivot, ref Vector3Int divisors) {
            if(isStatic && coords.y > Bin.WIDTH - 1) {
                return;
            }

            if(coords.x > 0) { 
                pivot.x += coords.x;
                ++divisors.x;
            }
            if(coords.y > 0) { 
                pivot.y += coords.y;
                ++divisors.y;
            }
            if(coords.z > 0) { 
                pivot.z += coords.z;
                ++divisors.z;
            }
        }

        if(divisors.x > 0) {
            pivot.x /= divisors.x;
        }
        if(divisors.y > 0) {
            pivot.y /= divisors.y;
        }
        if(divisors.z > 0) {
            pivot.z /= divisors.z;
        }

        if(isStatic) {
            pivot.y = 0f;
        }

        return pivot;
    }

    public VoxelCluster GetVoxelCluster() {
        return voxels;
    }
}