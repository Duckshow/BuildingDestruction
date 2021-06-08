using NUnit.Framework;
using UnityEngine;

public class VoxelGridTests
{
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
        {
            Vector3Int dimensions = new Vector3Int(8, 8, 8);
            Octree<bool> voxelMap = new Octree<bool>(Mathf.Max(dimensions.x, Mathf.Max(dimensions.y, dimensions.z)), startValue: true);

            Assert.AreEqual(new Vector3(3.5f, 3.5f, 3.5f), VoxelGrid.GetPivot(voxelMap, dimensions, isStatic: false));
            Assert.AreEqual(new Vector3(3.5f, -0.5f, 3.5f), VoxelGrid.GetPivot(voxelMap, dimensions, isStatic: true));

        }
        {
            Vector3Int dimensions = new Vector3Int(1, 1, 1);
            Octree<bool> voxelMap = new Octree<bool>(Mathf.Max(dimensions.x, Mathf.Max(dimensions.y, dimensions.z)), startValue: true);

            Assert.AreEqual(new Vector3(0f, 0f, 0f), VoxelGrid.GetPivot(voxelMap, dimensions, isStatic: false));
            Assert.AreEqual(new Vector3(0f, -0.5f, 0f), VoxelGrid.GetPivot(voxelMap, dimensions, isStatic: true));
        }
    }
}
