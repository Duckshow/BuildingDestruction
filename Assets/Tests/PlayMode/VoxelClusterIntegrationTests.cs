using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

using Assert = NUnit.Framework.Assert;

public class VoxelClusterIntegrationTests {

    private const float STEP_DURATION = 0.1f;

    [UnityTest]
    public IEnumerator TestSplitAlongZ() {
        const int WIDTH = 4;
        const int HEIGHT = 3;
        const int DEPTH = 2;

        VoxelCluster originalCluster = new VoxelCluster(new Vector3Int(WIDTH, HEIGHT, DEPTH), voxelBlockStartValue: byte.MaxValue);

        Vector3Int dimensions = new Vector3Int(WIDTH, HEIGHT, DEPTH);
        Bin CreateBin(int index) { return new Bin(index, dimensions, byte.MaxValue); }
        Bin[] originalVoxelBlocks = new Bin[WIDTH * HEIGHT * DEPTH] {

            // =========== z == 0 ===========

            CreateBin(0),   CreateBin(1),   CreateBin(2),   CreateBin(3),
            CreateBin(4),   CreateBin(5),   CreateBin(6),   CreateBin(7),
            CreateBin(8),   CreateBin(9),   CreateBin(10),  CreateBin(11),

            // =========== z == 1 ===========

            CreateBin(12),  CreateBin(13),  CreateBin(14),  CreateBin(15),
            CreateBin(16),  CreateBin(17),  CreateBin(18),  CreateBin(19),
            CreateBin(20),  CreateBin(21),  CreateBin(22),  CreateBin(23)

        };

        int[] voxelsToDelete = new int[] {

            // =========== z == 0 ===========

            Utils.GetVoxelIndex(0, 0, dimensions),
          //Utils.GetVoxelIndex(0, 1, dimensions),
            Utils.GetVoxelIndex(0, 2, dimensions),
          //Utils.GetVoxelIndex(0, 3, dimensions),
            Utils.GetVoxelIndex(0, 4, dimensions),
          //Utils.GetVoxelIndex(0, 5, dimensions),
            Utils.GetVoxelIndex(0, 6, dimensions),
          //Utils.GetVoxelIndex(0, 7, dimensions),

            Utils.GetVoxelIndex(4, 0, dimensions),
          //Utils.GetVoxelIndex(4, 1, dimensions),
            Utils.GetVoxelIndex(4, 2, dimensions),
          //Utils.GetVoxelIndex(4, 3, dimensions),
            Utils.GetVoxelIndex(4, 4, dimensions),
          //Utils.GetVoxelIndex(4, 5, dimensions),
            Utils.GetVoxelIndex(4, 6, dimensions),
          //Utils.GetVoxelIndex(4, 7, dimensions),

            Utils.GetVoxelIndex(8, 0, dimensions),
          //Utils.GetVoxelIndex(8, 1, dimensions),
            Utils.GetVoxelIndex(8, 2, dimensions),
          //Utils.GetVoxelIndex(8, 3, dimensions),
            Utils.GetVoxelIndex(8, 4, dimensions),
          //Utils.GetVoxelIndex(8, 5, dimensions),
            Utils.GetVoxelIndex(8, 6, dimensions),
          //Utils.GetVoxelIndex(8, 7, dimensions),

            // =========== z == 1 ===========

            Utils.GetVoxelIndex(12, 0, dimensions),
          //Utils.GetVoxelIndex(12, 1, dimensions),
            Utils.GetVoxelIndex(12, 2, dimensions),
          //Utils.GetVoxelIndex(12, 3, dimensions),
            Utils.GetVoxelIndex(12, 4, dimensions),
          //Utils.GetVoxelIndex(12, 5, dimensions),
            Utils.GetVoxelIndex(12, 6, dimensions),
          //Utils.GetVoxelIndex(12, 7, dimensions),

            Utils.GetVoxelIndex(16, 0, dimensions),
          //Utils.GetVoxelIndex(16, 1, dimensions),
            Utils.GetVoxelIndex(16, 2, dimensions),
          //Utils.GetVoxelIndex(16, 3, dimensions),
            Utils.GetVoxelIndex(16, 4, dimensions),
          //Utils.GetVoxelIndex(16, 5, dimensions),
            Utils.GetVoxelIndex(16, 6, dimensions),
          //Utils.GetVoxelIndex(16, 7, dimensions),

            Utils.GetVoxelIndex(20, 0, dimensions),
          //Utils.GetVoxelIndex(20, 1, dimensions),
            Utils.GetVoxelIndex(20, 2, dimensions),
          //Utils.GetVoxelIndex(20, 3, dimensions),
            Utils.GetVoxelIndex(20, 4, dimensions),
          //Utils.GetVoxelIndex(20, 5, dimensions),
            Utils.GetVoxelIndex(20, 6, dimensions),
          //Utils.GetVoxelIndex(20, 7, dimensions),
        };

        FauxVoxelClusterUpdaterUser user = new FauxVoxelClusterUpdaterUser(Vector3Int.zero, new Vector3Int(WIDTH, HEIGHT, DEPTH),
            onReceivedUpdateRequest: () => { },
            onUpdateStart: () => { return originalVoxelBlocks; },
            onUpdateFinish: ValidateResults
        );

        yield return VoxelClusterUpdater.RemoveVoxelsInCluster(user, voxelsToDelete.ToQueue(), STEP_DURATION);

        void ValidateResults(List<VoxelCluster> foundClusters) {
            Assert.AreEqual(1, foundClusters.Count);
            
            AssertClusterLooksCorrect(
                foundClusters[0], 
                expectedOffset: new Vector3Int(1, 0, 0),
                expectedDimensions: new Vector3Int(WIDTH, HEIGHT, DEPTH),
                shouldVoxelBlockBeExterior: (Vector3Int coords) => { return true; },
                shouldExteriorVoxelExist: (Vector3Int coords) => { return coords.x > 0; }
            );
        }
    }

