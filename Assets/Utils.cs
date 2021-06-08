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
}
