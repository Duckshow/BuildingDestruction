using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(VoxelBuilder), typeof(Rigidbody))]
public partial class VoxelGrid : MonoBehaviour
{
    [SerializeField] private Transform meshTransform;
    [SerializeField, HideInInspector] private bool isOriginal = true;

    private VoxelBuilder voxelBuilder;
    private new Rigidbody rigidbody;

    private Vector3Int binGridDimensions;
    private Bin[] bins;
    
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
            binGridDimensions = new Vector3Int(8, 8, 8);
            bins = new Bin[binGridDimensions.x * binGridDimensions.y * binGridDimensions.z];

            int index = 0;
            for(int z = 0; z < binGridDimensions.z; z++) {
                for(int y = 0; y < binGridDimensions.y; y++) {
                    for(int x = 0; x < binGridDimensions.x; x++) {
                        bins[index] = new Bin(index, binGridDimensions);
                        index++;
                    }
                }
            }

            // this just ensures that the initial building will be in the same spot as it was placed in the editor - a bit ugly, but I haven't figured out anything better yet
            Vector3Int voxelGridDimensions = VoxelGrid.CalculateVoxelGridDimensions(binGridDimensions);
            meshTransform.localPosition = new Vector3(-(voxelGridDimensions.x / 2f), 0.5f, -(voxelGridDimensions.z / 2f));

            ApplySettings(bins, binGridDimensions, offset: Vector3Int.zero, isStatic: true, isOriginalSetup: true);

