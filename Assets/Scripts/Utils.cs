using UnityEngine;

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

public static partial class Utils
{
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

    public static Vector3Int GetDirectionVector(Direction dir) {
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

    public static void DebugDrawOctree<T>(this Octree<T> octree, Color color, Color emptyNodeColor, float duration) where T : System.IEquatable<T> {
        int nodeCount = octree.Dimensions.Sum();

        for(int i = 0; i < nodeCount; i++) {
            if(!octree.TryGetNode(VoxelGrid.IndexToCoords(i, octree.Dimensions), out Octree<T>.Node node)) {
                throw new System.Exception();
            }

            Vector3Int nodeOffset = node.GetOffset(octree.Offset);

            Vector3 drawPos = new Vector3(
                nodeOffset.x + node.Size / 2f,
                nodeOffset.y + node.Size / 2f,
                nodeOffset.z + node.Size / 2f
            );

            if(node.HasValue()) {
                DebugDrawCube(drawPos, node.Size * 0.95f, color, duration);
            }
            else if(emptyNodeColor.a > 0) {
                DebugDrawDiamond(drawPos, node.Size * 0.1f, emptyNodeColor, duration);
            }
        }
    }

    public static void DebugDrawCube(Vector3 pos, float size, Color color, float duration) {
        Vector3 corner_0 = pos + new Vector3(-1, -1, -1) * (size / 2f);
        Vector3 corner_1 = pos + new Vector3(-1, -1,  1) * (size / 2f);
        Vector3 corner_2 = pos + new Vector3( 1, -1,  1) * (size / 2f);
        Vector3 corner_3 = pos + new Vector3( 1, -1, -1) * (size / 2f);
        Vector3 corner_4 = pos + new Vector3(-1,  1, -1) * (size / 2f);
        Vector3 corner_5 = pos + new Vector3(-1,  1,  1) * (size / 2f);
        Vector3 corner_6 = pos + new Vector3( 1,  1,  1) * (size / 2f);
        Vector3 corner_7 = pos + new Vector3( 1,  1, -1) * (size / 2f);

        Debug.DrawLine(corner_0, corner_1, color, duration);
        Debug.DrawLine(corner_1, corner_2, color, duration);
        Debug.DrawLine(corner_2, corner_3, color, duration);
        Debug.DrawLine(corner_3, corner_0, color, duration);

        Debug.DrawLine(corner_4, corner_5, color, duration);
        Debug.DrawLine(corner_5, corner_6, color, duration);
        Debug.DrawLine(corner_6, corner_7, color, duration);
        Debug.DrawLine(corner_7, corner_4, color, duration);

        Debug.DrawLine(corner_0, corner_4, color, duration);
        Debug.DrawLine(corner_1, corner_5, color, duration);
        Debug.DrawLine(corner_2, corner_6, color, duration);
        Debug.DrawLine(corner_3, corner_7, color, duration);
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

    public static int Sum(this Vector3Int v) {
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

    public static void GetOtherAxes(Axis relativeForward, out Axis relativeHorizontal, out Axis relativeVertical) {
        relativeHorizontal = Axis.None;
        relativeVertical = Axis.None;
        
        switch(relativeForward) {
            case Axis.X: {
                relativeHorizontal = Axis.Z;
                relativeVertical = Axis.Y;
                break; 
            }
            case Axis.Y: {
                relativeHorizontal = Axis.X;
                relativeVertical = Axis.Z;
                break; 
            }
            case Axis.Z: {
                relativeHorizontal = Axis.X;
                relativeVertical = Axis.Y;
                break; 
            }
        }
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
}
