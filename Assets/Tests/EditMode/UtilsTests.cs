using NUnit.Framework;
using UnityEngine;

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
    public void GetDirectionVector() {
        Assert.That(Utils.GetDirectionVector(Direction.None)  == Vector3Int.zero);
        Assert.That(Utils.GetDirectionVector(Direction.Right) == Vector3Int.right);
        Assert.That(Utils.GetDirectionVector(Direction.Left)  == Vector3Int.left);
        Assert.That(Utils.GetDirectionVector(Direction.Up)    == Vector3Int.up);
        Assert.That(Utils.GetDirectionVector(Direction.Down)  == Vector3Int.down);
        Assert.That(Utils.GetDirectionVector(Direction.Fore)  == Vector3Int.forward);
        Assert.That(Utils.GetDirectionVector(Direction.Back)  == Vector3Int.back);
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
}
