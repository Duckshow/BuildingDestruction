using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

using Assert = NUnit.Framework.Assert;

public class VoxelClusterHandlerIntegrationTests {

    private const float STEP_DURATION = 0.1f;


    [UnityTest]
    public IEnumerator TestTryFindCluster_Split() {
        //yield return TestSplitAlongAxis(offset: Vector3Int.zero, relativeZ: Axis.Z, 0);
        //yield return TestSplitAlongAxis(offset: Vector3Int.zero, relativeZ: Axis.Z, 1);
        //yield return TestSplitAlongAxis(offset: Vector3Int.zero, relativeZ: Axis.Z, 2);
        //yield return TestSplitAlongAxis(offset: Vector3Int.zero, relativeZ: Axis.Z, 3);

        yield return TestSplitAlongAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Z, 0);
        //yield return TestSplitAlongAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Z, 1);
        //yield return TestSplitAlongAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Z, 2);
        //yield return TestSplitAlongAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Z, 3);


        //yield return TestSplitAlongAxis(offset: Vector3Int.zero, relativeZ: Axis.Y, 0);
        //yield return TestSplitAlongAxis(offset: Vector3Int.zero, relativeZ: Axis.Y, 1);
        //yield return TestSplitAlongAxis(offset: Vector3Int.zero, relativeZ: Axis.Y, 2);
        //yield return TestSplitAlongAxis(offset: Vector3Int.zero, relativeZ: Axis.Y, 3);

        //yield return TestSplitAlongAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Y, 0);
        //yield return TestSplitAlongAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Y, 1);
        //yield return TestSplitAlongAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Y, 2);
        //yield return TestSplitAlongAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Y, 3);


        //yield return TestSplitAlongAxis(offset: Vector3Int.zero, relativeZ: Axis.X, 0);
        //yield return TestSplitAlongAxis(offset: Vector3Int.zero, relativeZ: Axis.X, 1);
        //yield return TestSplitAlongAxis(offset: Vector3Int.zero, relativeZ: Axis.X, 2);
        //yield return TestSplitAlongAxis(offset: Vector3Int.zero, relativeZ: Axis.X, 3);

