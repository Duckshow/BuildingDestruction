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

    public static IEnumerator FindVoxelClusters(Bin[] voxelBlocks, Vector3Int offset, Vector3Int dimensions, Queue<int> voxelBlocksToLookAt, float stepDuration, Callback<List<VoxelCluster>> onFinished) {
        Debug.Assert(voxelBlocksToLookAt.Count > 0);
        
        bool[] visitedVoxelBlocks = new bool[voxelBlocks.Length];

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

                    if(!origin.IsConnectedToNeighbor(direction)) {
                        return;
                    }

                    voxelBlocksToVisit.Enqueue(neighborIndex);
                }
            }

            if(foundBins.Count == 0) {
                continue;
            }

            Vector3Int newDimensions;
            Bin[] newVoxelBlocks = MoveBlocksAndTranslateData(voxelBlocks, dimensions, foundBins, minCoord, maxCoord, out newDimensions);

            yield return FindExteriorBlocksAroundCluster(newVoxelBlocks, newVoxelOffset, newDimensions, stepDuration);

            clusters.Add(new VoxelCluster(newVoxelBlocks, newVoxelOffset, newDimensions));
        }

        if(clusters.Count == 0) {
            throw new Exception("Failed to find a single cluster!");
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

        for(int z = 0; z < clusterDimensions.z; z++) {
            for(int y = 0; y < clusterDimensions.y; y++) {
                for(int x = 0; x < clusterDimensions.x; x++) {
                    if(x > 0 && x < clusterDimensions.x - 1 && y > 0 && y < clusterDimensions.y - 1 && z > 0 && z < clusterDimensions.z - 1) {
                        continue;
                    }

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
                }
            }
        }
    }
}