    [UnityTest]
    public IEnumerator TestDoubleSplitAlongZ() {
        const int WIDTH = 4;
        const int HEIGHT = 3;
        const int DEPTH = 2;

        FauxVoxelClusterUpdaterUser user;
        VoxelCluster mainCluster = null;

        VoxelCluster originalCluster = new VoxelCluster(new Vector3Int(WIDTH, HEIGHT, DEPTH), voxelBlockStartValue: byte.MaxValue);
        Vector3Int dimensions = new Vector3Int(WIDTH, HEIGHT, DEPTH);
        Bin CreateBin(int index) { return new Bin(index, dimensions, byte.MaxValue); }
        Bin[] voxelBlocks = new Bin[WIDTH * HEIGHT * DEPTH] {

            // =========== z == 0 ===========

            CreateBin(0),   CreateBin(1),   CreateBin(2),   CreateBin(3),
            CreateBin(4),   CreateBin(5),   CreateBin(6),   CreateBin(7),
            CreateBin(8),   CreateBin(9),   CreateBin(10),  CreateBin(11),

            // =========== z == 1 ===========

            CreateBin(12),  CreateBin(13),  CreateBin(14),  CreateBin(15),
            CreateBin(16),  CreateBin(17),  CreateBin(18),  CreateBin(19),
            CreateBin(20),  CreateBin(21),  CreateBin(22),  CreateBin(23)

        };

        int[] firstVoxelsToDelete = new int[] {

            // =========== z == 0 ===========

            Utils.GetVoxelIndex(1, 0, dimensions),
          //Utils.GetVoxelIndex(1, 1, dimensions),
            Utils.GetVoxelIndex(1, 2, dimensions),
          //Utils.GetVoxelIndex(1, 3, dimensions),
            Utils.GetVoxelIndex(1, 4, dimensions),
          //Utils.GetVoxelIndex(1, 5, dimensions),
            Utils.GetVoxelIndex(1, 6, dimensions),
          //Utils.GetVoxelIndex(1, 7, dimensions),

            Utils.GetVoxelIndex(5, 0, dimensions),
          //Utils.GetVoxelIndex(5, 1, dimensions),
            Utils.GetVoxelIndex(5, 2, dimensions),
          //Utils.GetVoxelIndex(5, 3, dimensions),
            Utils.GetVoxelIndex(5, 4, dimensions),
          //Utils.GetVoxelIndex(5, 5, dimensions),
            Utils.GetVoxelIndex(5, 6, dimensions),
          //Utils.GetVoxelIndex(5, 7, dimensions),

            Utils.GetVoxelIndex(9, 0, dimensions),
          //Utils.GetVoxelIndex(9, 1, dimensions),
            Utils.GetVoxelIndex(9, 2, dimensions),
          //Utils.GetVoxelIndex(9, 3, dimensions),
            Utils.GetVoxelIndex(9, 4, dimensions),
          //Utils.GetVoxelIndex(9, 5, dimensions),
            Utils.GetVoxelIndex(9, 6, dimensions),
          //Utils.GetVoxelIndex(9, 7, dimensions),

            // =========== z == 1 ===========

            Utils.GetVoxelIndex(13, 0, dimensions),
          //Utils.GetVoxelIndex(13, 1, dimensions),
            Utils.GetVoxelIndex(13, 2, dimensions),
          //Utils.GetVoxelIndex(13, 3, dimensions),
            Utils.GetVoxelIndex(13, 4, dimensions),
          //Utils.GetVoxelIndex(13, 5, dimensions),
            Utils.GetVoxelIndex(13, 6, dimensions),
          //Utils.GetVoxelIndex(13, 7, dimensions),

            Utils.GetVoxelIndex(17, 0, dimensions),
          //Utils.GetVoxelIndex(17, 1, dimensions),
            Utils.GetVoxelIndex(17, 2, dimensions),
          //Utils.GetVoxelIndex(17, 3, dimensions),
            Utils.GetVoxelIndex(17, 4, dimensions),
          //Utils.GetVoxelIndex(17, 5, dimensions),
            Utils.GetVoxelIndex(17, 6, dimensions),
          //Utils.GetVoxelIndex(17, 7, dimensions),

            Utils.GetVoxelIndex(21, 0, dimensions),
          //Utils.GetVoxelIndex(21, 1, dimensions),
            Utils.GetVoxelIndex(21, 2, dimensions),
          //Utils.GetVoxelIndex(21, 3, dimensions),
            Utils.GetVoxelIndex(21, 4, dimensions),
          //Utils.GetVoxelIndex(21, 5, dimensions),
            Utils.GetVoxelIndex(21, 6, dimensions),
          //Utils.GetVoxelIndex(21, 7, dimensions),
        };

        int[] secondVoxelsToDelete = new int[] { // NOTE: indexes are relative to future cluster

            // =========== z == 0 ===========

            Utils.GetVoxelIndex(1, 0, dimensions),
            Utils.GetVoxelIndex(1, 1, dimensions),
          //Utils.GetVoxelIndex(1, 2, dimensions),
          //Utils.GetVoxelIndex(1, 3, dimensions),
            Utils.GetVoxelIndex(1, 4, dimensions),
            Utils.GetVoxelIndex(1, 5, dimensions),
          //Utils.GetVoxelIndex(1, 6, dimensions),
          //Utils.GetVoxelIndex(1, 7, dimensions),

            // =========== z == 1 ===========

            Utils.GetVoxelIndex(4, 0, dimensions),
            Utils.GetVoxelIndex(4, 1, dimensions),
          //Utils.GetVoxelIndex(4, 2, dimensions),
          //Utils.GetVoxelIndex(4, 3, dimensions),
            Utils.GetVoxelIndex(4, 4, dimensions),
            Utils.GetVoxelIndex(4, 5, dimensions),
          //Utils.GetVoxelIndex(4, 6, dimensions),
          //Utils.GetVoxelIndex(4, 7, dimensions),
        };

        // ========== First split ==========

        user = new FauxVoxelClusterUpdaterUser(
            offset: Vector3Int.zero, 
            dimensions: new Vector3Int(WIDTH, HEIGHT, DEPTH),
            onReceivedUpdateRequest: () => { },
            onUpdateStart: () => { return voxelBlocks; },
            onUpdateFinish: ValidateFirstSplitResults
        );

        yield return VoxelClusterUpdater.RemoveVoxelsInCluster(user, firstVoxelsToDelete.ToQueue(), STEP_DURATION);

        void ValidateFirstSplitResults(List<VoxelCluster> foundClusters) {
            Assert.AreEqual(2, foundClusters.Count);

            if(foundClusters[1].VoxelOffset.x < foundClusters[0].VoxelOffset.x) {
                foundClusters.Reverse();
            }

            AssertClusterLooksCorrect(
                foundClusters[0],
                expectedOffset: new Vector3Int(0, 0, 0),
                expectedDimensions: new Vector3Int(1, HEIGHT, DEPTH),
                shouldVoxelBlockBeExterior: (Vector3Int coords) => { return true; },
                shouldExteriorVoxelExist: (Vector3Int coords) => { return true; }
            );

            AssertClusterLooksCorrect(
                foundClusters[1],
                expectedOffset: new Vector3Int(2, 0, 0),
                expectedDimensions: new Vector3Int(3, HEIGHT, DEPTH),
                shouldVoxelBlockBeExterior: (Vector3Int coords) => { return true; },
                shouldExteriorVoxelExist: (Vector3Int coords) => { return coords.x > 0; }
            );

            mainCluster = foundClusters[0];
        }

        // ========== Second split ==========

        user = new FauxVoxelClusterUpdaterUser(
            offset: mainCluster.VoxelOffset, 
            dimensions: mainCluster.Dimensions,
            onReceivedUpdateRequest: () => { },
            onUpdateStart: () => { return GetClusterVoxelBlocksAsArray(mainCluster); },
            onUpdateFinish: ValidateSecondSplitResults
        );

        yield return VoxelClusterUpdater.RemoveVoxelsInCluster(user, secondVoxelsToDelete.ToQueue(), STEP_DURATION);

        void ValidateSecondSplitResults(List<VoxelCluster> foundClusters) {
            Assert.AreEqual(2, foundClusters.Count);
            
            if(foundClusters[1].VoxelOffset.y < foundClusters[0].VoxelOffset.y) {
                foundClusters.Reverse();
            }

            AssertClusterLooksCorrect(
                foundClusters[0],
                expectedOffset: new Vector3Int(0, 0, 0),
                expectedDimensions: new Vector3Int(1, 1, DEPTH),
                shouldVoxelBlockBeExterior: (Vector3Int coords) => { return true; },
                shouldExteriorVoxelExist: (Vector3Int coords) => { return true; }
            );

            AssertClusterLooksCorrect(
                foundClusters[1],
                expectedOffset: new Vector3Int(0, 4, 0),
                expectedDimensions: new Vector3Int(1, 1, DEPTH),
                shouldVoxelBlockBeExterior: (Vector3Int coords) => { return true; },
                shouldExteriorVoxelExist: (Vector3Int coords) => { return true; }
            );

        }
    }

