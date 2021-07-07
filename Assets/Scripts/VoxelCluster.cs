using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

[assembly: InternalsVisibleTo("PlayMode")]
[System.Serializable]
public class VoxelCluster : IVoxelCluster {

	private IVoxelCluster.State state = IVoxelCluster.State.Idle;

	public Vector3Int VoxelOffset { get; private set; }
	public Vector3Int Dimensions { get; private set; }
	public Vector3Int VoxelDimensions { get { return Dimensions * Bin.WIDTH; } }

	private VoxelGrid owner;
	private Bin[] voxelBlocks;
	private Queue<int> voxelsToBeRemoved = new Queue<int>();

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

	public VoxelGrid GetOwner() {
		return owner;
	}

	public void SetOwner(VoxelGrid owner) {
		this.owner = owner;
	}

	public void TryRemoveVoxel(Vector3Int voxelCoords) {
		TryRemoveVoxel(Utils.CoordsToIndex(voxelCoords, VoxelDimensions));
	}

	public void TryRemoveVoxel(int voxelIndex) {
        if(state == IVoxelCluster.State.WaitingForUpdate) {
			return;
        }

        if(!GetVoxelExists(voxelIndex)) {
			return;
        }

		voxelsToBeRemoved.Enqueue(voxelIndex);

		if(voxelsToBeRemoved.Count == 1) {
			VoxelClusterUpdater.Instance.ScheduleUpdate(this);
		}
	}

	public void OnUpdateStart(out Bin[] voxelBlocks, out Queue<int> voxelsToRemove) {
		state = IVoxelCluster.State.WaitingForUpdate;

		voxelBlocks = this.voxelBlocks;
		voxelsToRemove = voxelsToBeRemoved;
	}

	public bool IsDirty() { 
		return voxelsToBeRemoved.Count > 0;
	}

	public void OnUpdateFinish(List<VoxelCluster> newClusters) {
		Debug.Assert(voxelsToBeRemoved.Count == 0);

		state = IVoxelCluster.State.Idle;

        if(owner != null) {
			Debug.Assert(ReferenceEquals(this, owner.GetVoxelCluster()));
			owner.OnClusterFinishedUpdate(newClusters);
		}
	}

	public IVoxelCluster.State GetCurrentState() {
		return state;
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
		if(state == IVoxelCluster.State.WaitingForUpdate || IsDirty()) { // not sure if IsDirty() is actually needed here - it's a bit confusing as well
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

	private bool GetVoxelExists(int voxelIndex) {
		Utils.GetVoxelBlockAndVoxelIndex(voxelIndex, Dimensions, out int voxelBlockIndex, out int localVoxelIndex);
		Bin voxelBlock = voxelBlocks[voxelBlockIndex];

        if(voxelBlock.IsExterior) {
			if(voxelBlock.IsWholeBinEmpty()) {
				return false;
			}

			return voxelBlock.GetVoxelExists(localVoxelIndex);
		}

		return true;
	}
}