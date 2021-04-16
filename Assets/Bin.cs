using UnityEngine;
using System.Collections.Generic;

public readonly struct Bin {
    public const int WIDTH = 2;
    public const int SIZE = 8; // must be WIDTH ^ 3

    public readonly int Index;
    public readonly Vector3Int VoxelGridDimensions;

    public Bin(int index, Vector3Int voxelGridDimensions) {
        Index = index;
        VoxelGridDimensions = voxelGridDimensions;
    }

    public Vector3Int[] GetContentCoords() {
        return GetContentCoords(Index, VoxelGridDimensions);
    }

    public static Vector3Int[] GetContentCoords(int index, Vector3Int voxelGridDimensions) {
        Vector3Int binCoords = VoxelGrid.IndexToCoords(index, VoxelGrid.CalculateBinGridDimensions(voxelGridDimensions));
        Vector3Int voxelCoords_0 = binCoords * WIDTH;

        return new Vector3Int[] { // TODO: this doesn't support any other width than 2!
                voxelCoords_0,
                voxelCoords_0 + new Vector3Int(1, 0, 0),
                voxelCoords_0 + new Vector3Int(0, 1, 0),
                voxelCoords_0 + new Vector3Int(1, 1, 0),
                voxelCoords_0 + new Vector3Int(0, 0, 1),
                voxelCoords_0 + new Vector3Int(1, 0, 1),
                voxelCoords_0 + new Vector3Int(0, 1, 1),
                voxelCoords_0 + new Vector3Int(1, 1, 1)
            };
    }

    public static void RunTests() {
        TestGetContentCoords();
    }

    private static void TestGetContentCoords() {
        Vector3Int voxelGridDimensions = new Vector3Int(16, 16, 16);
        Vector3Int binGridDimensions = VoxelGrid.CalculateBinGridDimensions(voxelGridDimensions);

        List<Vector3Int> foundContents = new List<Vector3Int>();

        Debug.LogFormat("GetContentIndexes Results (Bin Grid Dimensions = {0}):", binGridDimensions);
        for(int z = 0; z < binGridDimensions.z; z++) {
            for(int y = 0; y < binGridDimensions.y; y++) {
                for(int x = 0; x < binGridDimensions.x; x++) {
                    int binIndex = VoxelGrid.CoordsToIndex(x, y, z, binGridDimensions);
                    Vector3Int[] binContents = GetContentCoords(binIndex, voxelGridDimensions);
                    
                    string results = "";
                    for(int i = 0; i < binContents.Length; i++) {
                        Vector3Int content = binContents[i];
                        results += content.ToString();

                        if(i < binContents.Length - 1) {
                            results += ", ";
                        }

                        Debug.Assert(!foundContents.Contains(content));
                        foundContents.Add(content);
                    }

                    Debug.LogFormat("{0} contains {1}", binIndex, results);
                }
            }
        }
    }
}