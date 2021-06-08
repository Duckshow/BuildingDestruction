using UnityEngine;

public class VoxelCluster { // TODO: could probably be a struct

	public Octree<bool> VoxelMap { get; private set; }
	public Vector3Int VoxelOffset { get; private set; }
	public Vector3Int Dimensions { get; private set; }

	public VoxelCluster(int size) { // only for unit-testing purposes!
		VoxelMap = new Octree<bool>(size);
		Dimensions = new Vector3Int(size / 3, size / 3, size / 3);
	}

	public VoxelCluster(Octree<bool> voxelMap, Vector3Int voxelOffset, Vector3Int dimensions) {
		VoxelMap = voxelMap;
		VoxelOffset = voxelOffset;
		Dimensions = dimensions;
	}

	public bool ShouldBeStatic(bool wasOriginallyStatic) {
		return wasOriginallyStatic && VoxelOffset.y == 0;
	}
}