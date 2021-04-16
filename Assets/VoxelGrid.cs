using UnityEngine;
using System.Collections.Generic;

public class VoxelGrid
{
    private VoxelController voxelController;
    private VoxelBuilder voxelBuilder;

    private Vector3Int voxelGridDimensions;

    private Voxel[] voxels;
    private Bin[] bins;

    public delegate void VoxelEvent(int index);
    public VoxelEvent OnVoxelUpdated;

    #region Getter Functions
    public Vector3Int GetVoxelGridDimensions() { return voxelGridDimensions; }
    public static Vector3Int CalculateBinGridDimensions(Vector3Int voxelGridDimensions) {
        return new Vector3Int(
            Mathf.CeilToInt(voxelGridDimensions.x / (float)Bin.WIDTH),
            Mathf.CeilToInt(voxelGridDimensions.y / (float)Bin.WIDTH),
            Mathf.CeilToInt(voxelGridDimensions.z / (float)Bin.WIDTH)
        );
    }

    public Voxel GetVoxel(Vector3Int coords) { return voxels[CoordsToIndex(coords, voxelGridDimensions)]; }
    public Voxel GetVoxel(int index) { return voxels[index]; }
    public Voxel[] GetVoxels() { return voxels; }
    public int GetVoxelCount() { return voxels.Length; }
    public VoxelController GetVoxelController() { return voxelController; }
    #endregion

    public VoxelGrid(VoxelController voxelController, VoxelEvent onVoxelUpdatedCallback) {
        this.voxelController = voxelController;
        voxelBuilder = new VoxelBuilder(this, voxelController.GetMaterial());

        OnVoxelUpdated = onVoxelUpdatedCallback;
    }

    public void ApplySettings(Voxel[] voxels, Vector3Int dimensions, Vector3Int offset, bool isOriginalSetup) {
        Debug.AssertFormat(voxels.Length == dimensions.x * dimensions.y * dimensions.z, "{0} != {1}*{2}*{3} ({4})", voxels.Length, dimensions.x, dimensions.y, dimensions.z, dimensions.x * dimensions.y * dimensions.z);

        voxelGridDimensions = dimensions;
        Vector3Int binGridDimensions = CalculateBinGridDimensions(voxelGridDimensions);
        
        this.voxels = voxels;

        int binCount = binGridDimensions.x * binGridDimensions.y * binGridDimensions.z;
        bins = new Bin[binCount];
        for(int i = 0; i < binCount; i++) {
            bins[i] = new Bin(i, voxelGridDimensions); // TODO: if we're storing any values in bins, we should migrate them here
        }

        if(!isOriginalSetup) {
            voxelBuilder.BuildBinGridMeshes(bins, offset);
        }
    }

    public void SetVoxelIsFilled(int x, int y, int z, bool isFilled) {
        int index = CoordsToIndex(x, y, z, voxelGridDimensions);
        SetVoxelIsFilled(index, isFilled);
    }

    public void SetVoxelIsFilled(int index, bool isFilled) {
        voxels[index] = Voxel.GetChangedVoxel(voxels[index], isFilled);
        
        if(OnVoxelUpdated != null) {
            OnVoxelUpdated(index);
        }
    }

    public List<VoxelCluster> FindVoxelClusters(Queue<int> startingPoints) {
        return FindVoxelClusters(startingPoints, voxels, voxelGridDimensions);
    }

    private static List<VoxelCluster> FindVoxelClusters(Queue<int> startingPoints, Voxel[] grid, Vector3Int gridDimensions) {
        List<VoxelCluster> clusters = new List<VoxelCluster>(); // TODO: maybe replace this with a queue, but low prio

        bool[] visitedVoxels = new bool[grid.Length]; // TODO: if I cached how many voxels have been visited, we could just break the loop when that number is reached, thus potentially saving a bunch of iterating

        while(startingPoints.Count > 0) {
            int index = startingPoints.Dequeue();

            if(visitedVoxels[index]) {
                continue;
            }

            Voxel voxel = grid[index];
            if(!voxel.IsFilled) {
                continue;
            }

            clusters.Add(new VoxelCluster(voxel, grid, gridDimensions, visitedVoxels));
        }

        return clusters;
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
        TestCoordsToIndexAndIndexToCoords();
        TestVoxelToBinIndex();
        TestGetBiggestVoxelClusterIndex();
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

    #endregion
}