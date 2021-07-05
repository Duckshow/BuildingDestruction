using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

using Random = UnityEngine.Random;

[assembly: InternalsVisibleTo("PlayMode")]
public static class VoxelClusterFloodFillHandler {

    private struct MoveOrder {
        public int TargetIndex;
        public Direction DirectionToOrigin;

        public MoveOrder(int targetIndex, Direction direction) {
            TargetIndex = targetIndex;
            DirectionToOrigin = Utils.GetOppositeDirection(direction);
        }
    }

    private static List<VoxelCluster> clusters = new List<VoxelCluster>();
    private static Queue<int> voxelBlocksToVisit = new Queue<int>();
    private static Queue<int> foundBins = new Queue<int>();
    private static Queue<MoveOrder> moveOrders = new Queue<MoveOrder>();

#if UNITY_EDITOR
    private class FloodFillSubject {
        public Bin[] OriginalVoxelBlocks;
        public Vector3Int OriginalOffset;
        public Vector3Int OriginalDimensions;
        public Queue<Queue<int>> FloodFillSequences;

        public FloodFillSubject(Bin[] originalVoxelBlocks, Vector3Int originalOffset, Vector3Int originalDimensions) {
            OriginalVoxelBlocks = originalVoxelBlocks;
            OriginalOffset = originalOffset;
            OriginalDimensions = originalDimensions;
            FloodFillSequences = new Queue<Queue<int>>();
        }
    }

    private static FloodFillSubject latestFindVoxelClustersProcess;
    private static FloodFillSubject latestFindExteriorBlocksProcess;
#endif