        //yield return TestSplitAlongAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.X, 0);
        //yield return TestSplitAlongAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.X, 1);
        //yield return TestSplitAlongAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.X, 2);
        //yield return TestSplitAlongAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.X, 3);
    }

    private IEnumerator TestSplitAlongAxis(Vector3Int offset, Axis relativeZ, int relXIndex) {
        Utils.GetOtherAxes(relativeZ, out Axis relativeX, out Axis relativeY);

        Vector3Int dimensions = new Vector3Int().AsRelative(4, 5, 6, relativeX, relativeY, relativeZ);
        int width  = dimensions.Get(relativeX);
        int height = dimensions.Get(relativeY);
        int depth  = dimensions.Get(relativeZ);

        yield return TestFindClusters(offset, dimensions, ManipulateVoxels, ValidateResults);

        void ManipulateVoxels(Octree<bool> voxelMap, Queue<Vector3Int> dirtyVoxels) {
            for(int relYIndex = 0; relYIndex < height; relYIndex++) {
                for(int relZIndex = 0; relZIndex < depth; relZIndex++) {
                    Vector3Int pos = new Vector3Int().AsRelative(relXIndex, relYIndex, relZIndex, relativeX, relativeY, relativeZ);

                    VoxelGrid.TryRemoveVoxel(pos, voxelMap, dirtyVoxels);
                }
            }
        }

        void ValidateResults(List<Octree<bool>> foundClusters) {
            if(relXIndex == 0 || relXIndex == width - 1) {
                Assert.AreEqual(1, foundClusters.Count);

                Octree<bool> cluster = foundClusters[0];

                Debug.Log(dimensions + " -> " + width + ", " + height + ", " + depth + " -> " + cluster.Dimensions);
                Assert.AreEqual(width - 1, cluster.Dimensions.Get(relativeX));
                Assert.AreEqual(height,    cluster.Dimensions.Get(relativeY));
                Assert.AreEqual(depth,     cluster.Dimensions.Get(relativeZ));

                for(int relZ = 0; relZ < cluster.Dimensions.Get(relativeZ); relZ++) {
                    for(int relY = 0; relY < cluster.Dimensions.Get(relativeY); relY++) {
                        for(int relX = 0; relX < cluster.Dimensions.Get(relativeX); relX++) {
                            Vector3Int pos = new Vector3Int().AsRelative(relX, relY, relZ, relativeX, relativeY, relativeZ);

                            Assert.IsTrue(cluster.TryGetValue(pos, out bool value));
                            Assert.IsTrue(value);
                        }
                    }
                }
            }
            else {
                Assert.AreEqual(2, foundClusters.Count);

                bool isZeroSmaller = foundClusters[0].Dimensions.Get(relativeX) < foundClusters[1].Dimensions.Get(relativeX);
                Octree<bool> smallerCluster = isZeroSmaller ? foundClusters[0] : foundClusters[1];
                Octree<bool> biggerCluster = isZeroSmaller ? foundClusters[1] : foundClusters[0];

                Debug.Log(relXIndex);
                Assert.AreEqual(1,                     smallerCluster.Dimensions.Get(relativeX));
                Assert.AreEqual(height,                smallerCluster.Dimensions.Get(relativeY));
                Assert.AreEqual(depth,                 smallerCluster.Dimensions.Get(relativeZ));

                Assert.AreEqual(2,                     biggerCluster.Dimensions.Get(relativeX));
                Assert.AreEqual(height,                biggerCluster.Dimensions.Get(relativeY));
                Assert.AreEqual(depth,                 biggerCluster.Dimensions.Get(relativeZ));
            }
        }
    }

    [UnityTest]
    public IEnumerator TestTryFindCluster_DoubleSplit() {
        yield return TestDoubleSplitOnAxis(offset: Vector3Int.zero, relativeZ: Axis.Z, 1, 1);
        yield return TestDoubleSplitOnAxis(offset: Vector3Int.zero, relativeZ: Axis.Z, 1, 2);
        yield return TestDoubleSplitOnAxis(offset: Vector3Int.zero, relativeZ: Axis.Z, 2, 1);
        yield return TestDoubleSplitOnAxis(offset: Vector3Int.zero, relativeZ: Axis.Z, 2, 2);

        yield return TestDoubleSplitOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Z, 1, 1);
        yield return TestDoubleSplitOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Z, 1, 2);
        yield return TestDoubleSplitOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Z, 2, 1);
        yield return TestDoubleSplitOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Z, 2, 2);


        yield return TestDoubleSplitOnAxis(offset: Vector3Int.zero, relativeZ: Axis.Y, 1, 1);
        yield return TestDoubleSplitOnAxis(offset: Vector3Int.zero, relativeZ: Axis.Y, 1, 2);
        yield return TestDoubleSplitOnAxis(offset: Vector3Int.zero, relativeZ: Axis.Y, 2, 1);
        yield return TestDoubleSplitOnAxis(offset: Vector3Int.zero, relativeZ: Axis.Y, 2, 2);

        yield return TestDoubleSplitOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Y, 1, 1);
        yield return TestDoubleSplitOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Y, 1, 2);
        yield return TestDoubleSplitOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Y, 2, 1);
        yield return TestDoubleSplitOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Y, 2, 2);


        yield return TestDoubleSplitOnAxis(offset: Vector3Int.zero, relativeZ: Axis.X, 1, 1);
        yield return TestDoubleSplitOnAxis(offset: Vector3Int.zero, relativeZ: Axis.X, 1, 2);
        yield return TestDoubleSplitOnAxis(offset: Vector3Int.zero, relativeZ: Axis.X, 2, 1);
        yield return TestDoubleSplitOnAxis(offset: Vector3Int.zero, relativeZ: Axis.X, 2, 2);

        yield return TestDoubleSplitOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.X, 1, 1);
        yield return TestDoubleSplitOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.X, 1, 2);
        yield return TestDoubleSplitOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.X, 2, 1);
        yield return TestDoubleSplitOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.X, 2, 2);
    }

    private static IEnumerator TestDoubleSplitOnAxis(Vector3Int offset, Axis relativeZ, int relXIndex, int relYIndex) {
        Utils.GetOtherAxes(relativeZ, out Axis relativeX, out Axis relativeY);

        Vector3Int dimensions = new Vector3Int(4, 5, 6);
        int width  = dimensions.Get(relativeX);
        int height = dimensions.Get(relativeY);
        int depth  = dimensions.Get(relativeZ);


        if(relXIndex == 0 || relYIndex == 0 || relXIndex == width - 1 || relYIndex == height - 1) {
            throw new NotImplementedException();
        }

        Octree<bool> clusterToManipulateNext = null;

        {
            yield return TestFindClusters(offset, dimensions, ManipulateVoxels, ValidateResults);

            void ManipulateVoxels(Octree<bool> voxelMap, Queue<Vector3Int> dirtyVoxels) {
                for(int i = 0; i < height; i++) {
                    FireBeam(relXIndex, i, relativeX, relativeY, relativeZ, voxelMap, dirtyVoxels);
                }
            }

            void ValidateResults(List<Octree<bool>> foundClusters) {
                Assert.AreEqual(2, foundClusters.Count);

                Octree<bool> cluster_0 = foundClusters[0];
                Assert.AreEqual(width - 1 - relXIndex, cluster_0.Dimensions.Get(relativeX));
                Assert.AreEqual(height,                cluster_0.Dimensions.Get(relativeY));
                Assert.AreEqual(depth,                 cluster_0.Dimensions.Get(relativeZ));

                Octree<bool> cluster_1 = foundClusters[1];
                Assert.AreEqual(relXIndex,  cluster_1.Dimensions.Get(relativeX));
                Assert.AreEqual(height,     cluster_1.Dimensions.Get(relativeY));
                Assert.AreEqual(depth,      cluster_1.Dimensions.Get(relativeZ));

                clusterToManipulateNext = cluster_0.Dimensions.Get(relativeX) < cluster_1.Dimensions.Get(relativeX) ? cluster_0 : cluster_1;
            }
        }

        {
            yield return TestFindClusters(clusterToManipulateNext, ManipulateVoxels, ValidateResults);

            void ManipulateVoxels(Octree<bool> voxelMap, Queue<Vector3Int> dirtyVoxels) {
                for(int i = 0; i < height; i++) {
                    FireBeam(i, relYIndex, relativeX, relativeY, relativeZ, voxelMap, dirtyVoxels);
                }
            }

            void ValidateResults(List<Octree<bool>> foundClusters) {
                Assert.AreEqual(2, foundClusters.Count);

                bool isZeroSmaller = foundClusters[0].Dimensions.Get(relativeY) < foundClusters[1].Dimensions.Get(relativeY);
                Octree<bool> smallCluster = isZeroSmaller ? foundClusters[0] : foundClusters[1];
                Octree<bool> largeCluster = isZeroSmaller ? foundClusters[1] : foundClusters[0];

                int expectedWidth = relXIndex < width / 2 ? relXIndex : width - 1 - relXIndex;

                Assert.AreEqual(expectedWidth, smallCluster.Dimensions.Get(relativeX));
                Assert.AreEqual(1,             smallCluster.Dimensions.Get(relativeY));
                Assert.AreEqual(4,             smallCluster.Dimensions.Get(relativeZ));

                Assert.AreEqual(expectedWidth, largeCluster.Dimensions.Get(relativeX));
                Assert.AreEqual(2,             largeCluster.Dimensions.Get(relativeY));
                Assert.AreEqual(4,             largeCluster.Dimensions.Get(relativeZ));
            }
        }
    }

    [UnityTest]
    public IEnumerator TestTryFindCluster_Diagonal() {
        yield return TestDiagonalOnAxis(offset: Vector3Int.zero, relativeZ: Axis.Z);
        yield return TestDiagonalOnAxis(offset: Vector3Int.zero, relativeZ: Axis.Y);
        yield return TestDiagonalOnAxis(offset: Vector3Int.zero, relativeZ: Axis.X);

        yield return TestDiagonalOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Z);
        yield return TestDiagonalOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.Y);
        yield return TestDiagonalOnAxis(offset: new Vector3Int(1, 2, 3), relativeZ: Axis.X);
    }

    private static IEnumerator TestDiagonalOnAxis(Vector3Int offset, Axis relativeZ) {
        Utils.GetOtherAxes(relativeZ, out Axis relativeX, out Axis relativeY);

        Vector3Int dimensions = new Vector3Int(6, 6, 6);
        int width  = dimensions.Get(relativeX);
        int height = dimensions.Get(relativeY);
        int depth  = dimensions.Get(relativeZ);

        int relXMid = width / 2;
        int relYMid = height / 2;

        Octree<bool> clusterToManipulateNext = null;

        {
            yield return TestFindClusters(offset, dimensions, ManipulateVoxels, ValidateResults);

            void ManipulateVoxels(Octree<bool> voxelMap, Queue<Vector3Int> dirtyVoxels) {
                for(int relYIndex = 0; relYIndex < height; relYIndex++) {
                    for(int relXIndex = 0; relXIndex < width; relXIndex++) {
                        if(relXIndex < relXMid || relYIndex >= relYMid) {
                            continue;
                        }

                        FireBeam(relXIndex, relYIndex, relativeX, relativeY, relativeZ, voxelMap, dirtyVoxels);
                    }
                }
            }

            void ValidateResults(List<Octree<bool>> foundClusters) {
                Assert.AreEqual(1, foundClusters.Count);

                Octree<bool> cluster = foundClusters[0];

                Assert.AreEqual(width, cluster.Dimensions.Get(relativeX));
                Assert.AreEqual(height, cluster.Dimensions.Get(relativeY));
                Assert.AreEqual(depth, cluster.Dimensions.Get(relativeZ));

                for(int relZIndex = 0; relZIndex < depth; relZIndex++) {
                    for(int relYIndex = 0; relYIndex < height; relYIndex++) {
                        for(int relXIndex = 0; relXIndex < width; relXIndex++) {
                            if(relXIndex >= relXMid && relYIndex < relYMid) {
                                continue;
                            }

                            Vector3Int pos = new Vector3Int();
                            pos = pos.Set(relativeX, relXIndex);
                            pos = pos.Set(relativeY, relYIndex);
                            pos = pos.Set(relativeZ, relZIndex);

                            Assert.IsTrue(cluster.TryGetValue(pos, out bool value));
                            Assert.IsTrue(value);
                        }
                    }
                }

                Debug.Log(cluster);
                clusterToManipulateNext = cluster;
            }
        }

        Assert.NotNull(clusterToManipulateNext);

        {
            yield return TestFindClusters(clusterToManipulateNext, ManipulateVoxels, ValidateResults);

            void ManipulateVoxels(Octree<bool> voxelMap, Queue<Vector3Int> dirtyVoxels) {
                for(int relYIndex = 0; relYIndex < height; relYIndex++) {
                    for(int relXIndex = 0; relXIndex < width; relXIndex++) {
                        if(relXIndex >= relXMid || relYIndex < relYMid) {
                            continue;
                        }

                        FireBeam(relXIndex, relYIndex, relativeX, relativeY, relativeZ, voxelMap, dirtyVoxels);
                    }
                }
            }

            void ValidateResults(List<Octree<bool>> foundClusters) {
                Assert.AreEqual(2, foundClusters.Count);

                Octree<bool> cluster_0 = foundClusters[0];
                Assert.AreEqual(relXMid, cluster_0.Dimensions.Get(relativeX));
                Assert.AreEqual(relYMid, cluster_0.Dimensions.Get(relativeY));
                Assert.AreEqual(depth,   cluster_0.Dimensions.Get(relativeZ));

                for(int z = 0; z < cluster_0.Dimensions.z; z++) {
                    for(int y = 0; y < cluster_0.Dimensions.y; y++) {
                        for(int x = 0; x < cluster_0.Dimensions.x; x++) {
                            Assert.IsTrue(cluster_0.TryGetValue(new Vector3Int(x, y, z), out bool value));
                            Assert.IsTrue(value);
                        }
                    }
                }

                Octree<bool> cluster_1 = foundClusters[1];
                Assert.AreEqual(relXMid, cluster_1.Dimensions.Get(relativeX));
                Assert.AreEqual(relYMid, cluster_1.Dimensions.Get(relativeY));
                Assert.AreEqual(depth,   cluster_1.Dimensions.Get(relativeZ));

                for(int z = 0; z < cluster_1.Dimensions.z; z++) {
                    for(int y = 0; y < cluster_1.Dimensions.y; y++) {
                        for(int x = 0; x < cluster_1.Dimensions.x; x++) {
                            Assert.IsTrue(cluster_1.TryGetValue(new Vector3Int(x, y, z), out bool value));
                            Assert.IsTrue(value);
                        }
                    }
                }
            }
        }
    }

    [UnityTest]
    public IEnumerator TestTryFindCluster_DisappearingVoxelBug() {
        //yield return TestDisappearingVoxelBug(relativeZ: Axis.Z);
        yield return TestDisappearingVoxelBug(relativeZ: Axis.Y);
        //yield return TestDisappearingVoxelBug(relativeZ: Axis.X);
    }

    private static IEnumerator TestDisappearingVoxelBug(Axis relativeZ) {
        Utils.GetOtherAxes(relativeZ, out Axis relativeX, out Axis relativeY);

        Vector3Int dimensions = new Vector3Int(4, 8, 4);
        dimensions = dimensions.Set(relativeX, 4);
        dimensions = dimensions.Set(relativeY, 8);
        dimensions = dimensions.Set(relativeZ, 4);

        int width = dimensions.Get(relativeX);
        int height = dimensions.Get(relativeY);
        int depth = dimensions.Get(relativeZ);

        Octree<bool> clusterToManipulateNext = null;

        {
            yield return TestFindClusters(Vector3Int.zero, dimensions, ManipulateVoxels, ValidateResults);

            void ManipulateVoxels(Octree<bool> voxelMap, Queue<Vector3Int> dirtyVoxels) {
                FireBeam(0, 2, relativeX, relativeY, relativeZ, voxelMap, dirtyVoxels);
                FireBeam(1, 2, relativeX, relativeY, relativeZ, voxelMap, dirtyVoxels);
                FireBeam(2, 1, relativeX, relativeY, relativeZ, voxelMap, dirtyVoxels);
                FireBeam(3, 1, relativeX, relativeY, relativeZ, voxelMap, dirtyVoxels);
            }

            void ValidateResults(List<Octree<bool>> foundClusters) {
                Assert.AreEqual(2, foundClusters.Count);

                Octree<bool> cluster_0 = foundClusters[0];
                Assert.AreEqual(width,      cluster_0.Dimensions.Get(relativeX));
                Assert.AreEqual(6,          cluster_0.Dimensions.Get(relativeY));
                Assert.AreEqual(depth,      cluster_0.Dimensions.Get(relativeZ));

                Octree<bool> cluster_1 = foundClusters[1];
                Assert.AreEqual(width, cluster_1.Dimensions.Get(relativeX));
                Assert.AreEqual(2, cluster_1.Dimensions.Get(relativeY));
                Assert.AreEqual(depth, cluster_1.Dimensions.Get(relativeZ));


                clusterToManipulateNext = cluster_0;
            }
        }

        Assert.NotNull(clusterToManipulateNext);

        {
            yield return TestFindClusters(clusterToManipulateNext, ManipulateVoxels, ValidateResults);

            void ManipulateVoxels(Octree<bool> voxelMap, Queue<Vector3Int> dirtyVoxels) {
                FireBeam(2, 1, relativeX, relativeY, relativeZ, voxelMap, dirtyVoxels);
            }

            void ValidateResults(List<Octree<bool>> foundClusters) {
                Assert.AreEqual(1, foundClusters.Count);

                Octree<bool> cluster = foundClusters[0];
                Assert.AreEqual(width, cluster.Dimensions.Get(relativeX));
                Assert.AreEqual(6, cluster.Dimensions.Get(relativeY));
                Assert.AreEqual(depth, cluster.Dimensions.Get(relativeZ));

                for(int relZ = 0; relZ < 4; relZ++) {
                    for(int relY = 0; relY < 6; relY++) {
                        for(int relX = 0; relX < 4; relX++) {
                            Vector3Int pos = new Vector3Int();
                            pos = pos.Set(relativeX, relX);
                            pos = pos.Set(relativeY, relY);
                            pos = pos.Set(relativeZ, relZ);

                            if(relX < 2 && relY == 0 || relX == 2 && relY == 1) {
                                Assert.IsTrue(cluster.TryGetValue(pos, out bool value));
                                Assert.IsFalse(value);
                            }
                            else {
                                Assert.IsTrue(cluster.TryGetValue(pos, out bool value));
                                Assert.IsTrue(value);
                            }
                        }
                    }
                }
            }
        }
    }

    private static IEnumerator TestFindClusters(Vector3Int offset, Vector3Int dimensions, Action<Octree<bool>, Queue<Vector3Int>> manipulateVoxelsCallback, Action<List<Octree<bool>>> validateResultsCallback) {
        Octree<bool> voxelMap = new Octree<bool>(offset, dimensions, startValue: true);

        yield return TestFindClusters(voxelMap, manipulateVoxelsCallback, validateResultsCallback);
    }

    private static IEnumerator TestFindClusters(Octree<bool> voxelMap, Action<Octree<bool>, Queue<Vector3Int>> manipulateVoxelsCallback, Action<List<Octree<bool>>> validateResultsCallback) {
        Queue<Vector3Int> dirtyVoxels = new Queue<Vector3Int>();

        manipulateVoxelsCallback(voxelMap, dirtyVoxels);

        yield return VoxelClusterHandler.FindClusters(voxelMap, dirtyVoxels, debug: true, STEP_DURATION, onFinished: (List<Octree<bool>> result) => {
            validateResultsCallback(result);
        });
    }

    static void FireBeam(int relXIndex, int relYIndex, Axis relativeX, Axis relativeY, Axis relativeZ, Octree<bool> voxelMap, Queue<Vector3Int> dirtyVoxels) {
        for(int i = 0; i < voxelMap.Dimensions.Get(relativeZ); i++) {
            Vector3Int pos = new Vector3Int();

            pos = pos.Set(relativeX, relXIndex);
            pos = pos.Set(relativeY, relYIndex);
            pos = pos.Set(relativeZ, i);

            VoxelGrid.TryRemoveVoxel(pos, voxelMap, dirtyVoxels);
        }
    }
}
