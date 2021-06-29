using UnityEngine;
using NUnit.Framework;
using System;

public class BinTests {

    [Test]
    public void TestRefreshConnectivityInBin() {
        static void RunTest(Bin voxelBlock, byte expectedResultsRightLeft, byte expectedResultsUpDown, byte expectedResultsForeBack) {
            Assert.AreEqual(Convert.ToString(expectedResultsRightLeft, 2),  Convert.ToString(voxelBlock.voxelNeighborsRightLeft, 2));
            Assert.AreEqual(Convert.ToString(expectedResultsUpDown, 2),     Convert.ToString(voxelBlock.voxelNeighborsUpDown, 2));
            Assert.AreEqual(Convert.ToString(expectedResultsForeBack, 2),   Convert.ToString(voxelBlock.voxelNeighborsForeBack, 2));
        }

        Vector3Int dimensions = new Vector3Int(3, 4, 5);
        Vector3Int binToTestCoords = new Vector3Int(1, 1, 1);
        Bin[] voxelBlocks;
        int testIndex, indexRight, indexLeft, indexUp, indexDown, indexFore, indexBack;

        voxelBlocks = new Bin[dimensions.Product()];

        testIndex  = Utils.CoordsToIndex(binToTestCoords,                       dimensions);
        indexRight = Utils.CoordsToIndex(binToTestCoords + Vector3Int.right,    dimensions);
        indexLeft  = Utils.CoordsToIndex(binToTestCoords + Vector3Int.left,     dimensions);
        indexUp    = Utils.CoordsToIndex(binToTestCoords + Vector3Int.up,       dimensions);
        indexDown  = Utils.CoordsToIndex(binToTestCoords + Vector3Int.down,     dimensions);
        indexFore  = Utils.CoordsToIndex(binToTestCoords + Vector3Int.forward,  dimensions);
        indexBack  = Utils.CoordsToIndex(binToTestCoords + Vector3Int.back,     dimensions);

        voxelBlocks[testIndex]  = new Bin(testIndex,    dimensions, byte.MaxValue);
        voxelBlocks[indexRight] = new Bin(indexRight,   dimensions, byte.MinValue);
        voxelBlocks[indexLeft]  = new Bin(indexLeft,    dimensions, byte.MinValue);
        voxelBlocks[indexUp]    = new Bin(indexUp,      dimensions, byte.MinValue);
        voxelBlocks[indexDown]  = new Bin(indexDown,    dimensions, byte.MinValue);
        voxelBlocks[indexFore]  = new Bin(indexFore,    dimensions, byte.MinValue);
        voxelBlocks[indexBack]  = new Bin(indexBack,    dimensions, byte.MinValue);

        RunTest(
            Bin.RefreshConnectivity(voxelBlocks, testIndex, dimensions),
            expectedResultsRightLeft: 0b_0000_0000,
            expectedResultsUpDown: 0b_0000_0000,
            expectedResultsForeBack: 0b_0000_0000
        );

        voxelBlocks[testIndex]  = new Bin(testIndex,    dimensions, byte.MaxValue);
        voxelBlocks[indexRight] = new Bin(indexRight,   dimensions, byte.MaxValue);
        voxelBlocks[indexLeft]  = new Bin(indexLeft,    dimensions, byte.MaxValue);
        voxelBlocks[indexUp]    = new Bin(indexUp,      dimensions, byte.MaxValue);
        voxelBlocks[indexDown]  = new Bin(indexDown,    dimensions, byte.MaxValue);
        voxelBlocks[indexFore]  = new Bin(indexFore,    dimensions, byte.MaxValue);
        voxelBlocks[indexBack]  = new Bin(indexBack,    dimensions, byte.MaxValue);

        RunTest(
            Bin.RefreshConnectivity(voxelBlocks, testIndex, dimensions),
            expectedResultsRightLeft: 0b_1111_1111,
            expectedResultsUpDown: 0b_1111_1111,
            expectedResultsForeBack: 0b_1111_1111
        );

        for(int i = 0; i < Bin.SIZE; i++) {
            voxelBlocks[indexRight] = voxelBlocks[indexRight].SetVoxelExists(i, exists: Bin.LOCAL_COORDS_LOOKUP[i].y == 0);
            voxelBlocks[indexLeft]  = voxelBlocks[indexLeft]. SetVoxelExists(i, exists: Bin.LOCAL_COORDS_LOOKUP[i].y == Bin.WIDTH - 1);
            voxelBlocks[indexUp]    = voxelBlocks[indexUp].   SetVoxelExists(i, exists: Bin.LOCAL_COORDS_LOOKUP[i].z == 0);
            voxelBlocks[indexDown]  = voxelBlocks[indexDown]. SetVoxelExists(i, exists: Bin.LOCAL_COORDS_LOOKUP[i].z == Bin.WIDTH - 1);
            voxelBlocks[indexFore]  = voxelBlocks[indexFore]. SetVoxelExists(i, exists: Bin.LOCAL_COORDS_LOOKUP[i].x == 0);
            voxelBlocks[indexBack]  = voxelBlocks[indexBack]. SetVoxelExists(i, exists: Bin.LOCAL_COORDS_LOOKUP[i].x == Bin.WIDTH - 1);
        }

        RunTest(
            Bin.RefreshConnectivity(voxelBlocks, testIndex, dimensions),
            expectedResultsRightLeft: 0b_1010_0101,
            expectedResultsUpDown: 0b_1100_0011,
            expectedResultsForeBack: 0b_1010_0101
        );
    }

