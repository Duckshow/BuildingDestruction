using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public enum Direction { //! WARNING: changing the integers could seriously mess things up, check all places boxing Direction before doing so
    None = -1,
    Right = 0,
    Left = 1,
    Up = 2,
    Down = 3,
    Fore = 4,
    Back = 5
}

public enum Axis {
    None,
    X, 
    Y, 
    Z 
}


public delegate void Callback();
public delegate void Callback<T>(T obj);

public static partial class Utils
{
#if UNITY_EDITOR
    public static bool IsAppRunningAsUnitTest() { 
        return AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.ToLowerInvariant().StartsWith("nunit.framework"));
    }
        
#endif

    public static bool GetValueFromByte(byte b, int index) {
        byte shiftedVariable = (byte)(b >> index);

        return (shiftedVariable & 1) == 1;
    }

    public static void SetValueInByte(ref byte b, int index, bool value) {
        byte shiftedValue = (byte)(1 << index);

        if(value) {
            b |= shiftedValue;
        }
        else {
            b &= (byte)~shiftedValue;
        }
    }

    public static bool GetValueFromULong(ulong u, int index) {
        ulong shiftedVariable = u >> index;
        
        return (shiftedVariable & 1) == 1;
    }

    public static void SetValueInULong(ref ulong u, int index, bool value) {
        ulong shiftedValue = (ulong)(1 << index);

        if(value) {
            u |= shiftedValue;
        }
        else {
            u &= ~shiftedValue;
        }
    }

    public static Vector3Int DirectionToVector(Direction dir) {
        switch(dir) {
            case Direction.None:        return Vector3Int.zero;
            case Direction.Right:       return Vector3Int.right;
            case Direction.Left:        return Vector3Int.left;
            case Direction.Up:          return Vector3Int.up;
            case Direction.Down:        return Vector3Int.down;
            case Direction.Fore:        return Vector3Int.forward;
            case Direction.Back:        return Vector3Int.back;
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

    public static bool AreDirectionsOpposite(Direction dir1, Direction dir2) {
        switch(dir1) {
            case Direction.None:    return dir2 == Direction.None;
            case Direction.Right:   return dir2 == Direction.Left;
            case Direction.Left:    return dir2 == Direction.Right;
            case Direction.Up:      return dir2 == Direction.Down;
            case Direction.Down:    return dir2 == Direction.Up;
            case Direction.Fore:    return dir2 == Direction.Back;
            case Direction.Back:    return dir2 == Direction.Fore;
        }

        return false;
    }

    public static Vector3Int GetRandomVector3Int(int min, int max) {
        return new Vector3Int(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
    }

    public static Vector3Int GetRandomVector3Int(Vector3Int min, Vector3Int max) {
        return new Vector3Int(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
    }

    public static int RoundUpToPOT(int value) {
        --value;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return ++value;
    }

    public static void DebugDrawVoxelCluster(Bin[] voxelBlocks, Vector3Int clusterOffset, Color color, float duration, Predicate<Bin> shouldDrawVoxelBlock) { 
        for(int i = 0; i < voxelBlocks.Length; i++) {
            Bin voxelBlock = voxelBlocks[i];
            if(!shouldDrawVoxelBlock(voxelBlock)) {
                continue;
            }

            DebugDrawVoxelBlock(voxelBlock, clusterOffset, color, duration);
        }
    }

    public static void DebugDrawVoxelBlock(Bin voxelBlock, Vector3Int offset, Color color, float duration) {
        void TryDrawVoxel(Vector3Int voxelBlockPos, Vector3Int localVoxelCoords) {
            if(!voxelBlock.GetVoxelExists(CoordsToIndex(localVoxelCoords, Bin.WIDTH))) {
                return;
            }

            DebugDrawCube(offset + voxelBlockPos + localVoxelCoords, 0.95f, new Color(color.r, color.g, color.b, 0.25f), duration);
        }

        DebugDrawCube(offset + voxelBlock.Coords * Bin.WIDTH, Bin.WIDTH * 0.95f, color, duration);

        TryDrawVoxel(voxelBlock.Coords * Bin.WIDTH, new Vector3Int(0, 0, 0));
        TryDrawVoxel(voxelBlock.Coords * Bin.WIDTH, new Vector3Int(1, 0, 0));
        TryDrawVoxel(voxelBlock.Coords * Bin.WIDTH, new Vector3Int(0, 1, 0));
        TryDrawVoxel(voxelBlock.Coords * Bin.WIDTH, new Vector3Int(1, 1, 0));
        TryDrawVoxel(voxelBlock.Coords * Bin.WIDTH, new Vector3Int(0, 0, 1));
        TryDrawVoxel(voxelBlock.Coords * Bin.WIDTH, new Vector3Int(1, 0, 1));
        TryDrawVoxel(voxelBlock.Coords * Bin.WIDTH, new Vector3Int(0, 1, 1));
        TryDrawVoxel(voxelBlock.Coords * Bin.WIDTH, new Vector3Int(1, 1, 1));
    }

    public static void DebugDrawCube(Vector3 pos, float size, Color color, float duration) {
        Vector3 corner_0 = pos + new Vector3(0, 0, 0) * size;
        Vector3 corner_1 = pos + new Vector3(1, 0, 0) * size;
        Vector3 corner_2 = pos + new Vector3(0, 1, 0) * size;
        Vector3 corner_3 = pos + new Vector3(1, 1, 0) * size;
        Vector3 corner_4 = pos + new Vector3(0, 0, 1) * size;
        Vector3 corner_5 = pos + new Vector3(1, 0, 1) * size;
        Vector3 corner_6 = pos + new Vector3(0, 1, 1) * size;
        Vector3 corner_7 = pos + new Vector3(1, 1, 1) * size;

        Debug.DrawLine(corner_0, corner_1, color, duration);
        Debug.DrawLine(corner_1, corner_5, color, duration);
        Debug.DrawLine(corner_5, corner_4, color, duration);
        Debug.DrawLine(corner_4, corner_0, color, duration);

        Debug.DrawLine(corner_2, corner_3, color, duration);
        Debug.DrawLine(corner_3, corner_7, color, duration);
        Debug.DrawLine(corner_7, corner_6, color, duration);
        Debug.DrawLine(corner_6, corner_2, color, duration);

        Debug.DrawLine(corner_0, corner_2, color, duration);
        Debug.DrawLine(corner_1, corner_3, color, duration);
        Debug.DrawLine(corner_4, corner_6, color, duration);
        Debug.DrawLine(corner_5, corner_7, color, duration);
    }

    public static void DebugDrawDiamond(Vector3 pos, float size, Color color, float duration) {
        Vector3 corner_0 = pos + Vector3.left * (size / 2f);
        Vector3 corner_1 = pos + Vector3.forward * (size / 2f);
        Vector3 corner_2 = pos + Vector3.right * (size / 2f);
        Vector3 corner_3 = pos + Vector3.back * (size / 2f);
        Vector3 corner_4 = pos + Vector3.down * (size / 2f);
        Vector3 corner_5 = pos + Vector3.up * (size / 2f);

        Debug.DrawLine(corner_0, corner_1, color, duration);
        Debug.DrawLine(corner_1, corner_2, color, duration);
        Debug.DrawLine(corner_2, corner_3, color, duration);
        Debug.DrawLine(corner_3, corner_0, color, duration);

        Debug.DrawLine(corner_4, corner_0, color, duration);
        Debug.DrawLine(corner_4, corner_1, color, duration);
        Debug.DrawLine(corner_4, corner_2, color, duration);
        Debug.DrawLine(corner_4, corner_3, color, duration);

        Debug.DrawLine(corner_5, corner_0, color, duration);
        Debug.DrawLine(corner_5, corner_1, color, duration);
        Debug.DrawLine(corner_5, corner_2, color, duration);
        Debug.DrawLine(corner_5, corner_3, color, duration);
    }

    public static int Max(this Vector3Int v) {
        return Mathf.Max(v.x, Mathf.Max(v.y, v.z));
    }

    public static int Product(this Vector3Int v) {
        return v.x * v.y * v.z;
    }

    public static int Get(this Vector3Int v, Axis axis) {
        switch(axis) {
            case Axis.X: { return v.x; }
            case Axis.Y: { return v.y; }
            case Axis.Z: { return v.z; }
        }

        throw new System.NotImplementedException();
    }

    public static Vector3Int Set(this Vector3Int v, Axis axis, int value) {
        switch(axis) {
            case Axis.X: { v.x = value; break; }
            case Axis.Y: { v.y = value; break; }
            case Axis.Z: { v.z = value; break; }
        }

        return v;
    }

    public static Axis DirectionToAxis(Direction direction) {
        switch(direction) {
            case Direction.None:  return Axis.None;
            case Direction.Right: return Axis.X;
            case Direction.Left:  return Axis.X;
            case Direction.Up:    return Axis.Y;
            case Direction.Down:  return Axis.Y;
            case Direction.Fore:  return Axis.Z;
            case Direction.Back:  return Axis.Z;
        }

        return Axis.None;
    }

    public static bool IsPositiveDirection(Direction direction) {
        switch(direction) {
            case Direction.Right: return true;
            case Direction.Left:  return false;
            case Direction.Up:    return true;
            case Direction.Down:  return false;
            case Direction.Fore:  return true;
            case Direction.Back:  return false;
        }

        return false;
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

    public static bool AreCoordsOnTheEdge(Vector3Int coords, Vector3Int dimensions) {
        return AreCoordsOnTheEdge(coords.x, coords.y, coords.z, dimensions.x, dimensions.y, dimensions.z);
    }

    public static bool AreCoordsOnTheEdge(int x, int y, int z, int widthX, int widthY, int widthZ) {
        return x == 0 || x == widthX - 1 || y == 0 || y == widthY -1 || z == 0 || z == widthZ - 1;
    }

    public static bool AreCoordsOnTwoEdges(int x, int y, int z, int minX, int minY, int minZ, int maxX, int maxY, int maxZ) {
        return  (x == minX || x == maxX) && (y == minY || y == maxY) ||
                (y == minY || y == maxY) && (z == minZ || z == maxZ) ||
                (z == minZ || z == maxZ) && (x == minX || x == maxX);
    }

    public static bool AreCoordsAlignedWithCenter(int x, int y, int z, int minX, int minY, int minZ, int maxX, int maxY, int maxZ) {
        int centerX = minX + (maxX - minX) / 2;
        int centerY = minY + (maxY - minY) / 2;
        int centerZ = minZ + (maxZ - minZ) / 2;
        return x == centerX && y == centerY || y == centerY && z == centerZ || z == centerZ && x == centerX;
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
        if(!AreCoordsWithinDimensions(x, y, z, widthX, widthY, widthZ)) { // TODO: quite unclear that this check is made, maybe convert to a Try-function?
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
        if(i < 0 || i >= widthX * widthY * widthZ) {  // TODO: quite unclear that this check is made, maybe convert to a Try-function?
            return -Vector3Int.one;
        }

        return new Vector3Int(i % widthX, (i / widthX) % widthY, i / (widthX * widthY));
    }

    internal static void GetVoxelBlockAndVoxelIndex(int voxelIndex, Vector3Int voxelBlockGridDimensions, out int binIndex, out int localVoxelIndex) {
        Vector3Int voxelCoords = IndexToCoords(voxelIndex, voxelBlockGridDimensions * Bin.WIDTH);
        GetVoxelBlockAndVoxelIndex(voxelCoords, voxelBlockGridDimensions, out binIndex, out localVoxelIndex);
    }

    internal static void GetVoxelBlockAndVoxelIndex(Vector3Int voxelCoords, Vector3Int voxelBlockGridDimensions, out int voxelBlockIndex, out int localVoxelIndex) {
        Vector3Int binCoords = voxelCoords / Bin.WIDTH;
        voxelBlockIndex = CoordsToIndex(binCoords, voxelBlockGridDimensions);

        if(voxelBlockIndex < 0) {
            throw new Exception();
        }

        Vector3Int localVoxelCoords = voxelCoords - binCoords * Bin.WIDTH;
        localVoxelIndex = CoordsToIndex(localVoxelCoords, Bin.WIDTH);
    }

    internal static int GetVoxelIndex(int voxelBlockIndex, int localVoxelIndex, Vector3Int voxelBlockGridDimensions) {
        return CoordsToIndex(GetVoxelCoords(voxelBlockIndex, localVoxelIndex, voxelBlockGridDimensions), voxelBlockGridDimensions * Bin.WIDTH);
    }

    internal static Vector3Int GetVoxelCoords(int voxelBlockIndex, int localVoxelIndex, Vector3Int voxelBlockGridDimensions) {
        Vector3Int voxelBlockCoords = IndexToCoords(voxelBlockIndex, voxelBlockGridDimensions);
        Vector3Int localVoxelCoords = IndexToCoords(localVoxelIndex, Bin.WIDTH);

        return voxelBlockCoords * Bin.WIDTH + localVoxelCoords;
    }

    public static int RoundUpToEven(int value) {
        return value % 2 == 0 ? value : value + 1;
    }

    public static int RoundDownToOdd(int value) {
        return value % 2 == 0 ? value - 1 : value;
    }

    public static Queue<T> ToQueue<T>(this T[] array) {
        Queue<T> q = new Queue<T>();

        for(int i = 0; i < array.Length; i++) {
            q.Enqueue(array[i]);
        }

        return q;
    }

    public static bool Contains<T>(this T[] array, T item) where T : IEquatable<T> {
        for(int i = 0; i < array.Length; i++) {
            if(array[i].Equals(item)) {
                return true;
            }
        }

        return false;
    }
}
