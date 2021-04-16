using UnityEngine;

public readonly struct Voxel {
    public readonly int Index;
    public readonly bool IsFilled;
    public readonly bool HasNeighborRight;
    public readonly bool HasNeighborLeft;
    public readonly bool HasNeighborUp;
    public readonly bool HasNeighborDown;
    public readonly bool HasNeighborFore;
    public readonly bool HasNeighborBack;

    public Voxel(int index) {
        Index = index;

        IsFilled = false;
        HasNeighborRight = false;
        HasNeighborLeft = false;
        HasNeighborUp = false;
        HasNeighborDown = false;
        HasNeighborFore = false;
        HasNeighborBack = false;
    }

    public Voxel(int index, Voxel v) {
        Index = index;

        IsFilled = v.IsFilled;
        HasNeighborRight = v.HasNeighborRight;
        HasNeighborLeft = v.HasNeighborLeft;
        HasNeighborUp = v.HasNeighborUp;
        HasNeighborDown = v.HasNeighborDown;
        HasNeighborFore = v.HasNeighborFore;
        HasNeighborBack = v.HasNeighborBack;
    }

    public Voxel(int index, bool isFilled, bool hasNeighborRight, bool hasNeighborLeft, bool hasNeighborUp, bool hasNeighborDown, bool hasNeighborFore, bool hasNeighborBack) {
        Index = index;

        IsFilled = isFilled;
        HasNeighborRight = hasNeighborRight;
        HasNeighborLeft = hasNeighborLeft;
        HasNeighborUp = hasNeighborUp;
        HasNeighborDown = hasNeighborDown;
        HasNeighborFore = hasNeighborFore;
        HasNeighborBack = hasNeighborBack;
    }

    public static Voxel GetChangedVoxel(Voxel v, bool isFilled) {
        return new Voxel(v.Index, isFilled, v.HasNeighborRight, v.HasNeighborLeft, v.HasNeighborUp, v.HasNeighborDown, v.HasNeighborFore, v.HasNeighborBack);
    }

    public static Voxel GetChangedVoxel(Voxel v, bool hasNeighborRight, bool hasNeighborLeft, bool hasNeighborUp, bool hasNeighborDown, bool hasNeighborFore, bool hasNeighborBack) {
        return new Voxel(v.Index, v.IsFilled, hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack);
    }

    public static Voxel GetUpdatedHasNeighborValues(int index, Voxel[] grid, Vector3Int gridDimensions) {
        Vector3Int coords = VoxelGrid.IndexToCoords(index, gridDimensions);

        Voxel neighbor;
        bool hasNeighborRight = TryGetVoxel(coords.x + 1, coords.y, coords.z, grid, gridDimensions, out neighbor) && neighbor.IsFilled;
        bool hasNeighborLeft = TryGetVoxel(coords.x - 1, coords.y, coords.z, grid, gridDimensions, out neighbor) && neighbor.IsFilled;
        bool hasNeighborUp = TryGetVoxel(coords.x, coords.y + 1, coords.z, grid, gridDimensions, out neighbor) && neighbor.IsFilled;
        bool hasNeighborDown = TryGetVoxel(coords.x, coords.y - 1, coords.z, grid, gridDimensions, out neighbor) && neighbor.IsFilled;
        bool hasNeighborFore = TryGetVoxel(coords.x, coords.y, coords.z + 1, grid, gridDimensions, out neighbor) && neighbor.IsFilled;
        bool hasNeighborBack = TryGetVoxel(coords.x, coords.y, coords.z - 1, grid, gridDimensions, out neighbor) && neighbor.IsFilled;

        return GetChangedVoxel(grid[index], hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack);
    }

    public static bool TryGetVoxel(Vector3Int coords, VoxelGrid grid, out Voxel voxel) {
        return TryGetVoxel(coords.x, coords.y, coords.z, grid.GetVoxels(), grid.GetVoxelGridDimensions(), out voxel);
    }

    public static bool TryGetVoxel(int x, int y, int z, Voxel[] grid, Vector3Int dimensions, out Voxel voxel) {
        if(x < 0 || y < 0 || z < 0 || x >= dimensions.x || y >= dimensions.y || z >= dimensions.z) {
            voxel = new Voxel();
            return false;
        }

        voxel = grid[VoxelGrid.CoordsToIndex(x, y, z, dimensions)];
        return true;
    }
}