    [Test]
    public void TestGetVoxelHasNeighbor() {
        byte binVoxels, voxelsRightLeft, voxelsUpDown, voxelsForeBack;

        binVoxels = 0b_0000_0000;
        voxelsRightLeft = 0b_0000_0000;
        voxelsUpDown = 0b_0000_0000;
        voxelsForeBack = 0b_0000_0000;
        for(int i = 0; i < Bin.SIZE; i++) {
            Assert.IsFalse(Bin.GetVoxelHasNeighbor(i, Direction.Right, binVoxels, voxelsRightLeft, 0, 0));
            Assert.IsFalse(Bin.GetVoxelHasNeighbor(i, Direction.Left, binVoxels, voxelsRightLeft, 0, 0));
            Assert.IsFalse(Bin.GetVoxelHasNeighbor(i, Direction.Up, binVoxels, 0, voxelsUpDown, 0));
            Assert.IsFalse(Bin.GetVoxelHasNeighbor(i, Direction.Down, binVoxels, 0, voxelsUpDown, 0));
            Assert.IsFalse(Bin.GetVoxelHasNeighbor(i, Direction.Fore, binVoxels, 0, 0, voxelsForeBack));
            Assert.IsFalse(Bin.GetVoxelHasNeighbor(i, Direction.Back, binVoxels, 0, 0, voxelsForeBack));
        }

        binVoxels = 0b_1111_1111;
        voxelsRightLeft = 0b_1111_1111;
        voxelsUpDown = 0b_1111_1111;
        voxelsForeBack = 0b_1111_1111;
        for(int i = 0; i < Bin.SIZE; i++) {
            Assert.IsTrue(Bin.GetVoxelHasNeighbor(i, Direction.Right, binVoxels, voxelsRightLeft, 0, 0));
            Assert.IsTrue(Bin.GetVoxelHasNeighbor(i, Direction.Left, binVoxels, voxelsRightLeft, 0, 0));
            Assert.IsTrue(Bin.GetVoxelHasNeighbor(i, Direction.Up, binVoxels, 0, voxelsUpDown, 0));
            Assert.IsTrue(Bin.GetVoxelHasNeighbor(i, Direction.Down, binVoxels, 0, voxelsUpDown, 0));
            Assert.IsTrue(Bin.GetVoxelHasNeighbor(i, Direction.Fore, binVoxels, 0, 0, voxelsForeBack));
            Assert.IsTrue(Bin.GetVoxelHasNeighbor(i, Direction.Back, binVoxels, 0, 0, voxelsForeBack));
        }

        binVoxels = 0b_1111_1111;
        voxelsRightLeft = 0b_0000_0000;
        voxelsUpDown = 0b_0000_0000;
        voxelsForeBack = 0b_0000_0000;
        for(int i = 0; i < Bin.SIZE; i++) {
            bool expectedResultRight = Bin.LOCAL_COORDS_LOOKUP[i].x < Bin.WIDTH - 1;
            bool expectedResultLeft = Bin.LOCAL_COORDS_LOOKUP[i].x > 0;
            bool expectedResultUp = Bin.LOCAL_COORDS_LOOKUP[i].y < Bin.WIDTH - 1;
            bool expectedResultDown = Bin.LOCAL_COORDS_LOOKUP[i].y > 0;
            bool expectedResultFore = Bin.LOCAL_COORDS_LOOKUP[i].z < Bin.WIDTH - 1;
            bool expectedResultBack = Bin.LOCAL_COORDS_LOOKUP[i].z > 0;

            Assert.AreEqual(expectedResultRight, Bin.GetVoxelHasNeighbor(i, Direction.Right,  binVoxels, voxelsRightLeft, 0,            0));
            Assert.AreEqual(expectedResultLeft,  Bin.GetVoxelHasNeighbor(i, Direction.Left,   binVoxels, voxelsRightLeft, 0,            0));
            Assert.AreEqual(expectedResultUp,    Bin.GetVoxelHasNeighbor(i, Direction.Up,     binVoxels, 0,               voxelsUpDown, 0));
            Assert.AreEqual(expectedResultDown,  Bin.GetVoxelHasNeighbor(i, Direction.Down,   binVoxels, 0,               voxelsUpDown, 0));
            Assert.AreEqual(expectedResultFore,  Bin.GetVoxelHasNeighbor(i, Direction.Fore,   binVoxels, 0,               0,            voxelsForeBack));
            Assert.AreEqual(expectedResultBack,  Bin.GetVoxelHasNeighbor(i, Direction.Back,   binVoxels, 0,               0,            voxelsForeBack));
        }

        for(int i0 = 0; i0 < Bin.SIZE; i0++) {
            binVoxels = (byte)(1 << i0);
            voxelsRightLeft = 0b_0000_0000;
            voxelsUpDown = 0b_0000_0000;
            voxelsForeBack = 0b_0000_0000;

            for(int i1 = 0; i1 < Bin.SIZE; i1++) {
                Vector3Int filledLocalCoords = Bin.LOCAL_COORDS_LOOKUP[i0];

                bool expectedResultRight = filledLocalCoords - Bin.LOCAL_COORDS_LOOKUP[i1] == Vector3.right;
                bool expectedResultLeft = filledLocalCoords - Bin.LOCAL_COORDS_LOOKUP[i1] == Vector3.left;
                bool expectedResultUp = filledLocalCoords - Bin.LOCAL_COORDS_LOOKUP[i1] == Vector3.up;
                bool expectedResultDown = filledLocalCoords - Bin.LOCAL_COORDS_LOOKUP[i1] == Vector3.down;
                bool expectedResultFore = filledLocalCoords - Bin.LOCAL_COORDS_LOOKUP[i1] == Vector3.forward;
                bool expectedResultBack = filledLocalCoords - Bin.LOCAL_COORDS_LOOKUP[i1] == Vector3.back;

                Assert.AreEqual(expectedResultRight, Bin.GetVoxelHasNeighbor(i1, Direction.Right,  binVoxels, voxelsRightLeft, 0,            0));
                Assert.AreEqual(expectedResultLeft,  Bin.GetVoxelHasNeighbor(i1, Direction.Left,   binVoxels, voxelsRightLeft, 0,            0));
                Assert.AreEqual(expectedResultUp,    Bin.GetVoxelHasNeighbor(i1, Direction.Up,     binVoxels, 0,               voxelsUpDown, 0));
                Assert.AreEqual(expectedResultDown,  Bin.GetVoxelHasNeighbor(i1, Direction.Down,   binVoxels, 0,               voxelsUpDown, 0));
                Assert.AreEqual(expectedResultFore,  Bin.GetVoxelHasNeighbor(i1, Direction.Fore,   binVoxels, 0,               0,            voxelsForeBack));
                Assert.AreEqual(expectedResultBack,  Bin.GetVoxelHasNeighbor(i1, Direction.Back,   binVoxels, 0,               0,            voxelsForeBack));
            }
        }

        for(int i0 = 0; i0 < 6; i0++) {
            Direction dir = (Direction)i0;

            for(int i1 = 0; i1 < Bin.VOXELS_PER_FACE; i1++) {
                Bin.TryGetLocalVoxelIndex(i1, dir, out int neighborClosestLocalVoxelIndex);
                Vector3Int neighborClosestLocalCoords = Bin.LOCAL_COORDS_LOOKUP[neighborClosestLocalVoxelIndex];

                binVoxels = 0b_0000_0000;
                voxelsRightLeft = 0b_0000_0000;
                voxelsUpDown = 0b_0000_0000;
                voxelsForeBack = 0b_0000_0000;

                if(i0 == 0) { voxelsRightLeft = (byte)(1 << i1); }
                else if(i0 == 1) { voxelsRightLeft = (byte)(1 << Bin.VOXELS_PER_FACE + i1); }
                else if(i0 == 2) { voxelsUpDown = (byte)(1 << i1); }
                else if(i0 == 3) { voxelsUpDown = (byte)(1 << Bin.VOXELS_PER_FACE + i1); }
                else if(i0 == 4) { voxelsForeBack = (byte)(1 << i1); }
                else if(i0 == 5) { voxelsForeBack = (byte)(1 << Bin.VOXELS_PER_FACE + i1); }

                for(int i2 = 0; i2 < Bin.SIZE; i2++) {
                    Vector3Int localCoords = Bin.LOCAL_COORDS_LOOKUP[i2];

                    bool expectedResultRight = dir == Direction.Right && localCoords == neighborClosestLocalCoords;
                    bool expectedResultLeft = dir == Direction.Left && localCoords == neighborClosestLocalCoords;
                    bool expectedResultUp = dir == Direction.Up && localCoords == neighborClosestLocalCoords;
                    bool expectedResultDown = dir == Direction.Down && localCoords == neighborClosestLocalCoords;
                    bool expectedResultFore = dir == Direction.Fore && localCoords == neighborClosestLocalCoords;
                    bool expectedResultBack = dir == Direction.Back && localCoords == neighborClosestLocalCoords;

                    Assert.AreEqual(expectedResultRight, Bin.GetVoxelHasNeighbor(i2, Direction.Right, binVoxels, voxelsRightLeft, 0,            0));
                    Assert.AreEqual(expectedResultLeft,  Bin.GetVoxelHasNeighbor(i2, Direction.Left,  binVoxels, voxelsRightLeft, 0,            0));
                    Assert.AreEqual(expectedResultUp,    Bin.GetVoxelHasNeighbor(i2, Direction.Up,    binVoxels, 0,               voxelsUpDown, 0));
                    Assert.AreEqual(expectedResultDown,  Bin.GetVoxelHasNeighbor(i2, Direction.Down,  binVoxels, 0,               voxelsUpDown, 0));
                    Assert.AreEqual(expectedResultFore,  Bin.GetVoxelHasNeighbor(i2, Direction.Fore,  binVoxels, 0,               0,            voxelsForeBack));
                    Assert.AreEqual(expectedResultBack,  Bin.GetVoxelHasNeighbor(i2, Direction.Back,  binVoxels, 0,               0,            voxelsForeBack));
                }
            }
        }
    }

