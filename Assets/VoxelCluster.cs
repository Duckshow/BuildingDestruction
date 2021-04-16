using UnityEngine;
using System.Collections.Generic;

public class VoxelCluster {
	public Voxel[] Voxels { get; private set; }
	public Vector3Int Offset { get; private set; }
	public Vector3Int Dimensions { get; private set; }

	public VoxelCluster(int length) {
		Voxels = new Voxel[length]; // only for unit-testing purposes!
	}

	public VoxelCluster(Voxel startVoxel, Voxel[] grid, Vector3Int gridDimensions, bool[] visitedVoxels) {
		Vector3Int newDimensions;
		Vector3Int offset;
		bool isTouchingGround;
		Voxels = FindVoxels(startVoxel, grid, gridDimensions, visitedVoxels, out newDimensions, out offset, out isTouchingGround);

		Offset = offset;
		Dimensions = newDimensions;
	}

	private static Voxel[] FindVoxels(Voxel startVoxel, Voxel[] grid, Vector3Int dimensions, bool[] visitedVoxels, out Vector3Int newDimensions, out Vector3Int offset, out bool isTouchingGround) {
		Vector3Int minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
		Vector3Int maxCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

		Queue<int> q1 = new Queue<int>();
		Queue<int> q2 = new Queue<int>();

		q1.Enqueue(startVoxel.Index);

		while(q1.Count > 0) {
			EvaluateNextVoxel(grid, q1, q2, visitedVoxels, dimensions, ref minCoord, ref maxCoord);
		}

		newDimensions = maxCoord - minCoord + Vector3Int.one;
		Voxel[] newVoxels = new Voxel[newDimensions.x * newDimensions.y * newDimensions.z];

		while(q2.Count > 0) {
			int oldVoxel = q2.Dequeue();
			Voxel newVoxel = AdjustVoxelIndex(grid[oldVoxel], minCoord, dimensions, newDimensions);
			newVoxels[newVoxel.Index] = newVoxel;
		}

		offset = minCoord;
		isTouchingGround = minCoord.y == 0;

		return newVoxels;
	}

	private static void EvaluateNextVoxel(Voxel[] grid, Queue<int> q1, Queue<int> q2, bool[] visitedVoxels, Vector3Int dimensions, ref Vector3Int minCoord, ref Vector3Int maxCoord) {
		int index = q1.Dequeue();
		if(visitedVoxels[index]) {
			return;
		}

		visitedVoxels[index] = true;

		Voxel v = Voxel.GetUpdatedHasNeighborValues(index, grid, dimensions);
		grid[index] = v;

		if(!v.IsFilled) {
			return;
		}

		q2.Enqueue(index);

		Vector3Int coords = VoxelGrid.IndexToCoords(index, dimensions);
		minCoord.x = Mathf.Min(minCoord.x, coords.x);
		minCoord.y = Mathf.Min(minCoord.y, coords.y);
		minCoord.z = Mathf.Min(minCoord.z, coords.z);
		maxCoord.x = Mathf.Max(maxCoord.x, coords.x);
		maxCoord.y = Mathf.Max(maxCoord.y, coords.y);
		maxCoord.z = Mathf.Max(maxCoord.z, coords.z);

		if(v.HasNeighborRight)	q1.Enqueue(VoxelGrid.CoordsToIndex(coords + Vector3Int.right, dimensions));
		if(v.HasNeighborLeft)	q1.Enqueue(VoxelGrid.CoordsToIndex(coords + Vector3Int.left, dimensions));
		if(v.HasNeighborUp)		q1.Enqueue(VoxelGrid.CoordsToIndex(coords + Vector3Int.up, dimensions));
		if(v.HasNeighborDown)	q1.Enqueue(VoxelGrid.CoordsToIndex(coords + Vector3Int.down, dimensions));
		if(v.HasNeighborFore)	q1.Enqueue(VoxelGrid.CoordsToIndex(coords + Vector3Int.forward, dimensions));
		if(v.HasNeighborBack)	q1.Enqueue(VoxelGrid.CoordsToIndex(coords + Vector3Int.back, dimensions));
	}

	private static Voxel AdjustVoxelIndex(Voxel v, Vector3Int minCoord, Vector3Int oldDimensions, Vector3Int newDimensions) {
		Vector3Int oldCoords = VoxelGrid.IndexToCoords(v.Index, oldDimensions);
		Vector3Int newCoords = oldCoords - minCoord;
		int newIndex = VoxelGrid.CoordsToIndex(newCoords, newDimensions);

		return new Voxel(newIndex, v);
	}

	public static void RunTests() {
		TestEvaluateNextVoxel();
		TestAdjustVoxelIndex();
	}

	private static void TestEvaluateNextVoxel() {
		Vector3Int dimensions = new Vector3Int(8, 8, 8);
		Vector3Int clusterDimensions = new Vector3Int(4, 5, 6);
		Voxel[] grid = new Voxel[dimensions.x * dimensions.y * dimensions.z];

		Queue<int> q1 = new Queue<int>();
		Queue<int> q2 = new Queue<int>();
		bool[] visitedVoxels = new bool[dimensions.x * dimensions.y * dimensions.z];
		Vector3Int minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
		Vector3Int maxCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

		q1.Enqueue(0);

		while(q1.Count > 0) {
			int index = q1.Peek(); // dequeued inside EvaluateNextVoxel
			Vector3Int coords = VoxelGrid.IndexToCoords(index, dimensions);

			grid[index] = new Voxel(
						index,
						isFilled: true,
						hasNeighborRight: coords.x < clusterDimensions.x - 1,
						hasNeighborLeft: coords.x > 0,
						hasNeighborUp: coords.y < clusterDimensions.y - 1,
						hasNeighborDown: coords.y > 0,
						hasNeighborFore: coords.z < clusterDimensions.z - 1,
						hasNeighborBack: coords.z > 0);

			EvaluateNextVoxel(grid, q1, q2, visitedVoxels, dimensions, ref minCoord, ref maxCoord);
			Debug.Assert(visitedVoxels[index]);
		}

		Debug.LogFormat("EvaluateNextVoxel Result = Grid Dimensions: {0}, Cluster Dimensions: {1}, MinCoord: {2}, MaxCoord: {3}", dimensions, clusterDimensions, minCoord, maxCoord);
	}

	private static void TestAdjustVoxelIndex() {
		Vector3Int oldDimensions = new Vector3Int(8, 8, 8);
		Vector3Int newDimensions = new Vector3Int(4, 4, 4);
		Vector3Int minCoords = new Vector3Int(4, 3, 2);

		int lastIndexFound = -1;

		for(int z = 0; z < newDimensions.z; z++) {
			for(int y = 0; y < newDimensions.y; y++) {
				for(int x = 0; x < newDimensions.x; x++) {
					int oldIndex = VoxelGrid.CoordsToIndex(minCoords + new Vector3Int(x, y, z), oldDimensions);
					Voxel v = AdjustVoxelIndex(new Voxel(oldIndex), minCoords, oldDimensions, newDimensions);

					Debug.Assert(v.Index - lastIndexFound == 1);
					lastIndexFound = v.Index;
				}
			}
		}
	}
}