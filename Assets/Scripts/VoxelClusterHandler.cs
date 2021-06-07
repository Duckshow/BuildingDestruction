using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

[assembly: InternalsVisibleTo("PlayMode")]
public static class VoxelClusterHandler {

    private static List<Octree<bool>> clusters = new List<Octree<bool>>();

    public static void FindVoxelClustersAndSplit(VoxelGrid voxelGrid, Queue<Vector3Int> dirtyVoxels, Action onFinished) {
        voxelGrid.StartCoroutine(FindClusters(voxelGrid.GetVoxelMap(), dirtyVoxels, debug: false, debugDrawDuration: 0f, onFinished: (List<Octree<bool>> foundClusters) => {
            if(foundClusters.Count == 0) {
                Object.Destroy(voxelGrid.gameObject);
                return;
            }

            VoxelGrid[] splitVoxelGrids = TrySplit(voxelGrid, foundClusters);
            ApplyClusters(splitVoxelGrids, foundClusters);

            if(onFinished != null) {
                onFinished();
            }
        }));
    }

    internal static IEnumerator FindClusters(Octree<bool> voxelMap, Queue<Vector3Int> dirtyVoxels, bool debug, float debugDrawDuration, Action<List<Octree<bool>>> onFinished) {
        Octree<bool> visitedVoxels = new Octree<bool>(voxelMap.Offset, voxelMap.Dimensions, startValue: false);

        clusters.Clear();
        while(dirtyVoxels.Count > 0) {
            Vector3Int voxelCoords = dirtyVoxels.Dequeue();

            yield return TryFindCluster(voxelCoords, voxelMap, visitedVoxels, debug, debugDrawDuration, onFinished: (Octree<bool> foundCluster) => {
                if(foundCluster != null) {
                    clusters.Add(foundCluster);
                }
            });
        }

        for(int z = 0; z < voxelMap.Size; z++) {
            for(int y = 0; y < voxelMap.Size; y++) {
                for(int x = 0; x < voxelMap.Size; x++) {
                    bool voxelMapSuccess = voxelMap.TryGetValue(new Vector3Int(x, y, z), out bool voxelMapValue);
                    bool visitedVoxelsSuccess = visitedVoxels.TryGetValue(new Vector3Int(x, y, z), out bool visitedVoxelsValue);

                    Debug.Assert(voxelMapSuccess == visitedVoxelsSuccess);
                    Debug.Assert(voxelMapValue == visitedVoxelsValue, string.Format("{0} wasn't visited!", new Vector3Int(x, y, z)));

                }
            }
        }

        onFinished(clusters);
    }

    private static IEnumerator TryFindCluster(Vector3Int startVoxelCoords, Octree<bool> voxelMap, Octree<bool> visitedVoxels, bool debug, float debugDrawDuration, Action<Octree<bool>> onFinished) {
        if(!voxelMap.TryGetNode(startVoxelCoords, out Octree<bool>.Node startingNode)) {
            yield break;
        }
        if(!startingNode.HasValue()) {
            yield break;
        }

        Octree<bool> foundVoxels = new Octree<bool>(voxelMap.Offset, voxelMap.Dimensions, startValue: false);
        Vector3Int minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        Vector3Int maxCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

        Debug.Assert(voxelMap.Dimensions == visitedVoxels.Dimensions);
        Debug.Assert(voxelMap.Dimensions == foundVoxels.Dimensions);

        Queue<Octree<bool>.Node> voxelsToVisit = new Queue<Octree<bool>.Node>();
        voxelsToVisit.Enqueue(startingNode);

        Color clusterColor = !debug ? Color.clear : Random.ColorHSV(
             hueMin: 0f,
             hueMax: 1f,
             saturationMin: 1f,
             saturationMax: 1f,
             valueMin: 1f,
             valueMax: 1f
         );

        while(voxelsToVisit.Count > 0) {
            if(Input.GetKey(KeyCode.LeftShift)) {
                Debug.Log("boop");
            }

            bool success = GoToNextVoxel(voxelsToVisit, voxelMap, visitedVoxels, foundVoxels, ref minCoord, ref maxCoord, debug, debugDrawDuration, clusterColor);

            if(debug && success) {
                yield return new WaitForSeconds(debugDrawDuration);
            }
        }

        if(foundVoxels.IsEmpty()) {
            yield break;
        }

        Vector3Int newDimensions = new Vector3Int(
            maxCoord.x - minCoord.x + 1, 
            maxCoord.y - minCoord.y + 1, 
            maxCoord.z - minCoord.z + 1
        );

        foundVoxels.Resize(newOffset: foundVoxels.Offset + minCoord, newDimensions);
        onFinished(foundVoxels);
    }