    [Test]
    public void TestGetCachedVoxelNeighbors() {
        Assert.AreEqual(0b_0000, Bin.GetCachedVoxelNeighbors(Direction.Right, 0b_1111_0000, 0,               0));
        Assert.AreEqual(0b_1111, Bin.GetCachedVoxelNeighbors(Direction.Left,  0b_1111_0000, 0,               0));
        Assert.AreEqual(0b_0000, Bin.GetCachedVoxelNeighbors(Direction.Up,    0,            0b_1111_0000,    0));
        Assert.AreEqual(0b_1111, Bin.GetCachedVoxelNeighbors(Direction.Down,  0,            0b_1111_0000,    0));
        Assert.AreEqual(0b_0000, Bin.GetCachedVoxelNeighbors(Direction.Fore,  0,            0,               0b_1111_0000));
        Assert.AreEqual(0b_1111, Bin.GetCachedVoxelNeighbors(Direction.Back,  0,            0,               0b_1111_0000));

        Assert.AreEqual(0b_1010, Bin.GetCachedVoxelNeighbors(Direction.Right, 0b_0011_1010, 0,            0));
        Assert.AreEqual(0b_0011, Bin.GetCachedVoxelNeighbors(Direction.Left,  0b_0011_1010, 0,            0));
        Assert.AreEqual(0b_1010, Bin.GetCachedVoxelNeighbors(Direction.Up,    0,            0b_0011_1010, 0));
        Assert.AreEqual(0b_0011, Bin.GetCachedVoxelNeighbors(Direction.Down,  0,            0b_0011_1010, 0));
        Assert.AreEqual(0b_1010, Bin.GetCachedVoxelNeighbors(Direction.Fore,  0,            0,            0b_0011_1010));
        Assert.AreEqual(0b_0011, Bin.GetCachedVoxelNeighbors(Direction.Back,  0,            0,            0b_0011_1010));
    }

