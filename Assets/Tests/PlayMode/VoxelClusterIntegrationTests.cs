using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

using Assert = NUnit.Framework.Assert;

public class VoxelClusterIntegrationTests {

    private const float STEP_DURATION = 0.025f;

    [UnityTest]
    public IEnumerator TestSplitAlongZ() {
        const int WIDTH = 4;
        const int HEIGHT = 3;
        const int DEPTH = 2;

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
            onUpdateStart: () => { return voxelBlocks; },
            onUpdateFinish: ValidateResults
        );

        for(int i = 0; i < voxelBlocks.Length; i++) {
            voxelBlocks[i] = Bin.RefreshConnectivity(voxelBlocks, i, dimensions);
        }

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

        Vector3Int expectedFirstSplitMainClusterDimensions = new Vector3Int(1, HEIGHT, DEPTH);

        int[] secondVoxelsToDelete = new int[] { // NOTE: indexes are relative to future cluster

            // =========== z == 0 ===========

            Utils.GetVoxelIndex(1, 0, expectedFirstSplitMainClusterDimensions),
            Utils.GetVoxelIndex(1, 1, expectedFirstSplitMainClusterDimensions),
          //Utils.GetVoxelIndex(1, 2, expectedFirstSplitMainClusterDimensions),
          //Utils.GetVoxelIndex(1, 3, expectedFirstSplitMainClusterDimensions),
            Utils.GetVoxelIndex(1, 4, expectedFirstSplitMainClusterDimensions),
            Utils.GetVoxelIndex(1, 5, expectedFirstSplitMainClusterDimensions),
          //Utils.GetVoxelIndex(1, 6, expectedFirstSplitMainClusterDimensions),
          //Utils.GetVoxelIndex(1, 7, expectedFirstSplitMainClusterDimensions),

            // =========== z == 1 ===========

            Utils.GetVoxelIndex(4, 0, new Vector3Int(1, HEIGHT, DEPTH)),
            Utils.GetVoxelIndex(4, 1, new Vector3Int(1, HEIGHT, DEPTH)),
          //Utils.GetVoxelIndex(4, 2, new Vector3Int(1, HEIGHT, DEPTH)),
          //Utils.GetVoxelIndex(4, 3, new Vector3Int(1, HEIGHT, DEPTH)),
            Utils.GetVoxelIndex(4, 4, new Vector3Int(1, HEIGHT, DEPTH)),
            Utils.GetVoxelIndex(4, 5, new Vector3Int(1, HEIGHT, DEPTH)),
          //Utils.GetVoxelIndex(4, 6, new Vector3Int(1, HEIGHT, DEPTH)),
          //Utils.GetVoxelIndex(4, 7, new Vector3Int(1, HEIGHT, DEPTH)),
        };

        // ========== First split ==========

        user = new FauxVoxelClusterUpdaterUser(
            offset: Vector3Int.zero, 
            dimensions: new Vector3Int(WIDTH, HEIGHT, DEPTH),
            onReceivedUpdateRequest: () => { },
            onUpdateStart: () => { return voxelBlocks; },
            onUpdateFinish: ValidateFirstSplitResults
        );

