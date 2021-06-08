using NUnit.Framework;

public class OctreeTests
{
    [Test]
    public void CanSummarizeChildren() {
        Octree<bool>.Node n = new Octree<bool>.Node(null, -1, false);
        n.SetupChildren();

        Assert.IsTrue(Octree<bool>.Node.CanSummarizeChildren(n));

        for(int i = 0; i < Octree<bool>.CHILDREN_PER_PARENT; i++) {
            n = new Octree<bool>.Node(null, -1, true);
            n.SetupChildren();
            n.Children[i].SetValue(false, informParent: false);

            Assert.IsFalse(Octree<bool>.Node.CanSummarizeChildren(n));
        }

        for(int i = 0; i < Octree<bool>.CHILDREN_PER_PARENT; i++) {
            n = new Octree<bool>.Node(null, -1, true);
            n.SetupChildren();
            n.Children[i].SetValue(true, informParent: false);
            
            Assert.IsTrue(Octree<bool>.Node.CanSummarizeChildren(n));
        }
    }
}
