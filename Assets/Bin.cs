using UnityEngine;
using System;

public class Bin { // TODO: this doesn't support any other width than 2!
    public const int WIDTH = 2;
    public const int SIZE = 8; // must be WIDTH ^ 3

    public int Index { get; private set; }
    public Vector3Int Coords { get; private set; }
    public Voxel[] Voxels { get; private set; }
    public bool IsDirty { get; private set; }
    public bool IsWholeBinFilled { get; private set; }
    public bool IsWholeBinEmpty { get; private set; }

    private bool[] areVoxelsDirty;
    private NeighborRelationships[] voxelConnections;


    public Bin(int index, Vector3Int binGridDimensions) {
        Index = index;
        Coords = VoxelGrid.IndexToCoords(index, binGridDimensions);

        Vector3Int voxelGridDimensions = VoxelGrid.CalculateVoxelGridDimensions(binGridDimensions);

        Voxels = new Voxel[SIZE];
        for(int i = 0; i < SIZE; i++) {
            Voxels[i] = new Voxel(this, i, voxelGridDimensions);
        }

        areVoxelsDirty = new bool[SIZE];
    }

    public Bin(int index, Vector3Int binGridDimensions, Bin bin) {
        Index = index;
        Coords = VoxelGrid.IndexToCoords(index, binGridDimensions);

        Vector3Int voxelGridDimensions = VoxelGrid.CalculateVoxelGridDimensions(binGridDimensions);

        Voxels = new Voxel[SIZE];
        for(int i = 0; i < SIZE; i++) {
            Voxels[i] = new Voxel(this, i, voxelGridDimensions, bin.Voxels[i].IsFilled);
        }

        IsDirty = bin.IsDirty;
        IsWholeBinFilled = bin.IsWholeBinFilled;
        IsWholeBinEmpty = bin.IsWholeBinEmpty;

        areVoxelsDirty = bin.areVoxelsDirty;
        voxelConnections = bin.voxelConnections;
    }

    public void SetVoxelIsFilled(int localIndex, bool isFilled) {
        Voxels[localIndex] = new Voxel(Voxels[localIndex], isFilled);
    }

    public Voxel GetVoxel(int localIndex) {
        return Voxels[localIndex];
    }

    public bool HasFilledVoxelOnFace(Direction face) {
        int[] faceIndexes = GetVoxelIndexesForBinFace(face);
        for(int i = 0; i < faceIndexes.Length; i++) {
            int voxelLocalIndex = faceIndexes[i];

            if(GetVoxel(voxelLocalIndex).IsFilled) {
                return true;
            }
        }

        return false;
    }

    public static int[] GetVoxelIndexesForBinFace(Direction face) {
        switch(face) {
            case Direction.Right:{ return new int[] { 1, 3, 5, 7 }; }
            case Direction.Left: { return new int[] { 0, 2, 4, 6 }; }
            case Direction.Up:   { return new int[] { 2, 3, 6, 7 }; }
            case Direction.Down: { return new int[] { 0, 1, 4, 5 }; }
            case Direction.Fore: { return new int[] { 4, 5, 6, 7 }; }
            case Direction.Back: { return new int[] { 0, 1, 2, 3 }; }
        }

        return null;
    }

    public NeighborRelationships GetVoxelConnections(int index) {
        return voxelConnections[index];
    }

    public void SetVoxelConnections(int index, NeighborRelationships connections) {
        if(voxelConnections == null) {
            voxelConnections = new NeighborRelationships[SIZE];
        }

        voxelConnections[index] = connections;
    }

    public void RefreshIsWholeBinFilled() {
        IsWholeBinFilled = true;
        
        for(int i = 0; i < SIZE; i++) {
            if(!GetVoxel(i).IsFilled) {
                IsWholeBinFilled = false;
                return;
            }
        }
    }

    public void RefreshIsWholeBinEmpty() {
        IsWholeBinEmpty = true;

        for(int i = 0; i < SIZE; i++) {
            if(GetVoxel(i).IsFilled) {
                IsWholeBinEmpty = false;
                return;
            }
        }
    }

    public bool TryMarkVoxelAsDirty(int localIndex) {
        if(!GetVoxel(localIndex).IsFilled) {
            return false;
        }

        IsDirty = true;
        areVoxelsDirty[localIndex] = true;
        return true;
    }

