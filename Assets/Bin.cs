using UnityEngine;
using System.Collections.Generic;

public readonly struct Bin {
    public const int WIDTH = 2;
    public const int SIZE = 8; // must be WIDTH ^ 3

    public readonly int Index;

    public Bin(int index) {
        Index = index;
    }

    public static Voxel?[] GetBinVoxels(int binIndex, VoxelGrid owner) {
        Voxel?[] voxels = new Voxel?[SIZE];

        Vector3Int[] binContents = GetContents(binIndex, owner.GetVoxelGridDimensions());
        for(int i = 0; i < binContents.Length; i++) {
            Vector3Int coords = binContents[i];

            Voxel v;
            if(owner.TryGetVoxel(coords.x, coords.y, coords.z, out v)) {
                voxels[i] = v;
            }
            else {
                voxels[i] = null;
            }
        }

        return voxels;
    }

    public static Vector3Int[] GetContents(int index, Vector3Int voxelGridDimensions) {
        Vector3Int binCoords = VoxelGrid.IndexToCoords(index, VoxelGrid.CalculateBinGridDimensions(voxelGridDimensions));
        Vector3Int voxelCoords_0 = binCoords * WIDTH;

        return GetContentsLocalCoords(offset: voxelCoords_0);
    }

    public static Vector3Int[] GetContentsLocalCoords(Vector3Int offset) {
        return new Vector3Int[] { // TODO: this doesn't support any other width than 2!
            new Vector3Int(offset.x,        offset.y,       offset.z),
            new Vector3Int(offset.x + 1,    offset.y,       offset.z),
            new Vector3Int(offset.x,        offset.y + 1,   offset.z),
            new Vector3Int(offset.x + 1,    offset.y + 1,   offset.z),
            new Vector3Int(offset.x,        offset.y,       offset.z + 1),
            new Vector3Int(offset.x + 1,    offset.y,       offset.z + 1),
            new Vector3Int(offset.x,        offset.y + 1,   offset.z + 1),
            new Vector3Int(offset.x + 1,    offset.y + 1,   offset.z + 1)
        };
    }

    public static void RunTests() {
        TestGetContents();
    }

    private static void TestGetContents() {
        Vector3Int voxelGridDimensions = new Vector3Int(16, 16, 16);
        Vector3Int binGridDimensions = VoxelGrid.CalculateBinGridDimensions(voxelGridDimensions);

        List<Vector3Int> foundContents = new List<Vector3Int>();

        Debug.LogFormat("GetContentIndexes Results (Voxel Grid Dimensions = {0}):", voxelGridDimensions);

        int binCount = binGridDimensions.x * binGridDimensions.y * binGridDimensions.z;

        for(int binIndex = 0; binIndex < binCount; binIndex++) {
            Vector3Int[] binContents = GetContents(binIndex, voxelGridDimensions);

            string results = "";
            for(int contentIndex = 0; contentIndex < binContents.Length; contentIndex++) {
                Vector3Int content = binContents[contentIndex];
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
}