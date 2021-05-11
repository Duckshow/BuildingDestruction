using UnityEngine;
using System.Collections.Generic;

public static partial class VoxelClusterHandler {
    public static void RunTests() {
        TestGetBiggestVoxelClusterIndex();
        TestTryFindCluster();
        TestMoveBinsToNewGrid();
        TestMarkExteriorBins();
        Debug.Log("Tests done.");
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

        UnitTester.Assert<List<VoxelCluster>, int>(
            "GetBiggestVoxelClusterIndex()",
            GetBiggestVoxelClusterIndex,
            new UnitTester.Parameter("List", list),
            expectedResult: biggestIndex
        );
    }

    private static void TestTryFindCluster() {
        Vector3Int binGridDimensions = new Vector3Int(3, 3, 3);
        int binCount = binGridDimensions.x * binGridDimensions.y * binGridDimensions.z;
        
        Bin[] bins;
        bool[] visitedBins;
        VoxelCluster cluster0, cluster1;

        // ================ Test solid block ================ 

        bins = new Bin[binCount];
        visitedBins = new bool[binCount];

        for(int i = 0; i < binCount; i++) {
            AddBin(VoxelGrid.IndexToCoords(i, binGridDimensions));
        }
        RefreshBinGridConnectivity(bins, binGridDimensions);

        cluster0 = TryFindCluster(0, bins, binGridDimensions, visitedBins);

        UnitTester.Assert(
            "Testing Solid Block, Cluster Bin Count", 
            cluster0.Bins.Length == binCount, 
            expectedResult: true, 
            new UnitTester.Parameter("Cluster Bin Count", cluster0.Bins.Length), 
            new UnitTester.Parameter("Expected Bin Count", binCount)
        );

        UnitTester.Assert(
            "Testing Solid Block, Cluster Dimensions", 
            cluster0.Dimensions == binGridDimensions, 
            expectedResult: true, 
            new UnitTester.Parameter("Cluster Dimensions", cluster0.Dimensions), 
            new UnitTester.Parameter("Expected Dimensions", binGridDimensions)
        );

        UnitTester.Assert(
            "Testing Solid Block, Cluster Offset",
            cluster0.VoxelOffset == Vector3Int.zero,
            expectedResult: true,
            new UnitTester.Parameter("Cluster Offset", cluster0.VoxelOffset),
            new UnitTester.Parameter("Expected Offset", Vector3Int.zero)
        );

        // ================ Test arch-shape ================ 

        bins = new Bin[binCount];
        visitedBins = new bool[binCount];

        AddBin(new Vector3Int(0, 0, 0));
        AddBin(new Vector3Int(0, 0, 1));
        AddBin(new Vector3Int(0, 0, 2));

        AddBin(new Vector3Int(0, 1, 0));
        AddBin(new Vector3Int(0, 1, 1));
        AddBin(new Vector3Int(0, 1, 2));

        AddBin(new Vector3Int(0, 2, 0));
        AddBin(new Vector3Int(0, 2, 1));
        AddBin(new Vector3Int(0, 2, 2));

        AddBin(new Vector3Int(1, 2, 0));
        AddBin(new Vector3Int(1, 2, 1));
        AddBin(new Vector3Int(1, 2, 2));

        AddBin(new Vector3Int(2, 0, 0));
        AddBin(new Vector3Int(2, 0, 1));
        AddBin(new Vector3Int(2, 0, 2));

        AddBin(new Vector3Int(2, 1, 0));
        AddBin(new Vector3Int(2, 1, 1));
        AddBin(new Vector3Int(2, 1, 2));

        AddBin(new Vector3Int(2, 2, 0));
        AddBin(new Vector3Int(2, 2, 1));
        AddBin(new Vector3Int(2, 2, 2));

        RefreshBinGridConnectivity(bins, binGridDimensions);

        cluster0 = TryFindCluster(0, bins, binGridDimensions, visitedBins);

        UnitTester.Assert(
            "Testing Arch Block, Cluster Bin Count", 
            cluster0.Bins.Length == cluster0.Dimensions.x * cluster0.Dimensions.y * cluster0.Dimensions.z, 
            expectedResult: true, 
            new UnitTester.Parameter("Cluster Bin Count", cluster0.Bins.Length), 
            new UnitTester.Parameter("Expected Bin Count", cluster0.Dimensions.x * cluster0.Dimensions.y * cluster0.Dimensions.z)
        );

        UnitTester.Assert(
            "Testing Arch Block, Cluster Dimensions", 
            cluster0.Dimensions == binGridDimensions, 
            expectedResult: true, 
            new UnitTester.Parameter("Cluster Dimensions", cluster0.Dimensions), 
            new UnitTester.Parameter("Expected Dimensions", binGridDimensions)
        );

        UnitTester.Assert(
            "Testing Arch Block, Cluster Offset",
            cluster0.VoxelOffset == Vector3Int.zero,
            expectedResult: true,
            new UnitTester.Parameter("Cluster Offset", cluster0.VoxelOffset),
            new UnitTester.Parameter("Expected Offset", Vector3Int.zero)
        );

        // ================ Test two clusters touching diagonally ================ 

        bins = new Bin[binCount];
        visitedBins = new bool[binCount];

        AddBin(new Vector3Int(0, 0, 0));
        AddBin(new Vector3Int(0, 0, 1));
        AddBin(new Vector3Int(0, 0, 2));

        AddBin(new Vector3Int(1, 1, 0));
        AddBin(new Vector3Int(1, 1, 1));
        AddBin(new Vector3Int(1, 1, 2));

        AddBin(new Vector3Int(2, 0, 0));
        AddBin(new Vector3Int(2, 0, 1));
        AddBin(new Vector3Int(2, 0, 2));

        AddBin(new Vector3Int(2, 1, 0));
        AddBin(new Vector3Int(2, 1, 1));
        AddBin(new Vector3Int(2, 1, 2));

        AddBin(new Vector3Int(2, 2, 0));
        AddBin(new Vector3Int(2, 2, 1));
        AddBin(new Vector3Int(2, 2, 2));

        RefreshBinGridConnectivity(bins, binGridDimensions);

        cluster0 = TryFindCluster(VoxelGrid.CoordsToIndex(new Vector3Int(0, 0, 0), binGridDimensions), bins, binGridDimensions, visitedBins);
        cluster1 = TryFindCluster(VoxelGrid.CoordsToIndex(new Vector3Int(1, 1, 0), binGridDimensions), bins, binGridDimensions, visitedBins);

        //for(int i = 0; i < cluster1.Bins.Length; i++) {
        //    if(cluster1.Bins[i] == null) {
        //        continue;
        //    }

        //    Debug.Log(cluster1.Bins[i].Index + ", " + cluster1.Bins[i].Coords);
        //}

        UnitTester.Assert(
            "Testing Two Clusters Touching Diagonally, Cluster #1 Bin Count", 
            cluster0.Bins.Length == cluster0.Dimensions.x * cluster0.Dimensions.y * cluster0.Dimensions.z, 
            expectedResult: true, 
            new UnitTester.Parameter("Bin Count", cluster0.Bins.Length), 
            new UnitTester.Parameter("Expected Count", cluster0.Dimensions.x * cluster0.Dimensions.y * cluster0.Dimensions.z)
        );

        UnitTester.Assert(
            "Testing Two Clusters Touching Diagonally, Cluster #2 Bin Count",
            cluster1.Bins.Length == cluster1.Dimensions.x * cluster1.Dimensions.y * cluster1.Dimensions.z,
            expectedResult: true,
            new UnitTester.Parameter("Bin Count", cluster1.Bins.Length),
            new UnitTester.Parameter("Expected Count", cluster1.Dimensions.x * cluster1.Dimensions.y * cluster1.Dimensions.z)
        );

        UnitTester.Assert(
            "Testing Two Clusters Touching Diagonally, Cluster #1 Dimensions", 
            cluster0.Dimensions == new Vector3Int(1, 1, 3), 
            expectedResult: true, 
            new UnitTester.Parameter("Dimensions", cluster0.Dimensions), 
            new UnitTester.Parameter("Expected Dimensions", new Vector3Int(1, 1, 3))
        );

        UnitTester.Assert(
            "Testing Two Clusters Touching Diagonally, Cluster #2 Dimensions", 
            cluster1.Dimensions == new Vector3Int(2, 3, 3), 
            expectedResult: true, 
            new UnitTester.Parameter("Dimensions", cluster1.Dimensions), 
            new UnitTester.Parameter("Expected Dimensions", new Vector3Int(2, 3, 3))
        );

        UnitTester.Assert(
            "Testing Two Clusters Touching Diagonally, Cluster #1 Offset",
            cluster0.VoxelOffset == Vector3Int.zero,
            expectedResult: true,
            new UnitTester.Parameter("Offset", cluster0.VoxelOffset),
            new UnitTester.Parameter("Expected Offset", Vector3Int.zero)
        );

        UnitTester.Assert(
            "Testing Two Clusters Touching Diagonally, Cluster #2 Offset",
            cluster1.VoxelOffset == new Vector3Int(1, 0, 0) * Bin.WIDTH,
            expectedResult: true,
            new UnitTester.Parameter("Offset", cluster1.VoxelOffset),
            new UnitTester.Parameter("Expected Offset", new Vector3Int(1, 0, 0) * Bin.WIDTH)
        );

        // ================ Test two clusters touching... diagonally diagonally ================ 

        bins = new Bin[binCount];
        visitedBins = new bool[binCount];

        AddBin(new Vector3Int(0, 0, 2));

        AddBin(new Vector3Int(1, 1, 1));

        AddBin(new Vector3Int(2, 0, 0));
        AddBin(new Vector3Int(2, 0, 1));
        AddBin(new Vector3Int(2, 0, 2));

        AddBin(new Vector3Int(2, 1, 0));
        AddBin(new Vector3Int(2, 1, 1));
        AddBin(new Vector3Int(2, 1, 2));

        AddBin(new Vector3Int(2, 2, 0));
        AddBin(new Vector3Int(2, 2, 1));
        AddBin(new Vector3Int(2, 2, 2));

        RefreshBinGridConnectivity(bins, binGridDimensions);

        cluster0 = TryFindCluster(VoxelGrid.CoordsToIndex(new Vector3Int(0, 0, 2), binGridDimensions), bins, binGridDimensions, visitedBins);
        cluster1 = TryFindCluster(VoxelGrid.CoordsToIndex(new Vector3Int(1, 1, 1), binGridDimensions), bins, binGridDimensions, visitedBins);

        UnitTester.Assert(
            "Testing Two Clusters Touching Diagonally Diagonally, Cluster #1 Bin Count", 
            cluster0.Bins.Length == cluster0.Dimensions.x * cluster0.Dimensions.y * cluster0.Dimensions.z, 
            expectedResult: true, 
            new UnitTester.Parameter("Bin Count", cluster0.Bins.Length), 
            new UnitTester.Parameter("Expected Count", cluster0.Dimensions.x * cluster0.Dimensions.y * cluster0.Dimensions.z)
        );

        UnitTester.Assert(
            "Testing Two Clusters Touching Diagonally Diagonally, Cluster #2 Bin Count",
            cluster1.Bins.Length == cluster1.Dimensions.x * cluster1.Dimensions.y * cluster1.Dimensions.z,
            expectedResult: true,
            new UnitTester.Parameter("Bin Count", cluster1.Bins.Length),
            new UnitTester.Parameter("Expected Count", cluster1.Dimensions.x * cluster1.Dimensions.y * cluster1.Dimensions.z)
        );

        UnitTester.Assert(
            "Testing Two Clusters Touching Diagonally Diagonally, Cluster #1 Dimensions", 
            cluster0.Dimensions == new Vector3Int(1, 1, 1), 
            expectedResult: true, 
            new UnitTester.Parameter("Dimensions", cluster0.Dimensions), 
            new UnitTester.Parameter("Expected Dimensions", new Vector3Int(1, 1, 1))
        );

        UnitTester.Assert(
            "Testing Two Clusters Touching Diagonally Diagonally, Cluster #2 Dimensions", 
            cluster1.Dimensions == new Vector3Int(2, 3, 3), 
            expectedResult: true, 
            new UnitTester.Parameter("Dimensions", cluster1.Dimensions), 
            new UnitTester.Parameter("Expected Dimensions", new Vector3Int(2, 3, 3))
        );

        UnitTester.Assert(
            "Testing Two Clusters Touching Diagonally Diagonally, Cluster #1 Offset",
            cluster0.VoxelOffset == new Vector3Int(0, 0, 2) * Bin.WIDTH,
            expectedResult: true,
            new UnitTester.Parameter("Offset", cluster0.VoxelOffset),
            new UnitTester.Parameter("Expected Offset", new Vector3Int(0, 0, 2) * Bin.WIDTH)
        );

        UnitTester.Assert(
            "Testing Two Clusters Touching Diagonally Diagonally, Cluster #2 Offset",
            cluster1.VoxelOffset == new Vector3Int(1, 0, 0) * Bin.WIDTH,
            expectedResult: true,
            new UnitTester.Parameter("Offset", cluster1.VoxelOffset),
            new UnitTester.Parameter("Expected Offset", new Vector3Int(1, 0, 0) * Bin.WIDTH)
        );

        void AddBin(Vector3Int newBinCoords) {
            int binIndex = VoxelGrid.CoordsToIndex(newBinCoords, binGridDimensions);
            bins[binIndex] = new Bin(binIndex, binGridDimensions);
            Bin.SetBinAllVoxelsExists(bins, binIndex, exists: true);
        }

        static void RefreshBinGridConnectivity(Bin[] bins, Vector3Int binGridDimensions) {
            for(int i = 0; i < bins.Length; i++) {
                if(bins[i].IsWholeBinEmpty()) {
                    continue;
                }

                Bin.RefreshConnectivityInBin(bins, i, binGridDimensions);
            }
        }
    }

