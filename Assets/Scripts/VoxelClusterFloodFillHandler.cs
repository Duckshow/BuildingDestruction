using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

using Random = UnityEngine.Random;

[assembly: InternalsVisibleTo("PlayMode")]
public static class VoxelClusterFloodFillHandler {
    [BurstCompile]
    private struct FloodFillJob : IJobParallelFor {

        [ReadOnly] public NativeArray<Bin> VoxelBlocks;
        [ReadOnly] public Vector3Int Dimensions;

        public NativeArray<int> Targets;
        public NativeArray<bool> FoundTargets;
        
        [NativeDisableParallelForRestriction] 
        public NativeArray<bool> VisitedTargets;

        [NativeDisableParallelForRestriction] 
        public NativeArray<int> NextTargets;
        
        [NativeDisableParallelForRestriction] 
        public NativeArray<int> NextTargetsAbove;
        
        [NativeDisableParallelForRestriction] 
        public NativeArray<int> NextTargetsBelow;

        public NativeArray<int> NextTargetsCount;
        public NativeArray<int> NextTargetsAboveCount;
        public NativeArray<int> NextTargetsBelowCount;

        public void Execute(int level) {
            int voxelBlockIndex = Targets[level];
            if(VisitedTargets[voxelBlockIndex]) {
                return;
            }
            VisitedTargets[voxelBlockIndex] = true;

            Bin voxelBlock = VoxelBlocks[voxelBlockIndex];
            if(!voxelBlock.IsWalledIn()) {
                FoundTargets[level] = true;
            }

            for(int i = 0; i < 6; i++) {
                Direction dir = (Direction)i;

                Vector3Int dirVec = Utils.DirectionToVector(dir);
                Vector3Int neighborCoords = new Vector3Int(voxelBlock.Coords.x + dirVec.x, voxelBlock.Coords.y + dirVec.y, voxelBlock.Coords.z + dirVec.z);

                int nextTargetIndex = Utils.CoordsToIndex(neighborCoords, Dimensions);
                if(nextTargetIndex == -1) {
                    continue;
                }

                Bin neighbor = VoxelBlocks[nextTargetIndex];
                if(neighbor.IsWholeBinEmpty()) {
                    continue;
                }

                if(voxelBlock.IsWalledIn() && neighbor.IsWalledIn()) {
                    continue;
                }

                if(!voxelBlock.IsConnectedToNeighbor(dir)) {
                    continue;
                }

                if(dir == Direction.Up) {
                    int index = level * 6 + NextTargetsAboveCount[level];
                    NextTargetsAbove[index] = nextTargetIndex;
                    NextTargetsAboveCount[level]++;
                }
                else if(dir == Direction.Down) {
                    int index = level * 6 + NextTargetsBelowCount[level];
                    NextTargetsBelow[index] = nextTargetIndex;
                    NextTargetsBelowCount[level]++;
                }
                else {
                    int index = level * 6 + NextTargetsCount[level];
                    NextTargets[index] = nextTargetIndex;
                    NextTargetsCount[level]++;
                }
            }
        }
    }

    private struct MoveOrder {
        public int TargetIndex;
        public Direction DirectionToOrigin;

        public MoveOrder(int targetIndex, Direction direction) {
            TargetIndex = targetIndex;
            DirectionToOrigin = Utils.GetOppositeDirection(direction);
        }
    }

    private static List<VoxelCluster> clusters = new List<VoxelCluster>();
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

            Vector3Int newVoxelOffset = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
            Vector3Int minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
            Vector3Int maxCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

#if UNITY_EDITOR
            Color clusterColor = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
#endif

            Queue<int> foundVoxelBlocks = new Queue<int>();

            NativeArray<Bin> nativeVoxelBlocks  = new NativeArray<Bin>(voxelBlocks, Allocator.TempJob);
            NativeArray<int> targets            = new NativeArray<int>(dimensions.y, Allocator.TempJob);
            NativeArray<bool> foundTargets      = new NativeArray<bool>(dimensions.y, Allocator.TempJob);
            NativeArray<bool> visitedTargets    = new NativeArray<bool>(dimensions.Product(), Allocator.TempJob);

            NativeArray<int> nextTargets            = new NativeArray<int>(dimensions.y * 6, Allocator.TempJob);
            NativeArray<int> nextTargetsAbove       = new NativeArray<int>(dimensions.y * 6, Allocator.TempJob);
            NativeArray<int> nextTargetsBelow       = new NativeArray<int>(dimensions.y * 6, Allocator.TempJob);
            NativeArray<int> nextTargetsCount       = new NativeArray<int>(dimensions.y, Allocator.TempJob);
            NativeArray<int> nextTargetsAboveCount  = new NativeArray<int>(dimensions.y, Allocator.TempJob);
            NativeArray<int> nextTargetsBelowCount  = new NativeArray<int>(dimensions.y, Allocator.TempJob);