    [UnityTest]
    public static IEnumerator TestTryFindCluster_DiagonalAlongZ() {
        const int WIDTH = 4;
        const int HEIGHT = 3;
        const int DEPTH = 2;

        FauxVoxelClusterUpdaterUser user;
        VoxelCluster clusterToSplitAgain = null;

        VoxelCluster originalCluster = new VoxelCluster(new Vector3Int(WIDTH, HEIGHT, DEPTH), voxelBlockStartValue: byte.MaxValue);
        Vector3Int dimensions = new Vector3Int(WIDTH, HEIGHT, DEPTH);
        Bin CreateBin(int index) { return new Bin(index, dimensions, byte.MaxValue); }
        Bin[] voxelBlocks = new Bin[WIDTH * HEIGHT * DEPTH] {

            // =========== z == 0 ===========
            
            CreateBin(0),   CreateBin(1),   CreateBin(2),   CreateBin(3),
            CreateBin(4),   CreateBin(5),   CreateBin(6),   CreateBin(7),
            CreateBin(8),   CreateBin(9),   CreateBin(10),  CreateBin(11),

            // =========== z == 1 ===========

            CreateBin(12),  CreateBin(13),  CreateBin(14),  CreateBin(15),
            CreateBin(16),  CreateBin(17),  CreateBin(18),  CreateBin(19),
            CreateBin(20),  CreateBin(21),  CreateBin(22),  CreateBin(23)

        };

        int[] firstVoxelsToDelete = new int[] {
            
            // =========== z == 0 ===========
            
            Utils.GetVoxelIndex(2, 0, dimensions),
          //Utils.GetVoxelIndex(2, 1, dimensions),
            Utils.GetVoxelIndex(2, 2, dimensions),
          //Utils.GetVoxelIndex(2, 3, dimensions),
            Utils.GetVoxelIndex(2, 4, dimensions),
          //Utils.GetVoxelIndex(2, 5, dimensions),
            Utils.GetVoxelIndex(2, 6, dimensions),
          //Utils.GetVoxelIndex(2, 7, dimensions),

          /*Utils.GetVoxelIndex(6, 0, dimensions),*/    Utils.GetVoxelIndex(7, 0, dimensions),
            Utils.GetVoxelIndex(6, 1, dimensions),      Utils.GetVoxelIndex(7, 1, dimensions),
            Utils.GetVoxelIndex(6, 2, dimensions),    /*Utils.GetVoxelIndex(7, 2, dimensions),*/
          /*Utils.GetVoxelIndex(6, 3, dimensions),*/  /*Utils.GetVoxelIndex(7, 3, dimensions),*/
          /*Utils.GetVoxelIndex(6, 4, dimensions),*/    Utils.GetVoxelIndex(7, 4, dimensions),
            Utils.GetVoxelIndex(6, 5, dimensions),      Utils.GetVoxelIndex(7, 5, dimensions),
            Utils.GetVoxelIndex(6, 6, dimensions),    /*Utils.GetVoxelIndex(7, 6, dimensions),*/
          /*Utils.GetVoxelIndex(6, 7, dimensions),*/  /*Utils.GetVoxelIndex(7, 7, dimensions),*/

            // =========== z == 1 ===========

            Utils.GetVoxelIndex(14, 0, dimensions),
          //Utils.GetVoxelIndex(14, 1, dimensions),
            Utils.GetVoxelIndex(14, 2, dimensions),
          //Utils.GetVoxelIndex(14, 3, dimensions),
            Utils.GetVoxelIndex(14, 4, dimensions),
          //Utils.GetVoxelIndex(14, 5, dimensions),
            Utils.GetVoxelIndex(14, 6, dimensions),
          //Utils.GetVoxelIndex(14, 7, dimensions),

          /*Utils.GetVoxelIndex(18, 0, dimensions),*/    Utils.GetVoxelIndex(19, 0, dimensions),
            Utils.GetVoxelIndex(18, 1, dimensions),      Utils.GetVoxelIndex(19, 1, dimensions),
            Utils.GetVoxelIndex(18, 2, dimensions),    /*Utils.GetVoxelIndex(19, 2, dimensions),*/
          /*Utils.GetVoxelIndex(18, 3, dimensions),*/  /*Utils.GetVoxelIndex(19, 3, dimensions),*/
          /*Utils.GetVoxelIndex(18, 4, dimensions),*/    Utils.GetVoxelIndex(19, 4, dimensions),
            Utils.GetVoxelIndex(18, 5, dimensions),      Utils.GetVoxelIndex(19, 5, dimensions),
            Utils.GetVoxelIndex(18, 6, dimensions),    /*Utils.GetVoxelIndex(19, 6, dimensions),*/
          /*Utils.GetVoxelIndex(18, 7, dimensions),*/  /*Utils.GetVoxelIndex(19, 7, dimensions),*/
        };

        int[] secondVoxelsToDelete = new int[] { // NOTE: indexes are relative to future cluster
            
            // =========== z == 0 ===========

            Utils.GetVoxelIndex(6, 0, dimensions),
          /*Utils.GetVoxelIndex(6, 1, dimensions),*/  
          /*Utils.GetVoxelIndex(6, 2, dimensions),*/  
          /*Utils.GetVoxelIndex(6, 3, dimensions),*/
            Utils.GetVoxelIndex(6, 4, dimensions),
          /*Utils.GetVoxelIndex(6, 5, dimensions),*/
          /*Utils.GetVoxelIndex(6, 6, dimensions),*/
          /*Utils.GetVoxelIndex(6, 7, dimensions)*/

            // =========== z == 1 ===========

            Utils.GetVoxelIndex(18, 0, dimensions),
          /*Utils.GetVoxelIndex(18, 1, dimensions),*/  
          /*Utils.GetVoxelIndex(18, 2, dimensions),*/  
          /*Utils.GetVoxelIndex(18, 3, dimensions),*/
            Utils.GetVoxelIndex(18, 4, dimensions),
          /*Utils.GetVoxelIndex(18, 5, dimensions),*/
          /*Utils.GetVoxelIndex(18, 6, dimensions),*/
          /*Utils.GetVoxelIndex(18, 7, dimensions)*/
        };

        // ========== First split ==========

        user = new FauxVoxelClusterUpdaterUser(
            offset: Vector3Int.zero, 
            dimensions: new Vector3Int(WIDTH, HEIGHT, DEPTH),
            onReceivedUpdateRequest: () => { },
            onUpdateStart: () => { return voxelBlocks; },
            onUpdateFinish: ValidateFirstSplitResults
        );

        yield return VoxelClusterUpdater.RemoveVoxelsInCluster(user, firstVoxelsToDelete.ToQueue(), STEP_DURATION);

        void ValidateFirstSplitResults(List<VoxelCluster> foundClusters) {
            Assert.AreEqual(1, foundClusters.Count);

            AssertClusterLooksCorrect(
                foundClusters[0],
                expectedOffset: new Vector3Int(0, 0, 0),
                expectedDimensions: new Vector3Int(WIDTH, HEIGHT, DEPTH),
                shouldVoxelBlockBeExterior: (Vector3Int coords) => { return true; },
                shouldExteriorVoxelExist: (Vector3Int coords) => { return !firstVoxelsToDelete.Contains(Utils.CoordsToIndex(coords, dimensions * Bin.WIDTH)); }
            );

            clusterToSplitAgain = foundClusters[0];
        }

        // ========== Second split ==========

        user = new FauxVoxelClusterUpdaterUser(
            offset: clusterToSplitAgain.VoxelOffset, 
            dimensions: clusterToSplitAgain.Dimensions,
            onReceivedUpdateRequest: () => { },
            onUpdateStart: () => { return GetClusterVoxelBlocksAsArray(clusterToSplitAgain); },
            onUpdateFinish: ValidateSecondSplitResults
        );

        yield return VoxelClusterUpdater.RemoveVoxelsInCluster(user, secondVoxelsToDelete.ToQueue(), STEP_DURATION);

        void ValidateSecondSplitResults(List<VoxelCluster> foundClusters) {
            Assert.AreEqual(2, foundClusters.Count);

            if(foundClusters[1].VoxelOffset.y < foundClusters[0].VoxelOffset.y) {
                foundClusters.Reverse();
            }

            AssertClusterLooksCorrect(
                foundClusters[0],
                expectedOffset: new Vector3Int(0, 0, 0),
                expectedDimensions: new Vector3Int(WIDTH, HEIGHT, DEPTH),
                shouldVoxelBlockBeExterior: (Vector3Int coords) => { return true; },
                shouldExteriorVoxelExist: (Vector3Int coords) => {
                    int voxelIndex = Utils.CoordsToIndex(coords, dimensions * Bin.WIDTH);
                    return !firstVoxelsToDelete.Contains(voxelIndex) && !secondVoxelsToDelete.Contains(voxelIndex); 
                }
            );

            Vector3Int expectedOffset = new Vector3Int(5, 2, 0);

            AssertClusterLooksCorrect(
                foundClusters[1],
                expectedOffset: expectedOffset,
                expectedDimensions: new Vector3Int(2, 2, DEPTH),
                shouldVoxelBlockBeExterior: (Vector3Int coords) => { return true; },
                shouldExteriorVoxelExist: (Vector3Int coords) => {
                    int oldVoxelIndex = Utils.CoordsToIndex(expectedOffset + coords, dimensions * Bin.WIDTH);
                    return !firstVoxelsToDelete.Contains(oldVoxelIndex) && !secondVoxelsToDelete.Contains(oldVoxelIndex);
                }
            );
        }
    }