    private static bool GoToNextVoxel(Queue<Octree<bool>.Node> voxelsToVisit, Octree<bool> voxelMap, Octree<bool> visitedVoxels, Octree<bool> foundVoxels, ref Vector3Int minCoord, ref Vector3Int maxCoord, bool debug, float debugDrawDuration, Color debugColor) {
        Octree<bool>.Node node = voxelsToVisit.Dequeue();

        if(!node.HasValue()) {
            return false;
        }

        Vector3Int nodeOffset = node.GetOffset(voxelMap.Offset);

        if(visitedVoxels.TryGetValue(nodeOffset, out bool hasVisitedVoxel) && hasVisitedVoxel) {
            return false;
        }

        visitedVoxels.SetValue(nodeOffset, true, node.Size);
        foundVoxels.SetValue(nodeOffset, true, node.Size);

        minCoord.x = Mathf.Min(minCoord.x, nodeOffset.x);
        minCoord.y = Mathf.Min(minCoord.y, nodeOffset.y);
        minCoord.z = Mathf.Min(minCoord.z, nodeOffset.z);
        maxCoord.x = Mathf.Max(maxCoord.x, nodeOffset.x + node.Size - 1);
        maxCoord.y = Mathf.Max(maxCoord.y, nodeOffset.y + node.Size - 1);
        maxCoord.z = Mathf.Max(maxCoord.z, nodeOffset.z + node.Size - 1);

        TryAddNeighbor(Direction.Right);
        TryAddNeighbor(Direction.Left);
        TryAddNeighbor(Direction.Up);
        TryAddNeighbor(Direction.Down);
        TryAddNeighbor(Direction.Fore);
        TryAddNeighbor(Direction.Back);

        void TryAddNeighbor(Direction direction) {
            if(!voxelMap.TryGetAdjacentNodes(node, direction, out Octree<bool>.Node[] nodes, out int nodeCount)) {
                return;
            }

            for(int i = 0; i < nodeCount; i++) {
                voxelsToVisit.Enqueue(nodes[i]);
            }
        }

        if(debug) {
            voxelMap.DebugDrawOctree(Color.white, emptyNodeColor: Color.clear, debugDrawDuration);
            foundVoxels.DebugDrawOctree(debugColor, emptyNodeColor: Color.white, debugDrawDuration);
        }
        
        return true;
    }

    private static VoxelGrid[] TrySplit(VoxelGrid originalVoxelGrid, List<Octree<bool>> clusters) {
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
                
                VoxelGrid newVoxelGrid = go.GetComponent<VoxelGrid>();
                newVoxelGrid.transform.parent = originalVoxelGrid.transform.parent;
                newVoxelGrid.transform.localPosition = originalVoxelGrid.transform.localPosition;
                newVoxelGrid.transform.name = originalVoxelGrid.name + " (Cluster)";
                newVoxelGrid.MarkAsCopy();
                voxelGrids[i] = newVoxelGrid;
            }

            for(int i = 1; i < originalMeshObjects.Length; i++) {
                originalMeshObjects[i].parent = originalMeshTransform;
            }
        }

        return voxelGrids;
    }

    private static void ApplyClusters(VoxelGrid[] voxelGrids, List<Octree<bool>> clusters) {
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

    private static int GetBiggestVoxelClusterIndex(List<Octree<bool>> clusters) {
        int biggestClusterIndex = -1;
        int biggestClusterSize = int.MinValue;
        for(int i = 0; i < clusters.Count; i++) {
            Octree<bool> cluster = clusters[i];
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
}
