using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PlayMode")]
public static class VoxelClusterUpdater {

    private static Dictionary<IVoxelClusterUpdaterUser, Queue<int>> voxelsToRemove = new Dictionary<IVoxelClusterUpdaterUser, Queue<int>>();

    // NOTE: repeated connecting to StaticUpdateHandler is because we can't ensure that the gameobject hasn't been destroyed,
    // which would sever the connection - this way the singleton-call will re-instantiate it should that be the case
    private static bool isConnectedToUpdateHandler;

    public static void ScheduleRemoveVoxel(IVoxelClusterUpdaterUser user, int voxelIndex) {
        TryConnectToStaticUpdateHandler();

        user.OnReceivedUpdateRequest();

        Queue<int> updateQueue;
        if(voxelsToRemove.TryGetValue(user, out updateQueue)) {
            updateQueue.Enqueue(voxelIndex);
            voxelsToRemove[user] = updateQueue;
            return;
        }

        updateQueue = new Queue<int>();
        updateQueue.Enqueue(voxelIndex);
        voxelsToRemove.Add(user, updateQueue);
    }

    private static void LateUpdate() {
        foreach(var pair in voxelsToRemove) {
            StaticUpdateHandler.Instance.StartCoroutine(RemoveVoxelsInCluster(pair.Key, pair.Value, stepDuration: 0f));
        }

        TryDisconnectFromStaticUpdateHandler();
    }

    private static void TryConnectToStaticUpdateHandler() {
        if(isConnectedToUpdateHandler) {
            return;
        }

        StaticUpdateHandler.Instance.Subscribe_LateUpdate(LateUpdate);
        isConnectedToUpdateHandler = true;
    }

    private static void TryDisconnectFromStaticUpdateHandler() {
        if(!isConnectedToUpdateHandler) {
            return;
        }

        StaticUpdateHandler.Instance.Unsubscribe_LateUpdate(LateUpdate);
        isConnectedToUpdateHandler = false;
    }

    internal static IEnumerator RemoveVoxelsInCluster(IVoxelClusterUpdaterUser user, Queue<int> voxelsToRemove, float stepDuration) {
        Bin[] voxelBlocks = user.OnUpdateStart();
        Vector3Int dimensions = user.GetDimensions();

        Queue<int> generateInteriorsQueue = voxelsToRemove;
		Queue<int> deleteExteriorsQueue = new Queue<int>(voxelsToRemove.Count);
		Queue<int> refreshConnectivityQueue = new Queue<int>(voxelsToRemove.Count * 6 * 5); // 6 * 5 is a guesstimate of how many neighbors are affected when we get neighbors as well as their neighbors
        Queue<int> findClustersStartingPointQueue = new Queue<int>(voxelsToRemove.Count);

        // 1. generate affected interior blocks
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

        // 2. delete requested voxels
        while(deleteExteriorsQueue.Count > 0) {
            int voxelIndex = deleteExteriorsQueue.Dequeue();

            Utils.GetVoxelBlockAndVoxelIndex(voxelIndex, dimensions, out int voxelBlockIndex, out int localVoxelIndex);
            
            if(voxelBlocks[voxelBlockIndex].IsInterior) {
                throw new System.Exception();
            }

            voxelBlocks[voxelBlockIndex] = voxelBlocks[voxelBlockIndex].SetVoxelExists(localVoxelIndex, exists: false);
        }

        // 3. refresh affected voxelblocks' connectivity
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

        // 4. find clusters
        yield return VoxelClusterFloodFillHandler.FindVoxelClusters(voxelBlocks, user.GetOffset(), dimensions, findClustersStartingPointQueue, stepDuration, onFinished: (List<VoxelCluster> foundClusters) => { 
            user.OnUpdateFinish(foundClusters); 
        });
    }
}