    [Test]
    public void TestHasOpenPathBetweenFaces() {
        for(int i0 = -1; i0 < Enum.GetValues(typeof(Direction)).Length - 1; i0++) {
            for(int i1 = -1; i1 < Enum.GetValues(typeof(Direction)).Length - 1; i1++) {
                Direction dir0 = (Direction)i0;
                Direction dir1 = (Direction)i1;

                bool result0 = Bin.HasOpenPathBetweenFaces(byte.MinValue, dir0, dir1);
                bool result1 = Bin.HasOpenPathBetweenFaces(byte.MaxValue, dir0, dir1);

                Assert.That(dir0 == dir1 ? result0 == false : result0 == true, string.Format("Failed HasOpenPathBetweenFaces - {0}, {1}, {2}", byte.MinValue, dir0, dir1));
                Assert.IsFalse(result1, string.Format("Failed HasOpenPathBetweenFaces - {0}, {1}, {2}", byte.MaxValue, dir0, dir1));
            }
        }

        for(int i0 = 0; i0 < Bin.SIZE; i0++) {
            byte voxels = byte.MaxValue;
            Utils.SetValueInByte(ref voxels, i0, false);

            Vector3Int coords = Bin.LOCAL_COORDS_LOOKUP[i0];

            Assert.AreEqual(false,                          Bin.HasOpenPathBetweenFaces(voxels, Direction.None, Direction.None));
            Assert.AreEqual(coords.x == 1,                  Bin.HasOpenPathBetweenFaces(voxels, Direction.None, Direction.Right));
            Assert.AreEqual(coords.x == 0,                  Bin.HasOpenPathBetweenFaces(voxels, Direction.None, Direction.Left));
            Assert.AreEqual(coords.y == 1,                  Bin.HasOpenPathBetweenFaces(voxels, Direction.None, Direction.Up));
            Assert.AreEqual(coords.y == 0,                  Bin.HasOpenPathBetweenFaces(voxels, Direction.None, Direction.Down));
            Assert.AreEqual(coords.z == 1,                  Bin.HasOpenPathBetweenFaces(voxels, Direction.None, Direction.Fore));
            Assert.AreEqual(coords.z == 0,                  Bin.HasOpenPathBetweenFaces(voxels, Direction.None, Direction.Back));

            Assert.AreEqual(coords.x == 1,                  Bin.HasOpenPathBetweenFaces(voxels, Direction.Right, Direction.None));
            Assert.AreEqual(false,                          Bin.HasOpenPathBetweenFaces(voxels, Direction.Right, Direction.Right));
            Assert.AreEqual(false,                          Bin.HasOpenPathBetweenFaces(voxels, Direction.Right, Direction.Left));
            Assert.AreEqual(coords.x == 1 && coords.y == 1, Bin.HasOpenPathBetweenFaces(voxels, Direction.Right, Direction.Up));
            Assert.AreEqual(coords.x == 1 && coords.y == 0, Bin.HasOpenPathBetweenFaces(voxels, Direction.Right, Direction.Down));
            Assert.AreEqual(coords.x == 1 && coords.z == 1, Bin.HasOpenPathBetweenFaces(voxels, Direction.Right, Direction.Fore));
            Assert.AreEqual(coords.x == 1 && coords.z == 0, Bin.HasOpenPathBetweenFaces(voxels, Direction.Right, Direction.Back));

            Assert.AreEqual(coords.x == 0,                  Bin.HasOpenPathBetweenFaces(voxels, Direction.Left, Direction.None));            
            Assert.AreEqual(false,                          Bin.HasOpenPathBetweenFaces(voxels, Direction.Left, Direction.Right));
            Assert.AreEqual(false,                          Bin.HasOpenPathBetweenFaces(voxels, Direction.Left, Direction.Left));
            Assert.AreEqual(coords.x == 0 && coords.y == 1, Bin.HasOpenPathBetweenFaces(voxels, Direction.Left, Direction.Up));
            Assert.AreEqual(coords.x == 0 && coords.y == 0, Bin.HasOpenPathBetweenFaces(voxels, Direction.Left, Direction.Down));
            Assert.AreEqual(coords.x == 0 && coords.z == 1, Bin.HasOpenPathBetweenFaces(voxels, Direction.Left, Direction.Fore));
            Assert.AreEqual(coords.x == 0 && coords.z == 0, Bin.HasOpenPathBetweenFaces(voxels, Direction.Left, Direction.Back));


            Assert.AreEqual(coords.y == 1,                  Bin.HasOpenPathBetweenFaces(voxels, Direction.Up, Direction.None));
            Assert.AreEqual(coords.x == 1 && coords.y == 1, Bin.HasOpenPathBetweenFaces(voxels, Direction.Up, Direction.Right));
            Assert.AreEqual(coords.x == 0 && coords.y == 1, Bin.HasOpenPathBetweenFaces(voxels, Direction.Up, Direction.Left));
            Assert.AreEqual(false,                          Bin.HasOpenPathBetweenFaces(voxels, Direction.Up, Direction.Up));
            Assert.AreEqual(false,                          Bin.HasOpenPathBetweenFaces(voxels, Direction.Up, Direction.Down));
            Assert.AreEqual(coords.y == 1 && coords.z == 1, Bin.HasOpenPathBetweenFaces(voxels, Direction.Up, Direction.Fore));
            Assert.AreEqual(coords.y == 1 && coords.z == 0, Bin.HasOpenPathBetweenFaces(voxels, Direction.Up, Direction.Back));

            Assert.AreEqual(coords.y == 0,                  Bin.HasOpenPathBetweenFaces(voxels, Direction.Down, Direction.None));
            Assert.AreEqual(coords.x == 1 && coords.y == 0, Bin.HasOpenPathBetweenFaces(voxels, Direction.Down, Direction.Right));
            Assert.AreEqual(coords.x == 0 && coords.y == 0, Bin.HasOpenPathBetweenFaces(voxels, Direction.Down, Direction.Left));
            Assert.AreEqual(false,                          Bin.HasOpenPathBetweenFaces(voxels, Direction.Down, Direction.Up));
            Assert.AreEqual(false,                          Bin.HasOpenPathBetweenFaces(voxels, Direction.Down, Direction.Down));
            Assert.AreEqual(coords.y == 0 && coords.z == 1, Bin.HasOpenPathBetweenFaces(voxels, Direction.Down, Direction.Fore));
            Assert.AreEqual(coords.y == 0 && coords.z == 0, Bin.HasOpenPathBetweenFaces(voxels, Direction.Down, Direction.Back));

            Assert.AreEqual(coords.z == 1,                  Bin.HasOpenPathBetweenFaces(voxels, Direction.Fore, Direction.None));
            Assert.AreEqual(coords.x == 1 && coords.z == 1, Bin.HasOpenPathBetweenFaces(voxels, Direction.Fore, Direction.Right));
            Assert.AreEqual(coords.x == 0 && coords.z == 1, Bin.HasOpenPathBetweenFaces(voxels, Direction.Fore, Direction.Left));
            Assert.AreEqual(coords.y == 1 && coords.z == 1, Bin.HasOpenPathBetweenFaces(voxels, Direction.Fore, Direction.Up));
            Assert.AreEqual(coords.y == 0 && coords.z == 1, Bin.HasOpenPathBetweenFaces(voxels, Direction.Fore, Direction.Down));
            Assert.AreEqual(false,                          Bin.HasOpenPathBetweenFaces(voxels, Direction.Fore, Direction.Fore));
            Assert.AreEqual(false,                          Bin.HasOpenPathBetweenFaces(voxels, Direction.Fore, Direction.Back));

            Assert.AreEqual(coords.z == 0,                  Bin.HasOpenPathBetweenFaces(voxels, Direction.Back, Direction.None));
            Assert.AreEqual(coords.x == 1 && coords.z == 0, Bin.HasOpenPathBetweenFaces(voxels, Direction.Back, Direction.Right));
            Assert.AreEqual(coords.x == 0 && coords.z == 0, Bin.HasOpenPathBetweenFaces(voxels, Direction.Back, Direction.Left));
            Assert.AreEqual(coords.y == 1 && coords.z == 0, Bin.HasOpenPathBetweenFaces(voxels, Direction.Back, Direction.Up));
            Assert.AreEqual(coords.y == 0 && coords.z == 0, Bin.HasOpenPathBetweenFaces(voxels, Direction.Back, Direction.Down));
            Assert.AreEqual(false,                          Bin.HasOpenPathBetweenFaces(voxels, Direction.Back, Direction.Fore));
            Assert.AreEqual(false,                          Bin.HasOpenPathBetweenFaces(voxels, Direction.Back, Direction.Back));
        }

        TestOpposites(Direction.Right,  Direction.Left);
        TestOpposites(Direction.Up,     Direction.Down);
        TestOpposites(Direction.Fore,   Direction.Back);

        void TestOpposites(Direction face1, Direction face2) {
            for(int i0 = 0; i0 < Bin.WIDTH; i0++) {
                for(int i1 = 0; i1 < Bin.WIDTH; i1++) {
                    for(int i2 = 0; i2 < Bin.VOXELS_PER_FACE; i2++) {
                        Bin.TryGetLocalVoxelIndex(i2, face1, out int index1);
                        Bin.TryGetLocalVoxelIndex(i2, face2, out int index2);

                        byte voxels = byte.MaxValue;
                        Utils.SetValueInByte(ref voxels, index1, false);
                        Utils.SetValueInByte(ref voxels, index2, false);

                        Assert.IsTrue(Bin.HasOpenPathBetweenFaces(voxels, face1, face2));
                    }
                }
            }
        }

        TestDiagonals(Direction.Right, Direction.Left);
        TestDiagonals(Direction.Up, Direction.Down);
        TestDiagonals(Direction.Fore, Direction.Back);

        void TestDiagonals(Direction face1, Direction face2) {
            for(int i0 = 0; i0 < Bin.WIDTH; i0++) {
                for(int i1 = 0; i1 < Bin.WIDTH; i1++) {
                    for(int i2 = 0; i2 < Bin.VOXELS_PER_FACE; i2++) {
                        Bin.TryGetLocalVoxelIndex(i2, face1, out int index1);
                        Bin.TryGetLocalVoxelIndex((Bin.VOXELS_PER_FACE - 1) - i2, face2, out int index2);

                        byte voxels = byte.MaxValue;
                        Utils.SetValueInByte(ref voxels, index1, false);
                        Utils.SetValueInByte(ref voxels, index2, false);

                        Assert.IsFalse(Bin.HasOpenPathBetweenFaces(voxels, face1, face2));
                    }
                }
            }
        }
    }

