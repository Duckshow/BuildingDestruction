using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Assert = NUnit.Framework.Assert;

public class VoxelGridTests {

    [Test]
    public void GetPivot() {
        Test(new Vector3Int(1, 1, 1));
        Test(new Vector3Int(2, 4, 6));

        Test(new Vector3Int(1, 1, 1));
        Test(new Vector3Int(1, 1, 1));
        Test(new Vector3Int(1, 1, 1));
        Test(new Vector3Int(1, 1, 1));

        Test(new Vector3Int(7, 8, 9));
        Test(new Vector3Int(10, 1, 10));

        void Test(Vector3Int dimensions) {
            VoxelCluster voxelCluster = new VoxelCluster(dimensions, voxelBlockStartValue: byte.MaxValue);

            float expectedX = dimensions.x; // NOTE: remember that one voxelblock is 2 voxels/units wide
            float expectedY = dimensions.y;
            float expectedZ = dimensions.z;

            Assert.AreEqual(new Vector3(expectedX, expectedY, expectedZ), VoxelGrid.GetPivot(voxelCluster, isStatic: false), "Error, using dimensions {0}", dimensions);
            Assert.AreEqual(new Vector3(expectedX, 0f, expectedZ), VoxelGrid.GetPivot(voxelCluster, isStatic: true), "Error, using dimensions {0}", dimensions);
        }
    }

    [Test]
    public void TestGetBiggestVoxelClusterIndex() {
        for(int i = 0; i < 10; i++) {
            int biggest = 10;

            List<VoxelCluster> list = new List<VoxelCluster>() {
                new VoxelCluster(new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest))),
                new VoxelCluster(new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest))),
                new VoxelCluster(new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest))),
                new VoxelCluster(new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest))),
                new VoxelCluster(new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest))),
                new VoxelCluster(new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest))),
                new VoxelCluster(new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest))),
                new VoxelCluster(new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest))),
                new VoxelCluster(new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest))),
                new VoxelCluster(new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest)))
            };

            int biggestIndex = Random.Range(0, list.Count);
            list.Insert(biggestIndex, new VoxelCluster(new Vector3Int(biggest, biggest, biggest)));

            Assert.AreEqual(biggestIndex, VoxelGrid.GetBiggestVoxelClusterIndex(list));
        }
    }
}
