using UnityEngine;
using System.Collections.Generic;

public interface IVoxelClusterUpdaterUser {

    void OnReceivedUpdateRequest();

    Bin[] OnUpdateStart();
    void OnUpdateFinish(List<VoxelCluster> newClusters);

    Vector3Int GetDimensions();
    Vector3Int GetOffset();
}
