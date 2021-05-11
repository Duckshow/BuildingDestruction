using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public static partial class VoxelClusterHandler {
    //public struct FindClusterJob : IJobParallelFor {
    //    [ReadOnly] public NativeArray<int> startingPoints;
    //    [ReadOnly] public NativeArray<bool> visitedBins;

    //    public NativeArray<float> result;

    //    public void Execute(int workerIndex) {
    //        int startingPoint = startingPoints[workerIndex];

    //        Queue<MoveOrder> pathsNotTaken = new Queue<MoveOrder>();
    //        pathsNotTaken.Enqueue(new MoveOrder(startingPoint, Direction.None));

    //        while(pathsNotTaken.Count > 0) {
    //            MoveOrder? nextOrderOnCurrentPath = pathsNotTaken.Dequeue();

    //            while(nextOrderOnCurrentPath != null) {
    //                Bin currentBin = bins[nextOrderOnCurrentPath.Value.TargetIndex];
    //                Direction directionToPreviousBin = nextOrderOnCurrentPath.Value.DirectionToOrigin;
    //                nextOrderOnCurrentPath = null;

    //                if(visitedBins[currentBin.Index]) {
    //                    break;
    //                }
    //                visitedBins[currentBin.Index] = true;

    //                if(!currentBin.IsWalledIn()) {
    //                    foundBins.Enqueue(currentBin.Index);
    //                }

    //                Vector3Int binCoords = VoxelGrid.IndexToCoords(currentBin.Index, binGridDimensions);
    //                minCoord.x = Mathf.Min(minCoord.x, binCoords.x);
    //                minCoord.y = Mathf.Min(minCoord.y, binCoords.y);
    //                minCoord.z = Mathf.Min(minCoord.z, binCoords.z);
    //                maxCoord.x = Mathf.Max(maxCoord.x, binCoords.x);
    //                maxCoord.y = Mathf.Max(maxCoord.y, binCoords.y);
    //                maxCoord.z = Mathf.Max(maxCoord.z, binCoords.z);

    //                TryFollowPath(Direction.Right);
    //                TryFollowPath(Direction.Left);
    //                TryFollowPath(Direction.Fore);
    //                TryFollowPath(Direction.Back);

    //                MoveOrder newMoveOrder;
    //                if(TryGetNewMoveOrder(currentBin, Direction.Up, out newMoveOrder)) {
    //                    startingPoints.Enqueue(newMoveOrder.TargetIndex);
    //                }
    //                if(TryGetNewMoveOrder(currentBin, Direction.Down, out newMoveOrder)) {
    //                    startingPoints.Enqueue(newMoveOrder.TargetIndex);
    //                }

    //                void TryFollowPath(Direction direction) {
    //                    MoveOrder newMoveOrder;

    //                    if(TryGetNewMoveOrder(currentBin, direction, out newMoveOrder)) {
    //                        if(nextOrderOnCurrentPath == null) {
    //                            nextOrderOnCurrentPath = newMoveOrder;
    //                        }
    //                        else {
    //                            pathsNotTaken.Enqueue(newMoveOrder);
    //                        }
    //                    }
    //                }

    //                bool TryGetNewMoveOrder(Bin originBin, Direction direction, out MoveOrder newMoveOrder) {
    //                    newMoveOrder = new MoveOrder();

    //                    if(direction == directionToPreviousBin) {
    //                        return false;
    //                    }

    //                    Vector3Int neighborCoords = originBin.Coords + Utils.GetDirectionVector(direction);
    //                    if(!VoxelGrid.AreCoordsWithinDimensions(neighborCoords, binGridDimensions)) {
    //                        return false;
    //                    }

    //                    int neighborIndex = VoxelGrid.CoordsToIndex(neighborCoords, binGridDimensions);
    //                    if(visitedBins[neighborIndex]) {
    //                        return false;
    //                    }

    //                    Bin neighborBin = bins[neighborIndex];
    //                    if(neighborBin == null) {
    //                        return false;
    //                    }

    //                    if(neighborBin.IsWholeBinEmpty()) {
    //                        return false;
    //                    }

    //                    if(originBin.IsWalledIn() && neighborBin.IsWalledIn()) {
    //                        return false;
    //                    }

    //                    if(!originBin.IsConnectedToNeighbor(neighborBin, direction)) {
    //                        return false;
    //                    }

    //                    newMoveOrder = new MoveOrder(neighborIndex, direction);
    //                    return true;
    //                }
    //            }
    //        }
    //    }
    //}

    private struct MoveOrder {
        public int TargetIndex;
        public Direction DirectionToOrigin;

        public MoveOrder(int targetIndex, Direction direction) {
            TargetIndex = targetIndex;
            DirectionToOrigin = Utils.GetOppositeDirection(direction);
        }
    }

    public static void FindVoxelClustersAndSplit(VoxelGrid voxelGrid, Queue<int> newlyCleanedBins) {
        List<VoxelCluster> clusters = new List<VoxelCluster>();
        bool[] visitedBins = new bool[voxelGrid.GetBinCount()];

        while(newlyCleanedBins.Count > 0) {
            int binIndex = newlyCleanedBins.Dequeue();

            VoxelCluster cluster = TryFindCluster(binIndex, voxelGrid.GetBins(), voxelGrid.GetBinGridDimensions(), visitedBins);
            if(cluster != null) {
                clusters.Add(cluster);
            }
        }

        VoxelGrid[] splitVoxelGrids = TrySplit(voxelGrid, clusters);
        ApplyClusters(splitVoxelGrids, clusters);
    }

    private static VoxelCluster TryFindCluster(int startBinIndex, Bin[] bins, Vector3Int binGridDimensions, bool[] visitedBins) {
        Vector3Int minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        Vector3Int maxCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

        Queue<int> foundBins = new Queue<int>();
        Queue<int> startingPoints = new Queue<int>();
        startingPoints.Enqueue(startBinIndex);

        while(startingPoints.Count > 0) {
            int startingPoint = startingPoints.Dequeue();

            Queue<MoveOrder> pathsNotTaken = new Queue<MoveOrder>();
            pathsNotTaken.Enqueue(new MoveOrder(startingPoint, Direction.None));

            while(pathsNotTaken.Count > 0) {
                MoveOrder? nextOrderOnCurrentPath = pathsNotTaken.Dequeue();

                while(nextOrderOnCurrentPath != null) {
                    Bin currentBin = bins[nextOrderOnCurrentPath.Value.TargetIndex];
                    Direction directionToPreviousBin = nextOrderOnCurrentPath.Value.DirectionToOrigin;
                    nextOrderOnCurrentPath = null;

                    if(visitedBins[currentBin.Index]) {
                        break;
                    }
                    visitedBins[currentBin.Index] = true;

                    if(!currentBin.IsWalledIn()) {
                        foundBins.Enqueue(currentBin.Index);
                    }

                    Vector3Int binCoords = VoxelGrid.IndexToCoords(currentBin.Index, binGridDimensions);
                    minCoord.x = Mathf.Min(minCoord.x, binCoords.x);
                    minCoord.y = Mathf.Min(minCoord.y, binCoords.y);
                    minCoord.z = Mathf.Min(minCoord.z, binCoords.z);
                    maxCoord.x = Mathf.Max(maxCoord.x, binCoords.x);
                    maxCoord.y = Mathf.Max(maxCoord.y, binCoords.y);
                    maxCoord.z = Mathf.Max(maxCoord.z, binCoords.z);

                    TryFollowPath(Direction.Right);
                    TryFollowPath(Direction.Left);
                    TryFollowPath(Direction.Fore);
                    TryFollowPath(Direction.Back);

                    MoveOrder newMoveOrder;
                    if(TryGetNewMoveOrder(currentBin, Direction.Up, out newMoveOrder)) {
                        startingPoints.Enqueue(newMoveOrder.TargetIndex);
                    }
                    if(TryGetNewMoveOrder(currentBin, Direction.Down, out newMoveOrder)) {
                        startingPoints.Enqueue(newMoveOrder.TargetIndex);
                    }

                    void TryFollowPath(Direction direction) {
                        MoveOrder newMoveOrder;

                        if(TryGetNewMoveOrder(currentBin, direction, out newMoveOrder)) {
                            if(nextOrderOnCurrentPath == null) {
                                nextOrderOnCurrentPath = newMoveOrder;
                            }
                            else {
                                pathsNotTaken.Enqueue(newMoveOrder);
                            }
                        }
                    }

                    bool TryGetNewMoveOrder(Bin originBin, Direction direction, out MoveOrder newMoveOrder) {
                        newMoveOrder = new MoveOrder();

                        if(direction == directionToPreviousBin) {
                            return false;
                        }

                        Vector3Int neighborCoords = originBin.Coords + Utils.GetDirectionVector(direction);
                        if(!VoxelGrid.AreCoordsWithinDimensions(neighborCoords, binGridDimensions)) {
                            return false;
                        }

                        int neighborIndex = VoxelGrid.CoordsToIndex(neighborCoords, binGridDimensions);
                        if(visitedBins[neighborIndex]) {
                            return false;
                        }

                        Bin neighborBin = bins[neighborIndex];
                        if(neighborBin == null) {
                            return false;
                        }

                        if(neighborBin.IsWholeBinEmpty()) {
                            return false;
                        }

                        if(originBin.IsWalledIn() && neighborBin.IsWalledIn()) {
                            return false;
                        }

                        if(!originBin.IsConnectedToNeighbor(neighborBin, direction)) {
                            return false;
                        }

                        newMoveOrder = new MoveOrder(neighborIndex, direction);
                        return true;
                    }
                }
            }
        }

        if(foundBins.Count == 0) {
            return null;
        }

        Vector3Int newVoxelOffset = minCoord * Bin.WIDTH;
        Vector3Int newDimensions;
        Bin[] newBins = MoveBinsToNewGrid(bins, binGridDimensions, foundBins, minCoord, maxCoord, out newDimensions);
        bool[] interiorMap = GetInteriorMap(newBins, newDimensions);

        return new VoxelCluster(newBins, interiorMap, newVoxelOffset, newDimensions);
    }

    private static bool[] GetInteriorMap(Bin[] bins, Vector3Int binGridDimensions) {
        int binCount = bins.Length;

        bool[] visitedExteriorBins = new bool[binCount];
        bool[] interiorMap = new bool[binCount];

        for(int i = 0; i < binCount; ++i) {
            interiorMap[i] = true;
        }

        for(int z = 0; z < binGridDimensions.z; ++z) {
            for(int y = 0; y < binGridDimensions.y; y++) {
                FloodFillExteriorBins(new Vector3Int(0, y, z), bins, binGridDimensions, visitedExteriorBins, interiorMap);
                FloodFillExteriorBins(new Vector3Int(binGridDimensions.x - 1, y, z), bins, binGridDimensions, visitedExteriorBins, interiorMap);
            }
        }

        for(int z = 0; z < binGridDimensions.z; ++z) {
            for(int x = 0; x < binGridDimensions.x; x++) {
                FloodFillExteriorBins(new Vector3Int(x, 0, z), bins, binGridDimensions, visitedExteriorBins, interiorMap);
                FloodFillExteriorBins(new Vector3Int(x, binGridDimensions.y - 1, z), bins, binGridDimensions, visitedExteriorBins, interiorMap);
            }
        }

        for(int y = 0; y < binGridDimensions.y; ++y) {
            for(int x = 0; x < binGridDimensions.x; x++) {
                FloodFillExteriorBins(new Vector3Int(x, y, 0), bins, binGridDimensions, visitedExteriorBins, interiorMap);
                FloodFillExteriorBins(new Vector3Int(x, y, binGridDimensions.z - 1), bins, binGridDimensions, visitedExteriorBins, interiorMap);
            }
        }

        static void FloodFillExteriorBins(Vector3Int startBinCoords, Bin[] bins, Vector3Int binGridDimensions, bool[] visitedBins, bool[] interiorMap) {
            Queue<MoveOrder> binsToVisit = new Queue<MoveOrder>();
            binsToVisit.Enqueue(new MoveOrder(VoxelGrid.CoordsToIndex(startBinCoords, binGridDimensions), Direction.None));

            while(binsToVisit.Count > 0) {
                MoveOrder moveOrder = binsToVisit.Dequeue();
                if(moveOrder.TargetIndex == -1) {
                    continue;
                }

                int currentBinIndex = moveOrder.TargetIndex;
                if(visitedBins[currentBinIndex]) {
                    continue;
                }

                visitedBins[currentBinIndex] = true;
                interiorMap[currentBinIndex] = false;
                
                Bin currentBin = bins[currentBinIndex];
                if(currentBin != null && currentBin.IsWholeBinFilled()) {
                    continue;
                }

                Vector3Int currentBinCoords = VoxelGrid.IndexToCoords(currentBinIndex, binGridDimensions);

                MoveOrder newMoveOrder;
                if(TryGetNewMoveOrder(Direction.Right, currentBin, currentBinCoords, binGridDimensions, moveOrder, out newMoveOrder)) {
                    binsToVisit.Enqueue(newMoveOrder);
                }
                if(TryGetNewMoveOrder(Direction.Left, currentBin, currentBinCoords, binGridDimensions, moveOrder, out newMoveOrder)) {
                    binsToVisit.Enqueue(newMoveOrder);
                }
                if(TryGetNewMoveOrder(Direction.Up, currentBin, currentBinCoords, binGridDimensions, moveOrder, out newMoveOrder)) {
                    binsToVisit.Enqueue(newMoveOrder);
                }
                if(TryGetNewMoveOrder(Direction.Down, currentBin, currentBinCoords, binGridDimensions, moveOrder, out newMoveOrder)) {
                    binsToVisit.Enqueue(newMoveOrder);
                }
                if(TryGetNewMoveOrder(Direction.Fore, currentBin, currentBinCoords, binGridDimensions, moveOrder, out newMoveOrder)) {
                    binsToVisit.Enqueue(newMoveOrder);
                }
                if(TryGetNewMoveOrder(Direction.Back, currentBin, currentBinCoords, binGridDimensions, moveOrder, out newMoveOrder)) {
                    binsToVisit.Enqueue(newMoveOrder);
                }

                static bool TryGetNewMoveOrder(Direction direction, Bin currentBin, Vector3Int binCoords, Vector3Int binGridDimensions, MoveOrder currentMoveOrder, out MoveOrder newMoveOrder) {
                    if(currentBin != null && !currentBin.HasOpenPathBetweenFaces(currentMoveOrder.DirectionToOrigin, direction)) {
                        newMoveOrder = new MoveOrder();
                        return false;
                    }

                    newMoveOrder = new MoveOrder(VoxelGrid.CoordsToIndex(binCoords + Utils.GetDirectionVector(direction), binGridDimensions), direction);
                    return true;
                }
            }
        }

        return interiorMap;
    }

    private static Bin[] MoveBinsToNewGrid(Bin[] oldBins, Vector3Int oldBinGridDimensions, Queue<int> indexesToMove, Vector3Int minCoord, Vector3Int maxCoord, out Vector3Int newBinGridDimensions) {
        newBinGridDimensions = maxCoord - minCoord + Vector3Int.one;
        Bin[] newBins = new Bin[newBinGridDimensions.x * newBinGridDimensions.y * newBinGridDimensions.z];

        while(indexesToMove.Count > 0) {
            int oldBinIndex = indexesToMove.Dequeue();
            Vector3Int oldBinCoords = VoxelGrid.IndexToCoords(oldBinIndex, oldBinGridDimensions);
            int newBinIndex = VoxelGrid.CoordsToIndex(oldBinCoords - minCoord, newBinGridDimensions);

            newBins[newBinIndex] = new Bin(newBinIndex, newBinGridDimensions, oldBins[oldBinIndex]);
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
}
