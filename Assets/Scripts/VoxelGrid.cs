using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public partial class VoxelGrid : MonoBehaviour {
    private const float UPDATE_LATENCY = 0.1f;

    public enum UpdateState { Clean, Dirty, Processing }
    public UpdateState State { get; private set; }

    public Vector3Int StartDimensions;  // TODO: this should be removed once we have a more permanent way of saving and loading buildings
    [SerializeField] private bool debug;
    [SerializeField, HideInInspector] private bool isOriginal = true; // TODO: this should be removed once we have a more permanent way of saving and loading buildings
    [SerializeField] private bool isStatic;
    [SerializeField] private Octree<bool> voxelMap;

    private Transform meshTransform;
    private new Rigidbody rigidbody;
    private VoxelBuilder voxelBuilder;

    private float timeToUpdate;
    private Queue<Vector3Int> dirtyVoxels = new Queue<Vector3Int>();

    private void Awake() {
        rigidbody = GetComponent<Rigidbody>();

        for(int i = 0; i < transform.childCount; i++) {
            Transform t = transform.GetChild(i);
            if(t.tag == "MeshTransform") {
                meshTransform = t;
            }
        }

        if(meshTransform == null) {
            meshTransform = new GameObject("MeshTransform").transform;
            meshTransform.parent = transform;
            meshTransform.localPosition = Vector3.zero;
        }

        voxelBuilder = new VoxelBuilder(owner: this);
    }

    private void Start() {
        if(isOriginal) {
            voxelMap = new Octree<bool>(Vector3Int.zero, StartDimensions, startValue: true);

            // this just ensures that the initial building will be in the same spot as it was placed in the editor - a bit ugly, but I haven't figured out anything better yet
            meshTransform.localPosition = new Vector3(-(voxelMap.Dimensions.x / 2f), 0.5f, -(voxelMap.Dimensions.z / 2f));

            ApplyCluster(voxelMap);
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
                    voxelMap.TryGetValue(new Vector3Int(x, y, z), out bool value, DebugDrawNode);
                }
            }
        }
    }

    private void LateUpdate() {
        rigidbody.isKinematic = true;// isStatic;

        if(State == UpdateState.Processing) {
            return;
        }

        State = dirtyVoxels.Count > 0 ? UpdateState.Dirty : UpdateState.Clean; // TODO: separate UpdateState into it's own class (or interface) that handles itself

        if(State == UpdateState.Clean) {
            return;
        }

        if(Time.time < timeToUpdate) {
            return;
        }
        else {
            timeToUpdate = Time.time + UPDATE_LATENCY;
        }

        State = UpdateState.Processing;
        VoxelClusterHandler.FindVoxelClustersAndSplit(this, dirtyVoxels, onFinished: () => {
            State = UpdateState.Clean;
        });
    }

    public void ApplyCluster(Octree<bool> voxelCluster) {
        isStatic = voxelCluster.Offset.y == 0;
        
        Vector3 GetLocalPosWithWorldRotation(Vector3 localPos, Transform t) {
            return (t.TransformPoint(localPos) - t.position);
        }

        Vector3Int oldOffset = voxelMap.Offset;
        voxelMap = voxelCluster;

        meshTransform.position += GetLocalPosWithWorldRotation(voxelMap.Offset - oldOffset, meshTransform);
        Vector3 cachedMeshTransformPos = meshTransform.position;
        
        Vector3 pivot = GetPivot(voxelMap, isStatic);
        transform.position = meshTransform.position + GetLocalPosWithWorldRotation(pivot, meshTransform);
       
        meshTransform.position = cachedMeshTransformPos;

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

    public Vector3Int GetDimensions() {
        return voxelMap.Dimensions;
    }

    public int GetVoxelCount() { 
        return voxelMap.Dimensions.x * voxelMap.Dimensions.y * voxelMap.Dimensions.z;
    }

    public bool TryGetVoxel(Vector3Int voxelCoords, out bool value) {
        return voxelMap.TryGetValue(voxelCoords, out value);
    }

    public void TryRemoveVoxel(Vector3Int voxelCoords) {
        TryRemoveVoxel(voxelCoords, voxelMap, dirtyVoxels);
    }

    public static void TryRemoveVoxel(Vector3Int voxelCoords, Octree<bool> voxelMap, Queue<Vector3Int> dirtyVoxels) {
        if(!voxelMap.TryGetValue(voxelCoords, out bool doesVoxelExist) || !doesVoxelExist) {
            return;
        }

        voxelMap.SetValue(voxelCoords.x, voxelCoords.y, voxelCoords.z, false);

        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.None);
        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.Right);
        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.Left);
        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.Up);
        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.Down);
        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.Fore);
        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.Back);
        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.Up, Direction.Right);
        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.Up, Direction.Left);
        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.Up, Direction.Fore);
        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.Up, Direction.Back);
        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.Down, Direction.Right);
        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.Down, Direction.Left);
        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.Down, Direction.Fore);
        TrySetVoxelDirty(voxelCoords, voxelMap, dirtyVoxels, Direction.Down, Direction.Back);

        static void TrySetVoxelDirty(Vector3Int voxelCoords, Octree<bool> voxelMap, Queue<Vector3Int> dirtyVoxels, Direction direction, Direction additionalDirection = Direction.None) {
            voxelCoords += Utils.GetDirectionVector(direction);

            if(additionalDirection != Direction.None) {
                voxelCoords += Utils.GetDirectionVector(additionalDirection);
            }

            if(!voxelMap.TryGetValue(voxelCoords, out bool doesVoxelExist) || !doesVoxelExist) {
                return;
            }

            if(dirtyVoxels.Contains(voxelCoords)) {
                return;
            }

            dirtyVoxels.Enqueue(voxelCoords);
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

    private void DebugDrawNode(Vector3Int nodeOffset, int nodeSize, Vector3Int dimensions, bool value, float duration) {
        Random.seed = nodeOffset.x + dimensions.x * (nodeOffset.y + dimensions.y * nodeOffset.z);

        float halfSize = nodeSize * 0.5f;
        Vector3 drawPos = new Vector3(nodeOffset.x + halfSize, nodeOffset.y + halfSize, nodeOffset.z + halfSize);

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

        Debug.DrawLine(leftDownBack, leftUpBack, color, duration);
        Debug.DrawLine(leftUpBack, rightUpBack, color, duration);
        Debug.DrawLine(rightUpBack, rightDownBack, color, duration);
        Debug.DrawLine(rightDownBack, leftDownBack, color, duration);

        Debug.DrawLine(leftDownForward, leftUpForward, color, duration);
        Debug.DrawLine(leftUpForward, rightUpForward, color, duration);
        Debug.DrawLine(rightUpForward, rightDownForward, color, duration);
        Debug.DrawLine(rightDownForward, leftDownForward, color, duration);

        Debug.DrawLine(leftDownBack, leftDownForward, color, duration);
        Debug.DrawLine(leftDownForward, leftUpForward, color, duration);
        Debug.DrawLine(leftUpForward, leftUpBack, color, duration);
        Debug.DrawLine(leftUpBack, leftDownBack, color, duration);

        Debug.DrawLine(rightDownBack, rightDownForward, color, duration);
        Debug.DrawLine(rightDownForward, rightUpForward, color, duration);
        Debug.DrawLine(rightUpForward, rightUpBack, color, duration);
        Debug.DrawLine(rightUpBack, rightDownBack, color, duration);

        Debug.DrawLine(leftUpBack, leftUpForward, color, duration);
        Debug.DrawLine(leftUpForward, rightUpForward, color, duration);
        Debug.DrawLine(rightUpForward, rightUpBack, color, duration);
        Debug.DrawLine(rightUpBack, leftUpBack, color, duration);

        Debug.DrawLine(leftDownBack, leftDownForward, color, duration);
        Debug.DrawLine(leftDownForward, rightDownForward, color, duration);
        Debug.DrawLine(rightDownForward, rightDownBack, color, duration);
        Debug.DrawLine(rightDownBack, leftDownBack, color, duration);
    }

    public void GetVoxelNeighbors(Vector3Int voxelCoords, out bool hasNeighborRight, out bool hasNeighborLeft, out bool hasNeighborUp, out bool hasNeighborDown, out bool hasNeighborFore, out bool hasNeighborBack) {
        TryGetVoxel(new Vector3Int(voxelCoords.x + 1, voxelCoords.y, voxelCoords.z), out hasNeighborRight);
        TryGetVoxel(new Vector3Int(voxelCoords.x - 1, voxelCoords.y, voxelCoords.z), out hasNeighborLeft);
        TryGetVoxel(new Vector3Int(voxelCoords.x, voxelCoords.y + 1, voxelCoords.z), out hasNeighborUp);
        TryGetVoxel(new Vector3Int(voxelCoords.x, voxelCoords.y - 1, voxelCoords.z), out hasNeighborDown);
        TryGetVoxel(new Vector3Int(voxelCoords.x, voxelCoords.y, voxelCoords.z + 1), out hasNeighborFore);
        TryGetVoxel(new Vector3Int(voxelCoords.x, voxelCoords.y, voxelCoords.z - 1), out hasNeighborBack);
    }
}