    public static IEnumerator FindVoxelClusters(Bin[] voxelBlocks, Vector3Int offset, Vector3Int dimensions, Queue<int> voxelBlocksToLookAt, float stepDuration, Callback<List<VoxelCluster>> onFinished) {
        bool[] visitedVoxelBlocks = new bool[voxelBlocks.Length];

#if UNITY_EDITOR
        latestFindVoxelClustersProcess = new FloodFillSubject(voxelBlocks, offset, dimensions);
#endif

        clusters.Clear();
        while(voxelBlocksToLookAt.Count > 0) {
            int startIndex = voxelBlocksToLookAt.Dequeue();

            if(startIndex < 0) {
                continue;
            }
            
            Bin startVoxelBlock = voxelBlocks[startIndex];
            if(startVoxelBlock.IsWholeBinEmpty()) {
                continue;
            }

            foundBins.Clear();
            voxelBlocksToVisit.Clear();
            voxelBlocksToVisit.Enqueue(startIndex);

            Vector3Int newVoxelOffset = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
            Vector3Int minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
            Vector3Int maxCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

#if UNITY_EDITOR
            Color clusterColor = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
            Queue<int> sequence = new Queue<int>();
#endif

            while(voxelBlocksToVisit.Count > 0) {
                int voxelBlockIndex = voxelBlocksToVisit.Dequeue();
                if(visitedVoxelBlocks[voxelBlockIndex]) {
                    continue;
                }
                visitedVoxelBlocks[voxelBlockIndex] = true;

                Bin voxelBlock = voxelBlocks[voxelBlockIndex];
                
                if(!voxelBlock.IsWalledIn()) {
                    foundBins.Enqueue(voxelBlockIndex);

                    Vector3Int voxelBlockMinVoxelCoords = Bin.GetMinVoxelCoord(voxelBlock);
                    newVoxelOffset.x = Mathf.Min(newVoxelOffset.x, voxelBlock.Coords.x * Bin.WIDTH + voxelBlockMinVoxelCoords.x);
                    newVoxelOffset.y = Mathf.Min(newVoxelOffset.y, voxelBlock.Coords.y * Bin.WIDTH + voxelBlockMinVoxelCoords.y);
                    newVoxelOffset.z = Mathf.Min(newVoxelOffset.z, voxelBlock.Coords.z * Bin.WIDTH + voxelBlockMinVoxelCoords.z);

                    minCoord.x = Mathf.Min(minCoord.x, voxelBlock.Coords.x);
                    minCoord.y = Mathf.Min(minCoord.y, voxelBlock.Coords.y);
                    minCoord.z = Mathf.Min(minCoord.z, voxelBlock.Coords.z);
                    maxCoord.x = Mathf.Max(maxCoord.x, voxelBlock.Coords.x);
                    maxCoord.y = Mathf.Max(maxCoord.y, voxelBlock.Coords.y);
                    maxCoord.z = Mathf.Max(maxCoord.z, voxelBlock.Coords.z);

#if UNITY_EDITOR
                    if(stepDuration > 0f) {
                        Utils.DebugDrawVoxelCluster(voxelBlocks, offset, new Color(1f, 1f, 1f, 0.25f), stepDuration, shouldDrawVoxelBlock: (Bin voxelBlock) => voxelBlock.IsExterior);

                        foreach(var foundIndex in foundBins) {
                            Utils.DebugDrawVoxelBlock(voxelBlocks[foundIndex], offset, clusterColor, stepDuration);
                        }

                        yield return new WaitForSeconds(stepDuration);
                    }

                    sequence.Enqueue(voxelBlockIndex);
#endif
                }

                TryAddNeighborToVisit(voxelBlocks, voxelBlock, dimensions, Direction.Right, voxelBlocksToVisit);
                TryAddNeighborToVisit(voxelBlocks, voxelBlock, dimensions, Direction.Left, voxelBlocksToVisit);
                TryAddNeighborToVisit(voxelBlocks, voxelBlock, dimensions, Direction.Up, voxelBlocksToVisit);
                TryAddNeighborToVisit(voxelBlocks, voxelBlock, dimensions, Direction.Down, voxelBlocksToVisit);
                TryAddNeighborToVisit(voxelBlocks, voxelBlock, dimensions, Direction.Fore, voxelBlocksToVisit);
                TryAddNeighborToVisit(voxelBlocks, voxelBlock, dimensions, Direction.Back, voxelBlocksToVisit);

                static void TryAddNeighborToVisit(Bin[] voxelBlocks, Bin origin, Vector3Int dimensions, Direction direction, Queue<int> voxelBlocksToVisit) {
                    if(!origin.IsConnectedToNeighbor(direction)) {
                        return;
                    }

                    Vector3Int dirVec = Utils.DirectionToVector(direction);
                    Vector3Int neighborCoords = new Vector3Int(origin.Coords.x + dirVec.x, origin.Coords.y + dirVec.y, origin.Coords.z + dirVec.z);

                    int neighborIndex = Utils.CoordsToIndex(neighborCoords, dimensions);
                    if(neighborIndex == -1) {
                        return;
                    }

                    Bin neighbor = voxelBlocks[neighborIndex];

                    if(neighbor.IsWholeBinEmpty()) {
                        return;
                    }

                    if(origin.IsWalledIn() && neighbor.IsWalledIn()) {
                        return;
                    }

                    voxelBlocksToVisit.Enqueue(neighborIndex);
                }
            }

#if UNITY_EDITOR
            latestFindVoxelClustersProcess.FloodFillSequences.Enqueue(sequence);
#endif

            if(foundBins.Count == 0) {
                continue;
            }

            Vector3Int newDimensions;
            Bin[] newVoxelBlocks = MoveBlocksAndTranslateData(voxelBlocks, dimensions, foundBins, minCoord, maxCoord, out newDimensions);

            yield return FindExteriorBlocksAroundCluster(newVoxelBlocks, newVoxelOffset, newDimensions, stepDuration);

            clusters.Add(new VoxelCluster(newVoxelBlocks, newVoxelOffset, newDimensions));
        }

        if(onFinished != null) {
            onFinished(clusters);
        }
    }

