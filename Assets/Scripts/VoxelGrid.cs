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

    private Vector3Int voxelGridDimensions;
    private Octree<bool> voxelMap;

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
            voxelGridDimensions = new Vector3Int(16, 32, 16);
            int voxelCount = voxelGridDimensions.x * voxelGridDimensions.y * voxelGridDimensions.z;

            voxelMap = new Octree<bool>(Mathf.Max(voxelGridDimensions.x, Mathf.Max(voxelGridDimensions.y, voxelGridDimensions.z)));

            for(int i = 0; i < voxelCount; i++) {
                Vector3Int voxelCoords = IndexToCoords(i, voxelGridDimensions);
                voxelMap.SetValue(voxelCoords.x, voxelCoords.y, voxelCoords.z, true);
            }

            // this just ensures that the initial building will be in the same spot as it was placed in the editor - a bit ugly, but I haven't figured out anything better yet
            meshTransform.localPosition = new Vector3(-(voxelGridDimensions.x / 2f), 0.5f, -(voxelGridDimensions.z / 2f));

            ApplyCluster(new VoxelCluster(voxelMap, Vector3Int.zero, voxelGridDimensions));
        }
    }

    float timeToUpdateDrawing;
    private void Update() {
        if(!debug) {
            return;
        }
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

        meshTransform.position += GetLocalPosWithWorldRotation(voxelCluster.VoxelOffset, meshTransform);
        Vector3 cachedMeshTransformPos = meshTransform.position;
        
        Vector3 pivot = GetPivot(voxelCluster.VoxelMap, voxelCluster.Dimensions, isStatic);
        transform.position = meshTransform.position + GetLocalPosWithWorldRotation(pivot, meshTransform);
       
        meshTransform.position = cachedMeshTransformPos;

        voxelGridDimensions = voxelCluster.Dimensions;
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

    public Octree<bool> GetVoxelMap() {
        return voxelMap;
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

    public void TryRemoveVoxel(Vector3Int voxelCoords) {
        if(!voxelMap.TryGetValue(voxelCoords.x, voxelCoords.y, voxelCoords.z, out bool b, debugDrawCallback: null)) {
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