        for(int i = 0; i < voxelBlocks.Length; i++) {
            voxelBlocks[i] = Bin.RefreshConnectivity(voxelBlocks, i, dimensions);
        }

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
                expectedOffset: new Vector3Int(3, 0, 0),
                expectedDimensions: new Vector3Int(3, HEIGHT, DEPTH),
                shouldVoxelBlockBeExterior: (Vector3Int coords) => { return true; },
                shouldExteriorVoxelExist: (Vector3Int coords) => { return coords.x > 0; }
            );

            mainCluster = foundClusters[0];
        }

        // ========== Second split ==========

        Assert.IsNotNull(mainCluster);

        user = new FauxVoxelClusterUpdaterUser(
            offset: mainCluster.VoxelOffset, 
            dimensions: mainCluster.Dimensions,
            onReceivedUpdateRequest: () => { },
            onUpdateStart: () => { return GetClusterVoxelBlocksAsArray(mainCluster); },
            onUpdateFinish: ValidateSecondSplitResults
        );

        for(int i = 0; i < voxelBlocks.Length; i++) {
            voxelBlocks[i] = Bin.RefreshConnectivity(voxelBlocks, i, dimensions);
        }

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
                expectedOffset: new Vector3Int(0, 3, 0),
                expectedDimensions: new Vector3Int(1, 2, DEPTH),
                shouldVoxelBlockBeExterior: (Vector3Int coords) => { return true; },
                shouldExteriorVoxelExist: (Vector3Int coords) => { return coords.y != 0; }
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
            
            Utils.GetVoxelIndex(10, 0, dimensions),
          //Utils.GetVoxelIndex(10, 1, dimensions),
            Utils.GetVoxelIndex(10, 2, dimensions),
          //Utils.GetVoxelIndex(10, 3, dimensions),
            Utils.GetVoxelIndex(10, 4, dimensions),
          //Utils.GetVoxelIndex(10, 5, dimensions),
            Utils.GetVoxelIndex(10, 6, dimensions),
          //Utils.GetVoxelIndex(10, 7, dimensions),

          /*Utils.GetVoxelIndex(6, 0, dimensions),*/    Utils.GetVoxelIndex(7, 0, dimensions),
            Utils.GetVoxelIndex(6, 1, dimensions),      Utils.GetVoxelIndex(7, 1, dimensions),
            Utils.GetVoxelIndex(6, 2, dimensions),    /*Utils.GetVoxelIndex(7, 2, dimensions),*/
          /*Utils.GetVoxelIndex(6, 3, dimensions),*/  /*Utils.GetVoxelIndex(7, 3, dimensions),*/
          /*Utils.GetVoxelIndex(6, 4, dimensions),*/    Utils.GetVoxelIndex(7, 4, dimensions),
            Utils.GetVoxelIndex(6, 5, dimensions),      Utils.GetVoxelIndex(7, 5, dimensions),
            Utils.GetVoxelIndex(6, 6, dimensions),    /*Utils.GetVoxelIndex(7, 6, dimensions),*/
          /*Utils.GetVoxelIndex(6, 7, dimensions),*/  /*Utils.GetVoxelIndex(7, 7, dimensions),*/

            // =========== z == 1 ===========

            Utils.GetVoxelIndex(22, 0, dimensions),
          //Utils.GetVoxelIndex(22, 1, dimensions),
            Utils.GetVoxelIndex(22, 2, dimensions),
          //Utils.GetVoxelIndex(22, 3, dimensions),
            Utils.GetVoxelIndex(22, 4, dimensions),
          //Utils.GetVoxelIndex(22, 5, dimensions),
            Utils.GetVoxelIndex(22, 6, dimensions),
          //Utils.GetVoxelIndex(22, 7, dimensions),

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

        for(int i = 0; i < voxelBlocks.Length; i++) {
            voxelBlocks[i] = Bin.RefreshConnectivity(voxelBlocks, i, dimensions);
        }

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

        Assert.IsNotNull(clusterToSplitAgain);

        // ========== Second split ==========

        user = new FauxVoxelClusterUpdaterUser(
            offset: clusterToSplitAgain.VoxelOffset, 
            dimensions: clusterToSplitAgain.Dimensions,
            onReceivedUpdateRequest: () => { },
            onUpdateStart: () => { return GetClusterVoxelBlocksAsArray(clusterToSplitAgain); },
            onUpdateFinish: ValidateSecondSplitResults
        );

        for(int i = 0; i < voxelBlocks.Length; i++) {
            voxelBlocks[i] = Bin.RefreshConnectivity(voxelBlocks, i, dimensions);
        }

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
                shouldExteriorVoxelExist: (Vector3Int coords) => { return coords.x < 4 || coords.y < 2; }
            );

            AssertClusterLooksCorrect(
                foundClusters[1],
                expectedOffset: new Vector3Int(5, 3, 0),
                expectedDimensions: new Vector3Int(2, 2, DEPTH),
                shouldVoxelBlockBeExterior: (Vector3Int coords) => { return true; },
                shouldExteriorVoxelExist: (Vector3Int coords) => { return coords.x > 0 && coords.y > 0; }
            );
        }
    }

    [UnityTest]
    public IEnumerator TestRefreshExterior() {
        const int WIDTH = 5;
        const int HEIGHT = 6;
        const int DEPTH = 7;

        FauxVoxelClusterUpdaterUser user;

        VoxelCluster originalCluster = new VoxelCluster(new Vector3Int(WIDTH, HEIGHT, DEPTH), voxelBlockStartValue: byte.MaxValue);
        Vector3Int dimensions = new Vector3Int(WIDTH, HEIGHT, DEPTH);
        Bin CreateBin(int index) { return new Bin(index, dimensions, byte.MaxValue); }
        Bin[] voxelBlocks = new Bin[WIDTH * HEIGHT * DEPTH] {

            // =========== z == 0 ===========
            
            CreateBin(0),   CreateBin(1),   CreateBin(2),   CreateBin(3),   CreateBin(4),
            CreateBin(5),   CreateBin(6),   CreateBin(7),   CreateBin(8),   CreateBin(9),
            CreateBin(10),  CreateBin(11),  CreateBin(12),  CreateBin(13),  CreateBin(14),
            CreateBin(15),  CreateBin(16),  CreateBin(17),  CreateBin(18),  CreateBin(19),
            CreateBin(20),  CreateBin(21),  CreateBin(22),  CreateBin(23),  CreateBin(24),
            CreateBin(25),  CreateBin(26),  CreateBin(27),  CreateBin(28),  CreateBin(29),
            
            // =========== z == 1 ===========
            
            CreateBin(30),  CreateBin(31),  CreateBin(32),  CreateBin(33),  CreateBin(34),
            CreateBin(35),  CreateBin(36),  CreateBin(37),  CreateBin(38),  CreateBin(39),
            CreateBin(40),  CreateBin(41),  CreateBin(42),  CreateBin(43),  CreateBin(44),
            CreateBin(45),  CreateBin(46),  CreateBin(47),  CreateBin(48),  CreateBin(49),
            CreateBin(50),  CreateBin(51),  CreateBin(52),  CreateBin(53),  CreateBin(54),
            CreateBin(55),  CreateBin(56),  CreateBin(57),  CreateBin(58),  CreateBin(59),

            // =========== z == 3 ===========

            CreateBin(60),  CreateBin(61),  CreateBin(62),  CreateBin(63),  CreateBin(64),
            CreateBin(65),  CreateBin(66),  CreateBin(67),  CreateBin(68),  CreateBin(69),
            CreateBin(70),  CreateBin(71),  CreateBin(72),  CreateBin(73),  CreateBin(74),
            CreateBin(75),  CreateBin(76),  CreateBin(77),  CreateBin(78),  CreateBin(79),
            CreateBin(80),  CreateBin(81),  CreateBin(82),  CreateBin(83),  CreateBin(84),
            CreateBin(85),  CreateBin(86),  CreateBin(87),  CreateBin(88),  CreateBin(89),

            // =========== z == 4 ===========

            CreateBin(90),  CreateBin(91),  CreateBin(92),  CreateBin(93),  CreateBin(94),
            CreateBin(95),  CreateBin(96),  CreateBin(97),  CreateBin(98),  CreateBin(99),
            CreateBin(100), CreateBin(101), CreateBin(102), CreateBin(103), CreateBin(104),
            CreateBin(105), CreateBin(106), CreateBin(107), CreateBin(108), CreateBin(109),
            CreateBin(110), CreateBin(111), CreateBin(112), CreateBin(113), CreateBin(114),
            CreateBin(115), CreateBin(116), CreateBin(117), CreateBin(118), CreateBin(119),

            // =========== z == 5 ===========

            CreateBin(120), CreateBin(121), CreateBin(122), CreateBin(123), CreateBin(124),
            CreateBin(125), CreateBin(126), CreateBin(127), CreateBin(128), CreateBin(129),
            CreateBin(130), CreateBin(131), CreateBin(132), CreateBin(133), CreateBin(134),
            CreateBin(135), CreateBin(136), CreateBin(137), CreateBin(138), CreateBin(139),
            CreateBin(140), CreateBin(141), CreateBin(142), CreateBin(143), CreateBin(144),
            CreateBin(145), CreateBin(146), CreateBin(147), CreateBin(148), CreateBin(149),

            // =========== z == 6 ===========

            CreateBin(150), CreateBin(151), CreateBin(152), CreateBin(153), CreateBin(154),
            CreateBin(155), CreateBin(156), CreateBin(157), CreateBin(158), CreateBin(159),
            CreateBin(160), CreateBin(161), CreateBin(162), CreateBin(163), CreateBin(164),
            CreateBin(165), CreateBin(166), CreateBin(167), CreateBin(168), CreateBin(169),
            CreateBin(170), CreateBin(171), CreateBin(172), CreateBin(173), CreateBin(174),
            CreateBin(175), CreateBin(176), CreateBin(177), CreateBin(178), CreateBin(179),
            
            // =========== z == 7 ===========
            
            CreateBin(180), CreateBin(181), CreateBin(182), CreateBin(183), CreateBin(184),
            CreateBin(185), CreateBin(186), CreateBin(187), CreateBin(188), CreateBin(189),
            CreateBin(190), CreateBin(191), CreateBin(192), CreateBin(193), CreateBin(194),
            CreateBin(195), CreateBin(196), CreateBin(197), CreateBin(198), CreateBin(199),
            CreateBin(200), CreateBin(201), CreateBin(202), CreateBin(203), CreateBin(204),
            CreateBin(205), CreateBin(206), CreateBin(207), CreateBin(208), CreateBin(209)
        };

        int[] voxelsToDelete = new int[] {
            
            // =========== z == 0 ===========
            
            Utils.GetVoxelIndex(12, 0, dimensions),
          //Utils.GetVoxelIndex(12, 1, dimensions),
          //Utils.GetVoxelIndex(12, 2, dimensions),
          //Utils.GetVoxelIndex(12, 3, dimensions),
            Utils.GetVoxelIndex(12, 4, dimensions),
          //Utils.GetVoxelIndex(12, 5, dimensions),
          //Utils.GetVoxelIndex(12, 6, dimensions),
          //Utils.GetVoxelIndex(12, 7, dimensions),

            // =========== z == 1 ===========
            
            Utils.GetVoxelIndex(42, 0, dimensions),
          //Utils.GetVoxelIndex(42, 1, dimensions),
          //Utils.GetVoxelIndex(42, 2, dimensions),
          //Utils.GetVoxelIndex(42, 3, dimensions),
            Utils.GetVoxelIndex(42, 4, dimensions),
          //Utils.GetVoxelIndex(42, 5, dimensions),
          //Utils.GetVoxelIndex(42, 6, dimensions),
          //Utils.GetVoxelIndex(42, 7, dimensions),

            // =========== z == 2 ===========
            
            Utils.GetVoxelIndex(72, 0, dimensions),
          //Utils.GetVoxelIndex(72, 1, dimensions),
          //Utils.GetVoxelIndex(72, 2, dimensions),
          //Utils.GetVoxelIndex(72, 3, dimensions),
            Utils.GetVoxelIndex(72, 4, dimensions),
          //Utils.GetVoxelIndex(72, 5, dimensions),
          //Utils.GetVoxelIndex(72, 6, dimensions),
          //Utils.GetVoxelIndex(72, 7, dimensions),

            // =========== z == 3 ===========
            
            Utils.GetVoxelIndex(102, 0, dimensions),
          //Utils.GetVoxelIndex(102, 1, dimensions),
          //Utils.GetVoxelIndex(102, 2, dimensions),
          //Utils.GetVoxelIndex(102, 3, dimensions),
            Utils.GetVoxelIndex(102, 4, dimensions),
          //Utils.GetVoxelIndex(102, 5, dimensions),
          //Utils.GetVoxelIndex(102, 6, dimensions),
          //Utils.GetVoxelIndex(102, 7, dimensions),

            // =========== z == 4 ===========
            
            Utils.GetVoxelIndex(132, 0, dimensions),
          //Utils.GetVoxelIndex(132, 1, dimensions),
          //Utils.GetVoxelIndex(132, 2, dimensions),
          //Utils.GetVoxelIndex(132, 3, dimensions),
            Utils.GetVoxelIndex(132, 4, dimensions),
          //Utils.GetVoxelIndex(132, 5, dimensions),
          //Utils.GetVoxelIndex(132, 6, dimensions),
          //Utils.GetVoxelIndex(132, 7, dimensions),

            // =========== z == 5 ===========
            
            Utils.GetVoxelIndex(162, 0, dimensions),
          //Utils.GetVoxelIndex(162, 1, dimensions),
          //Utils.GetVoxelIndex(162, 2, dimensions),
          //Utils.GetVoxelIndex(162, 3, dimensions),
            Utils.GetVoxelIndex(162, 4, dimensions),
          //Utils.GetVoxelIndex(162, 5, dimensions),
          //Utils.GetVoxelIndex(162, 6, dimensions),
          //Utils.GetVoxelIndex(162, 7, dimensions),

            // =========== z == 6 ===========
            
            Utils.GetVoxelIndex(192, 0, dimensions),
          //Utils.GetVoxelIndex(192, 1, dimensions),
          //Utils.GetVoxelIndex(192, 2, dimensions),
          //Utils.GetVoxelIndex(192, 3, dimensions),
            Utils.GetVoxelIndex(192, 4, dimensions),
          //Utils.GetVoxelIndex(192, 5, dimensions),
          //Utils.GetVoxelIndex(192, 6, dimensions),
          //Utils.GetVoxelIndex(192, 7, dimensions),
        };

        user = new FauxVoxelClusterUpdaterUser(
            offset: Vector3Int.zero,
            dimensions: new Vector3Int(WIDTH, HEIGHT, DEPTH),
            onReceivedUpdateRequest: () => { },
            onUpdateStart: () => { return voxelBlocks; },
            onUpdateFinish: ValidateResults
        );

        for(int i = 0; i < voxelBlocks.Length; i++) {
            voxelBlocks[i] = Bin.RefreshConnectivity(voxelBlocks, i, dimensions);
        }

        yield return VoxelClusterUpdater.RemoveVoxelsInCluster(user, voxelsToDelete.ToQueue(), STEP_DURATION);

        void ValidateResults(List<VoxelCluster> foundClusters) {
            Assert.AreEqual(1, foundClusters.Count);

            AssertClusterLooksCorrect(
                foundClusters[0],
                expectedOffset: new Vector3Int(0, 0, 0),
                expectedDimensions: new Vector3Int(WIDTH, HEIGHT, DEPTH),
                shouldVoxelBlockBeExterior: (Vector3Int coords) => { 
                    return  Utils.AreCoordsOnTheEdge(coords, new Vector3Int(WIDTH, HEIGHT, DEPTH)) || 
                            coords.x == 2 && coords.y == 2 || 
                            coords.x == 1 && coords.y == 2 || 
                            coords.x == 2 && coords.y == 1; 
                },
                shouldExteriorVoxelExist: (Vector3Int coords) => { 
                    return !voxelsToDelete.Contains(Utils.CoordsToIndex(coords, dimensions * Bin.WIDTH)); 
                }
            );
        }
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
                        Assert.AreEqual(shouldExteriorVoxelExist(voxelCoords), doesVoxelExist, "Voxel at {0} is {1}, but should be {2}!", voxelCoords, doesVoxelExist, shouldExteriorVoxelExist(voxelCoords));
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
