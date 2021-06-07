using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;
using Assert = NUnit.Framework.Assert;

public class OctreeTests {
    [Test]
    public void CanSummarizeChildren() {
        Octree<bool>.Node n = new Octree<bool>.Node(null, -1, new Vector3Int(0, 0, 0), 1, value: false);
        n.SetupChildren();

        Assert.That(Octree<bool>.Node.CanSummarizeChildren(n) == true);

        for(int i = 0; i < Octree<bool>.Node.SIZE; i++) {
            n = new Octree<bool>.Node(null, -1, new Vector3Int(0, 0, 0), 1, value: true);
            n.SetupChildren();
            n.Children[i].SetValue(false, informParent: false);

            Assert.That(Octree<bool>.Node.CanSummarizeChildren(n) == false);
        }

        for(int i = 0; i < Octree<bool>.Node.SIZE; i++) {
            n = new Octree<bool>.Node(null, -1, new Vector3Int(0, 0, 0), 1, value: true);
            n.SetupChildren();
            n.Children[i].SetValue(true, informParent: false);

            Assert.That(Octree<bool>.Node.CanSummarizeChildren(n) == true);
        }
    }

    [Test]
    public void GetNodeOffset() {
        Assert.AreEqual(new Vector3Int(0, 0, 0), Octree<bool>.Node.GetOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 0)));

        Assert.AreEqual(new Vector3Int(1, 0, 0), Octree<bool>.Node.GetOffset(new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0)));
        Assert.AreEqual(new Vector3Int(0, 1, 0), Octree<bool>.Node.GetOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 1, 0)));
        Assert.AreEqual(new Vector3Int(0, 0, 1), Octree<bool>.Node.GetOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 1)));
        Assert.AreEqual(new Vector3Int(1, 1, 1), Octree<bool>.Node.GetOffset(new Vector3Int(0, 0, 0), new Vector3Int(1, 1, 1)));

        Assert.AreEqual(new Vector3Int(-1,  0,  0), Octree<bool>.Node.GetOffset(new Vector3Int(1, 0, 0), new Vector3Int(0, 0, 0)));
        Assert.AreEqual(new Vector3Int( 0, -1,  0), Octree<bool>.Node.GetOffset(new Vector3Int(0, 1, 0), new Vector3Int(0, 0, 0)));
        Assert.AreEqual(new Vector3Int( 0,  0, -1), Octree<bool>.Node.GetOffset(new Vector3Int(0, 0, 1), new Vector3Int(0, 0, 0)));
        Assert.AreEqual(new Vector3Int(-1, -1, -1), Octree<bool>.Node.GetOffset(new Vector3Int(1, 1, 1), new Vector3Int(0, 0, 0)));

        Assert.Throws<System.Exception>(() => Octree<bool>.Node.GetOffset(new Vector3Int(-1,  0,  0), new Vector3Int(0, 0, 0)));
        Assert.Throws<System.Exception>(() => Octree<bool>.Node.GetOffset(new Vector3Int( 0, -1,  0), new Vector3Int(0, 0, 0)));
        Assert.Throws<System.Exception>(() => Octree<bool>.Node.GetOffset(new Vector3Int( 0,  0, -1), new Vector3Int(0, 0, 0)));

        Assert.Throws<System.Exception>(() => Octree<bool>.Node.GetOffset(new Vector3Int(0, 0, 0), new Vector3Int(-1,  0,  0)));
        Assert.Throws<System.Exception>(() => Octree<bool>.Node.GetOffset(new Vector3Int(0, 0, 0), new Vector3Int( 0, -1,  0)));
        Assert.Throws<System.Exception>(() => Octree<bool>.Node.GetOffset(new Vector3Int(0, 0, 0), new Vector3Int( 0,  0, -1)));
    }

    [Test]
    public void GetNeighborNodeOffset() {
        Assert.AreEqual(new Vector3Int( 1,  0,  0), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 0), 1, Direction.Right));
        Assert.AreEqual(new Vector3Int(-1,  0,  0), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 0), 1, Direction.Left));
        Assert.AreEqual(new Vector3Int( 0,  1,  0), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 0), 1, Direction.Up));
        Assert.AreEqual(new Vector3Int( 0, -1,  0), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 0), 1, Direction.Down));
        Assert.AreEqual(new Vector3Int( 0,  0,  1), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 0), 1, Direction.Fore));
        Assert.AreEqual(new Vector3Int( 0,  0, -1), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 0), 1, Direction.Back));

        Assert.AreEqual(new Vector3Int( 4,  0,  0),  Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 0), 4, Direction.Right));
        Assert.AreEqual(new Vector3Int(-1,  0,  0),  Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 0), 4, Direction.Left));
        Assert.AreEqual(new Vector3Int( 0,  4,  0),  Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 0), 4, Direction.Up));
        Assert.AreEqual(new Vector3Int( 0, -1,  0),  Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 0), 4, Direction.Down));
        Assert.AreEqual(new Vector3Int( 0,  0,  4),  Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 0), 4, Direction.Fore));
        Assert.AreEqual(new Vector3Int( 0,  0, -1),  Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 0), 4, Direction.Back));

        Assert.AreEqual(new Vector3Int( 3,  2,  2), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2), 1, Direction.Right));
        Assert.AreEqual(new Vector3Int( 1,  2,  2), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2), 1, Direction.Left));
        Assert.AreEqual(new Vector3Int( 2,  3,  2), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2), 1, Direction.Up));
        Assert.AreEqual(new Vector3Int( 2,  1,  2), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2), 1, Direction.Down));
        Assert.AreEqual(new Vector3Int( 2,  2,  3), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2), 1, Direction.Fore));
        Assert.AreEqual(new Vector3Int( 2,  2,  1), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2), 1, Direction.Back));

        Assert.AreEqual(new Vector3Int( 6,  2,  2), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2), 4, Direction.Right));
        Assert.AreEqual(new Vector3Int( 1,  2,  2), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2), 4, Direction.Left));
        Assert.AreEqual(new Vector3Int( 2,  6,  2), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2), 4, Direction.Up));
        Assert.AreEqual(new Vector3Int( 2,  1,  2), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2), 4, Direction.Down));
        Assert.AreEqual(new Vector3Int( 2,  2,  6), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2), 4, Direction.Fore));
        Assert.AreEqual(new Vector3Int( 2,  2,  1), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2), 4, Direction.Back));

        Assert.AreEqual(new Vector3Int( 1,  0,  0), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(2, 2, 2), new Vector3Int(2, 2, 2), 1, Direction.Right));
        Assert.AreEqual(new Vector3Int(-1,  0,  0), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(2, 2, 2), new Vector3Int(2, 2, 2), 1, Direction.Left));
        Assert.AreEqual(new Vector3Int( 0,  1,  0), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(2, 2, 2), new Vector3Int(2, 2, 2), 1, Direction.Up));
        Assert.AreEqual(new Vector3Int( 0, -1,  0), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(2, 2, 2), new Vector3Int(2, 2, 2), 1, Direction.Down));
        Assert.AreEqual(new Vector3Int( 0,  0,  1), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(2, 2, 2), new Vector3Int(2, 2, 2), 1, Direction.Fore));
        Assert.AreEqual(new Vector3Int( 0,  0, -1), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(2, 2, 2), new Vector3Int(2, 2, 2), 1, Direction.Back));

        Assert.AreEqual(new Vector3Int( 4,  0,  0), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(2, 2, 2), new Vector3Int(2, 2, 2), 4, Direction.Right));
        Assert.AreEqual(new Vector3Int(-1,  0,  0), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(2, 2, 2), new Vector3Int(2, 2, 2), 4, Direction.Left));
        Assert.AreEqual(new Vector3Int( 0,  4,  0), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(2, 2, 2), new Vector3Int(2, 2, 2), 4, Direction.Up));
        Assert.AreEqual(new Vector3Int( 0, -1,  0), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(2, 2, 2), new Vector3Int(2, 2, 2), 4, Direction.Down));
        Assert.AreEqual(new Vector3Int( 0,  0,  4), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(2, 2, 2), new Vector3Int(2, 2, 2), 4, Direction.Fore));
        Assert.AreEqual(new Vector3Int( 0,  0, -1), Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(2, 2, 2), new Vector3Int(2, 2, 2), 4, Direction.Back));

        Assert.Throws<System.Exception>(() => Octree<bool>.Node.GetNeighborNodeOffset(new Vector3Int(4, 4, 4), new Vector3Int(2, 2, 2), 4, Direction.Right));
    }

    [Test]
    public void GetChildrenOnFace() {
        Octree<bool>.Node.GetChildIndexesFacingDirection(Direction.Right, out int childIndex0, out int childIndex1, out int childIndex2, out int childIndex3);

        Assert.AreEqual(1, childIndex0);
        Assert.AreEqual(3, childIndex1);
        Assert.AreEqual(5, childIndex2);
        Assert.AreEqual(7, childIndex3);

        Octree<bool>.Node.GetChildIndexesFacingDirection(Direction.Left, out childIndex0, out childIndex1, out childIndex2, out childIndex3);

        Assert.AreEqual(0, childIndex0);
        Assert.AreEqual(2, childIndex1);
        Assert.AreEqual(4, childIndex2);
        Assert.AreEqual(6, childIndex3);

        Octree<bool>.Node.GetChildIndexesFacingDirection(Direction.Up, out childIndex0, out childIndex1, out childIndex2, out childIndex3);

        Assert.AreEqual(2, childIndex0);
        Assert.AreEqual(3, childIndex1);
        Assert.AreEqual(6, childIndex2);
        Assert.AreEqual(7, childIndex3);

        Octree<bool>.Node.GetChildIndexesFacingDirection(Direction.Down, out childIndex0, out childIndex1, out childIndex2, out childIndex3);

        Assert.AreEqual(0, childIndex0);
        Assert.AreEqual(1, childIndex1);
        Assert.AreEqual(4, childIndex2);
        Assert.AreEqual(5, childIndex3);

        Octree<bool>.Node.GetChildIndexesFacingDirection(Direction.Fore, out childIndex0, out childIndex1, out childIndex2, out childIndex3);

        Assert.AreEqual(4, childIndex0);
        Assert.AreEqual(5, childIndex1);
        Assert.AreEqual(6, childIndex2);
        Assert.AreEqual(7, childIndex3);

        Octree<bool>.Node.GetChildIndexesFacingDirection(Direction.Back, out childIndex0, out childIndex1, out childIndex2, out childIndex3);

        Assert.AreEqual(0, childIndex0);
        Assert.AreEqual(1, childIndex1);
        Assert.AreEqual(2, childIndex2);
        Assert.AreEqual(3, childIndex3);
    }

    [Test]
    public void TryGetAdjacentNodes() {
        Test_Surrounded(Direction.Right);
        Test_Surrounded(Direction.Left);
        Test_Surrounded(Direction.Up);
        Test_Surrounded(Direction.Down);
        Test_Surrounded(Direction.Fore);
        Test_Surrounded(Direction.Back);

        void Test_Surrounded(Direction dir) {
            Octree<bool> octree = new Octree<bool>(Vector3Int.zero, new Vector3Int(4, 4, 4), startValue: true);
            octree.SetValue(new Vector3Int(1, 1, 1), false);
            octree.TryGetNode(new Vector3Int(1, 1, 1), out Octree<bool>.Node node);

            Assert.IsTrue(Octree<bool>.TryGetAdjacentNodes(octree, node, dir, out Octree<bool>.Node[] neighbors, out int neighborCount));
            Assert.AreEqual(1, neighborCount);

            Vector3Int neighborOffset = neighbors[0].GetOffset(octree.Offset);

            switch(dir) {
                case Direction.Right: { Assert.AreEqual(new Vector3Int(2, 0, 0), neighborOffset); break; }
                case Direction.Left:  { Assert.AreEqual(new Vector3Int(0, 1, 1), neighborOffset); break; }
                case Direction.Up:    { Assert.AreEqual(new Vector3Int(0, 2, 0), neighborOffset); break; }
                case Direction.Down:  { Assert.AreEqual(new Vector3Int(1, 0, 1), neighborOffset); break; }
                case Direction.Fore:  { Assert.AreEqual(new Vector3Int(0, 0, 2), neighborOffset); break; }
                case Direction.Back:  { Assert.AreEqual(new Vector3Int(1, 1, 0), neighborOffset); break; }
                default: throw new System.Exception();
            }
        }

        //

        Test_CornerBL(Direction.Right);
        Test_CornerBL(Direction.Left);
        Test_CornerBL(Direction.Up);
        Test_CornerBL(Direction.Down);
        Test_CornerBL(Direction.Fore);
        Test_CornerBL(Direction.Back);

        void Test_CornerBL(Direction dir) {
            Octree<bool> octree = new Octree<bool>(Vector3Int.zero, new Vector3Int(4, 4, 4), startValue: true);
            octree.SetValue(new Vector3Int(0, 0, 0), false);
            octree.TryGetNode(new Vector3Int(0, 0, 0), out Octree<bool>.Node node);

            bool success = Octree<bool>.TryGetAdjacentNodes(octree, node, dir, out Octree<bool>.Node[] neighbors, out int neighborCount);

            if(Utils.IsPositiveDirection(dir)) {
                Assert.IsTrue(success);
                Assert.AreEqual(1, neighborCount);
                Assert.AreEqual(Utils.GetDirectionVector(dir) * node.Size, neighbors[0].GetOffset(octree.Offset) - node.GetOffset(octree.Offset));
            }
            else {
                Assert.IsFalse(success);
            }
        }

        //

        Test_CornerTR(Direction.Right);
        Test_CornerTR(Direction.Left);
        Test_CornerTR(Direction.Up);
        Test_CornerTR(Direction.Down);
        Test_CornerTR(Direction.Fore);
        Test_CornerTR(Direction.Back);

        void Test_CornerTR(Direction dir) {
            Octree<bool> octree = new Octree<bool>(Vector3Int.zero, new Vector3Int(4, 4, 4), startValue: true);
            octree.SetValue(new Vector3Int(3, 3, 3), false);
            octree.TryGetNode(new Vector3Int(3, 3, 3), out Octree<bool>.Node node);

            bool success = Octree<bool>.TryGetAdjacentNodes(octree, node, dir, out Octree<bool>.Node[] neighbors, out int neighborCount);

            if(Utils.IsPositiveDirection(dir)) {
                Assert.IsFalse(success);
            }
            else {
                Assert.IsTrue(success);
                Assert.AreEqual(1, neighborCount);
                Assert.AreEqual(Utils.GetDirectionVector(dir) * node.Size, neighbors[0].GetOffset(octree.Offset) - node.GetOffset(octree.Offset));
            }
        }

        //

        Test_EveryEvenTrue(Direction.Right);
        Test_EveryEvenTrue(Direction.Left);
        Test_EveryEvenTrue(Direction.Up);
        Test_EveryEvenTrue(Direction.Down);
        Test_EveryEvenTrue(Direction.Fore);
        Test_EveryEvenTrue(Direction.Back);

        void Test_EveryEvenTrue(Direction dir) {
            Octree<bool> octree = new Octree<bool>(Vector3Int.zero, new Vector3Int(16, 16, 16), startValue: true);

            Vector3Int originPos;
            switch(dir) {
                case Direction.Right: { originPos = new Vector3Int(0, 0, 0); break; }
                case Direction.Left:  { originPos = new Vector3Int(8, 0, 0); break; }
                case Direction.Up:    { originPos = new Vector3Int(0, 0, 0); break; }
                case Direction.Down:  { originPos = new Vector3Int(0, 8, 0); break; }
                case Direction.Fore:  { originPos = new Vector3Int(0, 0, 0); break; }
                case Direction.Back:  { originPos = new Vector3Int(0, 0, 8); break; }
                default: throw new System.Exception();
            }

            octree.SetValue(originPos, false, size: 8);

            Axis axis = Utils.DirectionToAxis(dir);
            Utils.GetOtherAxes(axis, out Axis otherAxis1, out Axis otherAxis2);

            bool setFalse = true;
            for(int otherAxisIndex1 = 0; otherAxisIndex1 < 16; ++otherAxisIndex1) {
                for(int otherAxisIndex2 = 0; otherAxisIndex2 < 16; ++otherAxisIndex2) {
                    if(setFalse) {
                        Vector3Int pos = new Vector3Int();
                        pos = pos.Set(axis, Utils.IsPositiveDirection(dir) ? 8 : 7);
                        pos = pos.Set(otherAxis1, otherAxisIndex1);
                        pos = pos.Set(otherAxis2, otherAxisIndex2);

                        octree.SetValue(pos, value: false);
                    }

                    setFalse = !setFalse;
                }
            }

            octree.TryGetNode(originPos, out Octree<bool>.Node node);

            Assert.AreEqual(8, node.Size);
            Assert.IsTrue(Octree<bool>.TryGetAdjacentNodes(octree, node, dir, out Octree<bool>.Node[] neighbors, out int neighborCount));

            int amountWithValue = 0;
            for(int i = 0; i < neighborCount; i++) {
                Assert.AreEqual(1, neighbors[i].Size);

                if(neighbors[i].HasValue()) {
                    ++amountWithValue;
                }
            }

            Assert.AreEqual(64, neighborCount);
            Assert.AreEqual(64, neighbors.Length);
            Assert.AreEqual(32, amountWithValue);
        }

        //

        Test_Single(Direction.Right);
        Test_Single(Direction.Left);
        Test_Single(Direction.Up);
        Test_Single(Direction.Down);
        Test_Single(Direction.Fore);
        Test_Single(Direction.Back);

        void Test_Single(Direction dir) {
            Octree<bool> octree = new Octree<bool>(Vector3Int.zero, new Vector3Int(16, 16, 16), startValue: true);

            Vector3Int originPos;
            switch(dir) {
                case Direction.Right: { originPos = new Vector3Int(0, 0, 0); break; }
                case Direction.Left:  { originPos = new Vector3Int(8, 0, 0); break; }
                case Direction.Up:    { originPos = new Vector3Int(0, 0, 0); break; }
                case Direction.Down:  { originPos = new Vector3Int(0, 8, 0); break; }
                case Direction.Fore:  { originPos = new Vector3Int(0, 0, 0); break; }
                case Direction.Back:  { originPos = new Vector3Int(0, 0, 8); break; }
                default:
                    throw new System.Exception();
            }

            octree.SetValue(originPos, false, size: 8);

            Axis axis = Utils.DirectionToAxis(dir);
            Utils.GetOtherAxes(axis, out Axis otherAxis1, out Axis otherAxis2);

            for(int otherAxisIndex1 = 0; otherAxisIndex1 < 16; otherAxisIndex1++) {
                for(int otherAxisIndex2 = 0; otherAxisIndex2 < 16; otherAxisIndex2++) {
                    Vector3Int pos = new Vector3Int();
                    pos = pos.Set(axis, Utils.IsPositiveDirection(dir) ? 8 : 7);
                    pos = pos.Set(otherAxis1, otherAxisIndex1);
                    pos = pos.Set(otherAxis2, otherAxisIndex2);
                    octree.SetValue(pos, value: false);

                    octree.TryGetNode(originPos, out Octree<bool>.Node node);
                    
                    Assert.AreEqual(8, node.Size);
                    Assert.IsTrue(Octree<bool>.TryGetAdjacentNodes(octree, node, dir, out Octree<bool>.Node[] neighbors, out int neighborCount));

                    for(int i = 0; i < neighborCount; i++) {
                        if(!neighbors[i].HasValue()) {
                            Assert.AreEqual(pos, neighbors[i].GetOffset(octree.Offset));
                        }
                        else {
                            Assert.AreNotEqual(pos, neighbors[i].GetOffset(octree.Offset));
                        }
                    }

                    octree.SetValue(pos, value: true);
                }
            }
        }
    }
}
