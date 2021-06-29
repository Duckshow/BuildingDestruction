using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class UtilsTests
{
    [Test]
    public void GetAndSetValueFromByte()
    {
        Assert.IsFalse(Utils.GetValueFromByte(0b_0000_0000, 0));

        for(int i = 0; i < 8; i++) {
            Assert.IsTrue(Utils.GetValueFromByte((byte)(1 << i), i));
            Assert.That(Utils.GetValueFromByte(1 << 0, i) == (i == 0));
            Assert.That(Utils.GetValueFromByte(1 << 7, i) == (i == 7));
        }

        Assert.IsFalse(Utils.GetValueFromByte(0b_1111_1110, 0));
        Assert.IsFalse(Utils.GetValueFromByte(0b_1111_0111, 3));
        Assert.IsFalse(Utils.GetValueFromByte(0b_0111_1111, 7));

        Assert.IsTrue(Utils.GetValueFromByte(0b_0101_0101, 4));

        byte b = 0b_0000_0000;
        Utils.SetValueInByte(ref b, 0, true);
        Assert.IsTrue(Utils.GetValueFromByte(b, 0));
        Assert.IsFalse(Utils.GetValueFromByte(b, 1));
        Assert.IsFalse(Utils.GetValueFromByte(b, 2));
        Assert.IsFalse(Utils.GetValueFromByte(b, 3));
        Assert.IsFalse(Utils.GetValueFromByte(b, 4));
        Assert.IsFalse(Utils.GetValueFromByte(b, 5));
        Assert.IsFalse(Utils.GetValueFromByte(b, 6));
        Assert.IsFalse(Utils.GetValueFromByte(b, 7));

        Utils.SetValueInByte(ref b, 4, true);
        Assert.IsTrue(Utils.GetValueFromByte(b, 0));
        Assert.IsFalse(Utils.GetValueFromByte(b, 1));
        Assert.IsFalse(Utils.GetValueFromByte(b, 2));
        Assert.IsFalse(Utils.GetValueFromByte(b, 3));
        Assert.IsTrue(Utils.GetValueFromByte(b, 4));
        Assert.IsFalse(Utils.GetValueFromByte(b, 5));
        Assert.IsFalse(Utils.GetValueFromByte(b, 6));
        Assert.IsFalse(Utils.GetValueFromByte(b, 7));

        Utils.SetValueInByte(ref b, 0, false);
        Assert.IsFalse(Utils.GetValueFromByte(b, 0));
        Assert.IsFalse(Utils.GetValueFromByte(b, 1));
        Assert.IsFalse(Utils.GetValueFromByte(b, 2));
        Assert.IsFalse(Utils.GetValueFromByte(b, 3));
        Assert.IsTrue(Utils.GetValueFromByte(b, 4));
        Assert.IsFalse(Utils.GetValueFromByte(b, 5));
        Assert.IsFalse(Utils.GetValueFromByte(b, 6));
        Assert.IsFalse(Utils.GetValueFromByte(b, 7));
    }

    [Test]
    public void TestDirectionToVector() {
        Assert.That(Utils.DirectionToVector(Direction.None)  == Vector3Int.zero);
        Assert.That(Utils.DirectionToVector(Direction.Right) == Vector3Int.right);
        Assert.That(Utils.DirectionToVector(Direction.Left)  == Vector3Int.left);
        Assert.That(Utils.DirectionToVector(Direction.Up)    == Vector3Int.up);
        Assert.That(Utils.DirectionToVector(Direction.Down)  == Vector3Int.down);
        Assert.That(Utils.DirectionToVector(Direction.Fore)  == Vector3Int.forward);
        Assert.That(Utils.DirectionToVector(Direction.Back)  == Vector3Int.back);
    }

    [Test]
    public void GetOppositeDirection() {
        Assert.That(Utils.GetOppositeDirection(Direction.None)  == Direction.None);
        Assert.That(Utils.GetOppositeDirection(Direction.Right) == Direction.Left);
        Assert.That(Utils.GetOppositeDirection(Direction.Left)  == Direction.Right);
        Assert.That(Utils.GetOppositeDirection(Direction.Up)    == Direction.Down);
        Assert.That(Utils.GetOppositeDirection(Direction.Down)  == Direction.Up);
        Assert.That(Utils.GetOppositeDirection(Direction.Fore)  == Direction.Back);
        Assert.That(Utils.GetOppositeDirection(Direction.Back)  == Direction.Fore);
    }

    [Test]
    public void AreDirectionsOpposite() {
        Assert.IsTrue(Utils.AreDirectionsOpposite(Direction.None, Direction.None));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.None, Direction.Right));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.None, Direction.Left));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.None, Direction.Up));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.None, Direction.Down));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.None, Direction.Fore));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.None, Direction.Back));

        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Right, Direction.None));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Right, Direction.Right));
        Assert.IsTrue(Utils.AreDirectionsOpposite(Direction.Right, Direction.Left));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Right, Direction.Up));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Right, Direction.Down));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Right, Direction.Fore));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Right, Direction.Back));

        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Left, Direction.None));
        Assert.IsTrue(Utils.AreDirectionsOpposite(Direction.Left, Direction.Right));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Left, Direction.Left));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Left, Direction.Up));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Left, Direction.Down));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Left, Direction.Fore));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Left, Direction.Back));

        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Up, Direction.None));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Up, Direction.Right));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Up, Direction.Left));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Up, Direction.Up));
        Assert.IsTrue(Utils.AreDirectionsOpposite(Direction.Up, Direction.Down));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Up, Direction.Fore));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Up, Direction.Back));

        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Down, Direction.None));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Down, Direction.Right));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Down, Direction.Left));
        Assert.IsTrue(Utils.AreDirectionsOpposite(Direction.Down, Direction.Up));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Down, Direction.Down));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Down, Direction.Fore));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Down, Direction.Back));

        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Fore, Direction.None));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Fore, Direction.Right));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Fore, Direction.Left));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Fore, Direction.Up));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Fore, Direction.Down));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Fore, Direction.Fore));
        Assert.IsTrue(Utils.AreDirectionsOpposite(Direction.Fore, Direction.Back));

        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Back, Direction.None));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Back, Direction.Right));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Back, Direction.Left));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Back, Direction.Up));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Back, Direction.Down));
        Assert.IsTrue(Utils.AreDirectionsOpposite(Direction.Back, Direction.Fore));
        Assert.IsFalse(Utils.AreDirectionsOpposite(Direction.Back, Direction.Back));
    }

    [Test]
    public void RoundUpToPOT() {
        Assert.That(Utils.RoundUpToPOT(0) == 0);
        Assert.That(Utils.RoundUpToPOT(1) == 1);
        Assert.That(Utils.RoundUpToPOT(2) == 2);

        for(int i = 3; i <= 4; i++)         { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 2)); }
        for(int i = 5; i <= 8; i++)         { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 3)); }
        for(int i = 9; i <= 16; i++)        { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 4)); }
        for(int i = 17; i <= 32; i++)       { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 5)); }
        for(int i = 33; i <= 64; i++)       { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 6)); }
        for(int i = 65; i <= 128; i++)      { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 7)); }
        for(int i = 129; i <= 256; i++)     { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 8)); }
        for(int i = 257; i <= 512; i++)     { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 9)); }
        for(int i = 513; i <= 1024; i++)    { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 10)); }
        for(int i = 1025; i <= 2048; i++)   { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 11)); }
        for(int i = 2049; i <= 4096; i++)   { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 12)); }
        for(int i = 4097; i <= 8192; i++)   { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 13)); }
        for(int i = 8193; i <= 16384; i++)  { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 14)); }
        for(int i = 16385; i <= 32768; i++) { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 15)); }
        for(int i = 32769; i <= 65536; i++) { Assert.That(Utils.RoundUpToPOT(i) == (int)Mathf.Pow(2, 16)); }
    }

    [Test]
    public void AreCoordsWithinDimensions() {
        for(int i = 0; i < 25; i++) {
            Vector3Int dimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));
            Vector3Int coords = new Vector3Int(Random.Range(0, dimensions.x), Random.Range(0, dimensions.y), Random.Range(0, dimensions.z));

            Assert.IsTrue(Utils.AreCoordsWithinDimensions(coords, dimensions));
            Assert.IsFalse(Utils.AreCoordsWithinDimensions(dimensions, dimensions));
        }
    }

    [Test]
    public void AreCoordsOnTheEdge() {
        for(int i = 0; i < 50; i++) {
            Vector3Int dimensions = new Vector3Int(Random.Range(3, 10), Random.Range(3, 10), Random.Range(3, 10));

            Assert.IsTrue(Utils.AreCoordsOnTheEdge(new Vector3Int(0,                                     Random.Range(0, dimensions.y),          Random.Range(0, dimensions.z)),        dimensions));
            Assert.IsTrue(Utils.AreCoordsOnTheEdge(new Vector3Int(Random.Range(0, dimensions.x),         0,                                      Random.Range(0, dimensions.z)),        dimensions));
            Assert.IsTrue(Utils.AreCoordsOnTheEdge(new Vector3Int(Random.Range(0, dimensions.x),         Random.Range(0, dimensions.y),          0),                                    dimensions));

            Assert.IsTrue(Utils.AreCoordsOnTheEdge(new Vector3Int(dimensions.x - 1,                      Random.Range(1, dimensions.y - 1),      Random.Range(1, dimensions.z - 1)),    dimensions));
            Assert.IsTrue(Utils.AreCoordsOnTheEdge(new Vector3Int(Random.Range(1, dimensions.x - 1),     dimensions.y - 1,                       Random.Range(1, dimensions.z - 1)),    dimensions));
            Assert.IsTrue(Utils.AreCoordsOnTheEdge(new Vector3Int(Random.Range(1, dimensions.x - 1),     Random.Range(1, dimensions.y - 1),      dimensions.z - 1),                     dimensions));

            Assert.IsFalse(Utils.AreCoordsOnTheEdge(new Vector3Int(Random.Range(1, dimensions.x - 1), Random.Range(1, dimensions.y - 1), Random.Range(1, dimensions.z - 1)), dimensions));
        }
    }

    [Test]
    public void TestAreCoordsOnTwoEdges() {
        int minX = 0;
        int minY = -1;
        int minZ = -2;

        int maxX = 1;
        int maxY = 2;
        int maxZ = 3;

        List<Vector3Int> coordsOnTwoEdges = new List<Vector3Int>() { 
            new Vector3Int(minX, minY, -2),
            new Vector3Int(minX, minY, -1),
            new Vector3Int(minX, minY,  0),
            new Vector3Int(minX, minY,  1),
            new Vector3Int(minX, minY,  2),
            new Vector3Int(minX, minY,  3),

            new Vector3Int(minX, maxY, -3),
            new Vector3Int(minX, maxY, -2),
            new Vector3Int(minX, maxY, -1),
            new Vector3Int(minX, maxY,  0),
            new Vector3Int(minX, maxY,  1),
            new Vector3Int(minX, maxY,  2),
            new Vector3Int(minX, maxY,  3),
            new Vector3Int(minX, maxY,  4),

            new Vector3Int(minX, -1, minZ),
            new Vector3Int(minX,  0, minZ),
            new Vector3Int(minX,  1, minZ),
            new Vector3Int(minX,  2, minZ),

            new Vector3Int(minX, -1, maxZ),
            new Vector3Int(minX,  0, maxZ),
            new Vector3Int(minX,  1, maxZ),
            new Vector3Int(minX,  2, maxZ),

            new Vector3Int(maxX, minY, -2),
            new Vector3Int(maxX, minY, -1),
            new Vector3Int(maxX, minY,  0),
            new Vector3Int(maxX, minY,  1),
            new Vector3Int(maxX, minY,  2),
            new Vector3Int(maxX, minY,  3),

            new Vector3Int(maxX, maxY, -2),
            new Vector3Int(maxX, maxY, -1),
            new Vector3Int(maxX, maxY,  0),
            new Vector3Int(maxX, maxY,  1),
            new Vector3Int(maxX, maxY,  2),
            new Vector3Int(maxX, maxY,  3),

            new Vector3Int(maxX, -1, minZ),
            new Vector3Int(maxX,  0, minZ),
            new Vector3Int(maxX,  1, minZ),
            new Vector3Int(maxX,  2, minZ),

            new Vector3Int(maxX, -1,  maxZ),
            new Vector3Int(maxX,  0,  maxZ),
            new Vector3Int(maxX,  1,  maxZ),
            new Vector3Int(maxX,  2,  maxZ),

            new Vector3Int(0, minY, minZ),
            new Vector3Int(1, minY, minZ),

            new Vector3Int(0, minY, maxZ),
            new Vector3Int(1, minY, maxZ),

            new Vector3Int(0, maxY, minZ),
            new Vector3Int(1, maxY, minZ),

            new Vector3Int(0, maxY, maxZ),
            new Vector3Int(1, maxY, maxZ),
        };

        for(int z = minZ; z <= maxZ; ++z) {
            for(int y = minY; y <= maxY; ++y) {
                for(int x = minX; x <= maxX; ++x) {
                    if(coordsOnTwoEdges.Contains(new Vector3Int(x, y, z))) {
                        Assert.IsTrue(Utils.AreCoordsOnTwoEdges(x, y, z, minX, minY, minZ, maxX, maxY, maxZ), string.Format("({0}, {1}, {2}) was false, but expected to be true!", x, y, z));
                    }
                    else {
                        Assert.IsFalse(Utils.AreCoordsOnTwoEdges(x, y, z, minX, minY, minZ, maxX, maxY, maxZ), string.Format("({0}, {1}, {2}) was true, but expected to be false!", x, y, z));
                    }
                }
            }
        }
    }

    [Test]
    public void TestAreCoordsAlignedWithCenter() {
        int minX = 0;
        int minY = -1;
        int minZ = -2;

        int maxX = 2;
        int maxY = 3;
        int maxZ = 4;

        int centerX = 1;
        int centerY = 1;
        int centerZ = 1;

        List<Vector3Int> coordsAlignedWithCenter = new List<Vector3Int>() {
            new Vector3Int(0, centerY, centerZ),
            new Vector3Int(1, centerY, centerZ),
            new Vector3Int(2, centerY, centerZ),

            new Vector3Int(centerX, -1, centerZ),
            new Vector3Int(centerX,  0, centerZ),
            new Vector3Int(centerX,  1, centerZ),
            new Vector3Int(centerX,  2, centerZ),
            new Vector3Int(centerX,  3, centerZ),

            new Vector3Int(centerX, centerY, -2),
            new Vector3Int(centerX, centerY, -1),
            new Vector3Int(centerX, centerY,  0),
            new Vector3Int(centerX, centerY,  1),
            new Vector3Int(centerX, centerY,  2),
            new Vector3Int(centerX, centerY,  3),
            new Vector3Int(centerX, centerY,  4),
        };

        for(int z = minZ; z <= maxZ; ++z) {
            for(int y = minY; y <= maxY; ++y) {
                for(int x = minX; x <= maxX; ++x) {
                    if(coordsAlignedWithCenter.Contains(new Vector3Int(x, y, z))) {
                        Assert.IsTrue(Utils.AreCoordsAlignedWithCenter(x, y, z, minX, minY, minZ, maxX, maxY, maxZ), string.Format("({0}, {1}, {2}) was false, but expected to be true!", x, y, z));
                    }
                    else {
                        Assert.IsFalse(Utils.AreCoordsAlignedWithCenter(x, y, z, minX, minY, minZ, maxX, maxY, maxZ), string.Format("({0}, {1}, {2}) was true, but expected to be false!", x, y, z));
                    }
                }
            }
        }
    }

    [Test]
    public void CoordsToIndexAndViceVersa() {
        for(int i = 0; i < 25; i++) {
            Vector3Int dimensions = new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));

            int expectedIndex = 0;

            for(int z = 0; z < dimensions.z; z++) {
                for(int y = 0; y < dimensions.y; y++) {
                    for(int x = 0; x < dimensions.x; x++) {
                        Vector3Int expectedCoords = new Vector3Int(x, y, z);

                        Assert.AreEqual(expectedIndex, Utils.CoordsToIndex(expectedCoords, dimensions));
                        Assert.AreEqual(expectedCoords, Utils.IndexToCoords(expectedIndex, dimensions));

                        expectedIndex++;
                    }
                }
            }

            Assert.AreEqual(-1, Utils.CoordsToIndex(dimensions, dimensions));
            Assert.AreEqual(-1, Utils.CoordsToIndex(new Vector3Int(0, 0, -1), dimensions));

            Assert.AreEqual(-Vector3Int.one, Utils.IndexToCoords(-1, dimensions));
            Assert.AreEqual(-Vector3Int.one, Utils.IndexToCoords(dimensions.x * dimensions.y * dimensions.z, dimensions));
        }
    }

    [Test]
    public void TestGetVoxelBlockAndVoxelIndexAndViceVersa() {
        Vector3Int dimensions = new Vector3Int(10, 11, 12);
        Vector3Int voxelDimensions = dimensions * Bin.WIDTH;

        int expectedVoxelIndex = 0;
        for(int z = 0; z < voxelDimensions.z; z++) {
            for(int y = 0; y < voxelDimensions.y; y++) {
                for(int x = 0; x < voxelDimensions.x; x++) {
                    Vector3Int currentVoxelCoords = new Vector3Int(x, y, z);
                    Vector3Int currentVoxelBlockCoords = currentVoxelCoords / Bin.WIDTH;

                    int expectedVoxelBlockIndex = Utils.CoordsToIndex(currentVoxelBlockCoords, dimensions);
                    int expectedLocalVoxelIndex = Utils.CoordsToIndex(currentVoxelCoords - currentVoxelBlockCoords * Bin.WIDTH, Bin.WIDTH);

                    Utils.GetVoxelBlockAndVoxelIndex(currentVoxelCoords, dimensions, out int voxelBlockIndex, out int localVoxelIndex);
                    Assert.AreEqual(expectedVoxelBlockIndex, voxelBlockIndex);
                    Assert.AreEqual(expectedLocalVoxelIndex, localVoxelIndex);

                    Vector3Int voxelCoords = Utils.GetVoxelCoords(voxelBlockIndex, localVoxelIndex, dimensions);
                    Assert.AreEqual(voxelCoords, currentVoxelCoords);

                    int voxelIndex = Utils.GetVoxelIndex(voxelBlockIndex, localVoxelIndex, dimensions);
                    Assert.AreEqual(expectedVoxelIndex, voxelIndex);

                    expectedVoxelIndex++;
                }
            }
        }
    }

    [Test]
    public void TestVector3IntMax() {
        for(int i = 0; i < 10; i++) {
            int x = Random.Range(0, 1000);
            int y = Random.Range(0, 1000);
            int z = Random.Range(0, 1000);
            int max = Mathf.Max(x, Mathf.Max(y, z));

            Assert.AreEqual(max, new Vector3Int(x, y, z).Max());
        }
    }

    [Test]
    public void TestVector3IntProduct() {
        for(int i = 0; i < 10; i++) {
            int x = Random.Range(0, 1000);
            int y = Random.Range(0, 1000);
            int z = Random.Range(0, 1000);
            int product = x * y * z;

            Assert.AreEqual(product, new Vector3Int(x, y, z).Product());
        }
    }

    [Test]
    public void TestVector3IntGet() {
        int x = 4;
        int y = 5;
        int z = 6;

        Vector3Int v = new Vector3Int(x, y, z);

        Assert.AreEqual(x, v.Get(Axis.X));
        Assert.AreEqual(y, v.Get(Axis.Y));
        Assert.AreEqual(z, v.Get(Axis.Z));
    }
    
    [Test]
    public void TestVector3IntSet() {
        int x = 4;
        int y = 5;
        int z = 6;

        Vector3Int v = new Vector3Int();
        v = v.Set(Axis.X, x);
        v = v.Set(Axis.Y, y);
        v = v.Set(Axis.Z, z);

        Assert.AreEqual(x, v.x);
        Assert.AreEqual(y, v.y);
        Assert.AreEqual(z, v.z);
    }

    [Test]
    public void TestDirectionToAxis() {
        Assert.AreEqual(Axis.X, Utils.DirectionToAxis(Direction.Right));
        Assert.AreEqual(Axis.X, Utils.DirectionToAxis(Direction.Left));
        Assert.AreEqual(Axis.Y, Utils.DirectionToAxis(Direction.Up));
        Assert.AreEqual(Axis.Y, Utils.DirectionToAxis(Direction.Down));
        Assert.AreEqual(Axis.Z, Utils.DirectionToAxis(Direction.Fore));
        Assert.AreEqual(Axis.Z, Utils.DirectionToAxis(Direction.Back));
    }

    [Test]
    public void TestIsPositiveDirection() {
        Assert.AreEqual(true,   Utils.IsPositiveDirection(Direction.Right));
        Assert.AreEqual(false,  Utils.IsPositiveDirection(Direction.Left));
        Assert.AreEqual(true,   Utils.IsPositiveDirection(Direction.Up));
        Assert.AreEqual(false,  Utils.IsPositiveDirection(Direction.Down));
        Assert.AreEqual(true,   Utils.IsPositiveDirection(Direction.Fore));
        Assert.AreEqual(false,  Utils.IsPositiveDirection(Direction.Back));
    }

    [Test]
    public void TestRoundUpToEven() {
        Assert.AreEqual(0, Utils.RoundUpToEven(0));
        Assert.AreEqual(2, Utils.RoundUpToEven(1));
        Assert.AreEqual(2, Utils.RoundUpToEven(2));
        Assert.AreEqual(4, Utils.RoundUpToEven(3));
        Assert.AreEqual(4, Utils.RoundUpToEven(4));
        Assert.AreEqual(6, Utils.RoundUpToEven(5));
        Assert.AreEqual(6, Utils.RoundUpToEven(6));

        Assert.AreEqual(-1000, Utils.RoundUpToEven(-1001));
        Assert.AreEqual(-1000, Utils.RoundUpToEven(-1000));
        Assert.AreEqual(1000, Utils.RoundUpToEven(1000));
        Assert.AreEqual(1002, Utils.RoundUpToEven(1001));
    }

    [Test]
    public void TestRoundDownToOdd() {
        Assert.AreEqual(-1, Utils.RoundDownToOdd(0));
        Assert.AreEqual(1, Utils.RoundDownToOdd(1));
        Assert.AreEqual(1, Utils.RoundDownToOdd(2));
        Assert.AreEqual(3, Utils.RoundDownToOdd(3));
        Assert.AreEqual(3, Utils.RoundDownToOdd(4));
        Assert.AreEqual(5, Utils.RoundDownToOdd(5));
        Assert.AreEqual(5, Utils.RoundDownToOdd(6));

        Assert.AreEqual(-1001, Utils.RoundDownToOdd(-1001));
        Assert.AreEqual(-1001, Utils.RoundDownToOdd(-1000));
        Assert.AreEqual(999, Utils.RoundDownToOdd(1000));
        Assert.AreEqual(1001, Utils.RoundDownToOdd(1001));
    }
}
