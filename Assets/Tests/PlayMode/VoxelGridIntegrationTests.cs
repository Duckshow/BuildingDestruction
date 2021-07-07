using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class VoxelGridIntegrationTests
{
    [Test]
    public void TestApplyNewPivot() {

        GameObject pivotGO = new GameObject();
        GameObject meshGO = new GameObject();

        pivotGO.transform.position = Vector3.zero;
        meshGO.transform.position = Vector3.zero;
        meshGO.transform.parent = pivotGO.transform;

        Transform pivotTransform = pivotGO.transform;
        Transform meshTransform = meshGO.transform;

        {
            Vector3Int dimensions = new Vector3Int(4, 8, 5);
            Bin[] voxelBlocks = TestUtils.GetNewVoxelBlockGrid(dimensions.x, dimensions.y, dimensions.z, byte.MaxValue);
            VoxelCluster voxelCluster = new VoxelCluster(voxelBlocks, offset: new Vector3Int(0, 0, 0), dimensions);

            VoxelGrid.ApplyNewPivot(pivotTransform, meshTransform, voxelCluster, isStatic: true);

            Assert.AreEqual(new Vector3(0f, 0f, 0f), meshTransform.position);
            Assert.AreEqual(new Vector3(4f, 0f, 5f), pivotTransform.position);
        }

        {
            Vector3Int dimensions = new Vector3Int(1, 1, 5);
            Bin[] voxelBlocks = TestUtils.GetNewVoxelBlockGrid(dimensions.x, dimensions.y, dimensions.z, byte.MaxValue);

            for(int i = 0; i < voxelBlocks.Length; i++) {
                voxelBlocks[i] = new Bin(voxelBlocks[i], 0b_0001_0001);
            }

            VoxelCluster voxelCluster = new VoxelCluster(voxelBlocks, offset: new Vector3Int(0, 0, 0), dimensions);

            VoxelGrid.ApplyNewPivot(pivotTransform, meshTransform, voxelCluster, isStatic: false);

            Assert.AreEqual(new Vector3(0f, 0f, 0f), meshTransform.position);
            Assert.AreEqual(new Vector3(0.5f, 0.5f, 5f), pivotTransform.position);
        }

        {
            Vector3Int dimensions = new Vector3Int(1, 1, 5);
            Bin[] voxelBlocks = TestUtils.GetNewVoxelBlockGrid(dimensions.x, dimensions.y, dimensions.z, byte.MaxValue);

            for(int i = 0; i < voxelBlocks.Length; i++) {
                voxelBlocks[i] = new Bin(voxelBlocks[i], 0b_1000_1000);
            }

            VoxelCluster voxelCluster = new VoxelCluster(voxelBlocks, offset: new Vector3Int(7, 15, 0), dimensions);

            VoxelGrid.ApplyNewPivot(pivotTransform, meshTransform, voxelCluster, isStatic: false);

            Assert.AreEqual(new Vector3(6f, 14f, 0f), meshTransform.position);
            Assert.AreEqual(new Vector3(7.5f, 15.5f, 5f), pivotTransform.position);
        }
    }
}
