using System.Collections.Generic;
using UnityEngine;

public static partial class VoxelClusterHandler {

    private static List<VoxelCluster> clusters = new List<VoxelCluster>();
    private static Queue<int> binsToVisit = new Queue<int>();
    private static Queue<int> foundBins = new Queue<int>();
    private static Queue<MoveOrder> moveOrders = new Queue<MoveOrder>();

    public static void FindVoxelClustersAndSplit(VoxelGrid voxelGrid, Queue<int> newlyCleanedBins) {
        bool[] visitedBins = new bool[voxelGrid.GetBinCount()];

        clusters.Clear();
        while(newlyCleanedBins.Count > 0) {
            int binIndex = newlyCleanedBins.Dequeue();

            VoxelCluster cluster = TryFindCluster(binIndex, voxelGrid.GetBins(), voxelGrid.GetVoxelMap(), voxelGrid.GetBinGridDimensions(), visitedBins);
            if(cluster != null) {
                clusters.Add(cluster);
            }
        }

        VoxelGrid[] splitVoxelGrids = TrySplit(voxelGrid, clusters);
        ApplyClusters(splitVoxelGrids, clusters);
    }

    private static VoxelCluster TryFindCluster(int startBinIndex, Bin[] bins, Octree<bool> voxelMap, Vector3Int binGridDimensions, bool[] visitedBins) {
        Vector3Int minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        Vector3Int maxCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

        foundBins.Clear();
        binsToVisit.Clear();
        binsToVisit.Enqueue(startBinIndex);

        while(binsToVisit.Count > 0) {
            int binIndex = binsToVisit.Dequeue();
            if(visitedBins[binIndex]) {
                continue;
            }

            visitedBins[binIndex] = true;
            foundBins.Enqueue(binIndex);

            Vector3Int binCoords = VoxelGrid.IndexToCoords(binIndex, binGridDimensions);
            minCoord.x = Mathf.Min(minCoord.x, binCoords.x);
            minCoord.y = Mathf.Min(minCoord.y, binCoords.y);
            minCoord.z = Mathf.Min(minCoord.z, binCoords.z);
            maxCoord.x = Mathf.Max(maxCoord.x, binCoords.x);
            maxCoord.y = Mathf.Max(maxCoord.y, binCoords.y);
            maxCoord.z = Mathf.Max(maxCoord.z, binCoords.z);

            TryAddNeighborToVisit(binIndex, bins, binGridDimensions, Direction.Right, binsToVisit);
            TryAddNeighborToVisit(binIndex, bins, binGridDimensions, Direction.Left,  binsToVisit);
            TryAddNeighborToVisit(binIndex, bins, binGridDimensions, Direction.Up,    binsToVisit);
            TryAddNeighborToVisit(binIndex, bins, binGridDimensions, Direction.Down,  binsToVisit);
            TryAddNeighborToVisit(binIndex, bins, binGridDimensions, Direction.Fore,  binsToVisit);
            TryAddNeighborToVisit(binIndex, bins, binGridDimensions, Direction.Back,  binsToVisit);

            static void TryAddNeighborToVisit(int centerBinIndex, Bin[] bins, Vector3Int binGridDimension, Direction direction, Queue<int> binsToVisit) {
                Bin centerBin = bins[centerBinIndex];

                if(!centerBin.IsConnectedToNeighbor(direction)) {
                    return;
                }

                Vector3Int dirVec = Utils.GetDirectionVector(direction);
                Vector3Int neighborCoords = new Vector3Int(centerBin.Coords.x + dirVec.x, centerBin.Coords.y + dirVec.y, centerBin.Coords.z + dirVec.z);
                
                int neighborIndex = VoxelGrid.CoordsToIndex(neighborCoords, binGridDimension);
                if(neighborIndex == -1) {
                    return;
                }

                Bin neighborBin = bins[neighborIndex];
                if(neighborBin.IsWholeBinEmpty()) {
                    return;
                }

                if(centerBin.IsWalledIn() && neighborBin.IsWalledIn()) {
                    return;
                }

                binsToVisit.Enqueue(neighborIndex);
            }
        }

        if(foundBins.Count == 0) {
            return null;
        }

        Vector3Int newVoxelOffset = minCoord * Bin.WIDTH;
        Vector3Int newDimensions;
        Bin[] newBins = MoveBinsToNewGrid(bins, binGridDimensions, foundBins, minCoord, maxCoord, out newDimensions);
        MarkExteriorBins(newBins, newDimensions);

        Vector3Int newVoxelGridDimensions = VoxelGrid.CalculateVoxelGridDimensions(newDimensions);
        Octree<bool> newVoxelMap = new Octree<bool>(Mathf.Max(newVoxelGridDimensions.x, Mathf.Max(newVoxelGridDimensions.y, newVoxelGridDimensions.z)));
        for(int z = 0; z < newVoxelGridDimensions.z; z++) {
            for(int y = 0; y < newVoxelGridDimensions.y; y++) {
                for(int x = 0; x < newVoxelGridDimensions.x; x++) {
                    int binIndex, localVoxelIndex;
                    VoxelGrid.GetBinAndVoxelIndex(new Vector3Int(x, y, z), newBins, newDimensions, out binIndex, out localVoxelIndex);

                    newVoxelMap.SetValue(x, y, z, newBins[binIndex].GetVoxelExists(localVoxelIndex));
                }
            }
        }

        return new VoxelCluster(newBins, newVoxelMap, newVoxelOffset, newDimensions);
    }

