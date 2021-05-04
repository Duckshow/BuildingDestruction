using UnityEngine;
using System.Collections.Generic;

public class VoxelCluster {

	private struct BinMove {
		public int BinIndex;
		public Direction MoveDirection;

		public BinMove(int binIndex, Direction moveDirection) {
			BinIndex = binIndex;
			MoveDirection = moveDirection;
		}
	}

	public Bin[] Bins { get; private set; }
	public Vector3Int Offset { get; private set; }
	public Vector3Int Dimensions { get; private set; }

	public VoxelCluster(int length) {
		Bins = new Bin[length]; // only for unit-testing purposes!
	}

	public VoxelCluster(int startBinIndex, Bin[] bins, Vector3Int binGridDimensions, bool[] visitedBins) {
		Vector3Int minCoord, maxCoord;
		Queue<int> foundBins = FindCluster(startBinIndex, bins, binGridDimensions, visitedBins, out minCoord, out maxCoord);

		Vector3Int newDimensions;
		Bins = MoveBinsToNewGrid(bins, binGridDimensions, foundBins, minCoord, maxCoord, out newDimensions);
		
		Offset = minCoord * Bin.WIDTH;
		Dimensions = newDimensions;
	}

	private static Queue<int> FindCluster(int startBinIndex, Bin[] bins, Vector3Int binGridDimensions, bool[] visitedBins, out Vector3Int minCoord, out Vector3Int maxCoord) {
		minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
		maxCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

		Queue<BinMove> binQueue = new Queue<BinMove>();
		Queue<int> foundBinQueue = new Queue<int>();
		
		binQueue.Enqueue(new BinMove(startBinIndex, Direction.None));

		while(binQueue.Count > 0) {
			EvaluateNextBin(bins, binGridDimensions, binQueue, foundBinQueue, visitedBins, ref minCoord, ref maxCoord);
		}

		return foundBinQueue;
	}

	private static void EvaluateNextBin(Bin[] bins, Vector3Int binGridDimensions, Queue<BinMove> binQueue, Queue<int> foundBinQueue, bool[] visitedBins, ref Vector3Int minCoord, ref Vector3Int maxCoord) {
		BinMove binMove = binQueue.Dequeue();

		int binIndex = binMove.BinIndex;

        if(binIndex == -1) {
			return;
        }

		if(visitedBins[binIndex]) {
			return;
		}

		Bin bin = bins[binIndex];
        if(bin == null) {
			return;
        } 

		if(bin.IsWholeBinEmpty()) {
			return;
		}

		Direction movedFromDirection = VoxelGrid.GetOppositeDirection(binMove.MoveDirection);
        if(movedFromDirection != Direction.None && !bin.HasFilledVoxelOnFace(movedFromDirection)) {
			return;
        }

		visitedBins[binIndex] = true;
		foundBinQueue.Enqueue(binIndex);

		Vector3Int binCoords = VoxelGrid.IndexToCoords(binIndex, binGridDimensions);
		minCoord.x = Mathf.Min(minCoord.x, binCoords.x);
		minCoord.y = Mathf.Min(minCoord.y, binCoords.y);
		minCoord.z = Mathf.Min(minCoord.z, binCoords.z);
		maxCoord.x = Mathf.Max(maxCoord.x, binCoords.x);
		maxCoord.y = Mathf.Max(maxCoord.y, binCoords.y);
		maxCoord.z = Mathf.Max(maxCoord.z, binCoords.z);

        if(bin.IsWholeBinFilled() || bin.HasFilledVoxelOnFace(Direction.Right))	{ binQueue.Enqueue(new BinMove(VoxelGrid.CoordsToIndex(new Vector3Int(binCoords.x + 1, binCoords.y, binCoords.z), binGridDimensions), Direction.Right)); }
		if(bin.IsWholeBinFilled() || bin.HasFilledVoxelOnFace(Direction.Left))	{ binQueue.Enqueue(new BinMove(VoxelGrid.CoordsToIndex(new Vector3Int(binCoords.x - 1, binCoords.y, binCoords.z), binGridDimensions), Direction.Left)); }
		if(bin.IsWholeBinFilled() || bin.HasFilledVoxelOnFace(Direction.Up))	{ binQueue.Enqueue(new BinMove(VoxelGrid.CoordsToIndex(new Vector3Int(binCoords.x, binCoords.y + 1, binCoords.z), binGridDimensions), Direction.Up)); }
		if(bin.IsWholeBinFilled() || bin.HasFilledVoxelOnFace(Direction.Down))	{ binQueue.Enqueue(new BinMove(VoxelGrid.CoordsToIndex(new Vector3Int(binCoords.x, binCoords.y - 1, binCoords.z), binGridDimensions), Direction.Down)); }
		if(bin.IsWholeBinFilled() || bin.HasFilledVoxelOnFace(Direction.Fore))	{ binQueue.Enqueue(new BinMove(VoxelGrid.CoordsToIndex(new Vector3Int(binCoords.x, binCoords.y, binCoords.z + 1), binGridDimensions), Direction.Fore)); }
		if(bin.IsWholeBinFilled() || bin.HasFilledVoxelOnFace(Direction.Back))	{ binQueue.Enqueue(new BinMove(VoxelGrid.CoordsToIndex(new Vector3Int(binCoords.x, binCoords.y, binCoords.z - 1), binGridDimensions), Direction.Back)); }
	}

	private static Bin[] MoveBinsToNewGrid(Bin[] oldBins, Vector3Int oldBinGridDimensions, Queue<int> indexesToMove, Vector3Int minCoord, Vector3Int maxCoord, out Vector3Int newBinGridDimensions) {
		newBinGridDimensions = maxCoord - minCoord + Vector3Int.one;
		Bin[] newBins = new Bin[newBinGridDimensions.x * newBinGridDimensions.y * newBinGridDimensions.z];

		while(indexesToMove.Count > 0) {
			int oldBinIndex = indexesToMove.Dequeue();
			int newBinIndex = GetIndexInNewGrid(oldBinIndex, minCoord, oldBinGridDimensions, newBinGridDimensions);
			
			newBins[newBinIndex] = new Bin(newBinIndex, newBinGridDimensions, oldBins[oldBinIndex]);
		}

		return newBins;
	}

	private static int GetIndexInNewGrid(int oldIndex, Vector3Int newGridStartCoord, Vector3Int oldGridDimensions, Vector3Int newGridDimensions) {
		Vector3Int oldCoords = VoxelGrid.IndexToCoords(oldIndex, oldGridDimensions);
		return VoxelGrid.CoordsToIndex(oldCoords - newGridStartCoord, newGridDimensions);
	}
}