using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public partial class VoxelGrid
{
    public static void RunTests() {
        TestCalculateVoxelGridDimensions();
        TestAreCoordsWithinDimensions();
        TestCoordsToIndexAndViceVersa();
        TestGetDirectionVector();
        TestDoesFilledVoxelExist();
        TestTryGetVoxelAddress();
        TestVoxelAddressToVoxelCoordsAndViceVersa();
        TestGetVoxelAddressNeighbor();
        TestGetBiggestVoxelClusterIndex();
        TestGetPivot();
    }

    private static void TestCalculateVoxelGridDimensions() {
        for(int i = 0; i < 100; i++) {
            Vector3Int binGridDimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));

            UnitTester.Assert(
                "CalculateVoxelGridDimensions()", 
                CalculateVoxelGridDimensions, 
                new UnitTester.Parameter("BinGridDimensions", binGridDimensions), 
                expectedResult: binGridDimensions * Bin.WIDTH
            );
        }
    }

    private static void TestAreCoordsWithinDimensions() {
        for(int i = 0; i < 100; i++) {
            Vector3Int dimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));
            Vector3Int coords = new Vector3Int(Random.Range(0, dimensions.x), Random.Range(0, dimensions.y), Random.Range(0, dimensions.z));

            UnitTester.Assert(
                "AreCoordsWithinDimensions()",
                AreCoordsWithinDimensions,
                new UnitTester.Parameter("Coords", coords),
                new UnitTester.Parameter("Dimensions", dimensions),
                expectedResult: true
            );

            UnitTester.Assert(
                "AreCoordsWithinDimensions()",
                AreCoordsWithinDimensions,
                new UnitTester.Parameter("Coords", dimensions),
                new UnitTester.Parameter("Dimensions", dimensions),
                expectedResult: false
            );
        }
    }

    private static void TestCoordsToIndexAndViceVersa() {
        for(int i = 0; i < 100; i++) {
            Vector3Int dimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));

            void TestCoordsToIndex(Vector3Int coords, int expectedResult) {
                UnitTester.Assert(
                    "CoordsToIndex()",
                    CoordsToIndex,
                    new UnitTester.Parameter("Coords", coords),
                    new UnitTester.Parameter("Dimensions", dimensions),
                    expectedResult
                );
            }

            void TestIndexToCoords(int index, Vector3Int expectedResult) {
                UnitTester.Assert(
                    "IndexToCoords()",
                    IndexToCoords,
                    new UnitTester.Parameter("Index", index),
                    new UnitTester.Parameter("Dimensions", dimensions),
                    expectedResult
                );
            }

            int actualIndex = 0;
            Vector3Int actualCoords = new Vector3Int(0, 0, 0);

            for(int z = 0; z < dimensions.z; z++) {
                for(int y = 0; y < dimensions.y; y++) {
                    for(int x = 0; x < dimensions.x; x++) {
                        actualCoords = new Vector3Int(x, y, z);

                        TestCoordsToIndex(actualCoords, actualIndex);
                        TestIndexToCoords(actualIndex, actualCoords);

                        actualIndex++;
                    }
                }
            }

            TestCoordsToIndex(dimensions, -1);
            TestCoordsToIndex(new Vector3Int(0, 0, -1), -1);
            
            TestIndexToCoords(-1, -Vector3Int.one);
            TestIndexToCoords(dimensions.x * dimensions.y * dimensions.z, -Vector3Int.one);
        }
    }

    private static void TestGetDirectionVector() {
        static void RunTest(Direction dir, Vector3Int expectedResult) {
            UnitTester.Assert(
                "GetDirectionVector()",
                GetDirectionVector,
                new UnitTester.Parameter("Direction", dir),
                expectedResult
            );
        }

        RunTest(Direction.None,     Vector3Int.zero);
        RunTest(Direction.Right,    Vector3Int.right);
        RunTest(Direction.Left,     Vector3Int.left);
        RunTest(Direction.Up,       Vector3Int.up);
        RunTest(Direction.Down,     Vector3Int.down);
        RunTest(Direction.Fore,     Vector3Int.forward);
        RunTest(Direction.Back,     Vector3Int.back);
    }

    private static void TestDoesFilledVoxelExist() {
        for(int i = 0; i < 100; i++) {
            Vector3Int binGridDimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));
            Bin[] bins = GetBinsForTesting(binGridDimensions);

            void RunTest(Vector3Int coords, bool expectedResult) {
                UnitTester.Assert(
                    "DoesFilledVoxelExist()",
                    DoesFilledVoxelExist,
                    new UnitTester.Parameter("Coords", coords),
                    new UnitTester.Parameter("Bins", bins),
                    new UnitTester.Parameter("Dimensions", binGridDimensions),
                    expectedResult
                );
            }

            for(int binIndex = 0; binIndex < bins.Length; binIndex++) {
                for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {

                    VoxelAddress address = new VoxelAddress(binIndex, localVoxelIndex);
                    Vector3Int globalVoxelCoords = VoxelAddressToVoxelCoords(address, binGridDimensions);

                    RunTest(globalVoxelCoords, true);
                    bins[address.BinIndex].SetVoxelIsFilled(address.LocalVoxelIndex, false);
                    RunTest(globalVoxelCoords, false);
                }
            }

            RunTest(-Vector3Int.one, false);
            RunTest(CalculateVoxelGridDimensions(binGridDimensions), false);
        }
    }

    private static void TestTryGetVoxelAddress() {
        for(int i = 0; i < 100; i++) {
            Vector3Int binGridDimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));

            for(int z = 0; z < binGridDimensions.z; z++) {
                for(int y = 0; y < binGridDimensions.y; y++) {
                    for(int x = 0; x < binGridDimensions.x; x++) {
                        for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {

                            int binIndex = CoordsToIndex(x, y, z, binGridDimensions);
                            VoxelAddress actualAddress = new VoxelAddress(binIndex, localVoxelIndex);
                            Vector3Int voxelCoords = VoxelAddressToVoxelCoords(actualAddress, binGridDimensions);

                            VoxelAddress resultsAddress;

                            UnitTester.Assert(
                                "TryGetVoxelAddress()",
                                result: TryGetVoxelAddress(voxelCoords, binGridDimensions, out resultsAddress),
                                expectedResult: true,
                                new UnitTester.Parameter("Coords", voxelCoords),
                                new UnitTester.Parameter("Dimensions", binGridDimensions)
                            );

                            UnitTester.Assert(
                                "TryGetVoxelAddress()",
                                result: resultsAddress,
                                expectedResult: actualAddress,
                                new UnitTester.Parameter("Coords", voxelCoords),
                                new UnitTester.Parameter("Dimensions", binGridDimensions)
                            );
                        }
                    }
                }
            }

            Vector3Int testCoords;
            VoxelAddress address;

            testCoords = -Vector3Int.one;
            UnitTester.Assert(
                "TryGetVoxelAddress()",
                result: TryGetVoxelAddress(testCoords, binGridDimensions, out address),
                expectedResult: false,
                new UnitTester.Parameter("Coords", testCoords),
                new UnitTester.Parameter("Dimensions", binGridDimensions)
            );

            testCoords = CalculateVoxelGridDimensions(binGridDimensions);
            UnitTester.Assert(
                "TryGetVoxelAddress()",
                result: TryGetVoxelAddress(testCoords, binGridDimensions, out address),
                expectedResult: false,
                new UnitTester.Parameter("Coords", testCoords),
                new UnitTester.Parameter("Dimensions", binGridDimensions)
            );
        }
    }

    private static void TestVoxelAddressToVoxelCoordsAndViceVersa() {
        for(int i = 0; i < 100; i++) {
            Vector3Int binGridDimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));

            int binIndex = 0;
            for(int z = 0; z < binGridDimensions.z; z++) {
                for(int y = 0; y < binGridDimensions.y; y++) {
                    for(int x = 0; x < binGridDimensions.x; x++) {
                        for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {
                            
                            VoxelAddress address = new VoxelAddress(binIndex, localVoxelIndex);
                            Vector3Int actualCoords = new Vector3Int(x, y, z) * Bin.WIDTH + IndexToCoords(localVoxelIndex, Bin.WIDTH);

                            UnitTester.Assert(
                               "VoxelAddressToVoxelCoords()",
                               VoxelAddressToVoxelCoords,
                               new UnitTester.Parameter("Address", address),
                               new UnitTester.Parameter("Dimensions", binGridDimensions),
                               expectedResult: actualCoords
                            );

                            UnitTester.Assert(
                               "VoxelCoordsToVoxelAddress()",
                               VoxelCoordsToVoxelAddress,
                               new UnitTester.Parameter("Coords", actualCoords),
                               new UnitTester.Parameter("Dimensions", binGridDimensions),
                               expectedResult: address
                            );
                        }

                        binIndex++;
                    }
                }
            }
        }
    }

    private static void TestGetVoxelAddressNeighbor() {
        for(int i = 0; i < 100; i++) {
            Vector3Int binGridDimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));
            Vector3Int voxelGridDimensions = CalculateVoxelGridDimensions(binGridDimensions);

            int globalVoxelIndex = 0;
            for(int z = 0; z < binGridDimensions.z; z++) {
                for(int y = 0; y < binGridDimensions.y; y++) {
                    for(int x = 0; x < binGridDimensions.x; x++) {
                        for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {
                            Vector3Int globalVoxelCoords = IndexToCoords(globalVoxelIndex, voxelGridDimensions);
                            VoxelAddress address = VoxelCoordsToVoxelAddress(globalVoxelCoords, binGridDimensions);

                            void RunTestFromDirection(Direction dir) {
                                UnitTester.Assert(
                                    "TryGetVoxelAddressNeighbor()",
                                    TryGetVoxelAddressNeighbor,
                                    new UnitTester.Parameter("Address", VoxelCoordsToVoxelAddress(globalVoxelCoords + GetDirectionVector(dir), binGridDimensions)),
                                    new UnitTester.Parameter("Dimensions", binGridDimensions),
                                    new UnitTester.Parameter("Direction", GetOppositeDirection(dir)),
                                    expectedResult1: true,
                                    expectedResult2: address
                                );
                            }

                            if(globalVoxelCoords.x < binGridDimensions.x - 1) {
                                RunTestFromDirection(Direction.Right);
                            }

                            if(globalVoxelCoords.x > 0) {
                                RunTestFromDirection(Direction.Left);
                            }

                            if(globalVoxelCoords.y < binGridDimensions.y - 1) {
                                RunTestFromDirection(Direction.Up);
                            }

                            if(globalVoxelCoords.y > 0) {
                                RunTestFromDirection(Direction.Down);
                            }

                            if(globalVoxelCoords.z < binGridDimensions.z - 1) {
                                RunTestFromDirection(Direction.Fore);
                            }

                            if(globalVoxelCoords.z > 0) {
                                RunTestFromDirection(Direction.Back);
                            }

                            globalVoxelIndex++;
                        }
                    }
                }
            }
        }
    }

    private static void TestGetBiggestVoxelClusterIndex() {
        int biggest = Random.Range(1000, 10000);

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

        UnitTester.Assert(
            "GetBiggestVoxelClusterIndex()",
            GetBiggestVoxelClusterIndex,
            new UnitTester.Parameter("List", list),
            expectedResult: biggestIndex
        );
    }

    private static void TestGetPivot() {
        Vector3Int dimensions = new Vector3Int(8, 8, 8);
        Bin[] cluster = new Bin[dimensions.x * dimensions.y * dimensions.z];

        int binIndex = 0;
        for(int z = 0; z < dimensions.z; z++) {
            for(int y = 0; y < dimensions.y; y++) {
                for(int x = 0; x < dimensions.x; x++) {
                    bool isFilled = x == 0 || y == 0 || z == 0 || x == dimensions.x - 1 || y == dimensions.y - 1 || z == dimensions.z - 1;

                    cluster[binIndex] = new Bin(binIndex, dimensions);

                    for(int voxelIndex = 0; voxelIndex < Bin.SIZE; voxelIndex++) {
                        cluster[binIndex].SetVoxelIsFilled(voxelIndex, isFilled);
                    }

                    binIndex++;
                }
            }
        }

        UnitTester.Assert(
            "GetPivot",
            GetPivot,
            new UnitTester.Parameter("Cluster", cluster),
            new UnitTester.Parameter("IsStatic", false),
            expectedResult: new Vector3((dimensions.x * Bin.WIDTH - 1) / 2f, (dimensions.y * Bin.WIDTH - 1) / 2f, (dimensions.z * Bin.WIDTH - 1) / 2f)
        );

        UnitTester.Assert(
            "GetPivot",
            GetPivot,
            new UnitTester.Parameter("Cluster", cluster),
            new UnitTester.Parameter("IsStatic", true),
            expectedResult: new Vector3((dimensions.x * Bin.WIDTH - 1) / 2f, -0.5f, (dimensions.z * Bin.WIDTH - 1) / 2f)
        );
    }

    private static Bin[] GetBinsForTesting(Vector3Int dimensions) {
        int length = dimensions.x * dimensions.y * dimensions.z;

        Bin[] bins = new Bin[length];

        for(int binIndex = 0; binIndex < length; binIndex++) {
            bins[binIndex] = new Bin(binIndex, dimensions);

            for(int voxelIndex = 0; voxelIndex < Bin.SIZE; voxelIndex++) {
                bins[binIndex].SetVoxelIsFilled(voxelIndex, true);
            }
        }

        return bins;
    }
}