    private static void MarkExteriorBins(Bin[] bins, Vector3Int binGridDimensions) {
        int binCount = bins.Length;

        bool[] visitedExteriorBins = new bool[binCount];

        for(int z = 0; z < binGridDimensions.z; ++z) {
            for(int y = 0; y < binGridDimensions.y; y++) {
                FloodFillExteriorBins(new Vector3Int(0, y, z), bins, binGridDimensions, visitedExteriorBins);
                FloodFillExteriorBins(new Vector3Int(binGridDimensions.x - 1, y, z), bins, binGridDimensions, visitedExteriorBins);
            }
        }

        for(int z = 0; z < binGridDimensions.z; ++z) {
            for(int x = 0; x < binGridDimensions.x; x++) {
                FloodFillExteriorBins(new Vector3Int(x, 0, z), bins, binGridDimensions, visitedExteriorBins);
                FloodFillExteriorBins(new Vector3Int(x, binGridDimensions.y - 1, z), bins, binGridDimensions, visitedExteriorBins);
            }
        }

        for(int y = 0; y < binGridDimensions.y; ++y) {
            for(int x = 0; x < binGridDimensions.x; x++) {
                FloodFillExteriorBins(new Vector3Int(x, y, 0), bins, binGridDimensions, visitedExteriorBins);
                FloodFillExteriorBins(new Vector3Int(x, y, binGridDimensions.z - 1), bins, binGridDimensions, visitedExteriorBins);
            }
        }

        static void FloodFillExteriorBins(Vector3Int startBinCoords, Bin[] bins, Vector3Int binGridDimensions, bool[] visitedBins) {
            moveOrders.Clear();
            moveOrders.Enqueue(new MoveOrder(VoxelGrid.CoordsToIndex(startBinCoords, binGridDimensions), Direction.None));

            while(moveOrders.Count > 0) {
                MoveOrder moveOrder = moveOrders.Dequeue();
                if(moveOrder.TargetIndex == -1) {
                    continue;
                }

                int currentBinIndex = moveOrder.TargetIndex;
                if(visitedBins[currentBinIndex]) {
                    continue;
                }

                visitedBins[currentBinIndex] = true;
                Bin.SetBinIsExterior(bins, currentBinIndex, isExterior: true);
                
                Bin currentBin = bins[currentBinIndex];
                if(currentBin.IsWholeBinFilled()) {
                    continue;
                }

                Vector3Int currentBinCoords = VoxelGrid.IndexToCoords(currentBinIndex, binGridDimensions);

                MoveOrder newMoveOrder;
                if(TryGetNewMoveOrder(Direction.Right, currentBin, currentBinCoords, binGridDimensions, moveOrder, out newMoveOrder)) {
                    moveOrders.Enqueue(newMoveOrder);
                }
                if(TryGetNewMoveOrder(Direction.Left, currentBin, currentBinCoords, binGridDimensions, moveOrder, out newMoveOrder)) {
                    moveOrders.Enqueue(newMoveOrder);
                }
                if(TryGetNewMoveOrder(Direction.Up, currentBin, currentBinCoords, binGridDimensions, moveOrder, out newMoveOrder)) {
                    moveOrders.Enqueue(newMoveOrder);
                }
                if(TryGetNewMoveOrder(Direction.Down, currentBin, currentBinCoords, binGridDimensions, moveOrder, out newMoveOrder)) {
                    moveOrders.Enqueue(newMoveOrder);
                }
                if(TryGetNewMoveOrder(Direction.Fore, currentBin, currentBinCoords, binGridDimensions, moveOrder, out newMoveOrder)) {
                    moveOrders.Enqueue(newMoveOrder);
                }
                if(TryGetNewMoveOrder(Direction.Back, currentBin, currentBinCoords, binGridDimensions, moveOrder, out newMoveOrder)) {
                    moveOrders.Enqueue(newMoveOrder);
                }

                static bool TryGetNewMoveOrder(Direction direction, Bin currentBin, Vector3Int binCoords, Vector3Int binGridDimensions, MoveOrder currentMoveOrder, out MoveOrder newMoveOrder) {
                    if(direction == currentMoveOrder.DirectionToOrigin) {
                        newMoveOrder = new MoveOrder();
                        return false;
                    }
                    
                    if(!currentBin.IsWholeBinEmpty() && !currentBin.HasOpenPathBetweenFaces(currentMoveOrder.DirectionToOrigin, direction)) {
                        newMoveOrder = new MoveOrder();
                        return false;
                    }

                    newMoveOrder = new MoveOrder(VoxelGrid.CoordsToIndex(binCoords + Utils.GetDirectionVector(direction), binGridDimensions), direction);
                    return true;
                }
            }
        }
    }

