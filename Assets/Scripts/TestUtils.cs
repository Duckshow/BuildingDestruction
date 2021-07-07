using UnityEngine;

public static class TestUtils {
    public static Bin[] GetNewVoxelBlockGrid(int width, int height, int depth, byte defaultVoxelValue) {
        Bin[] voxelBlocks = new Bin[width * height * depth];

        int i = 0;
        for(int z = 0; z < depth; ++z) {
            for(int y = 0; y < height; ++y) {
                for(int x = 0; x < width; ++x) {
                    voxelBlocks[i] = new Bin(i, new Vector3Int(width, height, depth), defaultVoxelValue);
                    ++i;
                }
            }
        }

        return voxelBlocks;
    }
}
