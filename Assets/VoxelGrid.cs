using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(VoxelBuilder))]
public class VoxelGrid : MonoBehaviour
{
    private VoxelBuilder voxelBuilder;

    private Voxel[] voxels;
    private Bin[] bins;
    
    private Vector3Int voxelGridDimensions;

    public delegate void VoxelEvent(int index);
    private VoxelEvent onVoxelUpdated;

    private void Awake() {
        voxelBuilder = GetComponent<VoxelBuilder>();
    }

    public void SubscribeToOnVoxelUpdate(VoxelEvent onVoxelUpdatedCallback) {
        onVoxelUpdated = onVoxelUpdatedCallback;
    }

    public void ApplySettings(Voxel[] voxels, Bin[] bins, Vector3Int voxelGridDimensions, bool isOriginalSetup) {
        Debug.AssertFormat(voxels.Length == voxelGridDimensions.x * voxelGridDimensions.y * voxelGridDimensions.z, "{0} != {1}*{2}*{3} ({4})", voxels.Length, voxelGridDimensions.x, voxelGridDimensions.y, voxelGridDimensions.z, voxelGridDimensions.x * voxelGridDimensions.y * voxelGridDimensions.z);

        this.voxelGridDimensions = voxelGridDimensions;
        Vector3Int binGridDimensions = CalculateBinGridDimensions(this.voxelGridDimensions);
        
        this.voxels = voxels;

        if(bins != null) {
            this.bins = bins;
        }
        else {
            int binCount = binGridDimensions.x * binGridDimensions.y * binGridDimensions.z;
            bins = new Bin[binCount];
            for(int i = 0; i < binCount; i++) {
                bins[i] = new Bin(i, voxelGridDimensions);
            }
        }

        if(!isOriginalSetup) {
            voxelBuilder.Refresh();
        }
    }

    public void SetVoxelIsFilled(int index, bool isFilled) {
        voxels[index] = Voxel.GetChangedVoxel(voxels[index], isFilled);
        
        if(onVoxelUpdated != null) {
            onVoxelUpdated(index);
        }
    }

    public void RefreshVoxelHasNeighborValues(int index) {
        Vector3Int coords = IndexToCoords(index, voxelGridDimensions);

        Voxel neighbor;
        bool hasNeighborRight   = TryGetVoxel(coords.x + 1, coords.y, coords.z, out neighbor) && neighbor.IsFilled;
        bool hasNeighborLeft    = TryGetVoxel(coords.x - 1, coords.y, coords.z, out neighbor) && neighbor.IsFilled;
        bool hasNeighborUp      = TryGetVoxel(coords.x, coords.y + 1, coords.z, out neighbor) && neighbor.IsFilled;
        bool hasNeighborDown    = TryGetVoxel(coords.x, coords.y - 1, coords.z, out neighbor) && neighbor.IsFilled;
        bool hasNeighborFore    = TryGetVoxel(coords.x, coords.y, coords.z + 1, out neighbor) && neighbor.IsFilled;
        bool hasNeighborBack    = TryGetVoxel(coords.x, coords.y, coords.z - 1, out neighbor) && neighbor.IsFilled;

        voxels[index] = Voxel.GetChangedVoxel(voxels[index], hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack);
    }

    public void RefreshBinHasVoxelValues(int index) {
        bool hasVoxelRight = false;
        bool hasVoxelLeft = false;
        bool hasVoxelUp = false;
        bool hasVoxelDown = false;
        bool hasVoxelFore = false;
        bool hasVoxelBack = false;

        Bin bin = bins[index];
        
        for(int i = 0; i < Bin.SIZE; i++) {
            if(hasVoxelRight && hasVoxelLeft && hasVoxelUp && hasVoxelDown && hasVoxelFore && hasVoxelBack) {
                break;
            }

            if(bin.VoxelIndexes[i] == -1) {
                continue;
            }

            Voxel v;
            if(!TryGetVoxel(IndexToCoords(bin.VoxelIndexes[i], voxelGridDimensions), out v)) {
                continue;
            }

            if(!v.IsFilled) {
                continue;
            }

            if(!hasVoxelRight) {
                hasVoxelRight = i == 1 || i == 3 || i == 5 || i == 7;
            }
            if(!hasVoxelLeft) {
                hasVoxelLeft = i == 0 || i == 2 || i == 4 || i == 6;
            }
            if(!hasVoxelUp) {
                hasVoxelUp = i == 2 || i == 3 || i == 6 || i == 7;
            }
            if(!hasVoxelDown) {
                hasVoxelDown = i == 0 || i == 1 || i == 4 || i == 5;
            }
            if(!hasVoxelFore) {
                hasVoxelFore = i == 4 || i == 5 || i == 6 || i == 7;
            }
            if(!hasVoxelBack) {
                hasVoxelBack = i == 0 || i == 1 || i == 2 || i == 3;
            }
        }

        bins[index] = new Bin(
            bin, 
            hasVoxelRight, 
            hasVoxelLeft, 
            hasVoxelUp, 
            hasVoxelDown, 
            hasVoxelFore, 
            hasVoxelBack,
            bin.HasConnectionRight,
            bin.HasConnectionLeft,
            bin.HasConnectionUp,
            bin.HasConnectionDown,
            bin.HasConnectionFore,
            bin.HasConnectionBack
        );
    }

