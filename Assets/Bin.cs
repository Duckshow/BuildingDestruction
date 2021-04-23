using UnityEngine;
using System.Collections.Generic;

public readonly struct Bin { // TODO: this doesn't support any other width than 2!
    public const int WIDTH = 2;
    public const int SIZE = 8; // must be WIDTH ^ 3

    public readonly int Index;
    public readonly Vector3Int Coords;
    public readonly int[] VoxelIndexes;

    public readonly bool HasVoxelRight;
    public readonly bool HasVoxelLeft;
    public readonly bool HasVoxelUp;
    public readonly bool HasVoxelDown;
    public readonly bool HasVoxelFore;
    public readonly bool HasVoxelBack;

    public readonly bool HasConnectionRight;
    public readonly bool HasConnectionLeft;
    public readonly bool HasConnectionUp;
    public readonly bool HasConnectionDown;
    public readonly bool HasConnectionFore;
    public readonly bool HasConnectionBack;

    public Bin(int index, Vector3Int voxelGridDimensions) {
        Index = index;
        Coords = VoxelGrid.IndexToCoords(index, VoxelGrid.CalculateBinGridDimensions(voxelGridDimensions));
        VoxelIndexes = SetupVoxelIndexes(Coords, voxelGridDimensions);

        HasVoxelRight = false;
        HasVoxelLeft = false;
        HasVoxelUp   = false;
        HasVoxelDown = false;
        HasVoxelFore = false;
        HasVoxelBack = false;

        HasConnectionRight = false;
        HasConnectionLeft = false;
        HasConnectionUp = false;
        HasConnectionDown = false;
        HasConnectionFore = false;
        HasConnectionBack = false;
    }

    public Bin(Bin bin, bool hasVoxelRight, bool hasVoxelLeft, bool hasVoxelUp, bool hasVoxelDown, bool hasVoxelFore, bool hasVoxelBack, bool hasConnectionRight, bool hasConnectionLeft, bool hasConnectionUp, bool hasConnectionDown, bool hasConnectionFore, bool hasConnectionBack) {
        Index = bin.Index;
        Coords = bin.Coords;
        VoxelIndexes = bin.VoxelIndexes;

        HasVoxelRight = hasVoxelRight;
        HasVoxelLeft = hasVoxelLeft;
        HasVoxelUp = hasVoxelUp;
        HasVoxelDown = hasVoxelDown;
        HasVoxelFore = hasVoxelFore;
        HasVoxelBack = hasVoxelBack;

        HasConnectionRight  = hasConnectionRight;
        HasConnectionLeft   = hasConnectionLeft;
        HasConnectionUp     = hasConnectionUp;
        HasConnectionDown   = hasConnectionDown;
        HasConnectionFore   = hasConnectionFore;
        HasConnectionBack   = hasConnectionBack;
    }

    private static int[] SetupVoxelIndexes(Vector3Int binCoords, Vector3Int voxelGridDimensions) {
        Vector3Int voxelCoords_0 = binCoords * WIDTH;

        Vector3Int[] localCoords = GetContentsLocalCoords();
        Vector3Int[] contentCoords = new Vector3Int[SIZE] {
            voxelCoords_0 + localCoords[0],
            voxelCoords_0 + localCoords[1],
            voxelCoords_0 + localCoords[2],
            voxelCoords_0 + localCoords[3],
            voxelCoords_0 + localCoords[4],
            voxelCoords_0 + localCoords[5],
            voxelCoords_0 + localCoords[6],
            voxelCoords_0 + localCoords[7],
        };

        int[] indexes = new int[SIZE];
        for(int i = 0; i < SIZE; i++) {
            Vector3Int coords = contentCoords[i];
            if(!VoxelGrid.AreCoordsWithinDimensions(coords, voxelGridDimensions)) {
                indexes[i] = -1;
                continue;
            }

            indexes[i] = VoxelGrid.CoordsToIndex(coords, voxelGridDimensions);
        }

        return indexes;
    }

    public static Vector3Int[] GetContentsLocalCoords() {
        return new Vector3Int[SIZE] {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(1, 0, 1),
            new Vector3Int(0, 1, 1),
            new Vector3Int(1, 1, 1)
        };
    }

    public static bool GetHasVoxel(Bin bin, Direction direction) {
        switch(direction) {
            case Direction.None:    return true;
            case Direction.Right:   return bin.HasVoxelRight;
            case Direction.Left:    return bin.HasVoxelLeft;
            case Direction.Up:      return bin.HasVoxelUp;
            case Direction.Down:    return bin.HasVoxelDown;
            case Direction.Fore:    return bin.HasVoxelFore;
            case Direction.Back:    return bin.HasVoxelBack;
        }

        Debug.LogError("Unknown direction: " + direction);
        return false;
    }

    public static Vector3Int GetMinVoxelCoords(Vector3Int binCoords) {
        return binCoords * WIDTH;
    }

    public static Vector3Int GetMaxVoxelCoords(Vector3Int binCoords) {
        return GetMinVoxelCoords(binCoords) + Vector3Int.one;
    }

    public static void RunTests() {
        TestGetContents();
        TestGetVoxelIndexes();
        TestGetMinAndMaxVoxelCoords();
        Debug.Log("Tests done.");
    }

    private static void TestGetContents() {
        Vector3Int voxelGridDimensions = new Vector3Int(16, 16, 16);
        Vector3Int binGridDimensions = VoxelGrid.CalculateBinGridDimensions(voxelGridDimensions);

        List<int> foundContents = new List<int>();

        Debug.LogFormat("GetContentIndexes Results (Voxel Grid Dimensions = {0}):", voxelGridDimensions);

        int binCount = binGridDimensions.x * binGridDimensions.y * binGridDimensions.z;

        for(int binIndex = 0; binIndex < binCount; binIndex++) {
            int[] binContents = SetupVoxelIndexes(VoxelGrid.IndexToCoords(binIndex, binGridDimensions), voxelGridDimensions);

            string results = "";
            for(int contentIndex = 0; contentIndex < binContents.Length; contentIndex++) {
                int content = binContents[contentIndex];
                results += content.ToString();

                if(contentIndex < binContents.Length - 1) {
                    results += ", ";
                }

                Debug.Assert(!foundContents.Contains(content));
                foundContents.Add(content);
            }

            Debug.LogFormat("{0} contains {1}", binIndex, results);
        }
    }

    private static void TestGetVoxelIndexes() {
        void Test(Vector3Int binCoords, Vector3Int voxelGridDimensions) {
            int[] voxelIndexes = SetupVoxelIndexes(binCoords, voxelGridDimensions);

            string info = "";
            for(int i = 0; i < voxelIndexes.Length; i++) {
                info += voxelIndexes[i];

                if(i < voxelIndexes.Length - 1) {
                    info += ", ";
                }
            }

            Debug.LogFormat("VoxelIndexes in bin {0} in voxel grid {1}: {2}", binCoords, voxelGridDimensions, info);
        }

        Test(binCoords: new Vector3Int(0, 0, 0), voxelGridDimensions: new Vector3Int(8, 8, 8));
        Test(binCoords: new Vector3Int(1, 2, 3), voxelGridDimensions: new Vector3Int(8, 8, 8));
        Test(binCoords: new Vector3Int(100, 100, 100), voxelGridDimensions: new Vector3Int(8, 8, 8));
    }

    private static void TestGetMinAndMaxVoxelCoords() {
        Vector3Int binCoords;

        binCoords = new Vector3Int(0, 0, 0);
        Debug.LogFormat("Bin {0}: MinVoxelCoords: {1}, MaxVoxelCoords: {2}", binCoords, GetMinVoxelCoords(binCoords), GetMaxVoxelCoords(binCoords));

        binCoords = new Vector3Int(5, 5, 5);
        Debug.LogFormat("Bin {0}: MinVoxelCoords: {1}, MaxVoxelCoords: {2}", binCoords, GetMinVoxelCoords(binCoords), GetMaxVoxelCoords(binCoords));

        binCoords = new Vector3Int(8, 8, 8);
        Debug.LogFormat("Bin {0}: MinVoxelCoords: {1}, MaxVoxelCoords: {2}", binCoords, GetMinVoxelCoords(binCoords), GetMaxVoxelCoords(binCoords));

        binCoords = new Vector3Int(1, 2, 3);
        Debug.LogFormat("Bin {0}: MinVoxelCoords: {1}, MaxVoxelCoords: {2}", binCoords, GetMinVoxelCoords(binCoords), GetMaxVoxelCoords(binCoords));
    }
}