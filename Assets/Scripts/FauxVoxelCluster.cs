using UnityEngine;
using System;
using System.Collections.Generic;

public class FauxVoxelCluster : IVoxelCluster {

    Vector3Int offset, dimensions;
    Callback<Bin[], Queue<int>> onUpdateStart;
    Callback<List<VoxelCluster>> onUpdateFinish;

    public FauxVoxelCluster(Vector3Int offset, Vector3Int dimensions, Callback<Bin[], Queue<int>> onUpdateStart, Callback<List<VoxelCluster>> onUpdateFinish) {
        this.offset = offset;
        this.dimensions = dimensions;
        this.onUpdateStart = onUpdateStart;
        this.onUpdateFinish = onUpdateFinish;
    }

    public Vector3Int GetDimensions() {
        return dimensions;
    }

    public Vector3Int GetOffset() {
        return offset;
    }

    public void OnUpdateStart(out Bin[] voxelBlocks, out Queue<int> voxelsToRemove) {
        onUpdateStart(out voxelBlocks, out voxelsToRemove);
    }

    public void OnUpdateFinish(List<VoxelCluster> newClusters) {
        onUpdateFinish(newClusters);
    }

    public bool IsDirty() {
        return true;
    }

    public IVoxelCluster.State GetCurrentState() {
        return IVoxelCluster.State.Idle;
    }
}