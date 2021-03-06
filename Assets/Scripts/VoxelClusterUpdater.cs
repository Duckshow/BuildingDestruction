using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PlayMode")]
public class VoxelClusterUpdater : Singleton<VoxelClusterUpdater> {

    private const float UPDATE_LATENCY = 0.1f;

    private Queue<IVoxelCluster> dirtyClusters = new Queue<IVoxelCluster>();
    private float timer;

    private void Start() {
        timer = UPDATE_LATENCY;
    }

    private void LateUpdate() {
        timer -= Time.deltaTime;
        if(timer > 0f) {
            return;
        }
        timer = UPDATE_LATENCY;

        while(dirtyClusters.Count > 0) { // TODO: could probably multithread this
            IVoxelCluster cluster = dirtyClusters.Dequeue();

            if(!cluster.IsDirty()) {
                continue;
            }

            StartCoroutine(RemoveVoxelsInCluster(cluster, stepDuration: 0f));
        }
    }

    public void ScheduleUpdate(IVoxelCluster cluster) {
        dirtyClusters.Enqueue(cluster);
    }

    internal static IEnumerator RemoveVoxelsInCluster(IVoxelCluster cluster, float stepDuration) {
        cluster.OnUpdateStart(out Bin[] voxelBlocks, out Queue<int> voxelsToRemove);

        Debug.Assert(voxelsToRemove.Count > 0);

        Vector3Int dimensions = cluster.GetDimensions();
        bool[] visitedVoxels = new bool[(dimensions * Bin.WIDTH).Product()];

        Queue<int> generateInteriorsQueue           = new Queue<int>(visitedVoxels.Length);
		Queue<int> deleteExteriorsQueue             = new Queue<int>(visitedVoxels.Length);
		Queue<int> refreshConnectivityQueue         = new Queue<int>(visitedVoxels.Length);
        Queue<int> findClustersStartingPointQueue   = new Queue<int>(visitedVoxels.Length);

        // 1. clear out duplicate indexes (optimize?)
        while(voxelsToRemove.Count > 0) {
            int voxelIndex = voxelsToRemove.Dequeue();

            if(visitedVoxels[voxelIndex]) {
                continue;
            }
            visitedVoxels[voxelIndex] = true;

            generateInteriorsQueue.Enqueue(voxelIndex);
        }

        // 2. generate affected interior blocks
        while(generateInteriorsQueue.Count > 0) {
            int voxelIndex = generateInteriorsQueue.Dequeue();
			deleteExteriorsQueue.Enqueue(voxelIndex);

            Utils.GetVoxelBlockAndVoxelIndex(voxelIndex, dimensions, out int voxelBlockIndex, out int localVoxelIndex);
            Vector3Int voxelBlockCoords = Utils.IndexToCoords(voxelBlockIndex, dimensions);

            for(int z = -2; z <= 2; ++z) {
                for(int y = -2; y <= 2; ++y) {
                    for(int x = -2; x <= 2; ++x) {
                        if(Utils.AreCoordsOnTwoEdges(x, y, z, minX: -2, minY: -2, minZ: -2, maxX: 2, maxY: 2, maxZ: 2)) {
                            continue;
                        }

                        Vector3Int neighborCoords = new Vector3Int(voxelBlockCoords.x + x, voxelBlockCoords.y + y, voxelBlockCoords.z + z);
                        int neighborIndex = Utils.CoordsToIndex(neighborCoords, dimensions);
                        if(neighborIndex < 0) {
                            continue;
                        }

                        bool isWithinOneVoxelBlock = Utils.AreCoordsWithinDimensions(x + 1, y + 1, z + 1, widthX: 3, widthY: 3, widthZ: 3);

                        if(voxelBlocks[neighborIndex].IsExterior || isWithinOneVoxelBlock) {
                            refreshConnectivityQueue.Enqueue(neighborIndex);
                        }

                        if(voxelBlocks[neighborIndex].IsExterior) {
                            continue;
                        }

                        if(!Utils.AreCoordsAlignedWithCenter(x, y, z, minX: -2, minY: -2, minZ: -2, maxX: 2, maxY: 2, maxZ: 2)) {
                            continue;
                        }

                        if(isWithinOneVoxelBlock) {
                            // TODO: any procedural interior generation goes here
                            voxelBlocks[neighborIndex] = new Bin(voxelBlockIndex, dimensions, voxels: byte.MaxValue);
                        }

                    }
                }
            }
		}

        // 3. delete requested voxels
        while(deleteExteriorsQueue.Count > 0) {
            int voxelIndex = deleteExteriorsQueue.Dequeue();

            Utils.GetVoxelBlockAndVoxelIndex(voxelIndex, dimensions, out int voxelBlockIndex, out int localVoxelIndex);
            
            if(voxelBlocks[voxelBlockIndex].IsInterior) {
                throw new System.Exception();
            }

            voxelBlocks[voxelBlockIndex] = voxelBlocks[voxelBlockIndex].SetVoxelExists(localVoxelIndex, exists: false);
        }

        // 4. refresh affected voxelblocks' connectivity
        bool[] voxelBlockRefreshedStates = new bool[voxelBlocks.Length];
        while(refreshConnectivityQueue.Count > 0) {
            int voxelBlockIndex = refreshConnectivityQueue.Dequeue();

            if(voxelBlockRefreshedStates[voxelBlockIndex]) {
                continue;
            }

            if(voxelBlocks[voxelBlockIndex].IsInterior) {
                continue;
            }

            findClustersStartingPointQueue.Enqueue(voxelBlockIndex);
            voxelBlocks[voxelBlockIndex] = Bin.RefreshConnectivity(voxelBlocks, voxelBlockIndex, dimensions);
            voxelBlockRefreshedStates[voxelBlockIndex] = true;
        }

        // 5. find clusters
        yield return VoxelClusterFloodFillHandler.FindVoxelClusters(voxelBlocks, cluster.GetOffset(), dimensions, findClustersStartingPointQueue, stepDuration, onFinished: (List<VoxelCluster> foundClusters) => {
            cluster.OnUpdateFinish(foundClusters);
        });
    }
}