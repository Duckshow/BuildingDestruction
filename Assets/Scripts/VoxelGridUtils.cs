using UnityEngine;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EditMode")]
public partial class VoxelGrid {
    internal static Vector3 GetPivot(Octree<bool> voxelMap, bool isStatic) {
        Debug.Assert(voxelMap.Offset.x >= 0);
        Debug.Assert(voxelMap.Offset.y >= 0);
        Debug.Assert(voxelMap.Offset.z >= 0);

        Debug.Assert(voxelMap.Dimensions.x >= 0);
        Debug.Assert(voxelMap.Dimensions.y >= 0);
        Debug.Assert(voxelMap.Dimensions.z >= 0);

        Vector3 pivot = Vector3.zero;
        float divisor = 0f;

        for(int z = 0; z < voxelMap.Dimensions.z; z++) {
            for(int y = 0; y < voxelMap.Dimensions.y; y++) {
                for(int x = 0; x < voxelMap.Dimensions.x; x++) {
                    if(isStatic && y > 0) {
                        continue;
                    }

                    if(!voxelMap.TryGetValue(new Vector3Int(x, y, z), out bool doesVoxelExist) || !doesVoxelExist) {
                        continue;
                    }

                    pivot.x += x;
                    pivot.y += y;
                    pivot.z += z;
                    divisor++;
                }
            }
        }

        if(Mathf.Approximately(divisor, 0f)) {
            return Vector3.zero;
        }

        pivot /= divisor;

        //pivot.x += voxelMap.Offset.x;
        //pivot.y += voxelMap.Offset.y;
        //pivot.z += voxelMap.Offset.z;

        //pivot.x -= 0.5f;
        //pivot.y -= 0.5f;
        //pivot.z -= 0.5f;

        if(isStatic) {
            pivot.y = voxelMap.Offset.y - 0.5f;
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
