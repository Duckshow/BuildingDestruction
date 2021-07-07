using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EditMode")] [RequireComponent(typeof(Rigidbody))]
public partial class VoxelGrid : MonoBehaviour {

    [SerializeField] private Vector3Int startDimensions;
    [SerializeField] private Transform meshTransform;
    [SerializeField, HideInInspector] private bool isFirstStart = true; // TODO: this should be removed once we have a more permanent way of saving and loading buildings

    private VoxelBuilder voxelBuilder;
    private new Rigidbody rigidbody;

    private VoxelCluster voxelCluster;
    
    private bool isStatic;

    private void Awake() {
        rigidbody = GetComponent<Rigidbody>();
        voxelBuilder = new VoxelBuilder(this);
    }

    private void Start() {
        if(isFirstStart) {
            isFirstStart = false;

            // this just ensures that the initial building will be in the same spot as it was placed in the editor - a bit ugly, but I haven't figured out anything better yet
            //meshTransform.localPosition = new Vector3(-(startDimensions.x * Bin.WIDTH) / 2f, 0f, -(startDimensions.z * Bin.WIDTH) / 2f);

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

        for(int i = 0; i < clusterCount - 1; i++) {
            GameObject go = Instantiate(gameObject, transform.parent);
            go.name = transform.name + " (Cluster)";
            
            newVoxelGrids[i] = go.GetComponent<VoxelGrid>();
        }

        for(int i = 1; i < originalMeshObjects.Length; i++) {
            originalMeshObjects[i].parent = meshTransform;
        }

        return newVoxelGrids;
    }

    public void ApplyCluster(VoxelCluster newVoxelCluster) {
        isStatic = isStatic && newVoxelCluster.VoxelOffset.y == 0;

        ApplyNewPivot(transform, meshTransform, newVoxelCluster, isStatic);

        voxelCluster = newVoxelCluster;
        voxelCluster.SetOwner(this);

        voxelBuilder.Refresh();
    }

    internal static void ApplyNewPivot(Transform pivotTransform, Transform meshTransform, VoxelCluster cluster, bool isStatic) {
        Vector3 meshTransformOffset = new Vector3(Utils.RoundDownToEven(cluster.VoxelOffset.x), Utils.RoundDownToEven(cluster.VoxelOffset.y), Utils.RoundDownToEven(cluster.VoxelOffset.z));

        Vector3 newMeshTransformPos = meshTransform.position + GetLocalPosWithWorldRotation(meshTransformOffset, meshTransform);
        Vector3 pivot = GetPivot(cluster, isStatic);

        pivotTransform.position = newMeshTransformPos + GetLocalPosWithWorldRotation(pivot, meshTransform);
        meshTransform.position = newMeshTransformPos;

        Vector3 GetLocalPosWithWorldRotation(Vector3 localPos, Transform t) {
            return (t.TransformPoint(localPos) - t.position);
        }
    }

    public void OnClusterFinishedUpdate(List<VoxelCluster> newClusters) {

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
            Mathf.FloorToInt(targetLocalPos.x), 
            Mathf.FloorToInt(targetLocalPos.y), 
            Mathf.FloorToInt(targetLocalPos.z)
        );
    }

    internal static Vector3 GetPivot(VoxelCluster voxelCluster, bool isStatic) {
        Vector3 pivot = Vector3.zero;
        float divisor = 0f;

        for(int voxelBlockIndex = 0; voxelBlockIndex < voxelCluster.GetVoxelBlockCount(); voxelBlockIndex++) {
            if(!voxelCluster.TryGetVoxelBlock(voxelBlockIndex, out Bin voxelBlock)) {
                continue;
            }

            if(voxelBlock.IsExterior && voxelBlock.IsWholeBinEmpty()) {
                continue;
            }

            if(voxelBlock.IsInterior || voxelBlock.IsWholeBinFilled()) {
                AddToPivot(voxelBlock.Coords * Bin.WIDTH + new Vector3(0.5f, 0.5f, 0.5f), ref pivot, ref divisor);
                continue;
            }

            for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {
                if(!voxelBlock.GetVoxelExists(localVoxelIndex)) {
                    continue;
                }

                Vector3 coords = Bin.GetVoxelGlobalCoords(voxelBlockIndex, localVoxelIndex, voxelCluster.Dimensions);
                if(isStatic && coords.y > 0) {
                    continue;
                }

                AddToPivot(coords, ref pivot, ref divisor);
            }
        }

        static void AddToPivot(Vector3 coords, ref Vector3 pivot, ref float divisor) {
            pivot = new Vector3(pivot.x + coords.x, pivot.y + coords.y, pivot.z + coords.z);
            ++divisor;
        }

        pivot /= divisor;
        pivot = new Vector3(pivot.x + 0.5f, pivot.y + 0.5f, pivot.z + 0.5f);

        if(isStatic) {
            pivot.y = 0f;
        }

        return pivot;
    }

    public VoxelCluster GetVoxelCluster() {
        return voxelCluster;
    }

    public VoxelBuilder GetVoxelBuilder() {
        return voxelBuilder;
    }
}