using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(VoxelBuilder), typeof(Rigidbody))]
public partial class VoxelGrid : MonoBehaviour
{
    [SerializeField] private Transform meshTransform;
    [SerializeField, HideInInspector] private bool isOriginal = true; // TODO: this should be removed once we have a more permanent way of saving and loading buildings

    private VoxelBuilder voxelBuilder;
    private new Rigidbody rigidbody;

    private Vector3Int binGridDimensions;
    private Bin[] bins;
    private bool[] interiorMap;
    
    private const float UPDATE_LATENCY = 0.1f;
    private float timeToUpdate;
    private Queue<int> dirtyBins = new Queue<int>();

    private bool isStatic;


    private void Awake() {
        voxelBuilder = GetComponent<VoxelBuilder>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Start() {
        if(isOriginal) {
            Vector3Int voxelGridDimensions = new Vector3Int(16, 16, 16);
            int voxelCount = voxelGridDimensions.x * voxelGridDimensions.y * voxelGridDimensions.z;

            binGridDimensions = voxelGridDimensions / Bin.WIDTH;
            int binCount = binGridDimensions.x * binGridDimensions.y * binGridDimensions.z;

            bins = new Bin[binCount];
            interiorMap = new bool[binCount];

            int minX = Bin.WIDTH - 1;
            int minY = Bin.WIDTH - 1;
            int minZ = Bin.WIDTH - 1;

            int maxX = voxelGridDimensions.x - Bin.WIDTH;
            int maxY = voxelGridDimensions.y - Bin.WIDTH;
            int maxZ = voxelGridDimensions.z - Bin.WIDTH;

            for(int i = 0; i < voxelCount; i++) {
                Vector3Int voxelCoords = IndexToCoords(i, voxelGridDimensions);

                if(voxelCoords.x > minX && voxelCoords.y > minY && voxelCoords.z > minZ && voxelCoords.x < maxX && voxelCoords.y < maxY && voxelCoords.z < maxZ) {
                    continue;
                }

                Vector3Int binCoords = voxelCoords / Bin.WIDTH;
                int binIndex = CoordsToIndex(binCoords, binGridDimensions);

                Bin bin = bins[binIndex];
                if(bin == null) {
                    bins[binIndex] = new Bin(binIndex, binGridDimensions);
                    bin = bins[binIndex];
                }

                Vector3Int localVoxelCoords = voxelCoords - binCoords * Bin.WIDTH;
                int localVoxelIndex = CoordsToIndex(localVoxelCoords, Bin.WIDTH);

                bin.SetVoxelExists(localVoxelIndex, exists: true);
            }

            for(int i = 0; i < bins.Length; i++) {
                Bin bin = bins[i];

                interiorMap[i] = bin == null || bin.IsWholeBinEmpty();

                if(bin == null) {
                    continue;
                }

                bin.RefreshConnectivity(bins, binGridDimensions);
            }

            // this just ensures that the initial building will be in the same spot as it was placed in the editor - a bit ugly, but I haven't figured out anything better yet
            meshTransform.localPosition = new Vector3(-(voxelGridDimensions.x / 2f), 0.5f, -(voxelGridDimensions.z / 2f));

            ApplyCluster(new VoxelCluster(bins, interiorMap, Vector3Int.zero, binGridDimensions));
        }
    }

    public void ApplyCluster(VoxelCluster voxelCluster) {
        isStatic = isOriginal ? true : voxelCluster.ShouldBeStatic(isStatic);

        Vector3 GetLocalPosWithWorldRotation(Vector3 localPos, Transform t) {
            return (t.TransformPoint(localPos) - t.position);
        }
        
        Vector3 pivot = GetPivot(voxelCluster.Bins, voxelCluster.Dimensions, isStatic);

        meshTransform.position += GetLocalPosWithWorldRotation(voxelCluster.VoxelOffset, meshTransform);
        meshTransform.parent = null;
        transform.position = meshTransform.position + GetLocalPosWithWorldRotation(pivot, meshTransform);
        meshTransform.parent = transform;

        binGridDimensions = voxelCluster.Dimensions;
        bins = voxelCluster.Bins;
        interiorMap = voxelCluster.InteriorMap;

        voxelBuilder.Refresh();
    }

    private void LateUpdate() {
        rigidbody.isKinematic = isStatic;

        if(dirtyBins.Count == 0) {
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
        return CalculateVoxelGridDimensions(binGridDimensions);
    }

    public bool TryGetBin(Vector3Int coords, out Bin bin) {
        return TryGetBin(coords, bins, binGridDimensions, out bin);
    }

    public bool TryGetBin(int index, out Bin bin) {
        return TryGetBin(index, bins, out bin);
    }

    private bool GetVoxelExists(Vector3Int voxelCoords) {
        return GetVoxelExists(voxelCoords, bins, binGridDimensions);
    }

    public void SetVoxelExists(Vector3Int voxelCoords, bool exists) {
        int binIndex, localVoxelIndex;
        if(!GetBinAndVoxelIndex(voxelCoords, bins, binGridDimensions, out binIndex, out localVoxelIndex)) {
            return;
        }

        Bin bin;
        if(!TryGetBin(binIndex, bins, out bin)) {
            return;
        }

        bin.SetVoxelExists(localVoxelIndex, exists);
        
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.None,         bins, interiorMap, binGridDimensions, dirtyBins);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.Right,        bins, interiorMap, binGridDimensions, dirtyBins);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.Left,         bins, interiorMap, binGridDimensions, dirtyBins);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.Up,           bins, interiorMap, binGridDimensions, dirtyBins);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.Down,         bins, interiorMap, binGridDimensions, dirtyBins);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.Fore,         bins, interiorMap, binGridDimensions, dirtyBins);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.Back,         bins, interiorMap, binGridDimensions, dirtyBins);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.UpRight,      bins, interiorMap, binGridDimensions, dirtyBins);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.UpLeft,       bins, interiorMap, binGridDimensions, dirtyBins);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.UpFore,       bins, interiorMap, binGridDimensions, dirtyBins);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.UpBack,       bins, interiorMap, binGridDimensions, dirtyBins);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.DownRight,    bins, interiorMap, binGridDimensions, dirtyBins);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.DownLeft,     bins, interiorMap, binGridDimensions, dirtyBins);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.DownFore,     bins, interiorMap, binGridDimensions, dirtyBins);
        TryGetOrCreateVoxelAndSetDirty(voxelCoords, Direction.DownBack,     bins, interiorMap, binGridDimensions, dirtyBins);

        static void TryGetOrCreateVoxelAndSetDirty(Vector3Int voxelCoords, Direction direction, Bin[] bins, bool[] interiorMap, Vector3Int binGridDimensions, Queue<int> dirtyBins) {
            voxelCoords += Utils.GetDirectionVector(direction);

            int binIndex, localVoxelIndex;
            if(!GetBinAndVoxelIndex(voxelCoords, bins, binGridDimensions, out binIndex, out localVoxelIndex)) {
                return;
            }

            Bin neighborBin;
            if(!TryGetBin(binIndex, bins, out neighborBin)) {
                if(!interiorMap[binIndex]) {
                    return;
                }

                bins[binIndex] = new Bin(binIndex, binGridDimensions);
                neighborBin = bins[binIndex];

                neighborBin.SetAllVoxelExists(true);
            }

            bool wasBinAlreadyDirty = neighborBin.IsDirty();
            neighborBin.SetVoxelDirty(localVoxelIndex);

            if(wasBinAlreadyDirty) {
                return;
            }

            dirtyBins.Enqueue(binIndex);
        }
    }

    public bool IsBinInterior(int binIndex) {
        return interiorMap[binIndex];
    }

    public void UpdateDirtyBinsAndVoxels() {
        Queue<int> newlyCleanedBins = new Queue<int>();

        while(dirtyBins.Count > 0) {
            int binIndex = dirtyBins.Dequeue();

            Bin bin = bins[binIndex];
            bin.RefreshConnectivity(bins, binGridDimensions);
            bin.SetClean();

            newlyCleanedBins.Enqueue(binIndex);
        }

        VoxelClusterHandler.FindVoxelClustersAndSplit(this, newlyCleanedBins);
    }
}