    public void RefreshBinHasConnectionValues(int index) {
        Vector3Int coords = IndexToCoords(index, CalculateBinGridDimensions(voxelGridDimensions));

        Bin bin = bins[index];
        Bin neighbor;
        
        bins[index] = new Bin(
            bin, 
            bin.HasVoxelRight, 
            bin.HasVoxelLeft, 
            bin.HasVoxelUp, 
            bin.HasVoxelDown, 
            bin.HasVoxelFore, 
            bin.HasVoxelBack,
            bin.HasVoxelRight   && TryGetBin(coords.x + 1,  coords.y,       coords.z,       out neighbor) && neighbor.HasVoxelLeft,
            bin.HasVoxelLeft    && TryGetBin(coords.x - 1,  coords.y,       coords.z,       out neighbor) && neighbor.HasVoxelRight,
            bin.HasVoxelUp      && TryGetBin(coords.x,      coords.y + 1,   coords.z,       out neighbor) && neighbor.HasVoxelDown,
            bin.HasVoxelDown    && TryGetBin(coords.x,      coords.y - 1,   coords.z,       out neighbor) && neighbor.HasVoxelUp,
            bin.HasVoxelFore    && TryGetBin(coords.x,      coords.y,       coords.z + 1,   out neighbor) && neighbor.HasVoxelBack,
            bin.HasVoxelBack    && TryGetBin(coords.x,      coords.y,       coords.z - 1,   out neighbor) && neighbor.HasVoxelFore
        );
    }

    public bool TryFindVoxelCluster(int startBinIndex, bool[] visitedBins, out VoxelCluster cluster) {
        return TryFindVoxelCluster(startBinIndex, bins, voxels, CalculateBinGridDimensions(voxelGridDimensions), voxelGridDimensions, visitedBins, out cluster);
    }

    private static bool TryFindVoxelCluster(int startBinIndex, Bin[] bins, Voxel[] voxels, Vector3Int binGridDimensions, Vector3Int voxelGridDimensions, bool[] visitedBins, out VoxelCluster cluster) {
        cluster = null;

        if(visitedBins[startBinIndex]) {
            return false;
        }

        Bin bin = bins[startBinIndex];
        if(!bin.HasVoxelRight && !bin.HasVoxelLeft && !bin.HasVoxelUp && !bin.HasVoxelDown && !bin.HasVoxelFore && !bin.HasVoxelBack) {
            return false;
        }

        cluster = new VoxelCluster(startBinIndex, bins, voxels, binGridDimensions, voxelGridDimensions, visitedBins);
        return true;
    }

    public static int GetBiggestVoxelClusterIndex(List<VoxelCluster> clusters) {
        int biggestClusterIndex = -1;
        int biggestClusterSize = int.MinValue;
        for(int i = 0; i < clusters.Count; i++) {
            if(clusters[i].Voxels.Length > biggestClusterSize) {
                biggestClusterSize = clusters[i].Voxels.Length;
                biggestClusterIndex = i;
            }
        }

        Debug.Assert(biggestClusterIndex >= 0);
        Debug.Assert(biggestClusterIndex < clusters.Count);
        return biggestClusterIndex;
    }

    public static bool AreCoordsWithinDimensions(Vector3Int coords, Vector3Int dimensions) {
        return AreCoordsWithinDimensions(coords.x, coords.y, coords.z, dimensions);
    }

    public static bool AreCoordsWithinDimensions(int x, int y, int z, Vector3Int dimensions) {
        if(x < 0 || y < 0 || z < 0 || x >= dimensions.x || y >= dimensions.y || z >= dimensions.z) {
            return false;
        }

        return true;
    }

    #region Getter Functions
    public Vector3Int GetVoxelGridDimensions() { return voxelGridDimensions; }
    public static Vector3Int CalculateBinGridDimensions(Vector3Int voxelGridDimensions) {
        return new Vector3Int(
            Mathf.CeilToInt(voxelGridDimensions.x / (float)Bin.WIDTH),
            Mathf.CeilToInt(voxelGridDimensions.y / (float)Bin.WIDTH),
            Mathf.CeilToInt(voxelGridDimensions.z / (float)Bin.WIDTH)
        );
    }

    public int GetVoxelCount() { 
        return voxels.Length; 
    }

    public bool TryGetVoxel(Vector3Int coords, out Voxel voxel) {
        return TryGet(coords.x, coords.y, coords.z, voxels, voxelGridDimensions, out voxel);
    }

    public bool TryGetVoxel(int x, int y, int z, out Voxel voxel) {
        return TryGet(x, y, z, voxels, voxelGridDimensions, out voxel);
    }

    public bool TryGetBin(Vector3Int coords, out Bin bin) {
        return TryGet(coords.x, coords.y, coords.z, bins, CalculateBinGridDimensions(voxelGridDimensions), out bin);
    }

