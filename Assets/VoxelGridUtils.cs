using UnityEngine;
using System.Collections.Generic;

public partial class VoxelGrid
{
    public static bool TryGetBin(Vector3Int binCoords, Bin[] bins, Vector3Int binGridDimensions, out Bin bin) {
        int index = CoordsToIndex(binCoords, binGridDimensions);
        if(index == -1) {
            bin = null;
            return false;
        }

        return TryGetBin(index, bins, out bin);
    }

    public static bool TryGetBin(int index, Bin[] bins, out Bin bin) {
        bin = bins[index];
        return bin != null;
    }

    private static bool GetVoxelExists(Vector3Int voxelCoords, Bin[] bins, Vector3Int binGridDimensions) {
        int binIndex, localVoxelIndex;
        if(!GetBinAndVoxelIndex(voxelCoords, bins, binGridDimensions, out binIndex, out localVoxelIndex)) {
            return false;
        }

        Bin bin;
        if(!TryGetBin(binIndex, bins, out bin)) {
            return false;
        }

        return bin.GetVoxelExists(localVoxelIndex);
    }

    private static bool GetBinAndVoxelIndex(Vector3Int voxelCoords, Bin[] bins, Vector3Int binGridDimensions, out int binIndex, out int localVoxelIndex) {
        Vector3Int binCoords = voxelCoords / Bin.WIDTH;
        binIndex = CoordsToIndex(binCoords, binGridDimensions);

        if(binIndex == -1) {
            localVoxelIndex = -1;
            return false;
        }

        Vector3Int localVoxelCoords = voxelCoords - binCoords * Bin.WIDTH;
        localVoxelIndex = CoordsToIndex(localVoxelCoords, Bin.WIDTH);

        return localVoxelIndex != -1;
    }

    public static Vector3Int CalculateVoxelGridDimensions(Vector3Int dimensions) {
        return dimensions * Bin.WIDTH;
    }

    private static Vector3 GetPivot(Bin[] bins, Vector3Int binGridDimensions, bool isStatic) {
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

            if(bin.IsWholeBinEmpty()) {
                continue;
            }

            if(bin.IsWholeBinFilled()) {
                float binCenter = (Bin.WIDTH - 1) / 2f;
                TryAddToPivot(bin.Coords * Bin.WIDTH + new Vector3(binCenter, binCenter, binCenter), isStatic, ref pivot, ref divisor);
                continue;
            }

            for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {
                if(!bin.GetVoxelExists(localVoxelIndex)) {
                    continue;
                }

                TryAddToPivot(Bin.GetVoxelGlobalCoords(binIndex, localVoxelIndex, binGridDimensions), isStatic, ref pivot, ref divisor);
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

    public static bool AreCoordsWithinDimensions(Vector3Int coords, Vector3Int dimensions) {
        return AreCoordsWithinDimensions(coords.x, coords.y, coords.z, dimensions.x, dimensions.y, dimensions.z);
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
}
