using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class VoxelClusterHandlerTests {
    [Test]
    public void GetBiggestVoxelClusterIndex() {
        int biggest = 1000;

        List<VoxelCluster> list = new List<VoxelCluster>() {
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest)),
            new VoxelCluster(Random.Range(0, biggest))
        };

        int biggestIndex = Random.Range(0, list.Count);
        list.Insert(biggestIndex, new VoxelCluster(biggest));

        Assert.AreEqual(biggestIndex, VoxelClusterHandler.GetBiggestVoxelClusterIndex(list));
    }

    [Test]
    public void TestTryFindCluster() {
        //{
        //    Vector3Int dimensions = new Vector3Int(8, 8, 8);
        //    Octree<bool> voxelMap = new Octree<bool>(Mathf.Max(dimensions.x, Mathf.Max(dimensions.y, dimensions.z)), startValue: true);
        //    Octree<bool> visitedVoxels = new Octree<bool>(Mathf.Max(dimensions.x, Mathf.Max(dimensions.y, dimensions.z)), startValue: false);

        //    VoxelCluster cluster = VoxelClusterHandler.TryFindCluster(Vector3Int.zero, voxelMap, dimensions, visitedVoxels);

        //    Assert.IsNotNull(cluster);
        //    Assert.AreEqual(expected: voxelMap.Size, actual: cluster.VoxelMap.Size);
        //    Assert.AreEqual(expected: dimensions, actual: cluster.Dimensions);
        //    Assert.AreEqual(expected: Vector3Int.zero, actual: cluster.VoxelOffset);

        //    for(int z = 0; z < dimensions.z; z++) {
        //        for(int y = 0; y < dimensions.y; y++) {
        //            for(int x = 0; x < dimensions.x; x++) {
        //                Assert.IsTrue(visitedVoxels.TryGetValue(x, y, z, out bool b, debugDrawCallback: null));
        //            }
        //        }
        //    }
        //}

        //{
        //    Vector3Int dimensions = new Vector3Int(8, 8, 8);
        //    Octree<bool> voxelMap = new Octree<bool>(Mathf.Max(dimensions.x, Mathf.Max(dimensions.y, dimensions.z)), startValue: true);
        //    Octree<bool> visitedVoxels = new Octree<bool>(Mathf.Max(dimensions.x, Mathf.Max(dimensions.y, dimensions.z)), startValue: false);

        //    voxelMap.SetValue(Vector3Int.zero, false);

        //    VoxelCluster cluster = VoxelClusterHandler.TryFindCluster(Vector3Int.zero, voxelMap, dimensions, visitedVoxels);

        //    Assert.IsNull(cluster);

        //    for(int z = 0; z < dimensions.z; z++) {
        //        for(int y = 0; y < dimensions.y; y++) {
        //            for(int x = 0; x < dimensions.x; x++) {
        //                if(x == 0 && y == 0 && z == 0) {
        //                    Assert.IsTrue(visitedVoxels.TryGetValue(x, y, z, out bool b, debugDrawCallback: null));
        //                }
        //                else {
        //                    Assert.IsFalse(visitedVoxels.TryGetValue(x, y, z, out bool b, debugDrawCallback: null));
        //                }
        //            }
        //        }
        //    }
        //}

        {
            Vector3Int dimensions = new Vector3Int(2, 2, 2);
            Octree<bool> voxelMap = new Octree<bool>(Mathf.Max(dimensions.x, Mathf.Max(dimensions.y, dimensions.z)), startValue: false);
            Octree<bool> visitedVoxels = new Octree<bool>(Mathf.Max(dimensions.x, Mathf.Max(dimensions.y, dimensions.z)), startValue: false);

            voxelMap.SetValue(new Vector3Int(0, 0, 0), true);
            voxelMap.SetValue(new Vector3Int(1, 1, 1), true);

            VoxelCluster cluster = VoxelClusterHandler.TryFindCluster(Vector3Int.one, voxelMap, dimensions, visitedVoxels);

            Assert.IsNotNull(cluster);
            Assert.AreEqual(expected: 1, actual: cluster.VoxelMap.Size);
            Assert.AreEqual(expected: Vector3Int.one, actual: cluster.Dimensions);
            Assert.AreEqual(expected: Vector3Int.one, actual: cluster.VoxelOffset);

            for(int z = 0; z < dimensions.z; z++) {
                for(int y = 0; y < dimensions.y; y++) {
                    for(int x = 0; x < dimensions.x; x++) {
                        if((x == 1 && y == 1 && z == 1) || (x == 0 && y == 1 && z == 1) || (x == 1 && y == 0 && z == 1) || (x == 1 && y == 1 && z == 0)) {
                            Assert.IsTrue(visitedVoxels.TryGetValue(x, y, z, out bool b, debugDrawCallback: null));
                        }
                        else {
                            Assert.IsFalse(visitedVoxels.TryGetValue(x, y, z, out bool b, debugDrawCallback: null));
                        }
                    }
                }
            }
        }
    }

    [Test]
    public void TestMoveVoxelsToNewVoxelMap() {
        const int maxWidth = 10;

        for(int i = 0; i < 25; i++) {
            Vector3Int oldBinGridDimensions = Utils.GetRandomVector3Int(1, maxWidth + 1);

            Queue<Vector3Int> voxelsToMove = new Queue<Vector3Int>();
            Vector3Int minCoord = Utils.GetRandomVector3Int(Vector3Int.zero, oldBinGridDimensions);
            Vector3Int maxCoord = Utils.GetRandomVector3Int(minCoord, oldBinGridDimensions);
            Vector3Int intendedNewDimensions = maxCoord - minCoord + Vector3Int.one;

            for(int z = 0; z < intendedNewDimensions.z; z++) {
                for(int y = 0; y < intendedNewDimensions.y; y++) {
                    for(int x = 0; x < intendedNewDimensions.x; x++) {
                        voxelsToMove.Enqueue(new Vector3Int(x, y, z));
                    }
                }
            }

            Vector3Int actualNewDimensions;
            Octree<bool> newVoxelMap = VoxelClusterHandler.MoveVoxelsToNewVoxelMap(voxelsToMove, minCoord, maxCoord, out actualNewDimensions);

            Assert.AreEqual(intendedNewDimensions, actualNewDimensions);
            Assert.AreEqual(Utils.RoundUpToPOT(Mathf.Max(intendedNewDimensions.x, Mathf.Max(intendedNewDimensions.y, intendedNewDimensions.z))), Utils.RoundUpToPOT(newVoxelMap.Size));
        }
    }
}