    internal static Bin[] MoveBlocksAndTranslateData(Bin[] voxelBlocks, Vector3Int dimensions, Queue<int> indexesToMove, Vector3Int minCoord, Vector3Int maxCoord, out Vector3Int newBinGridDimensions) {
        newBinGridDimensions = maxCoord - minCoord + Vector3Int.one;
        Bin[] newVoxelBlocks = new Bin[newBinGridDimensions.x * newBinGridDimensions.y * newBinGridDimensions.z];

        for(int i = 0; i < newVoxelBlocks.Length; i++) {
            newVoxelBlocks[i] = new Bin(i, newBinGridDimensions, byte.MinValue);
        }

        while(indexesToMove.Count > 0) {
            int oldIndex = indexesToMove.Dequeue();
            Vector3Int oldBinCoords = Utils.IndexToCoords(oldIndex, dimensions);
            int newIndex = Utils.CoordsToIndex(oldBinCoords - minCoord, newBinGridDimensions);

            newVoxelBlocks[newIndex] = new Bin(voxelBlocks[oldIndex], newIndex, newBinGridDimensions);
        }

        return newVoxelBlocks;
    }

    private static IEnumerator FindExteriorBlocksAroundCluster(Bin[] voxelBlocks, Vector3Int clusterOffset, Vector3Int clusterDimensions, float stepDuration) {

        // NOTE: this function only finds exterior blocks *around* the cluster - connected blocks are marked automatically when their voxels are set
        // if we can come up with a way to do this automatically as well, please do.

        bool[] visitedExteriorBins = new bool[voxelBlocks.Length];

#if UNITY_EDITOR
        latestFindExteriorBlocksProcess = new FloodFillSubject(voxelBlocks, clusterOffset, clusterDimensions);
#endif

        for(int z = 0; z < clusterDimensions.z; z++) {
            for(int y = 0; y < clusterDimensions.y; y++) {
                for(int x = 0; x < clusterDimensions.x; x++) {
                    if(x > 0 && x < clusterDimensions.x - 1 && y > 0 && y < clusterDimensions.y - 1 && z > 0 && z < clusterDimensions.z - 1) {
                        continue;
                    }

#if UNITY_EDITOR
                    Queue<int> sequence = new Queue<int>();
#endif 

                    moveOrders.Clear();
                    moveOrders.Enqueue(new MoveOrder(Utils.CoordsToIndex(new Vector3Int(x, y, z), clusterDimensions), Direction.None));

                    while(moveOrders.Count > 0) {
                        MoveOrder currentMoveOrder = moveOrders.Dequeue();
                        if(currentMoveOrder.TargetIndex == -1) {
                            continue;
                        }

                        int currentBlockIndex = currentMoveOrder.TargetIndex;
                        if(visitedExteriorBins[currentBlockIndex]) {
                            continue;
                        }

                        Bin voxelBlock = voxelBlocks[currentBlockIndex];
                        if(voxelBlock.IsExterior) {
                            continue;
                        }

#if UNITY_EDITOR
                        if(stepDuration > 0f) {
                            Utils.DebugDrawVoxelCluster(voxelBlocks, clusterOffset, new Color(1f, 1f, 1f, 0.25f), stepDuration, shouldDrawVoxelBlock: (Bin voxelBlock) => voxelBlock.IsInterior);
                            Utils.DebugDrawVoxelCluster(voxelBlocks, clusterOffset, new Color(0f, 1f, 0f, 0.25f), stepDuration, shouldDrawVoxelBlock: (Bin voxelBlock) => voxelBlock.IsExterior && !voxelBlock.IsForcedExterior);
                            Utils.DebugDrawVoxelCluster(voxelBlocks, clusterOffset, new Color(0f, 1f, 0f, 1f), stepDuration, shouldDrawVoxelBlock: (Bin voxelBlock) => voxelBlock.IsForcedExterior);

                            yield return new WaitForSeconds(stepDuration);
                        }

                        sequence.Enqueue(currentBlockIndex);
#endif

                        visitedExteriorBins[currentBlockIndex] = true;
                        voxelBlocks[currentBlockIndex] = new Bin(voxelBlock, isForcedExterior: true);

                        if(voxelBlock.IsWholeBinFilled()) {
                            continue;
                        }

                        MoveOrder newMoveOrder;
                        if(TryGetNewMoveOrder(Direction.Right, out newMoveOrder)) {
                            moveOrders.Enqueue(newMoveOrder);
                        }
                        if(TryGetNewMoveOrder(Direction.Left, out newMoveOrder)) {
                            moveOrders.Enqueue(newMoveOrder);
                        }
                        if(TryGetNewMoveOrder(Direction.Up, out newMoveOrder)) {
                            moveOrders.Enqueue(newMoveOrder);
                        }
                        if(TryGetNewMoveOrder(Direction.Down, out newMoveOrder)) {
                            moveOrders.Enqueue(newMoveOrder);
                        }
                        if(TryGetNewMoveOrder(Direction.Fore, out newMoveOrder)) {
                            moveOrders.Enqueue(newMoveOrder);
                        }
                        if(TryGetNewMoveOrder(Direction.Back, out newMoveOrder)) {
                            moveOrders.Enqueue(newMoveOrder);
                        }

                        bool TryGetNewMoveOrder(Direction direction, out MoveOrder newMoveOrder) {
                            if(direction == currentMoveOrder.DirectionToOrigin) {
                                newMoveOrder = new MoveOrder();
                                return false;
                            }

                            if(!voxelBlock.IsWholeBinEmpty() && !voxelBlock.HasOpenPathBetweenFaces(currentMoveOrder.DirectionToOrigin, direction)) {
                                newMoveOrder = new MoveOrder();
                                return false;
                            }

                            newMoveOrder = new MoveOrder(Utils.CoordsToIndex(voxelBlock.Coords + Utils.DirectionToVector(direction), clusterDimensions), direction);
                            return true;
                        }
                    }

#if UNITY_EDITOR
                    latestFindExteriorBlocksProcess.FloodFillSequences.Enqueue(sequence);
#endif
                }
            }
        }
    }

#if UNITY_EDITOR
    public static IEnumerator ReplayAndClearLatestFindVoxelClustersProcess(float stepDuration) {
        yield return ReplayProcess(latestFindVoxelClustersProcess, stepDuration, shouldDrawVoxelBlock: (Bin voxelBlock) => { return true; });

        latestFindVoxelClustersProcess = null;
    }

