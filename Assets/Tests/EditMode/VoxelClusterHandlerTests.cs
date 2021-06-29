using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

using Assert = NUnit.Framework.Assert;

public class VoxelClusterHandlerTests {

    [Test]
    public void TestMoveBlocksAndTranslateData() {
        Vector3Int dimensions = new Vector3Int(6, 7, 8);
        Queue<int> indexesToMove = new Queue<int>();
        Vector3Int minCoord;
        Vector3Int maxCoord;

        Vector3Int[] stationaryCoords = new Vector3Int[] {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(2, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(2, 1, 0),
            new Vector3Int(0, 2, 0),
            new Vector3Int(1, 2, 0),
            new Vector3Int(2, 2, 0),
                                   
            new Vector3Int(0, 0, 1),
            new Vector3Int(1, 0, 1),
            new Vector3Int(2, 0, 1),
            new Vector3Int(0, 1, 1),
            new Vector3Int(1, 1, 1),
            new Vector3Int(2, 1, 1),
            new Vector3Int(0, 2, 1),
            new Vector3Int(1, 2, 1),
            new Vector3Int(2, 2, 1),
                                   
            new Vector3Int(0, 0, 2),
            new Vector3Int(1, 0, 2),
            new Vector3Int(2, 0, 2),
            new Vector3Int(0, 1, 2),
            new Vector3Int(1, 1, 2),
            new Vector3Int(2, 1, 2),
            new Vector3Int(0, 2, 2),
            new Vector3Int(1, 2, 2),
            new Vector3Int(2, 2, 2)
        };

        Vector3Int[] moveCoords = new Vector3Int[] {
            new Vector3Int(3, 3, 3),
            new Vector3Int(4, 3, 3),
            new Vector3Int(5, 3, 3),
            new Vector3Int(3, 4, 3),
            new Vector3Int(4, 4, 3),
            new Vector3Int(5, 4, 3),
            new Vector3Int(3, 5, 3),
            new Vector3Int(4, 5, 3),
            new Vector3Int(5, 5, 3),

            new Vector3Int(3, 3, 4),
            new Vector3Int(4, 3, 4),
            new Vector3Int(5, 3, 4),
            new Vector3Int(3, 4, 4),
            // empty
            new Vector3Int(5, 4, 4),
            new Vector3Int(3, 5, 4),
            new Vector3Int(4, 5, 4),
            new Vector3Int(5, 5, 4),

            new Vector3Int(3, 3, 5),
            new Vector3Int(4, 3, 5),
            new Vector3Int(5, 3, 5),
            new Vector3Int(3, 4, 5),
            new Vector3Int(4, 4, 5),
            new Vector3Int(5, 4, 5),
            new Vector3Int(3, 5, 5),
            new Vector3Int(4, 5, 5),
            new Vector3Int(5, 5, 5)
        };

        Bin[] voxelBlocks = new Bin[dimensions.Product()];
        for(int i = 0; i < stationaryCoords.Length; i++) {
            Vector3Int coords = stationaryCoords[i];
            int index = Utils.CoordsToIndex(coords, dimensions);

            voxelBlocks[index] = new Bin(index, dimensions, byte.MaxValue);
        }

        for(int i = 0; i < moveCoords.Length; i++) {
            Vector3Int coords = moveCoords[i];
            int index = Utils.CoordsToIndex(coords, dimensions);

            voxelBlocks[index] = new Bin(index, dimensions, byte.MaxValue);
            indexesToMove.Enqueue(Utils.CoordsToIndex(coords, dimensions));
        }

        VoxelCluster cluster = new VoxelCluster(voxelBlocks, Vector3Int.zero, dimensions);

        minCoord = moveCoords[0];
        maxCoord = moveCoords[moveCoords.Length - 1];

        Bin[] movedVoxelBlocks = VoxelClusterFloodFillHandler.MoveBlocksAndTranslateData(voxelBlocks, cluster.Dimensions, indexesToMove, minCoord, maxCoord, out Vector3Int newBinGridDimensions);

        Assert.AreEqual(27, movedVoxelBlocks.Length);
        Assert.AreEqual(new Vector3Int(3, 3, 3), newBinGridDimensions);

        VoxelCluster newCluster = new VoxelCluster(movedVoxelBlocks, minCoord * Bin.WIDTH, newBinGridDimensions);

        Assert.AreEqual(new Vector3Int(6, 6, 6), newCluster.VoxelOffset);

        int movedCount = 0;
        for(int i1 = 0; i1 < newCluster.GetVoxelBlockCount(); i1++) {
            if(!newCluster.TryGetVoxelBlock(i1, out Bin voxelBlock)) {
                continue;
            }
            
            Vector3Int oldCoords = newCluster.VoxelOffset / Bin.WIDTH + voxelBlock.Coords;

            bool wasMoved = false;
            for(int i2 = 0; i2 < moveCoords.Length; i2++) {
                if(oldCoords == moveCoords[i2]) {
                    wasMoved = true;
                    break;
                }
            }

            // NOTE: these asserts are not generalizable, as in they're specifically for this case

            if(wasMoved) {
                ++movedCount;
                Assert.IsTrue(voxelBlock.IsWholeBinFilled());
                Assert.IsTrue(voxelBlock.IsExterior);
            }
            else {
                Assert.IsTrue(voxelBlock.IsWholeBinEmpty());
                Assert.IsFalse(voxelBlock.IsExterior);
            }
        }

        Assert.AreEqual(26, movedCount);
    }
}
