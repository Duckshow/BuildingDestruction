using UnityEngine;

public partial class Octree : MonoBehaviour
{
    public class Node {
        public Node Parent;
        public Node[] Children;
        public int SiblingIndex = -1;

        public Color? Value { get; private set; }

        public Node(Node parent, int siblingIndex, Color? value) {
            Parent = parent;
            SiblingIndex = siblingIndex;
            SetValue(value, informParent: false);
        }

        public void SetupChildren() {
            Children = new Node[CHILDREN_PER_PARENT];

            for(int i = 0; i < CHILDREN_PER_PARENT; i++) {
                Children[i] = new Node(this, i, Value);
            }

            ClearValue(); // no need to inform parent in this case, I'm fairly certain
        }

        public void SetValue(Color? value, bool informParent) {
            Value = value;
            Children = null;

            if(!informParent) {
                return;
            }

            if(Parent == null) {
                return;
            }

            if(CanSummarizeChildren(Parent)) {
                Parent.SetValue(value, informParent: true);
            }
        }

        public void ClearValue() {
            Value = null;
        }

        public static bool CanSummarizeChildren(Node parent) {
            Node firstChild = parent.Children[0];

            if(firstChild.Children != null) {
                return false;
            }

            for(int i = 1; i < CHILDREN_PER_PARENT; i++) {
                Node currentChild = parent.Children[i];

                if(currentChild.Children != null) {
                    return false;
                }

                if(firstChild.Value != currentChild.Value) {
                    return false;
                }
            }

            return true;
        }
    }

    private const int CHILDREN_PER_PARENT = 8;

    private Node root;
    public int Size;
    private Vector3Int coords = new Vector3Int();

    private void Start() {
        Debug.Assert((Size != 0) && ((Size & (Size - 1)) == 0));

        root = new Node(null, -1, null);
    }

    float timeToUpdate;
    private void Update() {
        if(timeToUpdate - Time.time > 0f) {
            return;
        }

        timeToUpdate = Time.time + 0.05f;

        //TestSetValue();
        DebugDrawGrid(Size, root);
    }

    [EasyButtons.Button]
    public void ResetCoords() {
        coords = Vector3Int.zero;
    }

    [EasyButtons.Button]
    public void TestSetValueRed() {
        TestSetValue(Color.red);
    }

    [EasyButtons.Button]
    public void TestSetValueGreen() {
        TestSetValue(Color.green);
    }

    [EasyButtons.Button]
    public void TestSetValueBlue() {
        TestSetValue(Color.blue);
    }

    private void TestSetValue(Color value) {
        SetValue(coords.x, coords.y, coords.z, value);

        coords.x++;
        if(coords.x == Size) {
            coords.x = 0;
            coords.y++;
            if(coords.y == Size) {
                coords.y = 0;
                coords.z++;
            }
        }

        //DebugDrawGrid(Size, root);
    }

    [EasyButtons.Button]
    public void TestRemove() {
        coords.x--;
        if(coords.x == -1) {
            coords.x = Size - 1;
            coords.y--;
            if(coords.y == -1) {
                coords.y = Size - 1;
                coords.z--;

                if(coords.z == -1) {
                    coords = Vector3Int.zero;
                }
            }
        }

        ClearValue(coords.x, coords.y, coords.z);
        //DebugDrawGrid(Size, root);
    }

    public bool TryGetValue(int x, int y, int z, out Color value) {
        return TryGetValue(x, y, z, Size, root, debugDraw: false, out value);
    }

    public void SetValue(int x, int y, int z, Color value) {
        SetValue(x, y, z, Size, root, value);
    }

    public void ClearValue(int x, int y, int z) {
        SetValue(x, y, z, Size, root, null);
    }

    private static bool TryGetValue(int x, int y, int z, int treeSize, Node root, bool debugDraw, out Color value) {
        value = Color.clear;

        if(x < 0 || y < 0 || z < 0 || x >= treeSize || y >= treeSize || z >= treeSize) {
            Debug.LogWarningFormat("Failed to get : {0}, {1}, {2}", x, y, z);
            return false;
        }

        Node node = root;

        int nodeSize = treeSize;
        int childSize = nodeSize / 2;

        int nodeOffsetX = 0;
        int nodeOffsetY = 0;
        int nodeOffsetZ = 0;

        if(debugDraw) {
            if(node.Value.HasValue) {
                DebugDrawNode(new Vector3Int(nodeOffsetX, nodeOffsetY, nodeOffsetZ), nodeSize, treeSize, node.Value.Value, 0.05f);
            }
            else {
                DebugDrawNode(new Vector3Int(nodeOffsetX, nodeOffsetY, nodeOffsetZ), nodeSize, treeSize, Color.grey, 0.05f);
            }
        }

        while(!node.Value.HasValue) {
            Vector3Int childLocalPos = new Vector3Int(
                (int)Mathf.Clamp01(Mathf.Sign(x - (nodeOffsetX + childSize))),
                (int)Mathf.Clamp01(Mathf.Sign(y - (nodeOffsetY + childSize))),
                (int)Mathf.Clamp01(Mathf.Sign(z - (nodeOffsetZ + childSize)))
            );

            if(node.Children == null) {
                return false;
            }

            int childSiblingIndex = VoxelGrid.CoordsToIndex(childLocalPos, width: 2);
            node = node.Children[childSiblingIndex];

            nodeOffsetX += childSize * childLocalPos.x;
            nodeOffsetY += childSize * childLocalPos.y;
            nodeOffsetZ += childSize * childLocalPos.z;

            nodeSize = childSize;
            childSize = nodeSize / 2;

            if(debugDraw) {
                if(node.Value.HasValue) {
                    DebugDrawNode(new Vector3Int(nodeOffsetX, nodeOffsetY, nodeOffsetZ), nodeSize, treeSize, node.Value.Value, 0.05f);
                }
                else {
                    DebugDrawNode(new Vector3Int(nodeOffsetX, nodeOffsetY, nodeOffsetZ), nodeSize, treeSize, Color.grey, 0.05f);
                }
            }
        }

        value = node.Value.Value;
        return true;
    }

