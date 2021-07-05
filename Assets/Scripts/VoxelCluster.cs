using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

[assembly: InternalsVisibleTo("PlayMode")]
public class VoxelCluster : IVoxelCluster {

	public Vector3Int VoxelOffset { get; private set; }
	public Vector3Int Dimensions { get; private set; }
	public Vector3Int VoxelDimensions { get { return Dimensions * Bin.WIDTH; } }

	private Bin[] voxelBlocks;
	private Queue<int> voxelsToBeRemoved = new Queue<int>();
	private Callback<List<VoxelCluster>> onFinishedUpdate;
	private bool isWaitingForUpdate;

	public VoxelCluster Clone() {
		VoxelCluster clone = (VoxelCluster)MemberwiseClone();
		
		clone.voxelBlocks = new Bin[voxelBlocks.Length];
        for(int i = 0; i < voxelBlocks.Length; i++) {
			clone.voxelBlocks[i] = voxelBlocks[i].Clone();
        }

		return clone;
	}

	public VoxelCluster(Vector3Int dimensions, byte voxelBlockStartValue = 0) {
		VoxelOffset = Vector3Int.zero;
		Dimensions = dimensions;

		voxelBlocks = new Bin[Dimensions.Product()];
        
		for(int i = 0; i < voxelBlocks.Length; i++) {
			Vector3Int coords = Utils.IndexToCoords(i, dimensions);
			byte voxels = voxelBlockStartValue > 0 && Utils.AreCoordsOnTheEdge(coords, dimensions) ? voxelBlockStartValue : byte.MinValue;

			voxelBlocks[i] = new Bin(i, Dimensions, voxels);
        }

		for(int i = 0; i < voxelBlocks.Length; i++) {
			Bin voxelBlock = voxelBlocks[i];

			if(voxelBlock.IsWholeBinEmpty()) {
				continue;
			}

			voxelBlocks[i] = Bin.RefreshConnectivity(voxelBlocks, i, dimensions);
		}
    }

	public VoxelCluster(Bin[] voxelBlocks, Vector3Int offset, Vector3Int dimensions) {
		this.voxelBlocks = voxelBlocks;
		VoxelOffset = offset;
		Dimensions = dimensions;
	}

	public void RemoveVoxel(Vector3Int voxelCoords) {
		RemoveVoxel(Utils.CoordsToIndex(voxelCoords, VoxelDimensions));
	}

	public void RemoveVoxel(int voxelIndex) {
		voxelsToBeRemoved.Enqueue(voxelIndex);

		if(voxelsToBeRemoved.Count == 1) {
			VoxelClusterUpdater.Instance.ScheduleUpdate(this);
		}
	}

	public void OnUpdateStart(out Bin[] voxelBlocks, out Queue<int> voxelsToRemove) {
		isWaitingForUpdate = true;
		voxelBlocks = this.voxelBlocks;
		voxelsToRemove = voxelsToBeRemoved;
	}

	public bool IsDirty() { 
		return voxelsToBeRemoved.Count > 0;
	}

	public bool IsWaitingForUpdate() {
		return isWaitingForUpdate;
	}

	public void OnUpdateFinish(List<VoxelCluster> newClusters) {
		isWaitingForUpdate = false;
		if(onFinishedUpdate != null) {
			onFinishedUpdate(newClusters);
        }
	}

	public void SubscribeToOnFinishedUpdate(Callback<List<VoxelCluster>> subscriber) {
		Debug.Assert(onFinishedUpdate == null);
		onFinishedUpdate = subscriber;
	}

	public Vector3Int GetOffset() {
		return VoxelOffset;
	}

	public Vector3Int GetDimensions() {
		return Dimensions;
	}

	public int GetVoxelBlockCount() {
		return voxelBlocks.Length;
	}

	public bool TryGetVoxelBlock(Vector3Int coords, out Bin voxelBlock) {
		int index = Utils.CoordsToIndex(coords, Dimensions);
		return TryGetVoxelBlock(index, out voxelBlock);
	}

	public bool TryGetVoxelBlock(int index, out Bin voxelBlock) {
		if(IsDirty() || isWaitingForUpdate) {
			Debug.LogError("Tried to get VoxelBlock, but is currently awaiting update!");
			voxelBlock = new Bin();
			return false;
		}

		if(index == -1) {
			voxelBlock = new Bin();
			return false;
		}

		voxelBlock = voxelBlocks[index];
		return true;
	}

    public bool GetVoxelExists(Vector3Int voxelCoords) {
        return GetVoxelExists(voxelCoords, this);
    }

    internal static bool GetVoxelExists(Vector3Int voxelCoords, VoxelCluster voxelCluster) {
        int voxelBlockIndex, localVoxelIndex;
		Utils.GetVoxelBlockAndVoxelIndex(voxelCoords, voxelCluster.Dimensions, out voxelBlockIndex, out localVoxelIndex);

		Bin voxelBlock = voxelCluster.voxelBlocks[voxelBlockIndex];
        if(voxelBlock.IsWholeBinEmpty()) {
            return false;
        }

        return voxelBlock.GetVoxelExists(localVoxelIndex);
    }

	public bool ShouldBeStatic(bool wasOriginallyStatic) {
		return wasOriginallyStatic && VoxelOffset.y == 0;
	}
}