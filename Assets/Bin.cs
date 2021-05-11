using UnityEngine;
using System;

public partial class Bin {
    public const int WIDTH = 2; //! WARNING: WIDTH is hardcoded in many places, changing this will require a lot of work!
    public const int SIZE = 8; // must be WIDTH ^ 3

    private const byte VOXELS_PER_FACE = WIDTH * WIDTH;

    private static readonly Vector3Int[] LOCAL_COORDS_LOOKUP = new Vector3Int[] {
        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 1, 1)
    };

    private const int INDEX_LOOKUP_0_0_0 = 0;
    private const int INDEX_LOOKUP_1_0_0 = 1;
    private const int INDEX_LOOKUP_0_1_0 = 2;
    private const int INDEX_LOOKUP_1_1_0 = 3;
    private const int INDEX_LOOKUP_0_0_1 = 4;
    private const int INDEX_LOOKUP_1_0_1 = 5;
    private const int INDEX_LOOKUP_0_1_1 = 6;
    private const int INDEX_LOOKUP_1_1_1 = 7;

    public int Index { get; private set; }
    public Vector3Int Coords { get; private set; }

    private byte voxels;
    private byte areVoxelsDirty;
    private byte voxelNeighborsRightLeft;
    private byte voxelNeighborsUpDown;
    private byte voxelNeighborsForeBack;

    public Bin(int index, Vector3Int binGridDimensions) {
        Index = index;
        Coords = VoxelGrid.IndexToCoords(index, binGridDimensions);
    }

    public Bin(int index, Vector3Int binGridDimensions, Bin bin) {
        Index = index;
        Coords = VoxelGrid.IndexToCoords(index, binGridDimensions);

        voxels = bin.voxels;
        areVoxelsDirty = bin.areVoxelsDirty;

        voxelNeighborsRightLeft = bin.voxelNeighborsRightLeft;
        voxelNeighborsUpDown = bin.voxelNeighborsUpDown;
        voxelNeighborsForeBack = bin.voxelNeighborsForeBack;
    }

    public bool HasOpenPathBetweenFaces(Direction face1, Direction face2) {
        if(Utils.AreDirectionsOpposite(face1, face2)) {
            for(int i = 0; i < VOXELS_PER_FACE; i++) {
                int localVoxelIndex1 = FaceVoxelIndexToLocalVoxelIndex(i, face1);
                int localVoxelIndex2 = FaceVoxelIndexToLocalVoxelIndex(i, face2);

                if(!Utils.GetValueFromByte(voxels, localVoxelIndex1) && !Utils.GetValueFromByte(voxels, localVoxelIndex2)) {
                    return true;
                }
            }

            return false;
        }

        byte targetVoxels1 = GetTargetVoxelsForFace(face1);
        byte targetVoxels2 = GetTargetVoxelsForFace(face2);
        byte GetTargetVoxelsForFace(Direction face) {
            byte targetVoxels = 0;

            Utils.SetValueInByte(ref targetVoxels, FaceVoxelIndexToLocalVoxelIndex(0, face), true);
            Utils.SetValueInByte(ref targetVoxels, FaceVoxelIndexToLocalVoxelIndex(1, face), true);
            Utils.SetValueInByte(ref targetVoxels, FaceVoxelIndexToLocalVoxelIndex(2, face), true);
            Utils.SetValueInByte(ref targetVoxels, FaceVoxelIndexToLocalVoxelIndex(3, face), true);

            return targetVoxels;
        }

        return (~voxels & targetVoxels1 & targetVoxels2) > 0;
    }

    //public bool HasVoxelOnFace(Direction face) {
    //    for(int i = 0; i < VOXELS_PER_FACE; i++) {
    //        int localVoxelIndex = FaceVoxelIndexToLocalVoxelIndex(i, face);
    //        bool voxelExists = GetVoxelExists(localVoxelIndex);

    //        if(lookForEmptyVoxel && !voxelExists) {
    //            return true;
    //        }
    //        if(!lookForEmptyVoxel && voxelExists) {
    //            return true;
    //        }
    //    }

    //    return false;
    //}

    public bool IsConnectedToNeighbor(Bin neighbor, Direction direction) {
        Direction oppositeDirection = Utils.GetOppositeDirection(direction);

        byte cachedNeighbors0 = GetCachedVoxelNeighbors(direction);
        byte cachedNeighbors1 = neighbor.GetCachedVoxelNeighbors(oppositeDirection);

        return (cachedNeighbors0 & cachedNeighbors1) > 0;
    }

    public bool IsWholeBinFilled() {
        return voxels == byte.MaxValue;
    }
    
    public bool IsWholeBinEmpty() {
        return voxels == byte.MinValue;
    }

    public bool IsWalledIn() {
        return voxelNeighborsRightLeft == byte.MaxValue && voxelNeighborsUpDown == byte.MaxValue && voxelNeighborsForeBack == byte.MaxValue;
    }

    public bool IsDirty() {
        return areVoxelsDirty > 0;
    }

    public void SetClean() {
        areVoxelsDirty = 0;
    }

    public static Vector3 GetVoxelWorldPos(int binIndex, int localVoxelIndex, Vector3Int binGridDimensions, Transform meshTransform) {
        return meshTransform.TransformPoint(GetVoxelGlobalCoords(binIndex, localVoxelIndex, binGridDimensions));
    }

    public static Vector3Int GetVoxelGlobalCoords(int binIndex, int localVoxelIndex, Vector3Int binGridDimensions) {
        return VoxelGrid.IndexToCoords(binIndex, binGridDimensions) * WIDTH + GetVoxelLocalCoords(localVoxelIndex);
    }

    public static Vector3Int GetVoxelLocalCoords(int localVoxelIndex) {
        return LOCAL_COORDS_LOOKUP[localVoxelIndex];
    }

    public bool GetVoxelExists(int localVoxelIndex) {
        return Utils.GetValueFromByte(voxels, localVoxelIndex);
    }

    public void SetVoxelExists(int localVoxelIndex, bool exists) {
        Utils.SetValueInByte(ref voxels, localVoxelIndex, exists);
    }

    public void SetAllVoxelExists(bool exists) {
        voxels = exists ? byte.MaxValue : byte.MinValue;
    }

    public void SetVoxelDirty(int localVoxelIndex) {
        Utils.SetValueInByte(ref areVoxelsDirty, localVoxelIndex, true);
    }

    public void RefreshConnectivity(Bin[] bins, Vector3Int binGridDimensions) {
        Bin binRight, binLeft, binUp, binDown, binFore, binBack;
        VoxelGrid.TryGetBin(Coords + Vector3Int.right,      bins, binGridDimensions, out binRight);
        VoxelGrid.TryGetBin(Coords + Vector3Int.left,       bins, binGridDimensions, out binLeft);
        VoxelGrid.TryGetBin(Coords + Vector3Int.up,         bins, binGridDimensions, out binUp);
        VoxelGrid.TryGetBin(Coords + Vector3Int.down,       bins, binGridDimensions, out binDown);
        VoxelGrid.TryGetBin(Coords + Vector3Int.forward,    bins, binGridDimensions, out binFore);
        VoxelGrid.TryGetBin(Coords + Vector3Int.back,       bins, binGridDimensions, out binBack);

        RefreshConnectivity(binRight, binLeft, binUp, binDown, binFore, binBack, out voxelNeighborsRightLeft, out voxelNeighborsUpDown, out voxelNeighborsForeBack);
    }

    public void RefreshConnectivity(Bin binRight, Bin binLeft, Bin binUp, Bin binDown, Bin binFore, Bin binBack) {
        RefreshConnectivity(binRight, binLeft, binUp, binDown, binFore, binBack, out voxelNeighborsRightLeft, out voxelNeighborsUpDown, out voxelNeighborsForeBack);
    }

    private static void RefreshConnectivity(Bin binRight, Bin binLeft, Bin binUp, Bin binDown, Bin binFore, Bin binBack, out byte voxelNeighborsRightLeft, out byte voxelNeighborsUpDown, out byte voxelNeighborsForeBack) {
        voxelNeighborsRightLeft = 0;
        voxelNeighborsUpDown = 0;
        voxelNeighborsForeBack = 0;

        TryAddVoxelNeighbors(ref voxelNeighborsRightLeft, binRight, Direction.Right, bitOffset: 0);
        TryAddVoxelNeighbors(ref voxelNeighborsRightLeft, binLeft, Direction.Left, bitOffset: VOXELS_PER_FACE);
        TryAddVoxelNeighbors(ref voxelNeighborsUpDown, binUp, Direction.Up, bitOffset: 0);
        TryAddVoxelNeighbors(ref voxelNeighborsUpDown, binDown, Direction.Down, bitOffset: VOXELS_PER_FACE);
        TryAddVoxelNeighbors(ref voxelNeighborsForeBack, binFore, Direction.Fore, bitOffset: 0);
        TryAddVoxelNeighbors(ref voxelNeighborsForeBack, binBack, Direction.Back, bitOffset: VOXELS_PER_FACE);

        static void TryAddVoxelNeighbors(ref byte axisVoxelNeighbors, Bin neighborBin, Direction binDirection, byte bitOffset) {
            if(neighborBin == null) {
                return;
            }

            Direction neighborFace = Utils.GetOppositeDirection(binDirection);

            for(byte i = 0; i < VOXELS_PER_FACE; i++) {
                int faceVoxelIndex = FaceVoxelIndexToLocalVoxelIndex(i, neighborFace);
                bool hasVoxelNeighbor = (neighborBin.voxels >> faceVoxelIndex & 1) == 1;

                Utils.SetValueInByte(ref axisVoxelNeighbors, bitOffset + i, hasVoxelNeighbor);
            }
        }
    }

    public bool GetVoxelHasNeighbor(int localVoxelIndex, Direction direction) {
        return GetVoxelHasNeighbor(localVoxelIndex, direction, voxels, voxelNeighborsRightLeft, voxelNeighborsUpDown, voxelNeighborsForeBack);
    }

    private static bool GetVoxelHasNeighbor(int localVoxelIndex, Direction direction, byte binVoxels, byte voxelNeighborsRightLeft, byte voxelNeighborsUpDown, byte voxelNeighborsForeBack) {
        Vector3Int localCoords = LOCAL_COORDS_LOOKUP[localVoxelIndex];
        Vector3Int neighborCoords = localCoords + Utils.GetDirectionVector(direction);

        if(!VoxelGrid.AreCoordsWithinDimensions(neighborCoords.x, neighborCoords.y, neighborCoords.z, WIDTH, WIDTH, WIDTH)) {
            byte voxelNeighbors = GetCachedVoxelNeighbors(direction, voxelNeighborsRightLeft, voxelNeighborsUpDown, voxelNeighborsForeBack);
            int index = LocalVoxelIndexToFaceVoxelIndex(localVoxelIndex, direction);

            return ((voxelNeighbors >> index) & 1) > 0;
        }

        int neighborLocalVoxelIndex = VoxelGrid.CoordsToIndex(neighborCoords, WIDTH);
        return Utils.GetValueFromByte(binVoxels, neighborLocalVoxelIndex);
    }

    private byte GetCachedVoxelNeighbors(Direction direction) {
        return GetCachedVoxelNeighbors(direction, voxelNeighborsRightLeft, voxelNeighborsUpDown, voxelNeighborsForeBack);
    }

    private static byte GetCachedVoxelNeighbors(Direction direction, byte voxelNeighborsRightLeft, byte voxelNeighborsUpDown, byte voxelNeighborsForeBack) {
        const byte ONE_PER_NEIGHBOR = (1 << VOXELS_PER_FACE) - 1;

        switch(direction) {
            case Direction.Right: {
                return (byte)(voxelNeighborsRightLeft & ONE_PER_NEIGHBOR);
            }
            case Direction.Left: {
                return (byte)((voxelNeighborsRightLeft >> VOXELS_PER_FACE) & ONE_PER_NEIGHBOR);
            }
            case Direction.Up: {
                return (byte)(voxelNeighborsUpDown & ONE_PER_NEIGHBOR);
            }
            case Direction.Down: {
                return (byte)((voxelNeighborsUpDown >> VOXELS_PER_FACE) & ONE_PER_NEIGHBOR);
            }
            case Direction.Fore: {
                return (byte)(voxelNeighborsForeBack & ONE_PER_NEIGHBOR);
            }
            case Direction.Back: {
                return (byte)((voxelNeighborsForeBack >> VOXELS_PER_FACE) & ONE_PER_NEIGHBOR);
            }
            default: {
                throw new NotImplementedException();
            }
        }
    }

    private static int LocalVoxelIndexToFaceVoxelIndex(int localVoxelIndex, Direction face) {
        switch(face) {
            case Direction.Right: {
                if(localVoxelIndex == 1) { return 0; }
                if(localVoxelIndex == 3) { return 1; }
                if(localVoxelIndex == 5) { return 2; }
                if(localVoxelIndex == 7) { return 3; }
                break;
            }
            case Direction.Left: {
                if(localVoxelIndex == 0) { return 0; }
                if(localVoxelIndex == 2) { return 1; }
                if(localVoxelIndex == 4) { return 2; }
                if(localVoxelIndex == 6) { return 3; }
                break;
            }
            case Direction.Up: {
                if(localVoxelIndex == 2) { return 0; }
                if(localVoxelIndex == 3) { return 1; }
                if(localVoxelIndex == 6) { return 2; }
                if(localVoxelIndex == 7) { return 3; }
                break;
            }
            case Direction.Down: {
                if(localVoxelIndex == 0) { return 0; }
                if(localVoxelIndex == 1) { return 1; }
                if(localVoxelIndex == 4) { return 2; }
                if(localVoxelIndex == 5) { return 3; }
                break;
            }
            case Direction.Fore: {
                if(localVoxelIndex == 4) { return 0; }
                if(localVoxelIndex == 5) { return 1; }
                if(localVoxelIndex == 6) { return 2; }
                if(localVoxelIndex == 7) { return 3; }
                break;
            }
            case Direction.Back: {
                if(localVoxelIndex == 0) { return 0; }
                if(localVoxelIndex == 1) { return 1; }
                if(localVoxelIndex == 2) { return 2; }
                if(localVoxelIndex == 3) { return 3; }
                break;
            }
        }

        return -1;
    }

    private static int FaceVoxelIndexToLocalVoxelIndex(int faceVoxelIndex, Direction face) {
        switch(face) {
            case Direction.Right: {
                if(faceVoxelIndex == 0) { return 1; }
                if(faceVoxelIndex == 1) { return 3; }
                if(faceVoxelIndex == 2) { return 5; }
                if(faceVoxelIndex == 3) { return 7; }
                break;
            }
            case Direction.Left: {
                if(faceVoxelIndex == 0) { return 0; }
                if(faceVoxelIndex == 1) { return 2; }
                if(faceVoxelIndex == 2) { return 4; }
                if(faceVoxelIndex == 3) { return 6; }
                break;
            }
            case Direction.Up: {
                if(faceVoxelIndex == 0) { return 2; }
                if(faceVoxelIndex == 1) { return 3; }
                if(faceVoxelIndex == 2) { return 6; }
                if(faceVoxelIndex == 3) { return 7; }
                break;
            }
            case Direction.Down: {
                if(faceVoxelIndex == 0) { return 0; }
                if(faceVoxelIndex == 1) { return 1; }
                if(faceVoxelIndex == 2) { return 4; }
                if(faceVoxelIndex == 3) { return 5; }
                break;
            }
            case Direction.Fore: {
                if(faceVoxelIndex == 0) { return 4; }
                if(faceVoxelIndex == 1) { return 5; }
                if(faceVoxelIndex == 2) { return 6; }
                if(faceVoxelIndex == 3) { return 7; }
                break;
            }
            case Direction.Back: {
                if(faceVoxelIndex == 0) { return 0; }
                if(faceVoxelIndex == 1) { return 1; }
                if(faceVoxelIndex == 2) { return 2; }
                if(faceVoxelIndex == 3) { return 3; }
                break;
            }
        }

        return -1;
    }
}