    private static void TestMoveBinsToNewGrid() {
        const int maxWidth = 10;

        for(int i = 0; i < 25; i++) {
            Vector3Int oldBinGridDimensions = Utils.GetRandomVector3Int(1, maxWidth + 1);
            Bin[] oldBins = UnitTester.GetBinsForTesting(oldBinGridDimensions);

            Queue<int> indexesToMove = new Queue<int>();
            Vector3Int minCoord = Utils.GetRandomVector3Int(Vector3Int.zero, oldBinGridDimensions);
            Vector3Int maxCoord = Utils.GetRandomVector3Int(minCoord, oldBinGridDimensions);
            Vector3Int intendedNewBinGridDimensions = maxCoord - minCoord + Vector3Int.one;

            for(int z = 0; z < intendedNewBinGridDimensions.z; z++) {
                for(int y = 0; y < intendedNewBinGridDimensions.y; y++) {
                    for(int x = 0; x < intendedNewBinGridDimensions.x; x++) {
                        int oldIndex = VoxelGrid.CoordsToIndex(minCoord.x + x, minCoord.y + y, minCoord.z + z, oldBinGridDimensions);

                        indexesToMove.Enqueue(oldIndex);
                    }
                }
            }

            Vector3Int actualNewBinGridDimensions;
            Bin[] newBins = MoveBinsToNewGrid(oldBins, oldBinGridDimensions, indexesToMove, minCoord, maxCoord, out actualNewBinGridDimensions);

            UnitTester.Assert(
                "MoveBinsToNewGrid",
                actualNewBinGridDimensions == intendedNewBinGridDimensions,
                expectedResult: true,
                new UnitTester.Parameter("New Dimensions", actualNewBinGridDimensions),
                new UnitTester.Parameter("Expected Dimensions", intendedNewBinGridDimensions)
            );

            UnitTester.Assert(
                "MoveBinsToNewGrid",
                newBins.Length == intendedNewBinGridDimensions.x * intendedNewBinGridDimensions.y * intendedNewBinGridDimensions.z,
                expectedResult: true,
                new UnitTester.Parameter("New Count", newBins.Length),
                new UnitTester.Parameter("Expected Count", intendedNewBinGridDimensions.x * intendedNewBinGridDimensions.y * intendedNewBinGridDimensions.z)
            );

            for(int newBinIndex = 0; newBinIndex < newBins.Length; newBinIndex++) {
                UnitTester.Assert(
                    "MoveBinsToNewGrid",
                    newBins[newBinIndex].Index == newBinIndex,
                    expectedResult: true,
                    new UnitTester.Parameter("Bin Index", newBins[newBinIndex].Index),
                    new UnitTester.Parameter("Expected Index", newBinIndex)
                );

                UnitTester.Assert(
                    "MoveBinsToNewGrid",
                    newBins[newBinIndex].Coords == VoxelGrid.IndexToCoords(newBinIndex, actualNewBinGridDimensions),
                    expectedResult: true,
                    new UnitTester.Parameter("Bin Coords", newBins[newBinIndex].Coords),
                    new UnitTester.Parameter("Expected Coords", VoxelGrid.IndexToCoords(newBinIndex, actualNewBinGridDimensions))
                );
            }
        }
    }