    public bool TryGetBin(int x, int y, int z, out Bin bin) {
        return TryGet(x, y, z, bins, CalculateBinGridDimensions(voxelGridDimensions), out bin);
    }

    public static bool TryGet<T>(Vector3Int coords, T[] grid, Vector3Int dimensions, out T foundObj) where T : new() {
        return TryGet(coords.x, coords.y, coords.z, grid, dimensions, out foundObj);
    }

    public static bool TryGet<T>(int x, int y, int z, T[] grid, Vector3Int dimensions, out T foundObj) where T : new() {
        bool b = AreCoordsWithinDimensions(x, y, z, dimensions);
        foundObj = b ? grid[CoordsToIndex(x, y, z, dimensions)] : new T();
        return b;
    }

    public Voxel? TryGetVoxelUnsafe(int index) { // unsafe because unless you manually checked the coords before-hand this can wrap around a grid's dimensions and give you the wrong voxel
        if(index < 0 || index > voxels.Length) {
            return null;
        }

        return voxels[index];
    }

    public Voxel GetVoxelUnsafe(int index) {
        return voxels[index];
    }

    public Bin GetBinUnsafe(int index) {
        return bins[index];
    }

    #endregion

    #region Converter Functions

    public static int CoordsToIndex(Vector3Int coords, Vector3Int dimensions) {
        return CoordsToIndex(coords.x, coords.y, coords.z, dimensions);
    }

    public static int CoordsToIndex(int x, int y, int z, Vector3Int dimensions) {
        return x + dimensions.x * (y + dimensions.y * z);
    }

    public static Vector3Int IndexToCoords(int i, Vector3Int dimensions) {
        return new Vector3Int(i % dimensions.x, (i / dimensions.x) % dimensions.y, i / (dimensions.x * dimensions.y));
    }

    public static int VoxelToBinIndex(int voxelIndex, Vector3Int voxelGridDimensions) {
        Vector3Int voxelGridCoords = IndexToCoords(voxelIndex, voxelGridDimensions);
        Vector3Int binGridCoords = voxelGridCoords / Bin.WIDTH;

        return CoordsToIndex(binGridCoords, CalculateBinGridDimensions(voxelGridDimensions));
    }

    #endregion

    #region Test Functions
    public static void RunTests() {
        TestGetBiggestVoxelClusterIndex();
        TestCoordsToIndexAndIndexToCoords();
        TestVoxelToBinIndex();
        Debug.Log("Tests done.");
    }

    private static void TestGetBiggestVoxelClusterIndex() {
        int biggest = Random.Range(1000, 10000);
        
        List<VoxelCluster> list = new List<VoxelCluster>() {
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest))
        };

        int biggestIndex = Random.Range(0, list.Count);
        list.Insert(biggestIndex, new VoxelCluster(biggest));

        int result = GetBiggestVoxelClusterIndex(list);
        Debug.Assert(result == biggestIndex);
    }

    private static void TestCoordsToIndexAndIndexToCoords() {
        Vector3Int dimensions = new Vector3Int(8, 8, 8);

        for(int z = 0; z < dimensions.z; z++) {
            for(int y = 0; y < dimensions.y; y++) {
                for(int x = 0; x < dimensions.x; x++) {
                    Vector3Int coords = new Vector3Int(x, y, z);

                    int resultIndex = CoordsToIndex(coords, dimensions);
                    Vector3Int resultCoords = IndexToCoords(resultIndex, dimensions);

                    Debug.Assert(coords == resultCoords);
                }
            }
        }

        Vector3Int minCoords = new Vector3Int(0, 0, 0);
        Vector3Int maxCoords = dimensions - Vector3Int.one;

        int minIndex = 0;
        int maxIndex = (dimensions.x * dimensions.y * dimensions.z) - 1;

        int minResultIndex = CoordsToIndex(minCoords, dimensions);
        int maxResultIndex = CoordsToIndex(maxCoords, dimensions);
        Vector3Int minResultCoords = IndexToCoords(minIndex, dimensions);
        Vector3Int maxResultCoords = IndexToCoords(maxIndex, dimensions);

        Debug.Assert(minResultIndex == minIndex, string.Format("{0} != {1}", minResultIndex, minIndex));
        Debug.Assert(maxResultIndex == maxIndex, string.Format("{0} != {1}", maxResultIndex, maxIndex));
        Debug.Assert(minResultCoords == minCoords, string.Format("{0} != {1}", minResultCoords, minCoords));
        Debug.Assert(maxResultCoords == maxCoords, string.Format("{0} != {1}", maxResultCoords, maxCoords));
    }

    private static void TestVoxelToBinIndex() {
        Vector3Int voxelGridDimensions = new Vector3Int(4, 4, 4);

        int voxelCount = voxelGridDimensions.x * voxelGridDimensions.y * voxelGridDimensions.z;

        for(int i = 0; i < voxelCount; i++) {
            int binIndex = VoxelToBinIndex(i, voxelGridDimensions);
            Debug.LogFormat("VoxelToBinIndex Results: {0} -> {1}", i, binIndex);
        }
    }

        #endregion
}