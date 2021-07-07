using UnityEngine;
using System.Collections.Generic;

public interface IVoxelCluster {
    public enum State { Idle, WaitingForUpdate }

    Vector3Int GetDimensions();
    Vector3Int GetOffset();

    void OnUpdateStart(out Bin[] voxelBlocks, out Queue<int> voxelsToRemove);
    bool IsDirty(); 
    State GetCurrentState();
    void OnUpdateFinish(List<VoxelCluster> newClusters);
}
