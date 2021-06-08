using UnityEngine;

public class VoxelCluster { // TODO: could probably be a struct

	public Bin[] Bins { get; private set; }
	public Octree<bool> VoxelMap { get; private set; }
	public Vector3Int VoxelOffset { get; private set; }
	public Vector3Int Dimensions { get; private set; }

	public VoxelCluster(int length) {
		Bins = new Bin[length]; // only for unit-testing purposes!
	}

	public VoxelCluster(Bin[] bins, Octree<bool> voxelMap, Vector3Int voxelOffset, Vector3Int dimensions) {
		Bins = bins;
		VoxelMap = voxelMap;
		VoxelOffset = voxelOffset;
		Dimensions = dimensions;
	}

	public bool ShouldBeStatic(bool wasOriginallyStatic) {
		return wasOriginallyStatic && VoxelOffset.y == 0;
	}
}