    private static Bin[] MoveBinsToNewGrid(Bin[] oldBins, Vector3Int oldBinGridDimensions, Queue<int> indexesToMove, Vector3Int minCoord, Vector3Int maxCoord, out Vector3Int newBinGridDimensions) {
        newBinGridDimensions = maxCoord - minCoord + Vector3Int.one;
        Bin[] newBins = new Bin[newBinGridDimensions.x * newBinGridDimensions.y * newBinGridDimensions.z];

        while(indexesToMove.Count > 0) {
            int oldBinIndex = indexesToMove.Dequeue();
            Vector3Int oldBinCoords = VoxelGrid.IndexToCoords(oldBinIndex, oldBinGridDimensions);
            int newBinIndex = VoxelGrid.CoordsToIndex(oldBinCoords - minCoord, newBinGridDimensions);

            newBins[newBinIndex] = new Bin(oldBins[oldBinIndex], newBinIndex, newBinGridDimensions);
        }

        return newBins;
    }

    private static VoxelGrid[] TrySplit(VoxelGrid originalVoxelGrid, List<VoxelCluster> clusters) {
        Debug.Assert(clusters.Count > 0);

        VoxelGrid[] voxelGrids = new VoxelGrid[clusters.Count];
        voxelGrids[0] = originalVoxelGrid;

        if(clusters.Count > 1) {
            Transform originalMeshTransform = originalVoxelGrid.GetMeshTransform();
            Transform[] originalMeshObjects = originalMeshTransform.GetComponentsInChildren<Transform>(includeInactive: true);

            for(int i = 1; i < originalMeshObjects.Length; i++) {
                originalMeshObjects[i].parent = null;
            }

            for(int i = 1; i < clusters.Count; i++) {
                GameObject go = Object.Instantiate(originalVoxelGrid.gameObject, originalVoxelGrid.transform.parent);
                go.name = originalVoxelGrid.name + " (Cluster)";

                VoxelGrid newVoxelGrid = go.GetComponent<VoxelGrid>();
                newVoxelGrid.MarkAsCopy();

                voxelGrids[i] = newVoxelGrid;
            }

            for(int i = 1; i < originalMeshObjects.Length; i++) {
                originalMeshObjects[i].parent = originalMeshTransform;
            }
        }

        return voxelGrids;
    }

    private static void ApplyClusters(VoxelGrid[] voxelGrids, List<VoxelCluster> clusters) {
        Debug.Assert(voxelGrids.Length == clusters.Count);

        int biggestClusterIndex = GetBiggestVoxelClusterIndex(clusters);

        int voxelGridIndex = 1;
        for(int i = 0; i < clusters.Count; i++) {
            if(i == biggestClusterIndex) {
                continue;
            }
             
            voxelGrids[voxelGridIndex].ApplyCluster(clusters[i]);
            voxelGridIndex++;
        }

        voxelGrids[0].ApplyCluster(clusters[biggestClusterIndex]);
    }

    public static int GetBiggestVoxelClusterIndex(List<VoxelCluster> clusters) {
        int biggestClusterIndex = -1;
        int biggestClusterSize = int.MinValue;
        for(int i = 0; i < clusters.Count; i++) {
            if(clusters[i].Bins.Length > biggestClusterSize) {
                biggestClusterSize = clusters[i].Bins.Length;
                biggestClusterIndex = i;
            }
        }

        Debug.Assert(biggestClusterIndex >= 0);
        Debug.Assert(biggestClusterIndex < clusters.Count);
        return biggestClusterIndex;
    }

    private struct MoveOrder {
        public int TargetIndex;
        public Direction DirectionToOrigin;

        public MoveOrder(int targetIndex, Direction direction) {
            TargetIndex = targetIndex;
            DirectionToOrigin = Utils.GetOppositeDirection(direction);
        }
    }
}