    public static IEnumerator ReplayAndClearLatestFindExteriorBlocksProcess(float stepDuration) {
        yield return ReplayProcess(latestFindExteriorBlocksProcess, stepDuration, shouldDrawVoxelBlock: (Bin voxelBlock) => { return voxelBlock.IsExterior; });
        
        latestFindExteriorBlocksProcess = null;
    }

    private static IEnumerator ReplayProcess(FloodFillSubject subject, float stepDuration, Predicate<Bin> shouldDrawVoxelBlock) {
        while(subject.FloodFillSequences.Count > 0) {
            Queue<int> sequence = subject.FloodFillSequences.Dequeue();

            Utils.DebugDrawVoxelCluster(subject.OriginalVoxelBlocks, subject.OriginalOffset, new Color(1f, 1f, 1f, 0.5f), stepDuration * sequence.Count, shouldDrawVoxelBlock: (Bin voxelBlock) => { return !voxelBlock.IsWalledIn(); });

            Color clusterColor = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
            
            while(sequence.Count > 0) {
                int voxelBlockIndex = sequence.Dequeue();

                Bin voxelBlock = subject.OriginalVoxelBlocks[voxelBlockIndex];
                if(!shouldDrawVoxelBlock(voxelBlock)) {
                    continue;
                }

                Utils.DebugDrawVoxelBlock(voxelBlock, subject.OriginalOffset, clusterColor, stepDuration * (sequence.Count + 1));
                yield return new WaitForSeconds(stepDuration);
            }
        }
    }
#endif
}
