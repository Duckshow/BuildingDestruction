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
        TestGetVoxelNeighborStatuses();
        TestDoesFilledVoxelExist();
        TestTryGetVoxelAddress();
        TestVoxelIndexToVoxelAddressAndViceVersa();
        TestVoxelAddressToVoxelCoordsAndViceVersa();
        TestGetVoxelAddressNeighbor();
        TestGetBiggestVoxelClusterIndex();
        TestGetPivot();
    }

    public static void TestCalculateVoxelGridDimensions() {
        for(int i = 0; i < 100; i++) {
            Vector3Int binGridDimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));

            Assert(
                "CalculateVoxelGridDimensions()", 
                CalculateVoxelGridDimensions, 
                new Parameter("BinGridDimensions", binGridDimensions), 
                expectedResult: binGridDimensions * Bin.WIDTH
            );
        }
    }

    public static void TestAreCoordsWithinDimensions() {
        for(int i = 0; i < 100; i++) {
            Vector3Int dimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));
            Vector3Int coords = new Vector3Int(Random.Range(0, dimensions.x), Random.Range(0, dimensions.y), Random.Range(0, dimensions.z));
            
            Assert(
                "AreCoordsWithinDimensions()",
                AreCoordsWithinDimensions,
                new Parameter("Coords", coords),
                new Parameter("Dimensions", dimensions),
                expectedResult: true
            );

            Assert(
                "AreCoordsWithinDimensions()",
                AreCoordsWithinDimensions,
                new Parameter("Coords", dimensions),
                new Parameter("Dimensions", dimensions),
                expectedResult: false
            );
        }
    }

    private static void TestCoordsToIndexAndViceVersa() {
        for(int i = 0; i < 100; i++) {
            Vector3Int dimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));

            void TestCoordsToIndex(Vector3Int coords, int expectedResult) {
                Assert(
                    "CoordsToIndex()",
                    CoordsToIndex,
                    new Parameter("Coords", coords),
                    new Parameter("Dimensions", dimensions),
                    expectedResult
                );
            }

            void TestIndexToCoords(int index, Vector3Int expectedResult) {
                Assert(
                    "IndexToCoords()",
                    IndexToCoords,
                    new Parameter("Index", index),
                    new Parameter("Dimensions", dimensions),
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
            Assert(
                "GetDirectionVector()",
                GetDirectionVector,
                new Parameter("Direction", dir),
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

    private static void TestGetVoxelNeighborStatuses() {        
        for(int i = 0; i < 100; i++) {
            Vector3Int binGridDimensions = new Vector3Int(Random.Range(3, 10), Random.Range(3, 10), Random.Range(3, 10));
            Bin[] bins = GetBinsForTesting(binGridDimensions);

            void RunTest(Vector3Int coords, NeighborRelationships expectedResults) {
                Assert(
                    "GetVoxelNeighborStatuses()",
                    GetVoxelNeighborStatuses,
                    new Parameter("Coords", coords),
                    new Parameter("Bins", bins),
                    new Parameter("Dimensions", binGridDimensions),
                    expectedResults
                );
            }

            Vector3Int voxelGridDimensions = CalculateVoxelGridDimensions(binGridDimensions);

            for(int z = 0; z < binGridDimensions.z; z++) {
                for(int y = 0; y < binGridDimensions.y; y++) {
                    for(int x = 0; x < binGridDimensions.x; x++) {
                        for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {

                            int binIndex = CoordsToIndex(new Vector3Int(x, y, z), binGridDimensions);
                            Vector3Int voxelCoords = VoxelAddressToVoxelCoords(new VoxelAddress(binIndex, localVoxelIndex), binGridDimensions);
                            
                            RunTest(voxelCoords, expectedResults: new NeighborRelationships(
                                     right: voxelCoords.x < voxelGridDimensions.x - 1,
                                     left:  voxelCoords.x > 0,
                                     up:    voxelCoords.y < voxelGridDimensions.y - 1,
                                     down:  voxelCoords.y > 0,
                                     fore:  voxelCoords.z < voxelGridDimensions.z - 1,
                                     back:  voxelCoords.z > 0
                                 )
                             );
                        }
                    }
                }
            }

            RunTest(Vector3Int.zero, new NeighborRelationships(
                    right:  true,
                    left:   false,
                    up:     true,
                    down:   false,
                    fore:   true,
                    back:   false
                )
            );

            bins[0].SetVoxelIsFilled(1, false);

            RunTest(Vector3Int.zero, new NeighborRelationships(
                    right:  false,
                    left:   false,
                    up:     true,
                    down:   false,
                    fore:   true,
                    back:   false
                )
            );
        }
    }

    private static void TestDoesFilledVoxelExist() {
        for(int i = 0; i < 100; i++) {
            Vector3Int binGridDimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));
            Bin[] bins = GetBinsForTesting(binGridDimensions);

            void RunTest(Vector3Int coords, bool expectedResult) {
                Assert(
                    "DoesFilledVoxelExist()",
                    DoesFilledVoxelExist,
                    new Parameter("Coords", coords),
                    new Parameter("Bins", bins),
                    new Parameter("Dimensions", binGridDimensions),
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

                            Assert(
                                "TryGetVoxelAddress()",
                                result: TryGetVoxelAddress(voxelCoords, binGridDimensions, out resultsAddress),
                                expectedResult: true,
                                new Parameter("Coords", voxelCoords),
                                new Parameter("Dimensions", binGridDimensions)
                            );

                            Assert(
                                "TryGetVoxelAddress()",
                                result: resultsAddress,
                                expectedResult: actualAddress,
                                new Parameter("Coords", voxelCoords),
                                new Parameter("Dimensions", binGridDimensions)
                            );
                        }
                    }
                }
            }

            Vector3Int testCoords;
            VoxelAddress address;

            testCoords = -Vector3Int.one;
            Assert(
                "TryGetVoxelAddress()",
                result: TryGetVoxelAddress(testCoords, binGridDimensions, out address),
                expectedResult: false,
                new Parameter("Coords", testCoords),
                new Parameter("Dimensions", binGridDimensions)
            );

            testCoords = CalculateVoxelGridDimensions(binGridDimensions);
            Assert(
                "TryGetVoxelAddress()",
                result: TryGetVoxelAddress(testCoords, binGridDimensions, out address),
                expectedResult: false,
                new Parameter("Coords", testCoords),
                new Parameter("Dimensions", binGridDimensions)
            );
        }
    }

    private static void TestVoxelIndexToVoxelAddressAndViceVersa() {
        for(int i = 0; i < 100; i++) {
            Vector3Int binGridDimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));
            Vector3Int voxelGridDimensions = CalculateVoxelGridDimensions(binGridDimensions);

            int globalVoxelIndex = 0;
            for(int z = 0; z < voxelGridDimensions.z; z++) {
                for(int y = 0; y < voxelGridDimensions.y; y++) {
                    for(int x = 0; x < voxelGridDimensions.x; x++) {

                        Vector3Int voxelCoords = new Vector3Int(x, y, z);
                        Vector3Int binCoords = voxelCoords / Bin.WIDTH;
                        Vector3Int localVoxelCoords = voxelCoords - binCoords * Bin.WIDTH;

                        int binIndex = CoordsToIndex(binCoords, binGridDimensions);
                        int localVoxelIndex = CoordsToIndex(localVoxelCoords, Bin.WIDTH);

                        VoxelAddress actualAddress = new VoxelAddress(binIndex, localVoxelIndex);


                        Assert(
                            "VoxelIndexToVoxelAddress()",
                            VoxelIndexToVoxelAddress,
                            new Parameter("Index", globalVoxelIndex),
                            new Parameter("Dimensions", binGridDimensions),
                            expectedResult: actualAddress
                        );

                        Assert(
                            "VoxelAddressToVoxelIndex()",
                            VoxelAddressToVoxelIndex,
                            new Parameter("Address", actualAddress),
                            new Parameter("Dimensions", binGridDimensions),
                            expectedResult: globalVoxelIndex
                        );

                        globalVoxelIndex++;
                    }
                }
            }
        }
    }

    public static void TestVoxelAddressToVoxelCoordsAndViceVersa() {
        for(int i = 0; i < 100; i++) {
            Vector3Int binGridDimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));

            int binIndex = 0;
            for(int z = 0; z < binGridDimensions.z; z++) {
                for(int y = 0; y < binGridDimensions.y; y++) {
                    for(int x = 0; x < binGridDimensions.x; x++) {
                        for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {
                            
                            VoxelAddress address = new VoxelAddress(binIndex, localVoxelIndex);
                            Vector3Int actualCoords = new Vector3Int(x, y, z) * Bin.WIDTH + IndexToCoords(localVoxelIndex, Bin.WIDTH);

                            Assert(
                               "VoxelAddressToVoxelCoords()",
                               VoxelAddressToVoxelCoords,
                               new Parameter("Address", address),
                               new Parameter("Dimensions", binGridDimensions),
                               expectedResult: actualCoords
                            );

                            Assert(
                               "VoxelCoordsToVoxelAddress()",
                               VoxelCoordsToVoxelAddress,
                               new Parameter("Coords", actualCoords),
                               new Parameter("Dimensions", binGridDimensions),
                               expectedResult: address
                            );
                        }

                        binIndex++;
                    }
                }
            }
        }
    }

    public static void TestGetVoxelAddressNeighbor() {
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
                                Assert(
                                    "GetVoxelAddressNeighbor()",
                                    GetVoxelAddressNeighbor,
                                    new Parameter("Address", VoxelCoordsToVoxelAddress(globalVoxelCoords + GetDirectionVector(dir), binGridDimensions)),
                                    new Parameter("Dimensions", binGridDimensions),
                                    new Parameter("Direction", GetOppositeDirection(dir)),
                                    expectedResult: address
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

        Assert(
            "GetBiggestVoxelClusterIndex()",
            GetBiggestVoxelClusterIndex,
            new Parameter("List", list),
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

        bool isStatic;
        Vector3 pivot;

        isStatic = false;
        pivot = GetPivot(cluster, isStatic, Vector3Int.zero);
        Debug.LogFormat("GetClusterPivot Result = {0} in hollow cube measuring {1}, isStatic: {2}", pivot, VoxelGrid.CalculateVoxelGridDimensions(dimensions), isStatic);

        isStatic = true;
        pivot = GetPivot(cluster, true, Vector3Int.zero);
        Debug.LogFormat("GetClusterPivot Result = {0} in hollow cube measuring {1}, isStatic: {2}", pivot, VoxelGrid.CalculateVoxelGridDimensions(dimensions), isStatic);
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

    private static void Assert(string testName, Func<Vector3Int, Vector3Int> test, Parameter param, Vector3Int expectedResult) {
        Vector3Int result = test((Vector3Int)param.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param));
    }

    private static void Assert(string testName, Func<List<VoxelCluster>, int> test, Parameter param, int expectedResult) {
        int result = test((List<VoxelCluster>)param.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param));
    }

    private static void Assert(string testName, Func<Direction, Vector3Int> test, Parameter param, Vector3Int expectedResult) {
        Vector3Int result = test((Direction)param.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param));
    }

    private static void Assert(string testName, Func<VoxelAddress, Vector3Int, Direction[], VoxelAddress> test, Parameter param1, Parameter param2, Parameter param3, VoxelAddress expectedResult) {
        VoxelAddress result = test((VoxelAddress)param1.Value, (Vector3Int)param2.Value, (Direction[])param3.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2, param3));
    }

    private static void Assert(string testName, Func<Vector3Int, Vector3Int, bool> test, Parameter param1, Parameter param2, bool expectedResult) {
        bool result = test((Vector3Int)param1.Value, (Vector3Int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    private static void Assert(string testName, Func<Vector3Int, Vector3Int, int> test, Parameter param1, Parameter param2, int expectedResult) {
        int result = test((Vector3Int)param1.Value, (Vector3Int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    private static void Assert(string testName, Func<int, Vector3Int, Vector3Int> test, Parameter param1, Parameter param2, Vector3Int expectedResult) {
        Vector3Int result = test((int)param1.Value, (Vector3Int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    private static void Assert(string testName, Func<int, Vector3Int, VoxelAddress> test, Parameter param1, Parameter param2, VoxelAddress expectedResult) {
        VoxelAddress result = test((int)param1.Value, (Vector3Int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    private static void Assert(string testName, Func<VoxelAddress, Vector3Int, int> test, Parameter param1, Parameter param2, int expectedResult) {
        int result = test((VoxelAddress)param1.Value, (Vector3Int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    private static void Assert(string testName, Func<VoxelAddress, Vector3Int, Vector3Int> test, Parameter param1, Parameter param2, Vector3Int expectedResult) {
        Vector3Int result = test((VoxelAddress)param1.Value, (Vector3Int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    private static void Assert(string testName, Func<Vector3Int, Vector3Int, VoxelAddress> test, Parameter param1, Parameter param2, VoxelAddress expectedResult) {
        VoxelAddress result = test((Vector3Int)param1.Value, (Vector3Int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    private static void Assert(string testName, Func<Vector3Int, Bin[], Vector3Int, NeighborRelationships> test, Parameter param1, Parameter param2, Parameter param3, NeighborRelationships expectedResult) {
        NeighborRelationships result = test((Vector3Int)param1.Value, (Bin[])param2.Value, (Vector3Int)param3.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2, param3));
    }

    private static void Assert(string testName, Func<Vector3Int, Bin[], Vector3Int, bool> test, Parameter param1, Parameter param2, Parameter param3, bool expectedResult) {
        bool result = test((Vector3Int)param1.Value, (Bin[])param2.Value, (Vector3Int)param3.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2, param3));
    }

    private static void Assert(string testName, bool result, bool expectedResult, params Parameter[] parameters) {
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, parameters));
    }

    private static void Assert(string testName, VoxelAddress result, VoxelAddress expectedResult, params Parameter[] parameters) {
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, parameters));
    }

    private static string GetMessage(string testName, object result, object expectedResult, params Parameter[] parameters) {
        string message = string.Format("============== Fail: {0} ==============\n", testName);
        message += "\n";
        message += "Parameters:\n";
        foreach(Parameter parameter in parameters) {
            message += string.Format("\t-{0}: {1}\n", parameter.Name, parameter.Value);
        }
        message += "\n";
        message += string.Format("Expected Result: {0}\n", expectedResult);
        message += string.Format("Actual Result: {0}\n", result);
        message += "============================";

        return message;
    }

    private struct Parameter {
        public string Name;
        public object Value;

        public Parameter(string name, object value) {
            Name = name;
            Value = value;
        }
    }
}
