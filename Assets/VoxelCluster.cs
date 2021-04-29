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

		Bin bin = bins[binIndex];
        if(bin == null) {
			return;
        } 

		if(bin.IsWholeBinEmpty) {
			return;
		}

		Direction movedFromDirection = VoxelGrid.GetOppositeDirection(binMove.MoveDirection);
        if(movedFromDirection != Direction.None && !bin.HasFilledVoxelOnFace(movedFromDirection)) {
			return;
        }

		if(visitedBins[binIndex]) {
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

		NeighborRelationships hasVoxels = new NeighborRelationships();
        if(bin.IsWholeBinFilled) {
			hasVoxels = new NeighborRelationships(right: true, left: true, up: true, down: true, fore: true, back: true);
        }
        else {
            if(bin.HasFilledVoxelOnFace(Direction.Right)) {
				hasVoxels = NeighborRelationships.GetChanged(hasVoxels, Direction.Right, true);
			}
			if(bin.HasFilledVoxelOnFace(Direction.Left)) {
				hasVoxels = NeighborRelationships.GetChanged(hasVoxels, Direction.Left, true);
			}
			if(bin.HasFilledVoxelOnFace(Direction.Up)) {
				hasVoxels = NeighborRelationships.GetChanged(hasVoxels, Direction.Up, true);
			}
			if(bin.HasFilledVoxelOnFace(Direction.Down)) {
				hasVoxels = NeighborRelationships.GetChanged(hasVoxels, Direction.Down, true);
			}
			if(bin.HasFilledVoxelOnFace(Direction.Fore)) {
				hasVoxels = NeighborRelationships.GetChanged(hasVoxels, Direction.Fore, true);
			}
			if(bin.HasFilledVoxelOnFace(Direction.Back)) {
				hasVoxels = NeighborRelationships.GetChanged(hasVoxels, Direction.Back, true);
			}
		}

        if(hasVoxels.Right) { binQueue.Enqueue(new BinMove(VoxelGrid.CoordsToIndex(new Vector3Int(binCoords.x + 1, binCoords.y, binCoords.z), binGridDimensions), Direction.Right)); }
		if(hasVoxels.Left)	{ binQueue.Enqueue(new BinMove(VoxelGrid.CoordsToIndex(new Vector3Int(binCoords.x - 1, binCoords.y, binCoords.z), binGridDimensions), Direction.Left)); }
		if(hasVoxels.Up)	{ binQueue.Enqueue(new BinMove(VoxelGrid.CoordsToIndex(new Vector3Int(binCoords.x, binCoords.y + 1, binCoords.z), binGridDimensions), Direction.Up)); }
		if(hasVoxels.Down)	{ binQueue.Enqueue(new BinMove(VoxelGrid.CoordsToIndex(new Vector3Int(binCoords.x, binCoords.y - 1, binCoords.z), binGridDimensions), Direction.Down)); }
		if(hasVoxels.Fore)	{ binQueue.Enqueue(new BinMove(VoxelGrid.CoordsToIndex(new Vector3Int(binCoords.x, binCoords.y, binCoords.z + 1), binGridDimensions), Direction.Fore)); }
		if(hasVoxels.Back)	{ binQueue.Enqueue(new BinMove(VoxelGrid.CoordsToIndex(new Vector3Int(binCoords.x, binCoords.y, binCoords.z - 1), binGridDimensions), Direction.Back)); }
	}

	private static Bin[] MoveBinsToNewGrid(Bin[] oldBins, Vector3Int oldBinGridDimensions, Queue<int> indexesToMove, Vector3Int minCoord, Vector3Int maxCoord, out Vector3Int newBinGridDimensions) {
		newBinGridDimensions = maxCoord - minCoord + Vector3Int.one;
		Bin[] newBins = new Bin[newBinGridDimensions.x * newBinGridDimensions.y * newBinGridDimensions.z];

		while(indexesToMove.Count > 0) {
			int moveIndex = indexesToMove.Dequeue();

			int newIndex;
			Vector3Int newCoords;
			GetIndexAndCoordsInNewGrid(VoxelGrid.IndexToCoords(moveIndex, oldBinGridDimensions), newGridOffset: minCoord, newBinGridDimensions, out newIndex, out newCoords);

			newBins[newIndex] = new Bin(newIndex, newBinGridDimensions, oldBins[moveIndex]);
		}

		return newBins;
	}

	private static void GetIndexAndCoordsInNewGrid(Vector3Int coords, Vector3Int newGridOffset, Vector3Int newDimensions, out int newIndex, out Vector3Int newCoords) {
        newCoords = coords - newGridOffset;
		
		if(!VoxelGrid.AreCoordsWithinDimensions(newCoords, newDimensions)) {
			newCoords = -Vector3Int.one;
			newIndex = -1;
			return;
        }

        newIndex = VoxelGrid.CoordsToIndex(newCoords, newDimensions);
    }

	public static void RunTests() {
		TestGetIndexAndCoordsInNewGrid();
	}

	private static void TestGetIndexAndCoordsInNewGrid() {
		Debug.Log("== Testing GetIndexAndCoordsInNewGrid() ==");

		for(int i = 0; i < 100; i++) {
			Vector3Int oldDimensions = new Vector3Int(Random.Range(2, 10), Random.Range(2, 10), Random.Range(2, 10));
			Vector3Int newDimensions = new Vector3Int(Random.Range(1, oldDimensions.x), Random.Range(1, oldDimensions.y), Random.Range(1, oldDimensions.z));
			Vector3Int minCoords = new Vector3Int(Random.Range(0, oldDimensions.x - newDimensions.x), Random.Range(0, oldDimensions.y - newDimensions.y), Random.Range(0, oldDimensions.z - newDimensions.z));

			int lastIndexFound = -1;

			for(int z = 0; z < oldDimensions.z; z++) {
				for(int y = 0; y < oldDimensions.y; y++) {
					for(int x = 0; x < oldDimensions.x; x++) {
						Vector3Int coords = new Vector3Int(x, y, z);

						int newIndex;
						Vector3Int newCoords;
						GetIndexAndCoordsInNewGrid(coords, minCoords, newDimensions, out newIndex, out newCoords);

						if(VoxelGrid.AreCoordsWithinDimensions(coords - minCoords, newDimensions)) {
							Debug.AssertFormat(VoxelGrid.IndexToCoords(newIndex, newDimensions) == newCoords, "Fail: VoxelGrid.IndexToCoords({0}, {1}) ({2}) == {3}", newIndex, newDimensions, VoxelGrid.IndexToCoords(newIndex, newDimensions), newCoords);
							Debug.AssertFormat(VoxelGrid.CoordsToIndex(newCoords, newDimensions) == newIndex, "Fail: VoxelGrid.CoordsToIndex({0}, {1}) ({2}) == {3}", newCoords, newDimensions, VoxelGrid.CoordsToIndex(newCoords, newDimensions), newIndex);
							Debug.AssertFormat(newCoords == coords - minCoords, "Fail: {0} == {1} - {2} ({3})", newCoords, coords, minCoords, coords - minCoords);
							Debug.AssertFormat(newIndex == lastIndexFound + 1, "Fail: {0} == {1} + 1", newIndex, lastIndexFound);
						
							lastIndexFound = newIndex;
						}
						else {
							Debug.AssertFormat(newIndex == -1, "Fail: {0} == -1", newIndex);
							Debug.AssertFormat(newCoords == -Vector3Int.one, "Fail: {0} == -Vector3Int.one", newCoords);
						}
					}
				}
			}
		}

		Debug.Log("== Done ==");
	}
}