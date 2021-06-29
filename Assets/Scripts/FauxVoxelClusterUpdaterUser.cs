using UnityEngine;
using System;
using System.Collections.Generic;

public class FauxVoxelClusterUpdaterUser : IVoxelClusterUpdaterUser {

    Vector3Int offset, dimensions;
    Callback onReceivedUpdateRequest;
    Func<Bin[]> onUpdateStart;
    Callback<List<VoxelCluster>> onUpdateFinish;

    public FauxVoxelClusterUpdaterUser(Vector3Int offset, Vector3Int dimensions, Callback onReceivedUpdateRequest, Func<Bin[]> onUpdateStart, Callback<List<VoxelCluster>> onUpdateFinish) {
        this.offset = offset;
        this.dimensions = dimensions;
        this.onReceivedUpdateRequest = onReceivedUpdateRequest;
        this.onUpdateStart = onUpdateStart;
        this.onUpdateFinish = onUpdateFinish;
    }

    public Vector3Int GetDimensions() {
        return dimensions;
    }

    public Vector3Int GetOffset() {
        return offset;
    }

    public void OnReceivedUpdateRequest() {
        onReceivedUpdateRequest();
    }

    public Bin[] OnUpdateStart() {
        return onUpdateStart();
    }

    public void OnUpdateFinish(List<VoxelCluster> newClusters) {
        onUpdateFinish(newClusters);
    }
}