    [Test]
    public void TestIsConnectedToNeighbor() {
        void Test(Direction direction) {
            for(int i0 = 0; i0 < 8; i0++) {
                for(int i1 = 0; i1 < 8; i1++) {
                    byte voxels = 0b_0000_0000;
                    byte neighborsRightLeft = 0b_0000_0000;
                    byte neighborsUpDown = 0b_0000_0000;
                    byte neighborsForeBack = 0b_0000_0000;

                    Utils.SetValueInByte(ref voxels, i0, true);

                    if(Utils.DirectionToAxis(direction) == Axis.X) {
                        Utils.SetValueInByte(ref neighborsRightLeft, i1, true);
                    }
                    else if(Utils.DirectionToAxis(direction) == Axis.Y) {
                        Utils.SetValueInByte(ref neighborsUpDown, i1, true);
                    }
                    else {
                        Utils.SetValueInByte(ref neighborsForeBack, i1, true);
                    }

                    bool result = Bin.IsConnectedToNeighbor(direction, voxels, neighborsRightLeft, neighborsUpDown, neighborsForeBack);

                    if(Bin.TryGetFaceVoxelIndex(i0, direction, out int faceVoxelIndex)) {
                        continue;
                    }

                    Assert.AreEqual(faceVoxelIndex == i1, result);
                }
            }
        }

        Test(Direction.None);
        Test(Direction.Right);
        Test(Direction.Left);
        Test(Direction.Up);
        Test(Direction.Down);
        Test(Direction.Fore);
        Test(Direction.Back);
    }

