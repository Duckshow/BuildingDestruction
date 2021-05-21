using UnityEngine;

public partial class Octree
{
    public static void RunTests() {
        TestAreChildrenIdentical();
        Debug.Log("Tests done.");
    }

    private static void TestAreChildrenIdentical() {
        Node n = new Node(null, -1, Color.white);
        n.SetupChildren();

        UnitTester.Assert<Node, bool>(
            "AreChildrenIdentical()",
            Node.CanSummarizeChildren,
            new UnitTester.Parameter("Node", n),
            expectedResult: true
        );


        for(int i = 0; i < CHILDREN_PER_PARENT; i++) {
            n = new Node(null, -1, Color.white);
            n.SetupChildren();
            n.Children[i] = null;

            UnitTester.Assert<Node, bool>(
                "AreChildrenIdentical()",
                Node.CanSummarizeChildren,
                new UnitTester.Parameter("Node", n),
                expectedResult: false
            );
        }

        for(int i = 0; i < CHILDREN_PER_PARENT; i++) {
            n = new Node(null, -1, Color.white);
            n.SetupChildren();
            n.Children[i].SetValue(Color.red, informParent: false);

            UnitTester.Assert<Node, bool>(
                "AreChildrenIdentical()",
                Node.CanSummarizeChildren,
                new UnitTester.Parameter("Node", n),
                expectedResult: false
            );
        }

        for(int i = 0; i < CHILDREN_PER_PARENT; i++) {
            n = new Node(null, -1, Color.white);
            n.SetupChildren();
            n.Children[i].SetValue(Color.white, informParent: false);

            UnitTester.Assert<Node, bool>(
                "AreChildrenIdentical()",
                Node.CanSummarizeChildren,
                new UnitTester.Parameter("Node", n),
                expectedResult: true
            );
        }
    }
}
