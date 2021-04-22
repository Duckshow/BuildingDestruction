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

    #region Getter Functions
    public Vector3Int GetVoxelGridDimensions() { return voxelGridDimensions; }
    public static Vector3Int CalculateBinGridDimensions(Vector3Int voxelGridDimensions) {
        return new Vector3Int(
            Mathf.CeilToInt(voxelGridDimensions.x / (float)Bin.WIDTH),
            Mathf.CeilToInt(voxelGridDimensions.y / (float)Bin.WIDTH),
            Mathf.CeilToInt(voxelGridDimensions.z / (float)Bin.WIDTH)
        );
    }

    public bool TryGetVoxel(int x, int y, int z, out Voxel voxel) {
        if(x < 0 || y < 0 || z < 0 || x >= voxelGridDimensions.x || y >= voxelGridDimensions.y || z >= voxelGridDimensions.z) {
            voxel = new Voxel();
            return false;
        }

        voxel = voxels[CoordsToIndex(x, y, z, voxelGridDimensions)];
        return true;
    }

    public int GetVoxelCount() { return voxels.Length; }
    #endregion


    private void Awake() {
        voxelBuilder = GetComponent<VoxelBuilder>();
    }

    public void SubscribeToOnVoxelUpdate(VoxelEvent onVoxelUpdatedCallback) {
        onVoxelUpdated = onVoxelUpdatedCallback;
    }

    public void ApplySettings(Voxel[] voxels, Vector3Int dimensions, bool isOriginalSetup) {
        Debug.AssertFormat(voxels.Length == dimensions.x * dimensions.y * dimensions.z, "{0} != {1}*{2}*{3} ({4})", voxels.Length, dimensions.x, dimensions.y, dimensions.z, dimensions.x * dimensions.y * dimensions.z);

        voxelGridDimensions = dimensions;
        Vector3Int binGridDimensions = CalculateBinGridDimensions(voxelGridDimensions);
        
        this.voxels = voxels;

        int binCount = binGridDimensions.x * binGridDimensions.y * binGridDimensions.z;
        bins = new Bin[binCount];
        for(int i = 0; i < binCount; i++) {
            bins[i] = new Bin(i); // TODO: if we're storing any values in bins, we should migrate them here
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

    public bool TryFindVoxelCluster(int startVoxelIndex, bool[] visitedVoxels, out VoxelCluster cluster) {
        return TryFindVoxelCluster(startVoxelIndex, voxels, voxelGridDimensions, visitedVoxels, out cluster);
    }

    public static bool TryFindVoxelCluster(int startVoxelIndex, Voxel[] grid, Vector3Int gridDimensions, bool[] visitedVoxels, out VoxelCluster cluster) {
        cluster = null;

        if(visitedVoxels[startVoxelIndex]) {
            return false;
        }

        Voxel voxel = grid[startVoxelIndex];
        if(!voxel.IsFilled) {
            return false;
        }

        cluster = new VoxelCluster(voxel, grid, gridDimensions, visitedVoxels);
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

    #endregion
}