using UnityEngine;

//public readonly struct Voxel {
//    public readonly int Index;
//    public readonly Vector3Int Coords;
//    public readonly bool IsFilled;
//    public readonly bool HasNeighborRight;
//    public readonly bool HasNeighborLeft;
//    public readonly bool HasNeighborUp;
//    public readonly bool HasNeighborDown;
//    public readonly bool HasNeighborFore;
//    public readonly bool HasNeighborBack;

//    public Voxel(int index, Vector3Int coords) {
//        Index = index;
//        Coords = coords;

//        IsFilled = false;
//        HasNeighborRight = false;
//        HasNeighborLeft = false;
//        HasNeighborUp = false;
//        HasNeighborDown = false;
//        HasNeighborFore = false;
//        HasNeighborBack = false;
//    }

//    public Voxel(int index, Vector3Int coords, Voxel v) {
//        Index = index;
//        Coords = coords;

//        IsFilled = v.IsFilled;
//        HasNeighborRight = v.HasNeighborRight;
//        HasNeighborLeft = v.HasNeighborLeft;
//        HasNeighborUp = v.HasNeighborUp;
//        HasNeighborDown = v.HasNeighborDown;
//        HasNeighborFore = v.HasNeighborFore;
//        HasNeighborBack = v.HasNeighborBack;
//    }

//    public Voxel(int index, Vector3Int coords, bool isFilled, bool hasNeighborRight, bool hasNeighborLeft, bool hasNeighborUp, bool hasNeighborDown, bool hasNeighborFore, bool hasNeighborBack) {
//        Index = index;
//        Coords = coords;

//        IsFilled = isFilled;
//        HasNeighborRight = hasNeighborRight;
//        HasNeighborLeft = hasNeighborLeft;
//        HasNeighborUp = hasNeighborUp;
//        HasNeighborDown = hasNeighborDown;
//        HasNeighborFore = hasNeighborFore;
//        HasNeighborBack = hasNeighborBack;
//    }

//    public static Voxel GetChangedVoxel(Voxel v, bool isFilled) {
//        return new Voxel(v.Index, v.Coords, isFilled, v.HasNeighborRight, v.HasNeighborLeft, v.HasNeighborUp, v.HasNeighborDown, v.HasNeighborFore, v.HasNeighborBack);
//    }

//    public static Voxel GetChangedVoxel(Voxel v, bool hasNeighborRight, bool hasNeighborLeft, bool hasNeighborUp, bool hasNeighborDown, bool hasNeighborFore, bool hasNeighborBack) {
//        return new Voxel(v.Index, v.Coords, v.IsFilled, hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack);
//    }
//}
