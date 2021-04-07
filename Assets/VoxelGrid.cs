using UnityEngine;

public class VoxelGrid
{
    public readonly struct Bin {
        public const int WIDTH = 2;

        public static int[] GetContentIndexes(int index, Vector3Int voxelGridDimensions) {
            int voxelIndex_0 = index * WIDTH;
            int voxelGridDimensions2D = voxelGridDimensions.x * voxelGridDimensions.y;

            return new int[] {
                voxelIndex_0 + 0,
                voxelIndex_0 + 1,
                voxelIndex_0 + voxelGridDimensions.x + 0,
                voxelIndex_0 + voxelGridDimensions.x + 1,
                voxelIndex_0 + voxelGridDimensions2D + 0,
                voxelIndex_0 + voxelGridDimensions2D + 1,
                voxelIndex_0 + voxelGridDimensions2D + voxelGridDimensions.x + 0,
                voxelIndex_0 + voxelGridDimensions2D + voxelGridDimensions.x + 1
            };
        }
    }

    private Vector3Int voxelGridDimensions;
    private Vector3Int binGridDimensions;

    private Voxel[] voxels;
    private Bin[] bins;

    public VoxelGrid(Vector3Int dimensions, Voxel[] preExistingVoxels = null) {
        voxelGridDimensions = dimensions;
        binGridDimensions = dimensions / Bin.WIDTH;

        int voxelCount = dimensions.x * dimensions.y * dimensions.z;
        int binCount = binGridDimensions.x * binGridDimensions.y * binGridDimensions.z;

        voxels = new Voxel[voxelCount];
        bins = new Bin[binCount];

        if(preExistingVoxels != null) {
            voxels = preExistingVoxels;
        }
        else {
            for(int i = 0; i < voxelCount; i++) {
                voxels[i] = new Voxel(i, true);
            }
        }

        for(int i = 0; i < binCount; i++) {
            bins[i] = new Bin();
        }
    }

    public void ModifyVoxel(int x, int y, int z, bool isFilled) {
        int index = CoordsToIndex(x, y, z, voxelGridDimensions);
        ModifyVoxel(index, isFilled);
    }

    public void ModifyVoxel(int index, bool isFilled) {
        voxels[index] = Voxel.GetChangedVoxel(voxels[index], isFilled);
    }

    public Voxel GetVoxel(int index) {
        return voxels[index];
    }

    public int GetVoxelCount() {
        return voxels.Length;
    }

    public Vector3Int GetDimensions() { 
        return voxelGridDimensions;
    }

    public static int CoordsToIndex(Vector3Int coords, Vector3Int dimensions) {
        return CoordsToIndex(coords.x, coords.y, coords.z, dimensions);
    }

    public static int CoordsToIndex(int x, int y, int z, Vector3Int dimensions) {
        return x + dimensions.x * (y + dimensions.y * z);
    }

    public static Vector3Int IndexToCoords(int i, Vector3Int dimensions) {
        return new Vector3Int(i % dimensions.x, (i / dimensions.x) % dimensions.y, i / (dimensions.x * dimensions.y));
    }

    public static int VoxelToBinIndex(int voxelIndex, int binSize) {
        return voxelIndex / binSize;
    }

    public bool TryGetVoxel(int x, int y, int z, out Voxel voxel) {
        if(x < 0 || y < 0 || z < 0 || x >= voxelGridDimensions.x || y >= voxelGridDimensions.y || z >= voxelGridDimensions.z) {
            voxel = new Voxel();
            return false;
        }

        voxel = voxels[CoordsToIndex(x, y, z, voxelGridDimensions)];
        return true;
    }
}

public readonly struct Voxel {
    public readonly int Index;
    public readonly bool IsFilled;
    public readonly Color Color;
    public readonly bool HasNeighborRight;
    public readonly bool HasNeighborLeft;
    public readonly bool HasNeighborUp;
    public readonly bool HasNeighborDown;
    public readonly bool HasNeighborFore;
    public readonly bool HasNeighborBack;

    public Voxel(int index) {
        Index = index;

        IsFilled = false;
        Color = Color.clear;
        HasNeighborRight = false;
        HasNeighborLeft = false;
        HasNeighborUp = false;
        HasNeighborDown = false;
        HasNeighborFore = false;
        HasNeighborBack = false;
    }

    public Voxel(int index, bool isFilled) {
        Index = index;
        IsFilled = isFilled;

        Color = Color.clear;
        HasNeighborRight = false;
        HasNeighborLeft = false;
        HasNeighborUp = false;
        HasNeighborDown = false;
        HasNeighborFore = false;
        HasNeighborBack = false;
    }

    public Voxel(int index, bool isFilled, Color color, bool hasNeighborRight, bool hasNeighborLeft, bool hasNeighborUp, bool hasNeighborDown, bool hasNeighborFore, bool hasNeighborBack) {
        Index = index;
        IsFilled = isFilled;
        Color = color;

        HasNeighborRight = hasNeighborRight;
        HasNeighborLeft = hasNeighborLeft;
        HasNeighborUp = hasNeighborUp;
        HasNeighborDown = hasNeighborDown;
        HasNeighborFore = hasNeighborFore;
        HasNeighborBack = hasNeighborBack;
    }

    public static Voxel GetChangedVoxel(Voxel v, int index) {
        return new Voxel(index, v.IsFilled, v.Color, v.HasNeighborRight, v.HasNeighborLeft, v.HasNeighborUp, v.HasNeighborDown, v.HasNeighborFore, v.HasNeighborBack);
    }

    public static Voxel GetChangedVoxel(Voxel v, bool isFilled) {
        return new Voxel(v.Index, isFilled, v.Color, v.HasNeighborRight, v.HasNeighborLeft, v.HasNeighborUp, v.HasNeighborDown, v.HasNeighborFore, v.HasNeighborBack);
    }

    public static Voxel GetChangedVoxel(Voxel v, Color color) {
        return new Voxel(v.Index, v.IsFilled, color, v.HasNeighborRight, v.HasNeighborLeft, v.HasNeighborUp, v.HasNeighborDown, v.HasNeighborFore, v.HasNeighborBack);
    }

    public static Voxel GetChangedVoxel(Voxel v, int index, Color color) {
        return new Voxel(index, v.IsFilled, color, v.HasNeighborRight, v.HasNeighborLeft, v.HasNeighborUp, v.HasNeighborDown, v.HasNeighborFore, v.HasNeighborBack);
    }

    public static Voxel GetChangedVoxel(Voxel v, bool hasNeighborRight, bool hasNeighborLeft, bool hasNeighborUp, bool hasNeighborDown, bool hasNeighborFore, bool hasNeighborBack) {
        return new Voxel(v.Index, v.IsFilled, v.Color, hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack);
    }
}
