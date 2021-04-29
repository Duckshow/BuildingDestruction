using UnityEngine;
using System.Collections.Generic;

public partial class VoxelGrid
{
    public Transform GetMeshTransform() {
        return meshTransform;
    }

    public Bin GetBin(int index) {
        return bins[index];
    }

    public int GetBinCount() {
        return bins.Length;
    }

    public Vector3Int GetBinGridDimensions() {
        return binGridDimensions;
    }

    public static Vector3Int CalculateVoxelGridDimensions(Vector3Int dimensions) {
        return dimensions * Bin.WIDTH;
    }

    public static bool AreCoordsWithinDimensions(Vector3Int coords, Vector3Int dimensions) {
        return AreCoordsWithinDimensions(coords.x, coords.y, coords.z, dimensions);
    }

    public static bool AreCoordsWithinDimensions(int x, int y, int z, Vector3Int dimensions) {
        return AreCoordsWithinDimensions(x, y, z, dimensions.x, dimensions.y, dimensions.z);
    }

    public static bool AreCoordsWithinDimensions(int x, int y, int z, int widthX, int widthY, int widthZ) {
        if(x < 0 || y < 0 || z < 0 || x >= widthX || y >= widthY || z >= widthZ) {
            return false;
        }

        return true;
    }

    public static int CoordsToIndex(Vector3Int coords, Vector3Int dimensions) {
        return CoordsToIndex(coords.x, coords.y, coords.z, dimensions);
    }

    public static int CoordsToIndex(int x, int y, int z, Vector3Int dimensions) {
        return CoordsToIndex(x, y, z, dimensions.x, dimensions.y, dimensions.z);
    }

    public static int CoordsToIndex(Vector3Int coords, int width) {
        return CoordsToIndex(coords.x, coords.y, coords.z, width, width, width);
    }

    public static int CoordsToIndex(int x, int y, int z, int width) {
        return CoordsToIndex(x, y, z, width, width, width);
    }

    public static int CoordsToIndex(int x, int y, int z, int widthX, int widthY, int widthZ) {
        if(!AreCoordsWithinDimensions(x, y, z, widthX, widthY, widthZ)) {
            return -1;
        }

        return x + widthX * (y + widthY * z);
    }

    public static Vector3Int IndexToCoords(int i, Vector3Int dimensions) {
        return IndexToCoords(i, dimensions.x, dimensions.y, dimensions.z);
    }

    public static Vector3Int IndexToCoords(int i, int width) {
        return IndexToCoords(i, width, width, width);
    }

    private static Vector3Int IndexToCoords(int i, int widthX, int widthY, int widthZ) {
        if(i < 0 || i >= widthX * widthY * widthZ) {
            return -Vector3Int.one;
        }

        return new Vector3Int(i % widthX, (i / widthX) % widthY, i / (widthX * widthY));
    }

    public static Vector3 GetVoxelWorldPos(int index, Vector3Int binGridDimensions, Transform meshTransform) {
        Vector3 localPos = IndexToCoords(index, CalculateVoxelGridDimensions(binGridDimensions));
        Vector3 worldPos = meshTransform.TransformPoint(localPos);
        return worldPos;
    }

    public static Vector3Int GetDirectionVector(Direction dir) {
        switch(dir) {
            case Direction.None:    return Vector3Int.zero;
            case Direction.Right:   return Vector3Int.right;
            case Direction.Left:    return Vector3Int.left;
            case Direction.Up:      return Vector3Int.up;
            case Direction.Down:    return Vector3Int.down;
            case Direction.Fore:    return Vector3Int.forward;
            case Direction.Back:    return Vector3Int.back;
        }

        return Vector3Int.zero;
    }

    public static Direction GetOppositeDirection(Direction dir) {
        switch(dir) {
            case Direction.None:    return Direction.None;
            case Direction.Right:   return Direction.Left;
            case Direction.Left:    return Direction.Right;
            case Direction.Up:      return Direction.Down;
            case Direction.Down:    return Direction.Up;
            case Direction.Fore:    return Direction.Back;
            case Direction.Back:    return Direction.Fore;
        }

        return Direction.None;
    }

    private static NeighborRelationships GetVoxelNeighborStatuses(Vector3Int voxelCoords, Bin[] bins, Vector3Int binGridDimensions) {
        return new NeighborRelationships(
            right: DoesFilledVoxelExist(new Vector3Int(voxelCoords.x + 1, voxelCoords.y, voxelCoords.z), bins, binGridDimensions),
            left: DoesFilledVoxelExist(new Vector3Int(voxelCoords.x - 1, voxelCoords.y, voxelCoords.z), bins, binGridDimensions),
            up: DoesFilledVoxelExist(new Vector3Int(voxelCoords.x, voxelCoords.y + 1, voxelCoords.z), bins, binGridDimensions),
            down: DoesFilledVoxelExist(new Vector3Int(voxelCoords.x, voxelCoords.y - 1, voxelCoords.z), bins, binGridDimensions),
            fore: DoesFilledVoxelExist(new Vector3Int(voxelCoords.x, voxelCoords.y, voxelCoords.z + 1), bins, binGridDimensions),
            back: DoesFilledVoxelExist(new Vector3Int(voxelCoords.x, voxelCoords.y, voxelCoords.z - 1), bins, binGridDimensions)
        );
    }

