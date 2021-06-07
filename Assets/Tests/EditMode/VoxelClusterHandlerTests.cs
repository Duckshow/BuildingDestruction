using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Assert = NUnit.Framework.Assert;

public class VoxelClusterHandlerTests {

    private PrivateType voxelClusterHandlerType;

    [SetUp] 
    public void Setup() {
        voxelClusterHandlerType = new PrivateType(typeof(VoxelClusterHandler));
    }
    
    [Test] 
    public void GetBiggestVoxelClusterIndex() {
        for(int i = 0; i < 10; i++) {
            int biggest = 10;

            List<Octree<bool>> list = new List<Octree<bool>>() {
                new Octree<bool>(Vector3Int.zero, new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest)), startValue: false),
                new Octree<bool>(Vector3Int.zero, new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest)), startValue: false),
                new Octree<bool>(Vector3Int.zero, new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest)), startValue: false),
                new Octree<bool>(Vector3Int.zero, new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest)), startValue: false),
                new Octree<bool>(Vector3Int.zero, new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest)), startValue: false),
                new Octree<bool>(Vector3Int.zero, new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest)), startValue: false),
                new Octree<bool>(Vector3Int.zero, new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest)), startValue: false),
                new Octree<bool>(Vector3Int.zero, new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest)), startValue: false),
                new Octree<bool>(Vector3Int.zero, new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest)), startValue: false),
                new Octree<bool>(Vector3Int.zero, new Vector3Int(Random.Range(1, biggest), Random.Range(0, biggest), Random.Range(1, biggest)), startValue: false)
            };

            int biggestIndex = Random.Range(0, list.Count);
            list.Insert(biggestIndex, new Octree<bool>(Vector3Int.zero, new Vector3Int(biggest, biggest, biggest), startValue: false));

            Assert.AreEqual(biggestIndex, (int)voxelClusterHandlerType.InvokeStatic("GetBiggestVoxelClusterIndex", list));
        }
    }
}
