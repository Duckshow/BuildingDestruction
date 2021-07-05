using UnityEngine;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PlayMode")]
public readonly struct Bin {
    public const int WIDTH = 2; //! WARNING: very, very hardcoded - if you really want less granularity, consider just adding other bins on top instead!
    public const int SIZE = 8; // must be WIDTH ^ 3
    public const int FACES = 6; // must be WIDTH * 3

    public const byte VOXELS_PER_FACE = WIDTH * WIDTH;

    public static readonly Vector3Int[] LOCAL_COORDS_LOOKUP = new Vector3Int[] {
        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 1, 1)
    };

    public const int LOCAL_INDEX_LOOKUP_0_0_0 = 0;
    public const int LOCAL_INDEX_LOOKUP_1_0_0 = 1;
    public const int LOCAL_INDEX_LOOKUP_0_1_0 = 2;
    public const int LOCAL_INDEX_LOOKUP_1_1_0 = 3;
    public const int LOCAL_INDEX_LOOKUP_0_0_1 = 4;
    public const int LOCAL_INDEX_LOOKUP_1_0_1 = 5;
    public const int LOCAL_INDEX_LOOKUP_0_1_1 = 6;
    public const int LOCAL_INDEX_LOOKUP_1_1_1 = 7;

    public readonly int Index;
    public readonly Vector3Int Coords;

    private readonly byte voxels;
    internal readonly byte voxelNeighborsRightLeft;
    internal readonly byte voxelNeighborsUpDown;
    internal readonly byte voxelNeighborsForeBack;

    public readonly bool IsExterior { get { return isForcedExterior || voxels > 0; } }

#if UNITY_EDITOR
    public readonly bool IsForcedExterior { get { return isForcedExterior; } }
#endif

    public readonly bool IsInterior { get { return !IsExterior; } }
    private readonly bool isForcedExterior;

#if UNITY_EDITOR
    public bool Debug_HasVoxel_0 { get { return GetVoxelExists(0); } }
    public bool Debug_HasVoxel_1 { get { return GetVoxelExists(1); } }
    public bool Debug_HasVoxel_2 { get { return GetVoxelExists(2); } }
    public bool Debug_HasVoxel_3 { get { return GetVoxelExists(3); } }
    public bool Debug_HasVoxel_4 { get { return GetVoxelExists(4); } }
    public bool Debug_HasVoxel_5 { get { return GetVoxelExists(5); } }
    public bool Debug_HasVoxel_6 { get { return GetVoxelExists(6); } }
    public bool Debug_HasVoxel_7 { get { return GetVoxelExists(7); } }
