using UnityEngine;
using System.Collections.Generic;

public class VoxelCluster {

	public Bin[] Bins { get; private set; }
	public Vector3Int VoxelOffset { get; private set; }
	public Vector3Int Dimensions { get; private set; }

	public VoxelCluster(int length) {
		Bins = new Bin[length]; // only for unit-testing purposes!
	}

	public VoxelCluster(Bin[] bins, Vector3Int voxelOffset, Vector3Int dimensions) {
		Bins = bins;
		VoxelOffset = voxelOffset;
		Dimensions = dimensions;
	}

	public bool ShouldBeStatic(bool wasOriginallyStatic) {
		return wasOriginallyStatic && VoxelOffset.y == 0;
	}
}