            FloodFillJob floodFillJob           = new FloodFillJob();
            floodFillJob.VoxelBlocks            = nativeVoxelBlocks;
            floodFillJob.Dimensions             = dimensions;
            floodFillJob.FoundTargets           = foundTargets;
            floodFillJob.VisitedTargets         = visitedTargets;
            floodFillJob.NextTargets            = nextTargets;
            floodFillJob.NextTargetsCount       = nextTargetsCount;
            floodFillJob.NextTargetsAbove       = nextTargetsAbove;
            floodFillJob.NextTargetsAboveCount  = nextTargetsAboveCount;
            floodFillJob.NextTargetsBelow       = nextTargetsBelow;
            floodFillJob.NextTargetsBelowCount  = nextTargetsBelowCount;

            targets[startVoxelBlock.Coords.y] = startVoxelBlock.Index;
            
            while(true) {
                floodFillJob.Targets = targets;

                JobHandle handle = floodFillJob.Schedule(dimensions.y, 1);
                handle.Complete();

                bool foundNewTargets = false;
                
                for(int y = 0; y < dimensions.y; y++) {
                    if(foundTargets[y]) {
                        foundVoxelBlocks.Enqueue(targets[y]);

                        Vector3Int foundTargetCoords = Utils.IndexToCoords(targets[y], dimensions);
                        minCoord.x = Mathf.Min(minCoord.x, foundTargetCoords.x);
                        minCoord.y = Mathf.Min(minCoord.y, foundTargetCoords.y);
                        minCoord.z = Mathf.Min(minCoord.z, foundTargetCoords.z);
                        maxCoord.x = Mathf.Max(maxCoord.x, foundTargetCoords.x);
                        maxCoord.y = Mathf.Max(maxCoord.y, foundTargetCoords.y);
                        maxCoord.z = Mathf.Max(maxCoord.z, foundTargetCoords.z);

                        foundTargets[y] = false;
                    }
                    
                    if(y > 0) {
                        for(int i = 0; i < nextTargetsAboveCount[y - 1]; i++) {
                            int index      = GetIndexInTargetArray(x: nextTargetsCount[y] - 1, y);
                            int indexBelow = GetIndexInTargetArray(x: nextTargetsAboveCount[y - 1] - 1, y - 1);

                            nextTargets[index] = nextTargetsAbove[indexBelow];
                            nextTargetsCount[y]++;
                            nextTargetsAboveCount[indexBelow]--;
                        }
                    }

                    if(y < dimensions.y - 1) {
                        for(int i = 0; i < nextTargetsBelowCount[y + 1]; i++) {
                            int index      = GetIndexInTargetArray(x: nextTargetsCount[y] - 1, y);
                            int indexAbove = GetIndexInTargetArray(x: nextTargetsBelowCount[y + 1] - 1, y + 1);

                            nextTargets[index] = nextTargetsBelow[indexAbove];
                            nextTargetsCount[y]++;
                            nextTargetsBelowCount[indexAbove]--;
                        }
                    }

                    if(nextTargetsCount[y] == 0) {
                        continue;
                    }

                    targets[y] = nextTargets[GetIndexInTargetArray(x: nextTargetsCount[y] - 1, y)];
                    nextTargetsCount[y]--;
                    foundNewTargets = true;

                    static int GetIndexInTargetArray(int x, int y) {
                        return y * 6 + x;
                    }
                }

                if(!foundNewTargets) {
                    break;
                }

#if UNITY_EDITOR
                if(stepDuration > 0f) {
                    Utils.DebugDrawVoxelCluster(voxelBlocks, offset, new Color(1f, 1f, 1f, 0.25f), stepDuration, shouldDrawVoxelBlock: (Bin voxelBlock) => voxelBlock.IsExterior);

                    foreach(var foundIndex in foundVoxelBlocks) {
                        Utils.DebugDrawVoxelBlock(voxelBlocks[foundIndex], offset, clusterColor, stepDuration);
                        yield return new WaitForSeconds(stepDuration);
                    }
                }
#endif
            }

            nativeVoxelBlocks.Dispose();
            targets.Dispose();
            foundTargets.Dispose();
            visitedTargets.Dispose();
            nextTargets.Dispose();
            nextTargetsAbove.Dispose();
            nextTargetsBelow.Dispose();
            nextTargetsCount.Dispose();
            nextTargetsAboveCount.Dispose();
            nextTargetsBelowCount.Dispose();

            if(foundVoxelBlocks.Count == 0) {
                continue;
            }

            Vector3Int newDimensions;
            Bin[] newVoxelBlocks = MoveBlocksAndTranslateData(voxelBlocks, dimensions, foundVoxelBlocks, minCoord, maxCoord, out newDimensions);

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
        Bin[] newVoxelBlocks = new Bin[newBinGridDimensions.Product()];

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