#endif

    public Bin Clone() {
        return (Bin)MemberwiseClone();
    }

    public Bin(int index, Vector3Int binGridDimensions, byte voxels) {
        Index = index;
        Coords = Utils.IndexToCoords(index, binGridDimensions);
        
        Debug.Assert(Index >= 0);
        Debug.Assert(Coords.x >= 0);
        Debug.Assert(Coords.y >= 0);
        Debug.Assert(Coords.z >= 0);

        this.voxels = voxels;
        voxelNeighborsRightLeft = 0;
        voxelNeighborsUpDown = 0;
        voxelNeighborsForeBack = 0;

        isForcedExterior = false;
    }

    public Bin(Bin bin, int index, Vector3Int binGridDimensions) {
        Index = index;
        Coords = Utils.IndexToCoords(index, binGridDimensions);
        
        voxels = bin.voxels;

        voxelNeighborsRightLeft = bin.voxelNeighborsRightLeft;
        voxelNeighborsUpDown = bin.voxelNeighborsUpDown;
        voxelNeighborsForeBack = bin.voxelNeighborsForeBack;

        isForcedExterior = bin.isForcedExterior;
    }

    public Bin(Bin bin, byte voxels) {
        Index = bin.Index;
        Coords = bin.Coords;

        this.voxels = voxels;

        voxelNeighborsRightLeft = bin.voxelNeighborsRightLeft;
        voxelNeighborsUpDown = bin.voxelNeighborsUpDown;
        voxelNeighborsForeBack = bin.voxelNeighborsForeBack;

        isForcedExterior = bin.isForcedExterior;
    }

    private Bin(Bin bin, byte voxelNeighborsRightLeft, byte voxelNeighborsUpDown, byte voxelNeighborsForeBack) {
        Index = bin.Index;
        Coords = bin.Coords;

        voxels = bin.voxels;

        this.voxelNeighborsRightLeft = voxelNeighborsRightLeft;
        this.voxelNeighborsUpDown = voxelNeighborsUpDown;
        this.voxelNeighborsForeBack = voxelNeighborsForeBack;

        isForcedExterior = bin.isForcedExterior;
    }

    public Bin(Bin bin, bool isForcedExterior) {
        Index = bin.Index;
        Coords = bin.Coords;

        voxels = bin.voxels;

        voxelNeighborsRightLeft = bin.voxelNeighborsRightLeft;
        voxelNeighborsUpDown = bin.voxelNeighborsUpDown;
        voxelNeighborsForeBack = bin.voxelNeighborsForeBack;

        this.isForcedExterior = isForcedExterior;
    }

    public bool HasOpenPathBetweenFaces(Direction face1, Direction face2) {
        return HasOpenPathBetweenFaces(voxels, face1, face2);
    }

    internal static bool HasOpenPathBetweenFaces(byte voxels, Direction face1, Direction face2) {
        if(face1 == face2) {
            return false;
        }

        if(Utils.AreDirectionsOpposite(face1, face2)) {
            for(int i = 0; i < VOXELS_PER_FACE; i++) {
                TryGetLocalVoxelIndex(i, face1, out int localVoxelIndex1);
                TryGetLocalVoxelIndex(i, face2, out int localVoxelIndex2);

                if(!Utils.GetValueFromByte(voxels, localVoxelIndex1) && !Utils.GetValueFromByte(voxels, localVoxelIndex2)) {
                    return true;
                }
            }

            return false;
        }

        byte targetVoxels1 = GetTargetVoxelsForFace(face1);
        byte targetVoxels2 = GetTargetVoxelsForFace(face2);
        byte GetTargetVoxelsForFace(Direction face) {
            if(face == Direction.None) {
                return byte.MaxValue;
            }
            
            byte targetVoxels = 0;

            TryGetLocalVoxelIndex(0, face, out int localVoxelIndex0);
            TryGetLocalVoxelIndex(1, face, out int localVoxelIndex1);
            TryGetLocalVoxelIndex(2, face, out int localVoxelIndex2);
            TryGetLocalVoxelIndex(3, face, out int localVoxelIndex3);

            Utils.SetValueInByte(ref targetVoxels, localVoxelIndex0, true);
            Utils.SetValueInByte(ref targetVoxels, localVoxelIndex1, true);
            Utils.SetValueInByte(ref targetVoxels, localVoxelIndex2, true);
            Utils.SetValueInByte(ref targetVoxels, localVoxelIndex3, true);

            return targetVoxels;
        }

        return (~voxels & targetVoxels1 & targetVoxels2) > 0;
    }

    public bool IsConnectedToNeighbor(Direction direction) {
        return IsConnectedToNeighbor(direction, voxels, voxelNeighborsRightLeft, voxelNeighborsUpDown, voxelNeighborsForeBack);
    }

    internal static bool IsConnectedToNeighbor(Direction direction, byte voxels, byte voxelNeighborsRightLeft, byte voxelNeighborsUpDown, byte voxelNeighborsForeBack) {
        if(direction == Direction.None) {
            return false;
        }
        
        byte cachedNeighbors = GetCachedVoxelNeighbors(direction, voxelNeighborsRightLeft, voxelNeighborsUpDown, voxelNeighborsForeBack);

        byte voxelsOnFace = 0;
        for(int i = 0; i < VOXELS_PER_FACE; i++) {
            TryGetLocalVoxelIndex(i, direction, out int localVoxelIndex);
            Utils.SetValueInByte(ref voxelsOnFace, i, Utils.GetValueFromByte(voxels, localVoxelIndex));
        }

        return (cachedNeighbors & voxelsOnFace) > 0;
    }

    public bool IsWholeBinFilled() {
        return voxels == byte.MaxValue;
    }
    
    public bool IsWholeBinEmpty() {
        return voxels == byte.MinValue;
    }

    public bool IsWalledIn() {
        return IsInterior || voxelNeighborsRightLeft == byte.MaxValue && voxelNeighborsUpDown == byte.MaxValue && voxelNeighborsForeBack == byte.MaxValue;
    }

    public bool GetVoxelExists(int localVoxelIndex) {
        return Utils.GetValueFromByte(voxels, localVoxelIndex);
    }

    public bool GetVoxelHasNeighbor(int localVoxelIndex, Direction direction) {
        return GetVoxelHasNeighbor(localVoxelIndex, direction, voxels, voxelNeighborsRightLeft, voxelNeighborsUpDown, voxelNeighborsForeBack);
    }

    public static Vector3 GetVoxelWorldPos(int binIndex, int localVoxelIndex, Vector3Int binGridDimensions, Transform meshTransform) {
        return meshTransform.TransformPoint(GetVoxelGlobalCoords(binIndex, localVoxelIndex, binGridDimensions));
    }

    public static Vector3Int GetVoxelGlobalCoords(int binIndex, int localVoxelIndex, Vector3Int binGridDimensions) {
        return Utils.IndexToCoords(binIndex, binGridDimensions) * WIDTH + GetVoxelLocalCoords(localVoxelIndex);
    }

    public static int GetVoxelGlobalIndex(int binIndex, int localVoxelIndex, Vector3Int binGridDimensions) {
        Vector3Int globalCoords = GetVoxelGlobalCoords(binIndex, localVoxelIndex, binGridDimensions);
        Vector3Int voxelGridDimensions = binGridDimensions * WIDTH;

        return Utils.CoordsToIndex(globalCoords, voxelGridDimensions);
    }

    public static Vector3Int GetVoxelLocalCoords(int localVoxelIndex) {
        return LOCAL_COORDS_LOOKUP[localVoxelIndex];
    }

    public static int GetVoxelLocalIndex(int localX, int localY, int localZ) {
        if(localX == 0 && localY == 0 && localZ == 0) {
            return LOCAL_INDEX_LOOKUP_0_0_0;
        }
        if(localX == 1 && localY == 0 && localZ == 0) {
            return LOCAL_INDEX_LOOKUP_1_0_0;
        }
        if(localX == 0 && localY == 1 && localZ == 0) {
            return LOCAL_INDEX_LOOKUP_0_1_0;
        }
        if(localX == 1 && localY == 1 && localZ == 0) {
            return LOCAL_INDEX_LOOKUP_1_1_0;
        }
        if(localX == 0 && localY == 0 && localZ == 1) {
            return LOCAL_INDEX_LOOKUP_0_0_1;
        }
        if(localX == 1 && localY == 0 && localZ == 1) {
            return LOCAL_INDEX_LOOKUP_1_0_1;
        }
        if(localX == 0 && localY == 1 && localZ == 1) {
            return LOCAL_INDEX_LOOKUP_0_1_1;
        }
        if(localX == 1 && localY == 1 && localZ == 1) {
            return LOCAL_INDEX_LOOKUP_1_1_1;
        }

        return -1;
    }

    public Bin SetVoxelExists(int localVoxelIndex, bool exists) {
        byte cachedVoxels = voxels;
        Utils.SetValueInByte(ref cachedVoxels, localVoxelIndex, exists);
        return new Bin(this, cachedVoxels);
    }

    internal static Bin RefreshConnectivity(Bin[] voxelBlocks, int index, Vector3Int dimensions) {
        Vector3Int coords = Utils.IndexToCoords(index, dimensions);

        TryGetVoxelBlock(coords + Vector3Int.right,     out Bin voxelBlockRight);
        TryGetVoxelBlock(coords + Vector3Int.left,      out Bin voxelBlockLeft);
        TryGetVoxelBlock(coords + Vector3Int.up,        out Bin voxelBlockUp);
        TryGetVoxelBlock(coords + Vector3Int.down,      out Bin voxelBlockDown);
        TryGetVoxelBlock(coords + Vector3Int.forward,   out Bin voxelBlockFore);
        TryGetVoxelBlock(coords + Vector3Int.back,      out Bin voxelBlockBack);

        bool TryGetVoxelBlock(Vector3Int voxelBlockCoords, out Bin voxelBlock) {
            int voxelBlockIndex = Utils.CoordsToIndex(voxelBlockCoords, dimensions);

            if(voxelBlockIndex < 0) {
                voxelBlock = new Bin();
                return false;
            }

            voxelBlock = voxelBlocks[voxelBlockIndex];
            return true;
        }

        return RefreshConnectivity(voxelBlocks[index], voxelBlockRight, voxelBlockLeft, voxelBlockUp, voxelBlockDown, voxelBlockFore, voxelBlockBack);
    }

    internal static Bin RefreshConnectivity(Bin voxelBlock, Bin voxelBlockRight, Bin voxelBlockLeft, Bin voxelBlockUp, Bin voxelBlockDown, Bin voxelBlockFore, Bin voxelBlockBack) {
        byte voxelNeighborsRightLeft = 0;
        byte voxelNeighborsUpDown = 0;
        byte voxelNeighborsForeBack = 0;

        TryAddVoxelNeighbors(ref voxelNeighborsRightLeft,   voxelBlockRight, Direction.Right,   bitOffset: 0);
        TryAddVoxelNeighbors(ref voxelNeighborsRightLeft,   voxelBlockLeft,  Direction.Left,    bitOffset: VOXELS_PER_FACE);
        TryAddVoxelNeighbors(ref voxelNeighborsUpDown,      voxelBlockUp,    Direction.Up,      bitOffset: 0);
        TryAddVoxelNeighbors(ref voxelNeighborsUpDown,      voxelBlockDown,  Direction.Down,    bitOffset: VOXELS_PER_FACE);
        TryAddVoxelNeighbors(ref voxelNeighborsForeBack,    voxelBlockFore,  Direction.Fore,    bitOffset: 0);
        TryAddVoxelNeighbors(ref voxelNeighborsForeBack,    voxelBlockBack,  Direction.Back,    bitOffset: VOXELS_PER_FACE);

        static void TryAddVoxelNeighbors(ref byte axisVoxelNeighbors, Bin neighborBin, Direction binDirection, byte bitOffset) {
            if(neighborBin.IsWholeBinEmpty()) {
                return;
            }

            Direction neighborFace = Utils.GetOppositeDirection(binDirection);

            for(byte i = 0; i < VOXELS_PER_FACE; i++) {
                TryGetLocalVoxelIndex(i, neighborFace, out int localVoxelIndex);
                bool hasVoxelNeighbor = (neighborBin.voxels >> localVoxelIndex & 1) == 1;

                bool b0 = Utils.GetValueFromByte(neighborBin.voxels, 0);
                bool b1 = Utils.GetValueFromByte(neighborBin.voxels, 1);
                bool b2 = Utils.GetValueFromByte(neighborBin.voxels, 2);
                bool b3 = Utils.GetValueFromByte(neighborBin.voxels, 3);
                bool b4 = Utils.GetValueFromByte(neighborBin.voxels, 4);
                bool b5 = Utils.GetValueFromByte(neighborBin.voxels, 5);
                bool b6 = Utils.GetValueFromByte(neighborBin.voxels, 6);
                bool b7 = Utils.GetValueFromByte(neighborBin.voxels, 7); 

                //aercmamrecäame // something's going on here with refresh connectivity, although it may just be the DoubleSplitAlongZ-setup being wrong

                Utils.SetValueInByte(ref axisVoxelNeighbors, bitOffset + i, hasVoxelNeighbor);
            }
        }

        return new Bin(voxelBlock, voxelNeighborsRightLeft, voxelNeighborsUpDown, voxelNeighborsForeBack);
    }

    internal static bool GetVoxelHasNeighbor(int localVoxelIndex, Direction direction, byte voxels, byte voxelNeighborsRightLeft, byte voxelNeighborsUpDown, byte voxelNeighborsForeBack) {
        Vector3Int localCoords = GetVoxelLocalCoords(localVoxelIndex);

        bool isNeighborInSameBin = false;
        switch(direction) {
            case Direction.Right:   { isNeighborInSameBin = localCoords.x == 0; break; }
            case Direction.Left:    { isNeighborInSameBin = localCoords.x == 1; break; }
            case Direction.Up:      { isNeighborInSameBin = localCoords.y == 0; break; }
            case Direction.Down:    { isNeighborInSameBin = localCoords.y == 1; break; }
            case Direction.Fore:    { isNeighborInSameBin = localCoords.z == 0; break; }
            case Direction.Back:    { isNeighborInSameBin = localCoords.z == 1; break; }
        }

        if(isNeighborInSameBin) {
            Vector3Int dirVec = Utils.DirectionToVector(direction);
            int neighborIndex = GetVoxelLocalIndex(localCoords.x + dirVec.x, localCoords.y + dirVec.y, localCoords.z + dirVec.z);
            return Utils.GetValueFromByte(voxels, neighborIndex);
        }
        else {
            if(!TryGetFaceVoxelIndex(localVoxelIndex, direction, out int faceVoxelIndex)) {
                throw new Exception();
            }

            byte neighbors = GetCachedVoxelNeighbors(direction, voxelNeighborsRightLeft, voxelNeighborsUpDown, voxelNeighborsForeBack);
            return Utils.GetValueFromByte(neighbors, faceVoxelIndex);
        }
    }

    internal static byte GetCachedVoxelNeighbors(Direction direction, byte voxelNeighborsRightLeft, byte voxelNeighborsUpDown, byte voxelNeighborsForeBack) {
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

    internal static bool TryGetFaceVoxelIndex(int localVoxelIndex, Direction face, out int faceVoxelIndex) {
        faceVoxelIndex = -1;
        
        switch(face) {
            case Direction.Right: {
                if(localVoxelIndex == 1) { faceVoxelIndex = 0; }
                if(localVoxelIndex == 3) { faceVoxelIndex = 1; }
                if(localVoxelIndex == 5) { faceVoxelIndex = 2; }
                if(localVoxelIndex == 7) { faceVoxelIndex = 3; }
                break;
            }
            case Direction.Left: {
                if(localVoxelIndex == 0) { faceVoxelIndex = 0; }
                if(localVoxelIndex == 2) { faceVoxelIndex = 1; }
                if(localVoxelIndex == 4) { faceVoxelIndex = 2; }
                if(localVoxelIndex == 6) { faceVoxelIndex = 3; }
                break;
            }
            case Direction.Up: {
                if(localVoxelIndex == 2) { faceVoxelIndex = 0; }
                if(localVoxelIndex == 3) { faceVoxelIndex = 1; }
                if(localVoxelIndex == 6) { faceVoxelIndex = 2; }
                if(localVoxelIndex == 7) { faceVoxelIndex = 3; }
                break;
            }
            case Direction.Down: {
                if(localVoxelIndex == 0) { faceVoxelIndex = 0; }
                if(localVoxelIndex == 1) { faceVoxelIndex = 1; }
                if(localVoxelIndex == 4) { faceVoxelIndex = 2; }
                if(localVoxelIndex == 5) { faceVoxelIndex = 3; }
                break;
            }
            case Direction.Fore: {
                if(localVoxelIndex == 4) { faceVoxelIndex = 0; }
                if(localVoxelIndex == 5) { faceVoxelIndex = 1; }
                if(localVoxelIndex == 6) { faceVoxelIndex = 2; }
                if(localVoxelIndex == 7) { faceVoxelIndex = 3; }
                break;
            }
            case Direction.Back: {
                if(localVoxelIndex == 0) { faceVoxelIndex = 0; }
                if(localVoxelIndex == 1) { faceVoxelIndex = 1; }
                if(localVoxelIndex == 2) { faceVoxelIndex = 2; }
                if(localVoxelIndex == 3) { faceVoxelIndex = 3; }
                break;
            }
        }

        return faceVoxelIndex != -1;
    }

    internal static bool TryGetLocalVoxelIndex(int faceVoxelIndex, Direction face, out int localVoxelIndex) {
        localVoxelIndex = -1;
        
        switch(face) {
            case Direction.Right: {
                if(faceVoxelIndex == 0) { localVoxelIndex = 1; }
                if(faceVoxelIndex == 1) { localVoxelIndex = 3; }
                if(faceVoxelIndex == 2) { localVoxelIndex = 5; }
                if(faceVoxelIndex == 3) { localVoxelIndex = 7; }
                break;
            }
            case Direction.Left: {
                if(faceVoxelIndex == 0) { localVoxelIndex = 0; }
                if(faceVoxelIndex == 1) { localVoxelIndex = 2; }
                if(faceVoxelIndex == 2) { localVoxelIndex = 4; }
                if(faceVoxelIndex == 3) { localVoxelIndex = 6; }
                break;
            }
            case Direction.Up: {
                if(faceVoxelIndex == 0) { localVoxelIndex = 2; }
                if(faceVoxelIndex == 1) { localVoxelIndex = 3; }
                if(faceVoxelIndex == 2) { localVoxelIndex = 6; }
                if(faceVoxelIndex == 3) { localVoxelIndex = 7; }
                break;
            }
            case Direction.Down: {
                if(faceVoxelIndex == 0) { localVoxelIndex = 0; }
                if(faceVoxelIndex == 1) { localVoxelIndex = 1; }
                if(faceVoxelIndex == 2) { localVoxelIndex = 4; }
                if(faceVoxelIndex == 3) { localVoxelIndex = 5; }
                break;
            }
            case Direction.Fore: {
                if(faceVoxelIndex == 0) { localVoxelIndex = 4; }
                if(faceVoxelIndex == 1) { localVoxelIndex = 5; }
                if(faceVoxelIndex == 2) { localVoxelIndex = 6; }
                if(faceVoxelIndex == 3) { localVoxelIndex = 7; }
                break;
            }
            case Direction.Back: {
                if(faceVoxelIndex == 0) { localVoxelIndex = 0; }
                if(faceVoxelIndex == 1) { localVoxelIndex = 1; }
                if(faceVoxelIndex == 2) { localVoxelIndex = 2; }
                if(faceVoxelIndex == 3) { localVoxelIndex = 3; }
                break;
            }
        }

        return localVoxelIndex != -1;
    }

    public static uint GetVisualID(Bin voxelBlock) {
        uint id = voxelBlock.voxels;
        id |= ((uint)voxelBlock.voxelNeighborsRightLeft   << 8);
        id |= ((uint)voxelBlock.voxelNeighborsUpDown      << 16);
        id |= ((uint)voxelBlock.voxelNeighborsForeBack    << 24);
        return id;
    }

    public static Vector3Int GetMinVoxelCoord(Bin voxelBlock) {
        if(voxelBlock.IsWholeBinEmpty()) {
            return new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        }

        Vector3Int minVoxelCoords = new Vector3Int(0, 0, 0);

        if(voxelBlock.IsWholeBinFilled()) {
            return minVoxelCoords;
        }

        if(!voxelBlock.GetVoxelExists(0) && !voxelBlock.GetVoxelExists(2) && !voxelBlock.GetVoxelExists(4) && !voxelBlock.GetVoxelExists(6)) {
            ++minVoxelCoords.x;
        }

        if(!voxelBlock.GetVoxelExists(0) && !voxelBlock.GetVoxelExists(1) && !voxelBlock.GetVoxelExists(4) && !voxelBlock.GetVoxelExists(5)) {
            ++minVoxelCoords.y;
        }

        if(!voxelBlock.GetVoxelExists(0) && !voxelBlock.GetVoxelExists(1) && !voxelBlock.GetVoxelExists(2) && !voxelBlock.GetVoxelExists(3)) {
            ++minVoxelCoords.z;
        }

        return minVoxelCoords;
    }
}
