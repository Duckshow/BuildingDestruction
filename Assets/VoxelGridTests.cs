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
        TestGetVoxelExists();
        TestGetPivot();
        Debug.Log("Tests done.");
    }

    private static void TestCalculateVoxelGridDimensions() {
        for(int i = 0; i < 25; i++) {
            Vector3Int binGridDimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));

            UnitTester.Assert<Vector3Int, Vector3Int>(
                "CalculateVoxelGridDimensions()", 
                CalculateVoxelGridDimensions, 
                new UnitTester.Parameter("BinGridDimensions", binGridDimensions), 
                expectedResult: binGridDimensions * Bin.WIDTH
            );
        }
    }

    private static void TestAreCoordsWithinDimensions() {
        for(int i = 0; i < 25; i++) {
            Vector3Int dimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));
            Vector3Int coords = new Vector3Int(Random.Range(0, dimensions.x), Random.Range(0, dimensions.y), Random.Range(0, dimensions.z));

            UnitTester.Assert<Vector3Int, Vector3Int, bool>(
                "AreCoordsWithinDimensions()",
                AreCoordsWithinDimensions,
                new UnitTester.Parameter("Coords", coords),
                new UnitTester.Parameter("Dimensions", dimensions),
                expectedResult: true
            );

            UnitTester.Assert<Vector3Int, Vector3Int, bool>(
                "AreCoordsWithinDimensions()",
                AreCoordsWithinDimensions,
                new UnitTester.Parameter("Coords", dimensions),
                new UnitTester.Parameter("Dimensions", dimensions),
                expectedResult: false
            );
        }
    }

    private static void TestCoordsToIndexAndViceVersa() {
        for(int i = 0; i < 25; i++) {
            Vector3Int dimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));

            void TestCoordsToIndex(Vector3Int coords, int expectedResult) {
                UnitTester.Assert<Vector3Int, Vector3Int, int>(
                    "CoordsToIndex()",
                    CoordsToIndex,
                    new UnitTester.Parameter("Coords", coords),
                    new UnitTester.Parameter("Dimensions", dimensions),
                    expectedResult
                );
            }

            void TestIndexToCoords(int index, Vector3Int expectedResult) {
                UnitTester.Assert<int, Vector3Int, Vector3Int>(
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

    private static void TestGetVoxelExists() {
        for(int i = 0; i < 25; i++) {
            Vector3Int binGridDimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));
            Bin[] bins = UnitTester.GetBinsForTesting(binGridDimensions);

            void RunTest(Vector3Int coords, bool expectedResult) {
                UnitTester.Assert<Vector3Int, Bin[], Vector3Int, bool>(
                    "GetVoxelExists",
                    GetVoxelExists,
                    new UnitTester.Parameter("Coords", coords),
                    new UnitTester.Parameter("Bins", bins),
                    new UnitTester.Parameter("Dimensions", binGridDimensions),
                    expectedResult
                );
            }

            for(int binIndex = 0; binIndex < bins.Length; binIndex++) {
                for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {
                    Vector3Int globalVoxelCoords = Bin.GetVoxelGlobalCoords(binIndex, localVoxelIndex, binGridDimensions);

                    RunTest(globalVoxelCoords, true);
                    bins[binIndex].SetVoxelExists(localVoxelIndex, exists: false);
                    RunTest(globalVoxelCoords, false);
                }
            }

            RunTest(-Vector3Int.one, false);
            RunTest(CalculateVoxelGridDimensions(binGridDimensions), false);
        }
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
                    cluster[binIndex].SetAllVoxelExists(isFilled);

                    binIndex++;
                }
            }
        }

        UnitTester.Assert<Bin[], Vector3Int, bool, Vector3>(
            "GetPivot",
            GetPivot,
            new UnitTester.Parameter("Cluster", cluster),
            new UnitTester.Parameter("Dimensions", dimensions),
            new UnitTester.Parameter("IsStatic", false),
            expectedResult: new Vector3((dimensions.x * Bin.WIDTH - 1) / 2f, (dimensions.y * Bin.WIDTH - 1) / 2f, (dimensions.z * Bin.WIDTH - 1) / 2f)
        );

        UnitTester.Assert<Bin[], Vector3Int, bool, Vector3>(
            "GetPivot",
            GetPivot,
            new UnitTester.Parameter("Cluster", cluster),
            new UnitTester.Parameter("Dimensions", dimensions),
            new UnitTester.Parameter("IsStatic", true),
            expectedResult: new Vector3((dimensions.x * Bin.WIDTH - 1) / 2f, -0.5f, (dimensions.z * Bin.WIDTH - 1) / 2f)
        );
    }
}
