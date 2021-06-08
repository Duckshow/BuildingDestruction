using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(VoxelBuilder), typeof(Rigidbody))]
public partial class VoxelGrid : MonoBehaviour
{
    public enum UpdateState { UpToDate, AwaitingUpdate }
    public UpdateState State { get; private set; }

    [SerializeField] private bool debug;
    [SerializeField] private Transform meshTransform;
    [SerializeField, HideInInspector] private bool isOriginal = true; // TODO: this should be removed once we have a more permanent way of saving and loading buildings

    private VoxelBuilder voxelBuilder;
    private new Rigidbody rigidbody;

    private Vector3Int binGridDimensions;
    private Bin[] bins;
    
    private const float UPDATE_LATENCY = 0.1f;
    private float timeToUpdate;
    private Queue<Vector3Int> dirtyVoxels = new Queue<Vector3Int>();

    private bool isStatic;

    private Callback onUpdated;


    private void Awake() {
        voxelBuilder = GetComponent<VoxelBuilder>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Start() {
        if(isOriginal) {
            Vector3Int voxelGridDimensions = new Vector3Int(32, 64, 32);
            int voxelCount = voxelGridDimensions.x * voxelGridDimensions.y * voxelGridDimensions.z;

            binGridDimensions = voxelGridDimensions / Bin.WIDTH;
            int binCount = binGridDimensions.x * binGridDimensions.y * binGridDimensions.z;

            bins = new Bin[binCount];
            for(int i = 0; i < binCount; i++) {
                bins[i] = new Bin(i, binGridDimensions);
            }

            int minX = Bin.WIDTH - 1;
            int minY = Bin.WIDTH - 1;
            int minZ = Bin.WIDTH - 1;

            int maxX = voxelGridDimensions.x - Bin.WIDTH;
            int maxY = voxelGridDimensions.y - Bin.WIDTH;
            int maxZ = voxelGridDimensions.z - Bin.WIDTH;

            for(int i = 0; i < voxelCount; i++) {
                Vector3Int voxelCoords = IndexToCoords(i, voxelGridDimensions);
                Vector3Int binCoords = voxelCoords / Bin.WIDTH;
                
                int binIndex = CoordsToIndex(binCoords, binGridDimensions);

                if(voxelCoords.x > minX && voxelCoords.y > minY && voxelCoords.z > minZ && voxelCoords.x < maxX && voxelCoords.y < maxY && voxelCoords.z < maxZ) {
                    Bin.SetBinIsExterior(bins, binIndex, isExterior: false);
                    continue;
                }

                Bin.SetBinIsExterior(bins, binIndex, isExterior: true);

                Vector3Int localVoxelCoords = voxelCoords - binCoords * Bin.WIDTH;
                int localVoxelIndex = CoordsToIndex(localVoxelCoords, Bin.WIDTH);

                Bin.SetBinVoxelExists(bins, binIndex, localVoxelIndex, exists: true);
            }

            for(int i = 0; i < bins.Length; i++) {
                Bin bin = bins[i];

                if(bin.IsWholeBinEmpty()) {
                    continue;
                }

                Bin.RefreshConnectivityInBin(bins, i, binGridDimensions);
            }

            // this just ensures that the initial building will be in the same spot as it was placed in the editor - a bit ugly, but I haven't figured out anything better yet
            meshTransform.localPosition = new Vector3(-(voxelGridDimensions.x / 2f), 0.5f, -(voxelGridDimensions.z / 2f));

            ApplyCluster(new VoxelCluster(bins, Vector3Int.zero, binGridDimensions));
        }
    }

    private void LateUpdate() {
        State = dirtyVoxels.Count > 0 ? UpdateState.AwaitingUpdate : UpdateState.UpToDate;
        rigidbody.isKinematic = isStatic;

        if(State == UpdateState.UpToDate) {
            return;
        }

        if(Time.time < timeToUpdate) {
            return;
        }
        else {
            timeToUpdate = Time.time + UPDATE_LATENCY;
        }

        UpdateDirtyVoxels();
    }

    public void SubscribeToOnUpdate(Callback subscriber) {
        onUpdated += subscriber;
    }

    public void UnsubscribeToOnUpdate(Callback subscriber) {
        onUpdated -= subscriber;
    }

    public void ApplyCluster(VoxelCluster voxelCluster) {
        isStatic = isOriginal ? true : voxelCluster.ShouldBeStatic(isStatic);

        Vector3 GetLocalPosWithWorldRotation(Vector3 localPos, Transform t) {
            return (t.TransformPoint(localPos) - t.position);
        }

        Vector3 pivot = GetPivot(voxelCluster.Bins, voxelCluster.Dimensions, isStatic);

        //meshTransform.position += GetLocalPosWithWorldRotation(voxelCluster.VoxelOffset, meshTransform);
        //meshTransform.parent = null;
        //transform.position = meshTransform.position + GetLocalPosWithWorldRotation(pivot, meshTransform);
        //meshTransform.parent = transform;

        meshTransform.position += GetLocalPosWithWorldRotation(voxelCluster.VoxelOffset, meshTransform);
        
        Vector3 cachedMeshTransformPos = meshTransform.position;
        transform.position = meshTransform.position + GetLocalPosWithWorldRotation(pivot, meshTransform);
        meshTransform.position = cachedMeshTransformPos;

        binGridDimensions = voxelCluster.Dimensions;
        bins = voxelCluster.Bins;

        voxelBuilder.Refresh();
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

    public Bin[] GetBins() {
        return bins;
    }

    public int GetBinCount() {
        return bins.Length;
    }

    public Vector3Int GetBinGridDimensions() {
        return binGridDimensions;
    }

    public Vector3Int GetVoxelGridDimensions() {
        return voxelGridDimensions;
    }

    public int GetVoxelCount() { 
        return voxelGridDimensions.x * voxelGridDimensions.y * voxelGridDimensions.z;
    }

    public bool TryGetVoxel(Vector3Int voxelCoords) {
        return voxelMap.TryGetValue(voxelCoords, out bool b, debugDrawCallback: null);
    }

    public void TrySetVoxelExists(Vector3Int voxelCoords, bool exists) {

        int binIndex, localVoxelIndex;
        if(!GetBinAndVoxelIndex(voxelCoords, bins, binGridDimensions, out binIndex, out localVoxelIndex)) {
            return;
        }

        voxelMap.SetValue(voxelCoords.x, voxelCoords.y, voxelCoords.z, false);

        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.None);
        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.Right);
        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.Left);
        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.Up);
        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.Down);
        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.Fore);
        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.Back);
        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.Up,   Direction.Right);
        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.Up,   Direction.Left);
        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.Up,   Direction.Fore);
        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.Up,   Direction.Back);
        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.Down, Direction.Right);
        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.Down, Direction.Left);
        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.Down, Direction.Fore);
        TrySetVoxelDirty(voxelCoords, voxelMap, voxelGridDimensions, dirtyVoxels, Direction.Down, Direction.Back);

        static void TrySetVoxelDirty(Vector3Int voxelCoords, Octree<bool> voxelMap, Vector3Int voxelGridDimensions, Queue<Vector3Int> dirtyVoxels, Direction direction, Direction additionalDirection = Direction.None) {
            voxelCoords += Utils.GetDirectionVector(direction);
            
            if(additionalDirection != Direction.None) {
                voxelCoords += Utils.GetDirectionVector(additionalDirection);
            }

            if(!voxelMap.TryGetValue(voxelCoords.x, voxelCoords.y, voxelCoords.z, out bool b, debugDrawCallback: null)) {
                return;
            }

            if(dirtyVoxels.Contains(voxelCoords)) {
                return;
            }

            dirtyVoxels.Enqueue(voxelCoords);
        }
    }

    public void UpdateDirtyVoxels() {
        VoxelClusterHandler.FindVoxelClustersAndSplit(this, dirtyVoxels);

        if(onUpdated != null) {
            onUpdated();
        }
    }

    public Vector3Int GetVoxelCoordsFromWorldPos(Vector3 worldPos) {
        Vector3 targetLocalPos = meshTransform.InverseTransformPoint(worldPos);
        
        return new Vector3Int(
            Mathf.FloorToInt(targetLocalPos.x + 0.5f), 
            Mathf.FloorToInt(targetLocalPos.y + 0.5f), 
            Mathf.FloorToInt(targetLocalPos.z + 0.5f)
        );
    }
}
