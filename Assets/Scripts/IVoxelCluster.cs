using UnityEngine;
using System.Collections.Generic;

public interface IVoxelCluster {

    Vector3Int GetDimensions();
    Vector3Int GetOffset();

    void OnUpdateStart(out Bin[] voxelBlocks, out Queue<int> voxelsToRemove);
    bool IsDirty(); 
    bool IsWaitingForUpdate();
    void OnUpdateFinish(List<VoxelCluster> newClusters);
}