    [UnityTest]
    public IEnumerator TestRefreshExterior() {
        throw new NotImplementedException();
    }

    private static void AssertClusterLooksCorrect(VoxelCluster cluster, Vector3Int expectedOffset, Vector3Int expectedDimensions, Predicate<Vector3Int> shouldVoxelBlockBeExterior, Predicate<Vector3Int> shouldExteriorVoxelExist) {
        Assert.AreEqual(expectedOffset, cluster.VoxelOffset);
        Assert.AreEqual(expectedDimensions, cluster.Dimensions);

        for(int z = 0; z < cluster.VoxelDimensions.z; ++z) {
            for(int y = 0; y < cluster.VoxelDimensions.y; ++y) {
                for(int x = 0; x < cluster.VoxelDimensions.x; ++x) {
                    Vector3Int voxelCoords = new Vector3Int(x, y, z);
                    Vector3Int voxelBlockCoords = voxelCoords / Bin.WIDTH;

                    Assert.IsTrue(cluster.TryGetVoxelBlock(voxelBlockCoords, out Bin voxelBlock));

                    int voxelBlockIndex = Utils.CoordsToIndex(voxelBlockCoords, cluster.Dimensions);
                    Assert.AreEqual(voxelBlockIndex, voxelBlock.Index);
                    Assert.AreEqual(Utils.IndexToCoords(voxelBlockIndex, cluster.Dimensions), voxelBlock.Coords);

                    Vector3Int localVoxelCoords = voxelCoords - voxelBlockCoords * Bin.WIDTH;
                    int localVoxelIndex = Utils.CoordsToIndex(localVoxelCoords, Bin.WIDTH);
                    bool doesVoxelExist = voxelBlock.GetVoxelExists(localVoxelIndex);

                    Assert.AreEqual(shouldVoxelBlockBeExterior(voxelBlockCoords), voxelBlock.IsExterior);

                    if(voxelBlock.IsExterior) {
                        Assert.AreEqual(shouldExteriorVoxelExist(voxelCoords), doesVoxelExist);
                    }
                    else {
                        Assert.AreEqual(false, doesVoxelExist);
                    }
                }
            }
        }
    }

    private static Bin[] GetClusterVoxelBlocksAsArray(VoxelCluster cluster) {
        Bin[] voxelBlocks = new Bin[cluster.Dimensions.Product()];
        for(int z = 0; z < cluster.Dimensions.z; z++) {
            for(int y = 0; y < cluster.Dimensions.y; y++) {
                for(int x = 0; x < cluster.Dimensions.x; x++) {
                    int index = Utils.CoordsToIndex(x, y, z, cluster.Dimensions);
                    Assert.IsTrue(cluster.TryGetVoxelBlock(index, out Bin voxelBlock));

                    voxelBlocks[index] = voxelBlock;
                }
            }
        }

        return voxelBlocks;
    }
}