    private static void TestMarkExteriorBins() { // this code is horse shit, just toss it out if you run into problems
        Vector3Int binGridDimensions = new Vector3Int(5, 5, 5);
        Bin[] bins;
        bool[] expectedMarkings;
        List<Vector3Int> tweakedBins;

        // test #1
        bins = UnitTester.GetBinsForTesting(binGridDimensions);
        MarkExteriorBins(bins, binGridDimensions);

        expectedMarkings = new bool[bins.Length];
        for(int i = 0; i < expectedMarkings.Length; i++) {
            Vector3Int coords = VoxelGrid.IndexToCoords(i, binGridDimensions);
            expectedMarkings[i] = coords.x == 0 || coords.y == 0 || coords.z == 0 || coords.x == binGridDimensions.x - 1 || coords.y == binGridDimensions.y - 1 || coords.z == binGridDimensions.z - 1;
        }

        VerifyMarkings(bins, expectedMarkings, binGridDimensions);

        // test #2
        tweakedBins = new List<Vector3Int>() {
            new Vector3Int(1, 0, 1),
            new Vector3Int(2, 0, 1),
            new Vector3Int(3, 0, 1),
            new Vector3Int(1, 1, 1),
            new Vector3Int(2, 1, 1),
            new Vector3Int(3, 1, 1),
            new Vector3Int(1, 2, 1),
            new Vector3Int(2, 2, 1),
            new Vector3Int(3, 2, 1),

            new Vector3Int(2, 1, 2), // test further depth

            //new Vector3Int(2, 2, 3), // test diagonal

            //new Vector3Int(3, 3, 3) // test double diagonal
        };

        bins = UnitTester.GetBinsForTesting(binGridDimensions);

        for(int i = 0; i < tweakedBins.Count; i++) {
            Vector3Int coords = tweakedBins[i];
            int index = VoxelGrid.CoordsToIndex(coords, binGridDimensions);
            Bin.SetBinAllVoxelsExists(bins, index, exists: false);
        }

        MarkExteriorBins(bins, binGridDimensions);

        expectedMarkings = new bool[bins.Length];
        for(int i = 0; i < expectedMarkings.Length; i++) {
            Vector3Int coords = VoxelGrid.IndexToCoords(i, binGridDimensions);

            bool isAtEdge = coords.x == 0 || coords.y == 0 || coords.z == 0 || coords.x == binGridDimensions.x - 1 || coords.y == binGridDimensions.y - 1 || coords.z == binGridDimensions.z - 1;
            bool hasBeenTweaked = tweakedBins.Contains(coords);
            bool hasNeighborBeenTweakedRight    = tweakedBins.Contains(coords + Vector3Int.right);
            bool hasNeighborBeenTweakedLeft     = tweakedBins.Contains(coords + Vector3Int.left);
            bool hasNeighborBeenTweakedUp       = tweakedBins.Contains(coords + Vector3Int.up);
            bool hasNeighborBeenTweakedDown     = tweakedBins.Contains(coords + Vector3Int.down);
            bool hasNeighborBeenTweakedFore     = tweakedBins.Contains(coords + Vector3Int.forward);
            bool hasNeighborBeenTweakedBack     = tweakedBins.Contains(coords + Vector3Int.back);

            expectedMarkings[i] = isAtEdge || hasBeenTweaked || hasNeighborBeenTweakedRight || hasNeighborBeenTweakedLeft || hasNeighborBeenTweakedUp || hasNeighborBeenTweakedDown || hasNeighborBeenTweakedFore || hasNeighborBeenTweakedBack;
        }

        VerifyMarkings(bins, expectedMarkings, binGridDimensions);

        // test #3
        tweakedBins = new List<Vector3Int>() {
            new Vector3Int(2, 2, 2),
            new Vector3Int(3, 2, 2),
            new Vector3Int(3, 2, 3),
            new Vector3Int(3, 3, 3),
        };

        bins = UnitTester.GetBinsForTesting(binGridDimensions);

        for(int i = 0; i < tweakedBins.Count; i++) {
            Vector3Int coords = tweakedBins[i];
            int index = VoxelGrid.CoordsToIndex(coords, binGridDimensions);
            Bin.SetBinAllVoxelsExists(bins, index, exists: false);
        }

        MarkExteriorBins(bins, binGridDimensions);

        expectedMarkings = new bool[bins.Length];
        for(int i = 0; i < expectedMarkings.Length; i++) {
            Vector3Int coords = VoxelGrid.IndexToCoords(i, binGridDimensions);

            bool isAtEdge = coords.x == 0 || coords.y == 0 || coords.z == 0 || coords.x == binGridDimensions.x - 1 || coords.y == binGridDimensions.y - 1 || coords.z == binGridDimensions.z - 1;
            expectedMarkings[i] = isAtEdge;
        }

        VerifyMarkings(bins, expectedMarkings, binGridDimensions);

        static void VerifyMarkings(Bin[] bins, bool[] expectedMarkings, Vector3Int binGridDimensions) {
            Debug.Assert(bins.Length == expectedMarkings.Length);

            for(int i = 0; i < bins.Length; i++) {
                Debug.Assert(bins[i].IsExterior == expectedMarkings[i], string.Format("Failed: {0} was {1}, expected {2}!", VoxelGrid.IndexToCoords(i, binGridDimensions), bins[i].IsExterior, expectedMarkings[i]));
            }
        }
    }
}
