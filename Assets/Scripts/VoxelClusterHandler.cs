using System.Collections.Generic;
using UnityEngine;

public static partial class VoxelClusterHandler {

    private static List<VoxelCluster> clusters = new List<VoxelCluster>();
    private static Queue<Vector3Int> voxelsToVisit = new Queue<Vector3Int>();
    private static Queue<Vector3Int> foundVoxels = new Queue<Vector3Int>();
    private static Queue<MoveOrder> moveOrders = new Queue<MoveOrder>();

    public static void FindVoxelClustersAndSplit(VoxelGrid voxelGrid, Queue<Vector3Int> dirtyVoxels) {
        Octree<bool> visitedVoxels = new Octree<bool>(voxelGrid.GetVoxelMap().Size);

        clusters.Clear();
        while(dirtyVoxels.Count > 0) {
            Vector3Int voxelCoords = dirtyVoxels.Dequeue();

            VoxelCluster cluster = TryFindCluster(voxelCoords, voxelGrid.GetVoxelMap(), voxelGrid.GetVoxelGridDimensions(), visitedVoxels);
            if(cluster != null) {
                clusters.Add(cluster);
            }
        }

        if(clusters.Count == 0) {
            Object.Destroy(voxelGrid.gameObject);
            return;
        }

        VoxelGrid[] splitVoxelGrids = TrySplit(voxelGrid, clusters);
        ApplyClusters(splitVoxelGrids, clusters);
    }

    public static VoxelCluster TryFindCluster(Vector3Int startVoxelCoords, Octree<bool> voxelMap, Vector3Int voxelGridDimensions, Octree<bool> visitedVoxels) {
        Vector3Int minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        Vector3Int maxCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

        foundVoxels.Clear();
        voxelsToVisit.Clear();
        voxelsToVisit.Enqueue(startVoxelCoords);

        Debug.Log("TODO: refactor TryFindCluster!");

        while(voxelsToVisit.Count > 0) {
            Vector3Int voxelCoords = voxelsToVisit.Dequeue();
            if(visitedVoxels.TryGetValue(voxelCoords, out bool b1, debugDrawCallback: null)) {
                continue;
            }

            visitedVoxels.SetValue(voxelCoords, true);
            
            if(!voxelMap.TryGetValue(voxelCoords, out bool b2, debugDrawCallback: null)) {
                continue;
            }

            foundVoxels.Enqueue(voxelCoords);

            minCoord.x = Mathf.Min(minCoord.x, voxelCoords.x);
            minCoord.y = Mathf.Min(minCoord.y, voxelCoords.y);
            minCoord.z = Mathf.Min(minCoord.z, voxelCoords.z);
            maxCoord.x = Mathf.Max(maxCoord.x, voxelCoords.x);
            maxCoord.y = Mathf.Max(maxCoord.y, voxelCoords.y);
            maxCoord.z = Mathf.Max(maxCoord.z, voxelCoords.z);

            TryAddNeighborToVisit(voxelCoords, voxelMap, voxelGridDimensions, Direction.Right, voxelsToVisit);
            TryAddNeighborToVisit(voxelCoords, voxelMap, voxelGridDimensions, Direction.Left,  voxelsToVisit);
            TryAddNeighborToVisit(voxelCoords, voxelMap, voxelGridDimensions, Direction.Up,    voxelsToVisit);
            TryAddNeighborToVisit(voxelCoords, voxelMap, voxelGridDimensions, Direction.Down,  voxelsToVisit);
            TryAddNeighborToVisit(voxelCoords, voxelMap, voxelGridDimensions, Direction.Fore,  voxelsToVisit);
            TryAddNeighborToVisit(voxelCoords, voxelMap, voxelGridDimensions, Direction.Back,  voxelsToVisit);

            static void TryAddNeighborToVisit(Vector3Int centerCoords, Octree<bool> voxelMap, Vector3Int voxelGridDimensions, Direction direction, Queue<Vector3Int> voxelsToVisit) {
                Vector3Int dirVec = Utils.GetDirectionVector(direction);
                Vector3Int neighborCoords = new Vector3Int(centerCoords.x + dirVec.x, centerCoords.y + dirVec.y, centerCoords.z + dirVec.z);
                
                if(!VoxelGrid.AreCoordsWithinDimensions(neighborCoords, voxelGridDimensions)) {
                    return;
                }

                voxelsToVisit.Enqueue(neighborCoords);
            }
        }

        if(foundVoxels.Count == 0) {
            return null;
        }

        Vector3Int newVoxelOffset = minCoord;
        Vector3Int newDimensions;
        Octree<bool> newVoxelMap = MoveVoxelsToNewVoxelMap(foundVoxels, minCoord, maxCoord, out newDimensions);

        return new VoxelCluster(newVoxelMap, newVoxelOffset, newDimensions);
    }

    public static Octree<bool> MoveVoxelsToNewVoxelMap(Queue<Vector3Int> foundVoxels, Vector3Int minCoord, Vector3Int maxCoord, out Vector3Int newVoxelGridDimensions) {
        newVoxelGridDimensions = maxCoord - minCoord + Vector3Int.one;
        Octree<bool> newVoxels = new Octree<bool>(Mathf.Max(newVoxelGridDimensions.x, Mathf.Max(newVoxelGridDimensions.y, newVoxelGridDimensions.z)));

        while(foundVoxels.Count > 0) {
            Vector3Int oldCoords = foundVoxels.Dequeue();
            Vector3Int newCoords = oldCoords - minCoord;

            newVoxels.SetValue(newCoords, true);
        }

        return newVoxels;
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
            VoxelCluster cluster = clusters[i];
            int size = cluster.Dimensions.x * cluster.Dimensions.y * cluster.Dimensions.z;

            if(size > biggestClusterSize) {
                biggestClusterSize = size;
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
