using UnityEngine;
using System;

public static class OctreeTests {

    public static void RunTests() {
        TestAreChildrenIdentical();
        Debug.Log("Tests done.");
    }

    private static void TestAreChildrenIdentical() {
        Octree<Color>.Node<Color> n = new Octree<Color>.Node<Color>(null, -1, default);
        n.SetupChildren();

        UnitTester.Assert<Octree<Color>.Node<Color>, bool>(
            "AreChildrenIdentical()",
            Octree<Color>.Node<Color>.CanSummarizeChildren,
            new UnitTester.Parameter("Node", n),
            expectedResult: true
        );

        for(int i = 0; i < Octree<Color>.CHILDREN_PER_PARENT; i++) {
            n = new Octree<Color>.Node<Color>(null, -1, default);
            n.SetupChildren();
            n.Children[i].SetValue(Color.red, informParent: false);

            UnitTester.Assert<Octree<Color>.Node<Color>, bool>(
                "AreChildrenIdentical()",
                Octree<Color>.Node<Color>.CanSummarizeChildren,
                new UnitTester.Parameter("Node", n),
                expectedResult: false
            );
        }

        for(int i = 0; i < Octree<Color>.CHILDREN_PER_PARENT; i++) {
            n = new Octree<Color>.Node<Color>(null, -1, Color.white);
            n.SetupChildren();
            n.Children[i].SetValue(Color.white, informParent: false);

            UnitTester.Assert<Octree<Color>.Node<Color>, bool>(
                "AreChildrenIdentical()",
                Octree<Color>.Node<Color>.CanSummarizeChildren,
                new UnitTester.Parameter("Node", n),
                expectedResult: true
            );
        }
    }
}