    private static void SetValue(int x, int y, int z, int treeSize, Node root, Color? value) {
        if(x < 0 || y < 0 || z < 0 || x >= treeSize || y >= treeSize || z >= treeSize) {
            Debug.LogWarningFormat("Failed to set : {0}, {1}, {2}", x, y, z);
            return;
        }

        if(value == null && !TryGetValue(x, y, z, treeSize, root, debugDraw: false, out Color tmpValue)) {
            return;
        }

        Node node = root;

        int nodeSize = treeSize;
        int childSize = nodeSize / 2;

        int nodeOffsetX = 0;
        int nodeOffsetY = 0;
        int nodeOffsetZ = 0;

        while(nodeSize > 1) {
            if(value == node.Value && value != null) {
                return;
            }

            Vector3Int childLocalPos = new Vector3Int(
                (int)Mathf.Clamp01(Mathf.Sign(x - (nodeOffsetX + childSize))),
                (int)Mathf.Clamp01(Mathf.Sign(y - (nodeOffsetY + childSize))),
                (int)Mathf.Clamp01(Mathf.Sign(z - (nodeOffsetZ + childSize)))
            );

            int childSiblingIndex = VoxelGrid.CoordsToIndex(childLocalPos, width: 2);
            if(node.Children == null) {
                node.SetupChildren();

                if(value != null) {
                    node.Children[childSiblingIndex].SetValue(null, informParent: false);
                }
            }

            node = node.Children[childSiblingIndex];

            nodeOffsetX += childSize * childLocalPos.x;
            nodeOffsetY += childSize * childLocalPos.y;
            nodeOffsetZ += childSize * childLocalPos.z;

            nodeSize = childSize;
            childSize = nodeSize / 2;
        }

        if(value == node.Value) {
            return;
        }

        node.SetValue(value, informParent: true);
    }

    private static void DebugDrawGrid(int treeSize, Node root) {
        for(int z = 0; z < treeSize; z++) {
            for(int y = 0; y < treeSize; y++) {
                for(int x = 0; x < treeSize; x++) {
                    TryGetValue(x, y, z, treeSize, root, debugDraw: true, out Color value);
                }
            }
        }
    }

    private static void DebugDrawNode(Vector3Int nodeOffset, int nodeSize, int gridSize, Color color, float time) {
        Random.seed = nodeOffset.x + gridSize * (nodeOffset.y + gridSize * nodeOffset.z);

        float halfSize = nodeSize * 0.5f;
        Vector3 drawPos = nodeOffset + new Vector3(halfSize, halfSize, halfSize);

        float s = halfSize * 0.99f;

        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;
        Vector3 up = Vector3.up;
        Vector3 down = Vector3.down;
        Vector3 fore = Vector3.forward;
        Vector3 back = Vector3.back;

        Vector3 leftDownBack = drawPos + (left + down + back) * s;
        Vector3 rightDownBack = drawPos + (right + down + back) * s;
        Vector3 leftUpBack = drawPos + (left + up + back) * s;
        Vector3 rightUpBack = drawPos + (right + up + back) * s;
        Vector3 leftDownForward = drawPos + (left + down + fore) * s;
        Vector3 rightDownForward = drawPos + (right + down + fore) * s;
        Vector3 leftUpForward = drawPos + (left + up + fore) * s;
        Vector3 rightUpForward = drawPos + (right + up + fore) * s;

        Debug.DrawLine(leftDownBack, leftUpBack, color, time);
        Debug.DrawLine(leftUpBack, rightUpBack, color, time);
        Debug.DrawLine(rightUpBack, rightDownBack, color, time);
        Debug.DrawLine(rightDownBack, leftDownBack, color, time);

        Debug.DrawLine(leftDownForward, leftUpForward, color, time);
        Debug.DrawLine(leftUpForward, rightUpForward, color, time);
        Debug.DrawLine(rightUpForward, rightDownForward, color, time);
        Debug.DrawLine(rightDownForward, leftDownForward, color, time);

        Debug.DrawLine(leftDownBack, leftDownForward, color, time);
        Debug.DrawLine(leftDownForward, leftUpForward, color, time);
        Debug.DrawLine(leftUpForward, leftUpBack, color, time);
        Debug.DrawLine(leftUpBack, leftDownBack, color, time);

        Debug.DrawLine(rightDownBack, rightDownForward, color, time);
        Debug.DrawLine(rightDownForward, rightUpForward, color, time);
        Debug.DrawLine(rightUpForward, rightUpBack, color, time);
        Debug.DrawLine(rightUpBack, rightDownBack, color, time);

        Debug.DrawLine(leftUpBack, leftUpForward, color, time);
        Debug.DrawLine(leftUpForward, rightUpForward, color, time);
        Debug.DrawLine(rightUpForward, rightUpBack, color, time);
        Debug.DrawLine(rightUpBack, leftUpBack, color, time);

        Debug.DrawLine(leftDownBack, leftDownForward, color, time);
        Debug.DrawLine(leftDownForward, rightDownForward, color, time);
        Debug.DrawLine(rightDownForward, rightDownBack, color, time);
        Debug.DrawLine(rightDownBack, leftDownBack, color, time);
    }
}