    public void ClearDirty() {
        IsDirty = false;
        areVoxelsDirty = new bool[SIZE];
    }

    public bool[] GetAreVoxelsDirty() {
        return areVoxelsDirty;
    }
}

public readonly struct Voxel {
    public readonly Bin OwnerBin;

    public readonly int LocalIndex;
    public readonly Vector3Int LocalCoords;
    
    public readonly bool IsFilled;

    public Voxel(Bin ownerBin, int localIndex, Vector3Int voxelGridDimensions, bool isFilled = false) {
        OwnerBin = ownerBin;

        LocalIndex = localIndex;
        LocalCoords = VoxelGrid.IndexToCoords(LocalIndex, width: Bin.WIDTH);

        IsFilled = isFilled;
    }

    public Voxel(Voxel v, bool isFilled) {
        OwnerBin = v.OwnerBin;

        LocalIndex = v.LocalIndex;
        LocalCoords = v.LocalCoords;

        IsFilled = isFilled;
    }

    public Vector3 GetWorldPos(Transform meshTransform) {
        return meshTransform.TransformPoint(GetGlobalCoords());
    }

    public Vector3 GetGlobalCoords() {
        return OwnerBin.Coords * Bin.WIDTH + LocalCoords;
    }
}

public readonly struct NeighborRelationships : IEquatable<NeighborRelationships> {
    public readonly bool Right;
    public readonly bool Left;
    public readonly bool Up;
    public readonly bool Down;
    public readonly bool Fore;
    public readonly bool Back;

    public NeighborRelationships(bool right, bool left, bool up, bool down, bool fore, bool back) {
        Right = right;
        Left = left;
        Up = up;
        Down = down;
        Fore = fore;
        Back = back;
    }

    public bool Get(Direction dir) {
        switch(dir) {
            case Direction.Right:   return Right;
            case Direction.Left:    return Left;
            case Direction.Up:      return Up;
            case Direction.Down:    return Down;
            case Direction.Fore:    return Fore;
            case Direction.Back:    return Back;
        }

        return false;
    }

    public static NeighborRelationships GetChanged(NeighborRelationships neighborRelationships, Direction dir, bool b) {
        bool Right = neighborRelationships.Right;
        bool Left = neighborRelationships.Left;
        bool Up = neighborRelationships.Up;
        bool Down = neighborRelationships.Down;
        bool Fore = neighborRelationships.Fore;
        bool Back = neighborRelationships.Back;

        switch(dir) {
            case Direction.Right:   { Right = b; break; }
            case Direction.Left:    { Left = b; break; }
            case Direction.Up:      { Up = b; break; }
            case Direction.Down:    { Down = b; break; }
            case Direction.Fore:    { Fore = b; break; }
            case Direction.Back:    { Back = b; break; }
        }

        return new NeighborRelationships(Right, Left, Up, Down, Fore, Back);
    }

    public override bool Equals(object obj) {
        return base.Equals(obj);
    }

    public bool Equals(NeighborRelationships other) {
        return 
            Right == other.Right &&
            Left == other.Left &&
            Up == other.Up &&
            Down == other.Down &&
            Fore == other.Fore &&
            Back == other.Back;
    }

    public override int GetHashCode() {
        int hashCode = -370893141;
        hashCode = hashCode * -1521134295 + Right.GetHashCode();
        hashCode = hashCode * -1521134295 + Left.GetHashCode();
        hashCode = hashCode * -1521134295 + Up.GetHashCode();
        hashCode = hashCode * -1521134295 + Down.GetHashCode();
        hashCode = hashCode * -1521134295 + Fore.GetHashCode();
        hashCode = hashCode * -1521134295 + Back.GetHashCode();
        return hashCode;
    }

    public override string ToString() {
        return string.Format("(Right: {0}, Left: {1}, Up: {2}, Down: {3}, Fore: {4}, Back: {5})", Right, Left, Up, Down, Fore, Back);
    }

    public static bool operator ==(NeighborRelationships lhs, NeighborRelationships rhs) => lhs.Equals(rhs);
    public static bool operator !=(NeighborRelationships lhs, NeighborRelationships rhs) => !(lhs == rhs);
}
