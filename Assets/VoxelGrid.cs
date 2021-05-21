using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(VoxelBuilder), typeof(Rigidbody))]
public partial class VoxelGrid : MonoBehaviour
{
    public enum UpdateState { UpToDate, AwaitingUpdate }
    public UpdateState State { get; private set; }

    [SerializeField] private Transform meshTransform;
    [SerializeField, HideInInspector] private bool isOriginal = true; // TODO: this should be removed once we have a more permanent way of saving and loading buildings

    private VoxelBuilder voxelBuilder;
    private new Rigidbody rigidbody;

    private Vector3Int binGridDimensions;
    private Bin[] bins;
    private Octree<bool> voxelMap;

    private const float UPDATE_LATENCY = 0.1f;
    private float timeToUpdate;
    private Queue<int> dirtyBins = new Queue<int>();

    private bool isStatic;

    private Callback onUpdated;


    private void Awake() {
        voxelBuilder = GetComponent<VoxelBuilder>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Start() {
        if(isOriginal) {
            Vector3Int voxelGridDimensions = new Vector3Int(4, 8, 4);
            int voxelCount = voxelGridDimensions.x * voxelGridDimensions.y * voxelGridDimensions.z;

            binGridDimensions = voxelGridDimensions / Bin.WIDTH;
            int binCount = binGridDimensions.x * binGridDimensions.y * binGridDimensions.z;

            bins = new Bin[binCount];
            for(int i = 0; i < binCount; i++) {
                bins[i] = new Bin(i, binGridDimensions);
            }

            voxelMap = new Octree<bool>(Mathf.Max(voxelGridDimensions.x, Mathf.Max(voxelGridDimensions.y, voxelGridDimensions.z)));

            int minX = Bin.WIDTH - 1;
            int minY = Bin.WIDTH - 1;
            int minZ = Bin.WIDTH - 1;

            int maxX = voxelGridDimensions.x - Bin.WIDTH;
            int maxY = voxelGridDimensions.y - Bin.WIDTH;
            int maxZ = voxelGridDimensions.z - Bin.WIDTH;

            for(int i = 0; i < voxelCount; i++) {
                Vector3Int voxelCoords = IndexToCoords(i, voxelGridDimensions);
                voxelMap.SetValue(voxelCoords.x, voxelCoords.y, voxelCoords.z, true);
                
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

            ApplyCluster(new VoxelCluster(bins, voxelMap, Vector3Int.zero, binGridDimensions));
        }
    }

    float timeToUpdateDrawing;
    private void Update() {
        if(timeToUpdateDrawing - Time.time > 0f) {
            return;
        }

        timeToUpdateDrawing = Time.time + UPDATE_LATENCY;
        DebugDrawVoxelMap();
    }

    private void DebugDrawVoxelMap() {
        for(int z = 0; z < voxelMap.Size; z++) {
            for(int y = 0; y < voxelMap.Size; y++) {
                for(int x = 0; x < voxelMap.Size; x++) {
                    voxelMap.TryGetValue(x, y, z, out bool value, DebugDrawNode);
                }
            }
        }
    }

    private void LateUpdate() {
        State = dirtyBins.Count > 0 ? UpdateState.AwaitingUpdate : UpdateState.UpToDate;
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

        UpdateDirtyBinsAndVoxels();
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

        meshTransform.position += GetLocalPosWithWorldRotation(voxelCluster.VoxelOffset, meshTransform);
        Vector3 cachedMeshTransformPos = meshTransform.position;
        
        Vector3 pivot = GetPivot(voxelCluster.Bins, voxelCluster.Dimensions, isStatic);
        transform.position = meshTransform.position + GetLocalPosWithWorldRotation(pivot, meshTransform);
       
        meshTransform.position = cachedMeshTransformPos;

        binGridDimensions = voxelCluster.Dimensions;
        bins = voxelCluster.Bins;
        voxelMap = voxelCluster.VoxelMap;

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

    public Octree<bool> GetVoxelMap() {
        return voxelMap;
    }

    public Vector3Int GetBinGridDimensions() {
        return binGridDimensions;
    }

    public Vector3Int GetVoxelGridDimensions() {
        return CalculateVoxelGridDimensions(binGridDimensions);
    }

    public bool TryGetBin(Vector3Int coords, out Bin bin) {
        return TryGetBin(coords, bins, binGridDimensions, out bin);
    }

    public Bin GetBin(int index) {
        return bins[index];
    }

    public bool GetVoxelExists(Vector3Int voxelCoords) {
        return GetVoxelExists(voxelCoords, bins, binGridDimensions);
    }

    public void TrySetVoxelExists(Vector3Int voxelCoords, bool exists) {
        voxelMap.SetValue(voxelCoords.x, voxelCoords.y, voxelCoords.z, exists);

        int binIndex, localVoxelIndex;
        if(!GetBinAndVoxelIndex(voxelCoords, bins, binGridDimensions, out binIndex, out localVoxelIndex)) {
            return;
        }

        if(!GetVoxelExists(voxelCoords)) {
            return;
        }

        Bin.SetBinVoxelExists(bins, binIndex, localVoxelIndex, exists);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.None);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.Right);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.Left);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.Up);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.Down);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.Fore);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.Back);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.Up,   Direction.Right);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.Up,   Direction.Left);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.Up,   Direction.Fore);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.Up,   Direction.Back);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.Down, Direction.Right);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.Down, Direction.Left);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.Down, Direction.Fore);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, bins, binGridDimensions, dirtyBins, Direction.Down, Direction.Back);

        static void TryGetOrCreateVoxelAndSetDirty(Vector3Int voxelCoords, Bin[] bins, Vector3Int binGridDimensions, Queue<int> dirtyBins, Direction direction, Direction additionalDirection = Direction.None) {
            voxelCoords += Utils.GetDirectionVector(direction);
            
            if(additionalDirection != Direction.None) {
                voxelCoords += Utils.GetDirectionVector(additionalDirection);
            }

            int binIndex, localVoxelIndex;
            if(!GetBinAndVoxelIndex(voxelCoords, bins, binGridDimensions, out binIndex, out localVoxelIndex)) {
                return;
            }

            Bin neighborBin = bins[binIndex];
            if(neighborBin.IsWholeBinEmpty() && neighborBin.IsExterior) {
                return;
            }

            if(neighborBin.IsWholeBinEmpty()) {
                if(neighborBin.IsExterior) {
                    return;
                }

                bins[binIndex] = new Bin(binIndex, binGridDimensions);
                neighborBin = bins[binIndex];

                Bin.SetBinAllVoxelsExists(bins, binIndex, exists: true);
            }

            bool wasBinAlreadyDirty = neighborBin.IsDirty();
            Bin.SetBinVoxelDirty(bins, binIndex, localVoxelIndex);

            if(wasBinAlreadyDirty) {
                return;
            }

            dirtyBins.Enqueue(binIndex);
        }
    }

    public void UpdateDirtyBinsAndVoxels() {
        Queue<int> newlyCleanedBins = new Queue<int>();

        while(dirtyBins.Count > 0) {
            int binIndex = dirtyBins.Dequeue();

            Bin.RefreshConnectivityInBin(bins, binIndex, binGridDimensions);
            Bin.SetBinClean(bins, binIndex);

            newlyCleanedBins.Enqueue(binIndex);
        }

        VoxelClusterHandler.FindVoxelClustersAndSplit(this, newlyCleanedBins);

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

    private void DebugDrawNode(int nodeOffsetX, int nodeOffsetY, int nodeOffsetZ, int nodeSize, int gridSize, bool value) {
        Random.seed = nodeOffsetX + gridSize * (nodeOffsetY + gridSize * nodeOffsetZ);

        float halfSize = nodeSize * 0.5f;
        Vector3 drawPos = new Vector3(nodeOffsetX + halfSize, nodeOffsetY + halfSize, nodeOffsetZ + halfSize);

        float s = halfSize * 0.99f;

        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;
        Vector3 up = Vector3.up;
        Vector3 down = Vector3.down;
        Vector3 fore = Vector3.forward;
        Vector3 back = Vector3.back;

        Vector3 bottomLeftWorldPos = new Vector3(-0.5f, -0.5f, -0.5f);
        Vector3 leftDownBack =      meshTransform.TransformPoint(bottomLeftWorldPos + drawPos + (left + down + back)  * s);
        Vector3 rightDownBack =     meshTransform.TransformPoint(bottomLeftWorldPos + drawPos + (right + down + back) * s);
        Vector3 leftUpBack =        meshTransform.TransformPoint(bottomLeftWorldPos + drawPos + (left + up + back)    * s);
        Vector3 rightUpBack =       meshTransform.TransformPoint(bottomLeftWorldPos + drawPos + (right + up + back)   * s);
        Vector3 leftDownForward =   meshTransform.TransformPoint(bottomLeftWorldPos + drawPos + (left + down + fore)  * s);
        Vector3 rightDownForward =  meshTransform.TransformPoint(bottomLeftWorldPos + drawPos + (right + down + fore) * s);
        Vector3 leftUpForward =     meshTransform.TransformPoint(bottomLeftWorldPos + drawPos + (left + up + fore)    * s);
        Vector3 rightUpForward =    meshTransform.TransformPoint(bottomLeftWorldPos + drawPos + (right + up + fore)   * s);

        Color color = value ? Color.red : Color.clear;

        Debug.DrawLine(leftDownBack,        leftUpBack,         color, UPDATE_LATENCY);
        Debug.DrawLine(leftUpBack,          rightUpBack,        color, UPDATE_LATENCY);
        Debug.DrawLine(rightUpBack,         rightDownBack,      color, UPDATE_LATENCY);
        Debug.DrawLine(rightDownBack,       leftDownBack,       color, UPDATE_LATENCY);

        Debug.DrawLine(leftDownForward,     leftUpForward,      color, UPDATE_LATENCY);
        Debug.DrawLine(leftUpForward,       rightUpForward,     color, UPDATE_LATENCY);
        Debug.DrawLine(rightUpForward,      rightDownForward,   color, UPDATE_LATENCY);
        Debug.DrawLine(rightDownForward,    leftDownForward,    color, UPDATE_LATENCY);

        Debug.DrawLine(leftDownBack,        leftDownForward,    color, UPDATE_LATENCY);
        Debug.DrawLine(leftDownForward,     leftUpForward,      color, UPDATE_LATENCY);
        Debug.DrawLine(leftUpForward,       leftUpBack,         color, UPDATE_LATENCY);
        Debug.DrawLine(leftUpBack,          leftDownBack,       color, UPDATE_LATENCY);

        Debug.DrawLine(rightDownBack,       rightDownForward,   color, UPDATE_LATENCY);
        Debug.DrawLine(rightDownForward,    rightUpForward,     color, UPDATE_LATENCY);
        Debug.DrawLine(rightUpForward,      rightUpBack,        color, UPDATE_LATENCY);
        Debug.DrawLine(rightUpBack,         rightDownBack,      color, UPDATE_LATENCY);

        Debug.DrawLine(leftUpBack,          leftUpForward,      color, UPDATE_LATENCY);
        Debug.DrawLine(leftUpForward,       rightUpForward,     color, UPDATE_LATENCY);
        Debug.DrawLine(rightUpForward,      rightUpBack,        color, UPDATE_LATENCY);
        Debug.DrawLine(rightUpBack,         leftUpBack,         color, UPDATE_LATENCY);

        Debug.DrawLine(leftDownBack,        leftDownForward,    color, UPDATE_LATENCY);
        Debug.DrawLine(leftDownForward,     rightDownForward,   color, UPDATE_LATENCY);
        Debug.DrawLine(rightDownForward,    rightDownBack,      color, UPDATE_LATENCY);
        Debug.DrawLine(rightDownBack,       leftDownBack,       color, UPDATE_LATENCY);
    }
}