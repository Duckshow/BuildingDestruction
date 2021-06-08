using NUnit.Framework;
using UnityEngine;
using Assert = NUnit.Framework.Assert;

public class VoxelGridTests {
    [Test]
    public void AreCoordsWithinDimensions() {
        for(int i = 0; i < 25; i++) {
            Vector3Int dimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));
            Vector3Int coords = new Vector3Int(Random.Range(0, dimensions.x), Random.Range(0, dimensions.y), Random.Range(0, dimensions.z));

            Assert.IsTrue(VoxelGrid.AreCoordsWithinDimensions(coords, dimensions));
            Assert.IsFalse(VoxelGrid.AreCoordsWithinDimensions(dimensions, dimensions));
        }
    }

    [Test]
    public void CoordsToIndexAndViceVersa() {
        for(int i = 0; i < 25; i++) {
            Vector3Int dimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));

            int expectedIndex = 0;

            for(int z = 0; z < dimensions.z; z++) {
                for(int y = 0; y < dimensions.y; y++) {
                    for(int x = 0; x < dimensions.x; x++) {
                        Vector3Int expectedCoords = new Vector3Int(x, y, z);

                        Assert.AreEqual(expectedIndex, VoxelGrid.CoordsToIndex(expectedCoords, dimensions));
                        Assert.AreEqual(expectedCoords, VoxelGrid.IndexToCoords(expectedIndex, dimensions));

                        expectedIndex++;
                    }
                }
            }

            Assert.AreEqual(-1, VoxelGrid.CoordsToIndex(dimensions, dimensions));
            Assert.AreEqual(-1, VoxelGrid.CoordsToIndex(new Vector3Int(0, 0, -1), dimensions));

            Assert.AreEqual(-Vector3Int.one, VoxelGrid.IndexToCoords(-1, dimensions));
            Assert.AreEqual(-Vector3Int.one, VoxelGrid.IndexToCoords(dimensions.x * dimensions.y * dimensions.z, dimensions));
        }
    }

    [Test]
    public void GetPivot() {
        Test(new Vector3Int(0, 0, 0), new Vector3Int(1, 1, 1));
        Test(new Vector3Int(0, 0, 0), new Vector3Int(2, 4, 6));
        
        Test(new Vector3Int(1, 0, 0), new Vector3Int(1, 1, 1));
        Test(new Vector3Int(0, 1, 0), new Vector3Int(1, 1, 1));
        Test(new Vector3Int(0, 0, 1), new Vector3Int(1, 1, 1));
        Test(new Vector3Int(1, 1, 1), new Vector3Int(1, 1, 1));

        Test(new Vector3Int(4, 5, 6), new Vector3Int(7, 8, 9));
        Test(new Vector3Int(0, 20, 0), new Vector3Int(10, 1, 10));

        void Test(Vector3Int offset, Vector3Int dimensions) {
            int treeSize = Utils.RoundUpToPOT(dimensions.Max());
            Octree<bool> voxelMap = new Octree<bool>(Vector3Int.zero, new Vector3Int(treeSize, treeSize, treeSize), startValue: true);
            voxelMap.Resize(offset, dimensions);

            float expectedX = (dimensions.x - 1) / 2f;
            float expectedY = (dimensions.y - 1) / 2f;
            float expectedZ = (dimensions.z - 1) / 2f;

            Assert.AreEqual(new Vector3(expectedX, expectedY, expectedZ), VoxelGrid.GetPivot(voxelMap, isStatic: false), "Error, using offset {0}, dimensions {1}", offset, dimensions);
            Assert.AreEqual(new Vector3(expectedX, offset.y - 0.5f, expectedZ), VoxelGrid.GetPivot(voxelMap, isStatic: true), "Error, using offset {0}, dimensions {1}", offset, dimensions);
        }
    }
}