    [Test]
    public void TestGetVisualID() {
        Bin GetNewBin(byte voxels, bool fillNeighborRight = false, bool fillNeighborLeft = false, bool fillNeighborUp = false, bool fillNeighborDown = false, bool fillNeighborFore = false, bool fillNeighborBack = false, Direction alterNeighbor = Direction.None, int alterIndex = -1) {
            Bin bin      = new Bin(0, Vector3Int.one, voxels);
            Bin binRight = new Bin(0, Vector3Int.one, byte.MinValue);
            Bin binLeft  = new Bin(0, Vector3Int.one, byte.MinValue);
            Bin binUp    = new Bin(0, Vector3Int.one, byte.MinValue);
            Bin binDown  = new Bin(0, Vector3Int.one, byte.MinValue);
            Bin binFore  = new Bin(0, Vector3Int.one, byte.MinValue);
            Bin binBack  = new Bin(0, Vector3Int.one, byte.MinValue);

            binRight = new Bin(binRight, voxels: fillNeighborRight ? byte.MaxValue : byte.MinValue);
            binLeft  = new Bin(binLeft,  voxels: fillNeighborLeft  ? byte.MaxValue : byte.MinValue);
            binUp    = new Bin(binUp,    voxels: fillNeighborUp    ? byte.MaxValue : byte.MinValue);
            binDown  = new Bin(binDown,  voxels: fillNeighborDown  ? byte.MaxValue : byte.MinValue);
            binFore  = new Bin(binFore,  voxels: fillNeighborFore  ? byte.MaxValue : byte.MinValue);
            binBack  = new Bin(binBack,  voxels: fillNeighborBack  ? byte.MaxValue : byte.MinValue);

            if(alterNeighbor != Direction.None && alterIndex != -1) {
                switch(alterNeighbor) {
                    case Direction.Right: { binRight = binRight.SetVoxelExists(alterIndex, !fillNeighborRight); break; }
                    case Direction.Left:  { binLeft  = binLeft.SetVoxelExists( alterIndex, !fillNeighborLeft);  break; }
                    case Direction.Up:    { binUp    = binUp.SetVoxelExists(alterIndex, !fillNeighborUp);       break; }
                    case Direction.Down:  { binDown  = binDown.SetVoxelExists(alterIndex, !fillNeighborDown);   break; }
                    case Direction.Fore:  { binFore  = binFore.SetVoxelExists(alterIndex, !fillNeighborFore);   break; }
                    case Direction.Back:  { binBack  = binBack.SetVoxelExists(alterIndex, !fillNeighborBack);   break; }
                }
            }

            return Bin.RefreshConnectivity(bin, binRight, binLeft, binUp, binDown, binFore, binBack);
        }

        Bin bin = GetNewBin(voxels: byte.MaxValue);
        Assert.AreEqual(Convert.ToString(0b_11111111, 2), Convert.ToString(Bin.GetVisualID(bin), 2));
        Assert.AreEqual(Convert.ToString(0b_0000_0000_0000_0000_0000_0000, 2), Convert.ToString(Bin.GetVisualID(bin) >> 8, 2));

        bin = GetNewBin(voxels: byte.MinValue);
        Assert.AreEqual(Convert.ToString(0b_00000000, 2), Convert.ToString(Bin.GetVisualID(bin) & 0b_11111111, 2));
        Assert.AreEqual(Convert.ToString(0b_0000_0000_0000_0000_0000_0000, 2), Convert.ToString(Bin.GetVisualID(bin) >> 8, 2));

        bin = GetNewBin(voxels: 0b_10000000);
        Assert.AreEqual(Convert.ToString(0b_10000000, 2), Convert.ToString(Bin.GetVisualID(bin) & 0b_11111111, 2));
        Assert.AreEqual(Convert.ToString(0b_0000_0000_0000_0000_0000_0000, 2), Convert.ToString(Bin.GetVisualID(bin) >> 8, 2));

        bin = GetNewBin(voxels: 0b_01000000);
        Assert.AreEqual(Convert.ToString(0b_01000000, 2), Convert.ToString(Bin.GetVisualID(bin) & 0b_11111111, 2));
        Assert.AreEqual(Convert.ToString(0b_0000_0000_0000_0000_0000_0000, 2), Convert.ToString(Bin.GetVisualID(bin) >> 8, 2));

        bin = GetNewBin(voxels: 0b_00100000);
        Assert.AreEqual(Convert.ToString(0b_00100000, 2), Convert.ToString(Bin.GetVisualID(bin) & 0b_11111111, 2));
        Assert.AreEqual(Convert.ToString(0b_0000_0000_0000_0000_0000_0000, 2), Convert.ToString(Bin.GetVisualID(bin) >> 8, 2));

        bin = GetNewBin(voxels: byte.MaxValue);
        Assert.AreEqual(Convert.ToString(0b_0000_0000_0000_0000_0000_0000, 2), Convert.ToString(Bin.GetVisualID(bin) >> 8, 2));

        bin = GetNewBin(voxels: byte.MaxValue, fillNeighborRight: true, fillNeighborLeft: true, fillNeighborUp: true, fillNeighborDown: true, fillNeighborFore: true, fillNeighborBack: true);
        Assert.AreEqual(Convert.ToString(0b_1111_1111_1111_1111_1111_1111, 2), Convert.ToString(Bin.GetVisualID(bin) >> 8, 2));

        TestNeighbor(Direction.Right);
        TestNeighbor(Direction.Left);
        TestNeighbor(Direction.Up);
        TestNeighbor(Direction.Down);
        TestNeighbor(Direction.Fore);
        TestNeighbor(Direction.Back);

        void TestNeighbor(Direction dir) {
            bool fillNeighborRight = dir == Direction.Right;
            bool fillNeighborLeft = dir == Direction.Left;
            bool fillNeighborUp = dir == Direction.Up;
            bool fillNeighborDown = dir == Direction.Down;
            bool fillNeighborFore = dir == Direction.Fore;
            bool fillNeighborBack = dir == Direction.Back;

            int bitsToShift = 8 + (int)dir * 4;

            bin = GetNewBin(voxels: byte.MaxValue, fillNeighborRight, fillNeighborLeft, fillNeighborUp, fillNeighborDown, fillNeighborFore, fillNeighborBack);
            Assert.AreEqual(Convert.ToString(0b_1111, 2), Convert.ToString(Bin.GetVisualID(bin) >> bitsToShift, 2));

            Bin.TryGetLocalVoxelIndex(0, Utils.GetOppositeDirection(dir), out int localVoxelIndex0);
            bin = GetNewBin(voxels: byte.MaxValue, fillNeighborRight, fillNeighborLeft, fillNeighborUp, fillNeighborDown, fillNeighborFore, fillNeighborBack, alterNeighbor: dir, alterIndex: localVoxelIndex0);
            Assert.AreEqual(Convert.ToString(0b_1110, 2), Convert.ToString(Bin.GetVisualID(bin) >> bitsToShift, 2));

            Bin.TryGetLocalVoxelIndex(1, Utils.GetOppositeDirection(dir), out int localVoxelIndex1);
            bin = GetNewBin(voxels: byte.MaxValue, fillNeighborRight, fillNeighborLeft, fillNeighborUp, fillNeighborDown, fillNeighborFore, fillNeighborBack, alterNeighbor: dir, alterIndex: localVoxelIndex1);
            Assert.AreEqual(Convert.ToString( 0b_1101, 2), Convert.ToString( Bin.GetVisualID(bin) >> bitsToShift, 2));

            Bin.TryGetLocalVoxelIndex(2, Utils.GetOppositeDirection(dir), out int localVoxelIndex2);
            bin = GetNewBin(voxels: byte.MaxValue, fillNeighborRight, fillNeighborLeft, fillNeighborUp, fillNeighborDown, fillNeighborFore, fillNeighborBack, alterNeighbor: dir, alterIndex: localVoxelIndex2);
            Assert.AreEqual(Convert.ToString(0b_1011, 2), Convert.ToString(Bin.GetVisualID(bin) >> bitsToShift, 2));

            Bin.TryGetLocalVoxelIndex(3, Utils.GetOppositeDirection(dir), out int localVoxelIndex3);
            bin = GetNewBin(voxels: byte.MaxValue, fillNeighborRight, fillNeighborLeft, fillNeighborUp, fillNeighborDown, fillNeighborFore, fillNeighborBack, alterNeighbor: dir, alterIndex: localVoxelIndex3);
            Assert.AreEqual(Convert.ToString(0b_0111, 2), Convert.ToString(Bin.GetVisualID(bin) >> bitsToShift, 2));
        }
    }

