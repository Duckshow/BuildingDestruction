using UnityEngine;

public class UnitTester : MonoBehaviour
{
    // TODO: would be nice with a better way of running tests

    [EasyButtons.Button]
    public void TestVoxelCluster() { 
        VoxelCluster.RunTests();
    }

    [EasyButtons.Button]
    public void TestVoxelController() {
        VoxelController.RunTests();
    }

    [EasyButtons.Button]
    public void TestVoxelGrid() {
        VoxelGrid.RunTests();
    }

    [EasyButtons.Button]
    public void TestBin() {
        Bin.RunTests();
    }

    [EasyButtons.Button]
    public void TestVoxelBuilder() {
        VoxelBuilder.RunTests();
    }

    [EasyButtons.Button]
    public void TestVoxelMeshFactory() {
        VoxelMeshFactory.RunTests();
    }
}
