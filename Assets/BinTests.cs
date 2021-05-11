using UnityEngine;
using System;

public readonly partial struct Bin {

    public static void RunTests() {
        TestRefreshConnectivityInBin();
        TestGetVoxelHasNeighbor();
        TestGetCachedVoxelNeighbors();
        TestHasOpenPathBetweenFaces();
        Debug.Log("Tests done.");
    }

    private static void TestRefreshConnectivityInBin() {
        static void RunTest(Bin[] bins, int binIndex, Vector3Int binGridDimensions, byte expectedResultsRightLeft, byte expectedResultsUpDown, byte expectedResultsForeBack) {
            RefreshConnectivityInBin(bins, binIndex, binGridDimensions);

            UnitTester.Assert(
               "RefreshConnectivityInBin, Right + Left",
               bins[binIndex].voxelNeighborsRightLeft == expectedResultsRightLeft,
               true,
               new UnitTester.Parameter("Bins", bins),
               new UnitTester.Parameter("Index", binIndex),
               new UnitTester.Parameter("Dimensions", binGridDimensions)
           );

            UnitTester.Assert(
               "RefreshConnectivityInBin, Up + Down",
               bins[binIndex].voxelNeighborsUpDown == expectedResultsUpDown,
               true,
               new UnitTester.Parameter("Bins", bins),
               new UnitTester.Parameter("Index", binIndex),
               new UnitTester.Parameter("Dimensions", binGridDimensions)
           );

            UnitTester.Assert(
               "RefreshConnectivityInBin, Fore + Back",
               bins[binIndex].voxelNeighborsForeBack == expectedResultsForeBack,
               true,
               new UnitTester.Parameter("Bins", bins),
               new UnitTester.Parameter("Index", binIndex),
               new UnitTester.Parameter("Dimensions", binGridDimensions)
           );
        }

        Vector3Int binGridDimensions = new Vector3Int(3, 3, 3);
        Bin[] bins = UnitTester.GetBinsForTesting(binGridDimensions);

        Vector3Int binToTestCoords = new Vector3Int(1, 1, 1);
        int binToTestIndex = VoxelGrid.CoordsToIndex(binToTestCoords, binGridDimensions);

        SetBinAllVoxelsExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.right,   binGridDimensions), false);
        SetBinAllVoxelsExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.left,    binGridDimensions), false);
        SetBinAllVoxelsExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.up,      binGridDimensions), false);
        SetBinAllVoxelsExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.down,    binGridDimensions), false);
        SetBinAllVoxelsExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.forward, binGridDimensions), false);
        SetBinAllVoxelsExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.back,    binGridDimensions), false);

        RunTest(bins, binToTestIndex, binGridDimensions,
            expectedResultsRightLeft:   0b_0000_0000,
            expectedResultsUpDown:      0b_0000_0000,
            expectedResultsForeBack:    0b_0000_0000
        );

        SetBinAllVoxelsExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.right,   binGridDimensions), true);
        SetBinAllVoxelsExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.left,    binGridDimensions), true);
        SetBinAllVoxelsExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.up,      binGridDimensions), true);
        SetBinAllVoxelsExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.down,    binGridDimensions), true);
        SetBinAllVoxelsExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.forward, binGridDimensions), true);
        SetBinAllVoxelsExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.back,    binGridDimensions), true);

        RunTest(bins, binToTestIndex, binGridDimensions,
            expectedResultsRightLeft:   0b_1111_1111,
            expectedResultsUpDown:      0b_1111_1111,
            expectedResultsForeBack:    0b_1111_1111
        );

        for(int i = 0; i < SIZE; i++) {
            SetBinVoxelExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.right,   binGridDimensions), i, LOCAL_COORDS_LOOKUP[i].y == 0);
            SetBinVoxelExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.left,    binGridDimensions), i, LOCAL_COORDS_LOOKUP[i].y == WIDTH - 1);
            SetBinVoxelExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.up,      binGridDimensions), i, LOCAL_COORDS_LOOKUP[i].z == 0);
            SetBinVoxelExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.down,    binGridDimensions), i, LOCAL_COORDS_LOOKUP[i].z == WIDTH - 1);
            SetBinVoxelExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.forward, binGridDimensions), i, LOCAL_COORDS_LOOKUP[i].x == 0);
            SetBinVoxelExists(bins, VoxelGrid.CoordsToIndex(binToTestCoords + Vector3Int.back,    binGridDimensions), i, LOCAL_COORDS_LOOKUP[i].x == WIDTH - 1);
        }

        RunTest(bins, binToTestIndex, binGridDimensions,
            expectedResultsRightLeft:   0b_1010_0101,
            expectedResultsUpDown:      0b_1100_0011,
            expectedResultsForeBack:    0b_1010_0101
        );
    }

    private static void TestGetVoxelHasNeighbor() {
        static void RunTest(int localVoxelIndex, Direction direction, byte binVoxels, byte voxelNeighborsRightLeft, byte voxelNeighborsUpDown, byte voxelNeighborsForeBack, bool expectedResult) {
            UnitTester.Assert<int, Direction, byte, byte, byte, byte, bool>(
                "GetVoxelHasNeighbor",
                GetVoxelHasNeighbor,
                new UnitTester.Parameter("LocalVoxelIndex", localVoxelIndex),
                new UnitTester.Parameter("Direction", direction),
                new UnitTester.Parameter("BinVoxels", binVoxels),
                new UnitTester.Parameter("VoxelNeighborsRightLeft", voxelNeighborsRightLeft),
                new UnitTester.Parameter("VoxelNeighborsUpDown", voxelNeighborsUpDown),
                new UnitTester.Parameter("VoxelNeighborsForeBack", voxelNeighborsForeBack),
                expectedResult
            );
        }

        byte binVoxels, voxelsRightLeft, voxelsUpDown, voxelsForeBack;

        binVoxels = 0b_0000_0000;
        voxelsRightLeft = 0b_0000_0000;
        voxelsUpDown = 0b_0000_0000;
        voxelsForeBack = 0b_0000_0000;
        for(int i = 0; i < SIZE; i++) {
            RunTest(i, Direction.Right, binVoxels, voxelsRightLeft, 0, 0, false);
            RunTest(i, Direction.Left, binVoxels, voxelsRightLeft, 0, 0, false);
            RunTest(i, Direction.Up, binVoxels, 0, voxelsUpDown, 0, false);
            RunTest(i, Direction.Down, binVoxels, 0, voxelsUpDown, 0, false);
            RunTest(i, Direction.Fore, binVoxels, 0, 0, voxelsForeBack, false);
            RunTest(i, Direction.Back, binVoxels, 0, 0, voxelsForeBack, false);
        }

        binVoxels = 0b_1111_1111;
        voxelsRightLeft = 0b_1111_1111;
        voxelsUpDown = 0b_1111_1111;
        voxelsForeBack = 0b_1111_1111;
        for(int i = 0; i < SIZE; i++) {
            RunTest(i, Direction.Right, binVoxels, voxelsRightLeft, 0, 0, true);
            RunTest(i, Direction.Left, binVoxels, voxelsRightLeft, 0, 0, true);
            RunTest(i, Direction.Up, binVoxels, 0, voxelsUpDown, 0, true);
            RunTest(i, Direction.Down, binVoxels, 0, voxelsUpDown, 0, true);
            RunTest(i, Direction.Fore, binVoxels, 0, 0, voxelsForeBack, true);
            RunTest(i, Direction.Back, binVoxels, 0, 0, voxelsForeBack, true);
        }

        binVoxels = 0b_1111_1111;
        voxelsRightLeft = 0b_0000_0000;
        voxelsUpDown = 0b_0000_0000;
        voxelsForeBack = 0b_0000_0000;
        for(int i = 0; i < SIZE; i++) {
            bool expectedResultRight = LOCAL_COORDS_LOOKUP[i].x < WIDTH - 1;
            bool expectedResultLeft = LOCAL_COORDS_LOOKUP[i].x > 0;
            bool expectedResultUp = LOCAL_COORDS_LOOKUP[i].y < WIDTH - 1;
            bool expectedResultDown = LOCAL_COORDS_LOOKUP[i].y > 0;
            bool expectedResultFore = LOCAL_COORDS_LOOKUP[i].z < WIDTH - 1;
            bool expectedResultBack = LOCAL_COORDS_LOOKUP[i].z > 0;

            RunTest(i, Direction.Right, binVoxels, voxelsRightLeft, 0, 0, expectedResultRight);
            RunTest(i, Direction.Left, binVoxels, voxelsRightLeft, 0, 0, expectedResultLeft);
            RunTest(i, Direction.Up, binVoxels, 0, voxelsUpDown, 0, expectedResultUp);
            RunTest(i, Direction.Down, binVoxels, 0, voxelsUpDown, 0, expectedResultDown);
            RunTest(i, Direction.Fore, binVoxels, 0, 0, voxelsForeBack, expectedResultFore);
            RunTest(i, Direction.Back, binVoxels, 0, 0, voxelsForeBack, expectedResultBack);
        }

        for(int i0 = 0; i0 < SIZE; i0++) {
            binVoxels = (byte)(1 << i0);
            voxelsRightLeft = 0b_0000_0000;
            voxelsUpDown = 0b_0000_0000;
            voxelsForeBack = 0b_0000_0000;

            for(int i1 = 0; i1 < SIZE; i1++) {
                Vector3Int filledLocalCoords = LOCAL_COORDS_LOOKUP[i0];

                bool expectedResultRight = filledLocalCoords - LOCAL_COORDS_LOOKUP[i1] == Vector3.right;
                bool expectedResultLeft = filledLocalCoords - LOCAL_COORDS_LOOKUP[i1] == Vector3.left;
                bool expectedResultUp = filledLocalCoords - LOCAL_COORDS_LOOKUP[i1] == Vector3.up;
                bool expectedResultDown = filledLocalCoords - LOCAL_COORDS_LOOKUP[i1] == Vector3.down;
                bool expectedResultFore = filledLocalCoords - LOCAL_COORDS_LOOKUP[i1] == Vector3.forward;
                bool expectedResultBack = filledLocalCoords - LOCAL_COORDS_LOOKUP[i1] == Vector3.back;

                RunTest(i1, Direction.Right, binVoxels, voxelsRightLeft, 0, 0, expectedResultRight);
                RunTest(i1, Direction.Left, binVoxels, voxelsRightLeft, 0, 0, expectedResultLeft);
                RunTest(i1, Direction.Up, binVoxels, 0, voxelsUpDown, 0, expectedResultUp);
                RunTest(i1, Direction.Down, binVoxels, 0, voxelsUpDown, 0, expectedResultDown);
                RunTest(i1, Direction.Fore, binVoxels, 0, 0, voxelsForeBack, expectedResultFore);
                RunTest(i1, Direction.Back, binVoxels, 0, 0, voxelsForeBack, expectedResultBack);
            }
        }

        for(int i0 = 0; i0 < 6; i0++) {
            Direction dir = (Direction)i0;

            for(int i1 = 0; i1 < VOXELS_PER_FACE; i1++) {
                int neighborClosestLocalVoxelIndex = FaceVoxelIndexToLocalVoxelIndex(i1, dir);
                Vector3Int neighborClosestLocalCoords = LOCAL_COORDS_LOOKUP[neighborClosestLocalVoxelIndex];

                binVoxels = 0b_0000_0000;
                voxelsRightLeft = 0b_0000_0000;
                voxelsUpDown = 0b_0000_0000;
                voxelsForeBack = 0b_0000_0000;

                if(i0 == 0) { voxelsRightLeft = (byte)(1 << i1); }
                else if(i0 == 1) { voxelsRightLeft = (byte)(1 << VOXELS_PER_FACE + i1); }
                else if(i0 == 2) { voxelsUpDown = (byte)(1 << i1); }
                else if(i0 == 3) { voxelsUpDown = (byte)(1 << VOXELS_PER_FACE + i1); }
                else if(i0 == 4) { voxelsForeBack = (byte)(1 << i1); }
                else if(i0 == 5) { voxelsForeBack = (byte)(1 << VOXELS_PER_FACE + i1); }

                for(int i2 = 0; i2 < SIZE; i2++) {
                    Vector3Int localCoords = LOCAL_COORDS_LOOKUP[i2];

                    bool expectedResultRight = dir == Direction.Right && localCoords == neighborClosestLocalCoords;
                    bool expectedResultLeft = dir == Direction.Left && localCoords == neighborClosestLocalCoords;
                    bool expectedResultUp = dir == Direction.Up && localCoords == neighborClosestLocalCoords;
                    bool expectedResultDown = dir == Direction.Down && localCoords == neighborClosestLocalCoords;
                    bool expectedResultFore = dir == Direction.Fore && localCoords == neighborClosestLocalCoords;
                    bool expectedResultBack = dir == Direction.Back && localCoords == neighborClosestLocalCoords;

                    RunTest(i2, Direction.Right, binVoxels, voxelsRightLeft, 0, 0, expectedResultRight);
                    RunTest(i2, Direction.Left, binVoxels, voxelsRightLeft, 0, 0, expectedResultLeft);
                    RunTest(i2, Direction.Up, binVoxels, 0, voxelsUpDown, 0, expectedResultUp);
                    RunTest(i2, Direction.Down, binVoxels, 0, voxelsUpDown, 0, expectedResultDown);
                    RunTest(i2, Direction.Fore, binVoxels, 0, 0, voxelsForeBack, expectedResultFore);
                    RunTest(i2, Direction.Back, binVoxels, 0, 0, voxelsForeBack, expectedResultBack);
                }
            }
        }
    }

    private static void TestGetCachedVoxelNeighbors() {
        static void RunTest(Direction dir, byte voxelsRightLeft, byte voxelsUpDown, byte voxelsForeBack, byte expectedResult) {
            UnitTester.Assert<Direction, byte, byte, byte, byte>(
                "GetCachedVoxelNeighbors",
                GetCachedVoxelNeighbors,
                new UnitTester.Parameter("Direction", dir),
                new UnitTester.Parameter("VoxelNeighborsRightLeft", voxelsRightLeft),
                new UnitTester.Parameter("VoxelNeighborsUpDown", voxelsUpDown),
                new UnitTester.Parameter("VoxelNeighborsForeBack", voxelsForeBack),
                expectedResult
            );
        }

        RunTest(Direction.Right, 0b_1111_0000, 0, 0, 0b_0000);
        RunTest(Direction.Left, 0b_1111_0000, 0, 0, 0b_1111);
        RunTest(Direction.Up, 0, 0b_1111_0000, 0, 0b_0000);
        RunTest(Direction.Down, 0, 0b_1111_0000, 0, 0b_1111);
        RunTest(Direction.Fore, 0, 0, 0b_1111_0000, 0b_0000);
        RunTest(Direction.Back, 0, 0, 0b_1111_0000, 0b_1111);

        RunTest(Direction.Right, 0b_0011_1010, 0, 0, 0b_1010);
        RunTest(Direction.Left, 0b_0011_1010, 0, 0, 0b_0011);
        RunTest(Direction.Up, 0, 0b_0011_1010, 0, 0b_1010);
        RunTest(Direction.Down, 0, 0b_0011_1010, 0, 0b_0011);
        RunTest(Direction.Fore, 0, 0, 0b_0011_1010, 0b_1010);
        RunTest(Direction.Back, 0, 0, 0b_0011_1010, 0b_0011);
    }

    private static void TestHasOpenPathBetweenFaces() {
        Bin bin = new Bin(0, new Vector3Int(3, 3, 3));
        
        bin = SetBinAllVoxelsExists(bin, exists: false);

        for(int i0 = 0; i0 < 6; i0++) {
            for(int i1 = 0; i1 < 6; i1++) {
                if(i0 == i1) {
                    continue;
                }
                
                UnitTester.Assert<Direction, Direction, bool>(
                    "HasOpenPathBetweenFaces, No voxels",
                    bin.HasOpenPathBetweenFaces,
                    new UnitTester.Parameter("Face 1", i0),
                    new UnitTester.Parameter("Face 2", i1),
                    expectedResult: true
                );
            }
        }

        bin = SetBinAllVoxelsExists(bin, exists: true);

        for(int i0 = 0; i0 < 6; i0++) {
            for(int i1 = 0; i1 < 6; i1++) {
                if(i0 == i1) {
                    continue;
                }

                UnitTester.Assert<Direction, Direction, bool>(
                    "HasOpenPathBetweenFaces, All voxels",
                    bin.HasOpenPathBetweenFaces,
                    new UnitTester.Parameter("Face 1", i0),
                    new UnitTester.Parameter("Face 2", i1),
                    expectedResult: false
                );
            }
        }

        for(int i = 0; i < SIZE; i++) {
            bin = SetBinAllVoxelsExists(bin, exists: true);
            bin = SetBinVoxelExists(bin, i, exists: false);

            Vector3Int coords = LOCAL_COORDS_LOOKUP[i];

            TestCorners(Direction.Right, Direction.Left, false);
            TestCorners(Direction.Right, Direction.Up,      coords.x == 1 && coords.y == 1);
            TestCorners(Direction.Right, Direction.Down,    coords.x == 1 && coords.y == 0);
            TestCorners(Direction.Right, Direction.Fore,    coords.x == 1 && coords.z == 1);
            TestCorners(Direction.Right, Direction.Back,    coords.x == 1 && coords.z == 0);

            TestCorners(Direction.Left, Direction.Up,   coords.x == 0 && coords.y == 1);
            TestCorners(Direction.Left, Direction.Down, coords.x == 0 && coords.y == 0);
            TestCorners(Direction.Left, Direction.Fore, coords.x == 0 && coords.z == 1);
            TestCorners(Direction.Left, Direction.Back, coords.x == 0 && coords.z == 0);

            TestCorners(Direction.Up, Direction.Down, false);
            TestCorners(Direction.Up, Direction.Fore, coords.y == 1 && coords.z == 1);
            TestCorners(Direction.Up, Direction.Back, coords.y == 1 && coords.z == 0);

            TestCorners(Direction.Down, Direction.Fore, coords.y == 0 && coords.z == 1);
            TestCorners(Direction.Down, Direction.Back, coords.y == 0 && coords.z == 0);

            TestCorners(Direction.Fore, Direction.Back, false);

            void TestCorners(Direction face1, Direction face2, bool expectedResult) {
                UnitTester.Assert<Direction, Direction, bool>(
                    string.Format("HasOpenPathBetweenFaces, Voxel #{0} removed", i),
                    bin.HasOpenPathBetweenFaces,
                    new UnitTester.Parameter("Face 1", face1),
                    new UnitTester.Parameter("Face 2", face2),
                    expectedResult
                );
            }
        }

        TestOpposites(Direction.Right, Direction.Left);
        TestOpposites(Direction.Up, Direction.Down);
        TestOpposites(Direction.Fore, Direction.Back);

        void TestOpposites(Direction face1, Direction face2) {
            for(int i0 = 0; i0 < WIDTH; i0++) {
                for(int i1 = 0; i1 < WIDTH; i1++) {
                    for(int i2 = 0; i2 < VOXELS_PER_FACE; i2++) {
                        int index1 = FaceVoxelIndexToLocalVoxelIndex(i2, face1);
                        int index2 = FaceVoxelIndexToLocalVoxelIndex(i2, face2);

                        bin = SetBinAllVoxelsExists(bin, exists: true);
                        bin = SetBinVoxelExists(bin, index1, exists: false);
                        bin = SetBinVoxelExists(bin, index2, exists: false);

                        UnitTester.Assert<Direction, Direction, bool>(
                            string.Format("HasOpenPathBetweenFaces, Voxel #{0} and #{1} removed", index1, index2),
                            bin.HasOpenPathBetweenFaces,
                            new UnitTester.Parameter("Face 1", face1),
                            new UnitTester.Parameter("Face 2", face2),
                            true
                        );
                    }
                }
            }
        }

        TestDiagonals(Direction.Right, Direction.Left);
        TestDiagonals(Direction.Up, Direction.Down);
        TestDiagonals(Direction.Fore, Direction.Back);

        void TestDiagonals(Direction face1, Direction face2) {
            for(int i0 = 0; i0 < WIDTH; i0++) {
                for(int i1 = 0; i1 < WIDTH; i1++) {
                    for(int i2 = 0; i2 < VOXELS_PER_FACE; i2++) {
                        int index1 = FaceVoxelIndexToLocalVoxelIndex(i2, face1);
                        int index2 = FaceVoxelIndexToLocalVoxelIndex((VOXELS_PER_FACE - 1) - i2, face2);

                        bin = SetBinAllVoxelsExists(bin, exists: true);
                        bin = SetBinVoxelExists(bin, index1, exists: false);
                        bin = SetBinVoxelExists(bin, index2, exists: false);

                        UnitTester.Assert<Direction, Direction, bool>(
                            string.Format("HasOpenPathBetweenFaces, Voxel #{0} and #{1} removed", index1, index2),
                            bin.HasOpenPathBetweenFaces,
                            new UnitTester.Parameter("Face 1", face1),
                            new UnitTester.Parameter("Face 2", face2),
                            false
                        );
                    }
                }
            }
        }
    }
}