    [Test]
    public void TestGetMinVoxelCoords() {
        Assert.AreEqual(new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue), Bin.GetMinVoxelCoord(new Bin(0, new Vector3Int(0, 0, 0), 0b_0000_0000)));
        Assert.AreEqual(new Vector3Int(0, 0, 0), Bin.GetMinVoxelCoord(new Bin(0, Vector3Int.zero, 0b_1111_1111)));
        Assert.AreEqual(new Vector3Int(1, 0, 0), Bin.GetMinVoxelCoord(new Bin(0, Vector3Int.zero, 0b_0000_0010)));
        Assert.AreEqual(new Vector3Int(0, 1, 0), Bin.GetMinVoxelCoord(new Bin(0, Vector3Int.zero, 0b_0000_0100)));
        Assert.AreEqual(new Vector3Int(0, 0, 1), Bin.GetMinVoxelCoord(new Bin(0, Vector3Int.zero, 0b_0001_0000)));
    }

    [Test]
    public void TestGetVoxelGlobalIndex() {
        Vector3Int voxelBlockDimensions = new Vector3Int(4, 5, 6);

        Assert.AreEqual(0,  Bin.GetVoxelGlobalIndex(binIndex: 0, localVoxelIndex: 0, voxelBlockDimensions));
        Assert.AreEqual(1,  Bin.GetVoxelGlobalIndex(binIndex: 0, localVoxelIndex: 1, voxelBlockDimensions));
        Assert.AreEqual(2,  Bin.GetVoxelGlobalIndex(binIndex: 1, localVoxelIndex: 0, voxelBlockDimensions));
        Assert.AreEqual(3,  Bin.GetVoxelGlobalIndex(binIndex: 1, localVoxelIndex: 1, voxelBlockDimensions));

        Assert.AreEqual(8,  Bin.GetVoxelGlobalIndex(binIndex: 0, localVoxelIndex: 2, voxelBlockDimensions));
        Assert.AreEqual(9,  Bin.GetVoxelGlobalIndex(binIndex: 0, localVoxelIndex: 3, voxelBlockDimensions));
        Assert.AreEqual(10, Bin.GetVoxelGlobalIndex(binIndex: 1, localVoxelIndex: 2, voxelBlockDimensions));
        Assert.AreEqual(11, Bin.GetVoxelGlobalIndex(binIndex: 1, localVoxelIndex: 3, voxelBlockDimensions));

        Assert.AreEqual(80, Bin.GetVoxelGlobalIndex(binIndex: 0, localVoxelIndex: 4, voxelBlockDimensions));
        Assert.AreEqual(81, Bin.GetVoxelGlobalIndex(binIndex: 0, localVoxelIndex: 5, voxelBlockDimensions));
        Assert.AreEqual(82, Bin.GetVoxelGlobalIndex(binIndex: 1, localVoxelIndex: 4, voxelBlockDimensions));
        Assert.AreEqual(83, Bin.GetVoxelGlobalIndex(binIndex: 1, localVoxelIndex: 5, voxelBlockDimensions));

        Assert.AreEqual(88, Bin.GetVoxelGlobalIndex(binIndex: 0, localVoxelIndex: 6, voxelBlockDimensions));
        Assert.AreEqual(89, Bin.GetVoxelGlobalIndex(binIndex: 0, localVoxelIndex: 7, voxelBlockDimensions));
        Assert.AreEqual(90, Bin.GetVoxelGlobalIndex(binIndex: 1, localVoxelIndex: 6, voxelBlockDimensions));
        Assert.AreEqual(91, Bin.GetVoxelGlobalIndex(binIndex: 1, localVoxelIndex: 7, voxelBlockDimensions));
    }

    [Test]
    public void TestGetVoxelGlobalCoords() {
        Vector3Int voxelBlockDimensions = new Vector3Int(4, 5, 6);

        Assert.AreEqual(new Vector3Int(0, 0, 0), Bin.GetVoxelGlobalCoords(binIndex: 0, localVoxelIndex: 0, voxelBlockDimensions));
        Assert.AreEqual(new Vector3Int(1, 0, 0), Bin.GetVoxelGlobalCoords(binIndex: 0, localVoxelIndex: 1, voxelBlockDimensions));
        Assert.AreEqual(new Vector3Int(2, 0, 0), Bin.GetVoxelGlobalCoords(binIndex: 1, localVoxelIndex: 0, voxelBlockDimensions));
        Assert.AreEqual(new Vector3Int(3, 0, 0), Bin.GetVoxelGlobalCoords(binIndex: 1, localVoxelIndex: 1, voxelBlockDimensions));

        Assert.AreEqual(new Vector3Int(0, 1, 0), Bin.GetVoxelGlobalCoords(binIndex: 0, localVoxelIndex: 2, voxelBlockDimensions));
        Assert.AreEqual(new Vector3Int(1, 1, 0), Bin.GetVoxelGlobalCoords(binIndex: 0, localVoxelIndex: 3, voxelBlockDimensions));
        Assert.AreEqual(new Vector3Int(2, 1, 0), Bin.GetVoxelGlobalCoords(binIndex: 1, localVoxelIndex: 2, voxelBlockDimensions));
        Assert.AreEqual(new Vector3Int(3, 1, 0), Bin.GetVoxelGlobalCoords(binIndex: 1, localVoxelIndex: 3, voxelBlockDimensions));

        Assert.AreEqual(new Vector3Int(0, 0, 1), Bin.GetVoxelGlobalCoords(binIndex: 0, localVoxelIndex: 4, voxelBlockDimensions));
        Assert.AreEqual(new Vector3Int(1, 0, 1), Bin.GetVoxelGlobalCoords(binIndex: 0, localVoxelIndex: 5, voxelBlockDimensions));
        Assert.AreEqual(new Vector3Int(2, 0, 1), Bin.GetVoxelGlobalCoords(binIndex: 1, localVoxelIndex: 4, voxelBlockDimensions));
        Assert.AreEqual(new Vector3Int(3, 0, 1), Bin.GetVoxelGlobalCoords(binIndex: 1, localVoxelIndex: 5, voxelBlockDimensions));

        Assert.AreEqual(new Vector3Int(0, 1, 1), Bin.GetVoxelGlobalCoords(binIndex: 0, localVoxelIndex: 6, voxelBlockDimensions));
        Assert.AreEqual(new Vector3Int(1, 1, 1), Bin.GetVoxelGlobalCoords(binIndex: 0, localVoxelIndex: 7, voxelBlockDimensions));
        Assert.AreEqual(new Vector3Int(2, 1, 1), Bin.GetVoxelGlobalCoords(binIndex: 1, localVoxelIndex: 6, voxelBlockDimensions));
        Assert.AreEqual(new Vector3Int(3, 1, 1), Bin.GetVoxelGlobalCoords(binIndex: 1, localVoxelIndex: 7, voxelBlockDimensions));
    }
}