    private static bool DoesFilledVoxelExist(Vector3Int voxelCoords, Bin[] bins, Vector3Int binGridDimensions) {
        VoxelAddress address;

        if(!TryGetVoxelAddress(voxelCoords, binGridDimensions, out address)) {
            return false;
        }

        if(bins[address.BinIndex] == null) {
            return false;
        }

        return bins[address.BinIndex].GetVoxel(address.LocalVoxelIndex).IsFilled;
    }

    private static bool TryGetVoxelAddress(Vector3Int voxelCoords, Vector3Int binGridDimensions, out VoxelAddress address) {
        Vector3Int voxelGridDimensions = CalculateVoxelGridDimensions(binGridDimensions);

        if(!AreCoordsWithinDimensions(voxelCoords, voxelGridDimensions)) {
            address = new VoxelAddress(-1, -1);
            return false;
        }

        int voxelIndex = CoordsToIndex(voxelCoords, voxelGridDimensions);
        address = VoxelIndexToVoxelAddress(voxelIndex, binGridDimensions);
        return true;
    }

    public static VoxelAddress VoxelCoordsToVoxelAddress(Vector3Int voxelCoords, Vector3Int binGridDimensions) {
        int voxelIndex = CoordsToIndex(voxelCoords, CalculateVoxelGridDimensions(binGridDimensions));
        return VoxelIndexToVoxelAddress(voxelIndex, binGridDimensions);
    }

    public static Vector3Int VoxelAddressToVoxelCoords(VoxelAddress address, Vector3Int binGridDimensions) {
        Vector3Int binCoords = IndexToCoords(address.BinIndex, binGridDimensions);
        Vector3Int localVoxelCoords = IndexToCoords(address.LocalVoxelIndex, Bin.WIDTH);
        return binCoords * Bin.WIDTH + localVoxelCoords;
    }

    private static VoxelAddress VoxelIndexToVoxelAddress(int voxelIndex, Vector3Int binGridDimensions) {
        Vector3Int voxelGridCoords = IndexToCoords(voxelIndex, CalculateVoxelGridDimensions(binGridDimensions));
        Vector3Int binGridCoords = voxelGridCoords / Bin.WIDTH;
        Vector3Int localVoxelCoords = voxelGridCoords - binGridCoords * Bin.WIDTH;

        int binIndex = CoordsToIndex(binGridCoords, binGridDimensions);
        int localVoxelIndex = CoordsToIndex(localVoxelCoords, Bin.WIDTH);

        return new VoxelAddress(binIndex, localVoxelIndex);
    }

    public static int VoxelAddressToVoxelIndex(VoxelAddress address, Vector3Int binGridDimensions) {
        Vector3Int voxelCoords = VoxelAddressToVoxelCoords(address, binGridDimensions);
        return CoordsToIndex(voxelCoords, CalculateVoxelGridDimensions(binGridDimensions));
    }

    public static VoxelAddress GetVoxelAddressNeighbor(VoxelAddress address, Vector3Int binGridDimensions, params Direction[] directions) {
        Vector3Int dir = new Vector3Int();

        for(int i = 0; i < directions.Length; i++) {
            dir += GetDirectionVector(directions[i]);
        }

        Vector3Int newBinCoords = IndexToCoords(address.BinIndex, binGridDimensions);
        Vector3Int newLocalVoxelCoords = IndexToCoords(address.LocalVoxelIndex, Bin.WIDTH) + dir;

        if(newLocalVoxelCoords.x == -1) {
            newLocalVoxelCoords.x = Bin.WIDTH - 1;
            newBinCoords.x--;
        }
        else if(newLocalVoxelCoords.x == Bin.WIDTH) {
            newLocalVoxelCoords.x = 0;
            newBinCoords.x++;
        }

        if(newLocalVoxelCoords.y == -1) {
            newLocalVoxelCoords.y = Bin.WIDTH - 1;
            newBinCoords.y--;
        }
        else if(newLocalVoxelCoords.y == Bin.WIDTH) {
            newLocalVoxelCoords.y = 0;
            newBinCoords.y++;
        }

        if(newLocalVoxelCoords.z == -1) {
            newLocalVoxelCoords.z = Bin.WIDTH - 1;
            newBinCoords.z--;
        }
        else if(newLocalVoxelCoords.z == Bin.WIDTH) {
            newLocalVoxelCoords.z = 0;
            newBinCoords.z++;
        }

        int newBinIndex;
        int newLocalVoxelIndex;

        if(AreCoordsWithinDimensions(newBinCoords, binGridDimensions)) {
            newBinIndex = CoordsToIndex(newBinCoords, binGridDimensions);
            newLocalVoxelIndex = CoordsToIndex(newLocalVoxelCoords, Bin.WIDTH);
        }
        else {
            newBinIndex = -1;
            newLocalVoxelIndex = -1;
        }

        return new VoxelAddress(newBinIndex, newLocalVoxelIndex);
    }

    public static int GetBiggestVoxelClusterIndex(List<VoxelCluster> clusters) {
        int biggestClusterIndex = -1;
        int biggestClusterSize = int.MinValue;
        for(int i = 0; i < clusters.Count; i++) {
            if(clusters[i].Bins.Length > biggestClusterSize) {
                biggestClusterSize = clusters[i].Bins.Length;
                biggestClusterIndex = i;
            }
        }

        Debug.Assert(biggestClusterIndex >= 0);
        Debug.Assert(biggestClusterIndex < clusters.Count);
        return biggestClusterIndex;
    }
}