            for(int i = 0; i < voxelGridDimensions.x * voxelGridDimensions.y * voxelGridDimensions.z; i++) {
                SetVoxelIsFilled(i, true);
            }
        }
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

    public void SetVoxelIsFilled(int voxelIndex, bool isFilled) {
        VoxelAddress address = VoxelIndexToVoxelAddress(voxelIndex, binGridDimensions);

        Bin bin = bins[address.BinIndex];
        if(bin == null) {
            return;
        }

        bin.SetVoxelIsFilled(address.LocalVoxelIndex, isFilled);

        TryMarkVoxelAsDirty(address);
        TryMarkVoxelAsDirty(GetVoxelAddressNeighbor(address, binGridDimensions, Direction.Right));
        TryMarkVoxelAsDirty(GetVoxelAddressNeighbor(address, binGridDimensions, Direction.Left));
        TryMarkVoxelAsDirty(GetVoxelAddressNeighbor(address, binGridDimensions, Direction.Up));
        TryMarkVoxelAsDirty(GetVoxelAddressNeighbor(address, binGridDimensions, Direction.Down));
        TryMarkVoxelAsDirty(GetVoxelAddressNeighbor(address, binGridDimensions, Direction.Fore));
        TryMarkVoxelAsDirty(GetVoxelAddressNeighbor(address, binGridDimensions, Direction.Back));

        TryMarkVoxelAsDirty(GetVoxelAddressNeighbor(address, binGridDimensions, Direction.Up, Direction.Right));
        TryMarkVoxelAsDirty(GetVoxelAddressNeighbor(address, binGridDimensions, Direction.Up, Direction.Left));
        TryMarkVoxelAsDirty(GetVoxelAddressNeighbor(address, binGridDimensions, Direction.Up, Direction.Fore));
        TryMarkVoxelAsDirty(GetVoxelAddressNeighbor(address, binGridDimensions, Direction.Up, Direction.Back));

        TryMarkVoxelAsDirty(GetVoxelAddressNeighbor(address, binGridDimensions, Direction.Down, Direction.Right));
        TryMarkVoxelAsDirty(GetVoxelAddressNeighbor(address, binGridDimensions, Direction.Down, Direction.Left));
        TryMarkVoxelAsDirty(GetVoxelAddressNeighbor(address, binGridDimensions, Direction.Down, Direction.Fore));
        TryMarkVoxelAsDirty(GetVoxelAddressNeighbor(address, binGridDimensions, Direction.Down, Direction.Back));
    }

    private void TryMarkVoxelAsDirty(VoxelAddress address) {
        if(address.BinIndex == -1) {
            return;
        }

        if(address.LocalVoxelIndex == -1) {
            return;
        }

        Bin bin = bins[address.BinIndex];
        if(bin == null) {
            return;
        }

        if(bin.TryMarkVoxelAsDirty(address.LocalVoxelIndex)) {
            dirtyBins.Enqueue(address.BinIndex);
        }
    }



    public void UpdateDirtyBinsAndVoxels() {
        Queue<int> updatedDirtyBins = new Queue<int>();

        while(dirtyBins.Count > 0) {
            int binIndex = dirtyBins.Dequeue();

            Bin bin = bins[binIndex];
            if(!bin.IsDirty) {
                continue;
            }

            bool[] areVoxelsDirty = bin.GetAreVoxelsDirty();
            for(int i = 0; i < Bin.SIZE; i++) {
                if(!areVoxelsDirty[i]) {
                    continue;
                }

                VoxelAddress address = new VoxelAddress(bin.Index, i);
                Vector3Int voxelCoords = VoxelAddressToVoxelCoords(address, binGridDimensions);
                bins[address.BinIndex].SetVoxelConnections(address.LocalVoxelIndex, GetVoxelNeighborStatuses(voxelCoords, bins, binGridDimensions));
            }

            bin.RefreshIsWholeBinFilled();
            bin.RefreshIsWholeBinEmpty();
            bin.ClearDirty();

            updatedDirtyBins.Enqueue(binIndex);
        }

        bool[] visitedBins = new bool[GetBinCount()];
        List<VoxelCluster> clusters = new List<VoxelCluster>();

        while(updatedDirtyBins.Count > 0) {
            int binIndex = updatedDirtyBins.Dequeue();

            VoxelCluster cluster;
            if(TryFindVoxelCluster(binIndex, bins, binGridDimensions, visitedBins, out cluster)) {
                clusters.Add(cluster);
            }
        }

        ApplyClustersToVoxelGrids(clusters, this);
    }

    private static bool TryFindVoxelCluster(int startBinIndex, Bin[] bins, Vector3Int binGridDimensions, bool[] visitedBins, out VoxelCluster cluster) {
        cluster = null;

        if(visitedBins[startBinIndex]) {
            return false;
        }

        if(bins[startBinIndex].IsWholeBinEmpty) {
            return false;
        }

        cluster = new VoxelCluster(startBinIndex, bins, binGridDimensions, visitedBins);
        return true;
    }

    private static void ApplyClustersToVoxelGrids(List<VoxelCluster> clusters, VoxelGrid caller) { // TODO: very strange having a static method that just gets the instance as a parameter - like why even be static then? figure out something better.
        Debug.Assert(clusters.Count > 0);

        if(clusters.Count > 1) {
            Debug.LogFormat("==========SPLIT: {0}==========", clusters.Count);
            for(int i = 0; i < clusters.Count; i++) {
                VoxelCluster cluster = clusters[i];
                Vector3Int binGridDimensions = cluster.Dimensions;
                Vector3Int voxelGridDimensions = CalculateVoxelGridDimensions(binGridDimensions);

                Debug.LogFormat("Cluster #{0}: Voxels: {1}, Bins: {2}, Offset: {3}", i, voxelGridDimensions, binGridDimensions, cluster.Offset);
            }
            Debug.LogFormat("==============================");
        }

        int biggestClusterIndex = GetBiggestVoxelClusterIndex(clusters);

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

            VoxelGrid voxelGrid = go.GetComponent<VoxelGrid>();
            voxelGrid.isOriginal = false;

            VoxelCluster cluster = clusters[i0];
            voxelGrid.ApplySettings(cluster.Bins, cluster.Dimensions, cluster.Offset, ShouldClusterBeStatic(caller.isStatic, cluster.Offset));
        }

        VoxelCluster biggestCluster = clusters[biggestClusterIndex];
        caller.ApplySettings(biggestCluster.Bins, biggestCluster.Dimensions, biggestCluster.Offset, ShouldClusterBeStatic(caller.isStatic, biggestCluster.Offset));
    }

    private static bool ShouldClusterBeStatic(bool wasOriginallyStatic, Vector3Int offset) {
        return wasOriginallyStatic && offset.y == 0;
    }

    private void ApplySettings(Bin[] bins, Vector3Int binGridDimensions, Vector3Int offset, bool isStatic, bool isOriginalSetup = false) {
        this.isStatic = isStatic;

        Vector3 pivot = GetPivot(bins, isStatic, binGridDimensions);

        Vector3 GetLocalPosWithWorldRotation(Vector3 localPos, Transform t) {
            return (t.TransformPoint(localPos) - t.position);
        }

        meshTransform.position += GetLocalPosWithWorldRotation(offset, meshTransform);
        meshTransform.parent = null;
        transform.position = meshTransform.position + GetLocalPosWithWorldRotation(pivot, meshTransform);
        meshTransform.parent = transform;

        this.binGridDimensions = binGridDimensions;
        this.bins = bins;

        if(!isOriginalSetup) {
            voxelBuilder.Refresh();
        }
    }

    private static Vector3 GetPivot(Bin[] bins, bool isStatic, Vector3Int dimensions) {
        Vector3 pivot = Vector3.zero;
        float divisor = 0f;

        static void TryAddToPivot(Vector3 coords, bool isStatic, ref Vector3 pivot, ref float divisor) {
            if(isStatic && coords.y > Bin.WIDTH - 1) {
                return;
            }

            pivot += coords;
            divisor++;
        }

        for(int binIndex = 0; binIndex < bins.Length; binIndex++) {
            Bin bin = bins[binIndex];

            if(bin == null) {
                continue;
            }

            if(bin.IsWholeBinEmpty) {
                continue;
            }

            if(bin.IsWholeBinFilled) {
                float binCenter = (Bin.WIDTH - 1) / 2f;
                TryAddToPivot(bin.Coords * Bin.WIDTH + new Vector3(binCenter, binCenter, binCenter), isStatic, ref pivot, ref divisor);
                continue;
            }

            for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {
                Voxel v = bin.GetVoxel(localVoxelIndex);

                if(!v.IsFilled) {
                    continue;
                }

                TryAddToPivot(v.GlobalCoords, isStatic, ref pivot, ref divisor);
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
}