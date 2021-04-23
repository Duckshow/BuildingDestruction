using UnityEngine;
using System.Collections.Generic;

public class VoxelCluster {

	public Voxel[] Voxels { get; private set; }
	public Vector3Int Offset { get; private set; }
	public Vector3Int Dimensions { get; private set; }

	public VoxelCluster(int length) {
		Voxels = new Voxel[length]; // only for unit-testing purposes!
	}

	public VoxelCluster(int startBinIndex, Bin[] bins, Voxel[] voxels, Vector3Int binGridDimensions, Vector3Int voxelGridDimensions, bool[] visitedBins) {
		Vector3Int newDimensions;
		Vector3Int minCoord, maxCoord;
		Queue<int> foundBins = FindCluster(startBinIndex, bins, binGridDimensions, visitedBins, out minCoord, out maxCoord);
		
		Voxels = MoveVoxelsToNewGrid(voxels, voxelGridDimensions, foundBins, minCoord, maxCoord, out newDimensions);
		Offset = minCoord;
		Dimensions = newDimensions;

		//bool isTouchingGround = minCoord.y == 0;
	}

	private static Queue<int> FindCluster(int startBinIndex, Bin[] bins, Vector3Int binGridDimensions, bool[] visitedBins, out Vector3Int minCoord, out Vector3Int maxCoord) {
		minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
		maxCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

		Queue<int> binQueue = new Queue<int>();
		Queue<int> foundBinQueue = new Queue<int>();
		
		binQueue.Enqueue(startBinIndex);

		while(binQueue.Count > 0) {
			EvaluateNextBin(bins, binGridDimensions, binQueue, foundBinQueue, visitedBins, ref minCoord, ref maxCoord);
		}

		return foundBinQueue;
	}

	private static void EvaluateNextBin(Bin[] bins, Vector3Int binGridDimensions, Queue<int> binQueue, Queue<int> foundBins, bool[] visitedBins, ref Vector3Int minCoord, ref Vector3Int maxCoord) {
		int index = binQueue.Dequeue();
		Bin bin = bins[index];
		
		if(!bin.HasVoxelRight && !bin.HasVoxelLeft && !bin.HasVoxelUp && !bin.HasVoxelDown && !bin.HasVoxelFore && !bin.HasVoxelBack) {
			return;
		}

        if(visitedBins[index]) {
			return;
		}
		visitedBins[index] = true;

		Vector3Int minVoxelCoords = Bin.GetMinVoxelCoords(bin.Coords);
		Vector3Int maxVoxelCoords = Bin.GetMaxVoxelCoords(bin.Coords);
		minCoord.x = Mathf.Min(minCoord.x, minVoxelCoords.x);
		minCoord.y = Mathf.Min(minCoord.y, minVoxelCoords.y);
		minCoord.z = Mathf.Min(minCoord.z, minVoxelCoords.z);
		maxCoord.x = Mathf.Max(maxCoord.x, maxVoxelCoords.x);
		maxCoord.y = Mathf.Max(maxCoord.y, maxVoxelCoords.y);
		maxCoord.z = Mathf.Max(maxCoord.z, maxVoxelCoords.z);

		foundBins.Enqueue(index);

		if(bin.HasConnectionRight)	binQueue.Enqueue(VoxelGrid.CoordsToIndex(bin.Coords.x + 1, bin.Coords.y, bin.Coords.z, binGridDimensions));
		if(bin.HasConnectionLeft)	binQueue.Enqueue(VoxelGrid.CoordsToIndex(bin.Coords.x - 1, bin.Coords.y, bin.Coords.z, binGridDimensions));
		if(bin.HasConnectionUp)		binQueue.Enqueue(VoxelGrid.CoordsToIndex(bin.Coords.x, bin.Coords.y + 1, bin.Coords.z, binGridDimensions));
		if(bin.HasConnectionDown)	binQueue.Enqueue(VoxelGrid.CoordsToIndex(bin.Coords.x, bin.Coords.y - 1, bin.Coords.z, binGridDimensions));
		if(bin.HasConnectionFore)	binQueue.Enqueue(VoxelGrid.CoordsToIndex(bin.Coords.x, bin.Coords.y, bin.Coords.z + 1, binGridDimensions));
		if(bin.HasConnectionBack)	binQueue.Enqueue(VoxelGrid.CoordsToIndex(bin.Coords.x, bin.Coords.y, bin.Coords.z - 1, binGridDimensions));
	}

	private static T[] MoveToNewGrid<T>(T[] oldGrid, Vector3Int oldDimensions, Queue<int> indexesToMove, Vector3Int minCoord, Vector3Int maxCoord, out Vector3Int newDimensions) {
		newDimensions = maxCoord - minCoord + Vector3Int.one;
		T[] newObjs = new T[newDimensions.x * newDimensions.y * newDimensions.z];

		while(indexesToMove.Count > 0) {
			int moveIndex = indexesToMove.Dequeue();

			int newIndex;
			Vector3Int newCoords;
			GetIndexAndCoordsInNewGrid(VoxelGrid.IndexToCoords(moveIndex, oldDimensions), newGridOffset: minCoord, newDimensions, out newIndex, out newCoords);

			newObjs[newIndex] = newObject;
		}

		return newObjs;
	}

	private static void GetIndexAndCoordsInNewGrid(Vector3Int coords, Vector3Int newGridOffset, Vector3Int newDimensions, out int newIndex, out Vector3Int newCoords) {
        newCoords = coords - newGridOffset;
        newIndex = VoxelGrid.CoordsToIndex(newCoords, newDimensions);
    }

	public static void RunTests() {
		TestAdjustVoxelIndex();
		Debug.Log("Run tests.");
	}

	private static void TestAdjustVoxelIndex() {
		Vector3Int oldDimensions = new Vector3Int(8, 8, 8);
		Vector3Int newDimensions = new Vector3Int(4, 4, 4);
		Vector3Int minCoords = new Vector3Int(4, 3, 2);

		int lastIndexFound = -1;

		for(int z = 0; z < newDimensions.z; z++) {
			for(int y = 0; y < newDimensions.y; y++) {
				for(int x = 0; x < newDimensions.x; x++) {
					Vector3Int oldCoords = minCoords + new Vector3Int(x, y, z);
					int oldIndex = VoxelGrid.CoordsToIndex(oldCoords, oldDimensions);
					Voxel v = AdjustVoxelIndex(new Voxel(oldIndex, oldCoords), minCoords, oldDimensions, newDimensions);

					Debug.Assert(v.Index - lastIndexFound == 1);
					lastIndexFound = v.Index;
				}
			}
		}
	}
}