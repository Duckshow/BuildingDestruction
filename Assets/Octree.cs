using UnityEngine;

public class Octree : MonoBehaviour
{
    public class Node {
        public Node Parent;
        public Node[] Children;
        public int SiblingIndex = -1;

        public Node(Node parent, int siblingIndex) {
            Parent = parent;
            SiblingIndex = siblingIndex;
        }
    }

    private Node root;
    public int Size;
    private Vector3Int tmp = new Vector3Int();

    private void OnValidate() {
        Debug.Assert((Size != 0) && ((Size & (Size - 1)) == 0));
    }

    private void Start() {

        root = new Node(null, -1);

        //for(int z = 0; z < size.z; z += 2) {
        //    for(int y = 0; y < size.y; y += 2) {
        //        for(int x = 0; x < size.x; x += 2) {
        //            Add(x, y, z);
        //        }
        //    }
        //}
    }

    float timeToUpdate;
    private void Update() {
        if(timeToUpdate - Time.time > 0f) {
            return;
        }

        timeToUpdate = Time.time + 0f;

        //Debug.Log(tmp);
        //TestAdd();
        DebugDrawGrid();
    }

    [EasyButtons.Button]
    public void TestAdd() {
        Get(tmp.x, tmp.y, tmp.z, addNewIfNull: true, debugDraw: true);

        tmp.x++;
        if(tmp.x == Size) {
            tmp.x = 0;
            tmp.y++;
            if(tmp.y == Size) {
                tmp.y = 0;
                tmp.z++;
            }
        }
    }

    [EasyButtons.Button]
    public void TestRemove() {
        tmp.x--;
        if(tmp.x == -1) {
            tmp.x = Size - 1;
            tmp.y--;
            if(tmp.y == -1) {
                tmp.y = Size - 1;
                tmp.z--;

                if(tmp.z == -1) {
                    tmp = Vector3Int.zero;
                }
            }
        }

        Remove(tmp.x, tmp.y, tmp.z);
    }


    private Node Get(int x, int y, int z, bool addNewIfNull = false, bool debugDraw = false) {
        if(x < 0 || y < 0 || z < 0 || x >= Size || y >= Size || z >= Size) {
            Debug.LogWarningFormat("Failed to get : {0}, {1}, {2}", x, y, z);
            return null;
        }

        Node n = root;

        int parentSize = Size;
        int childSize = Size;

        int parentOffsetX = 0;
        int parentOffsetY = 0;
        int parentOffsetZ = 0;

        while(true) {
            parentSize = childSize;
            childSize = parentSize / 2;

            Vector3Int childLocalPos = new Vector3Int(
                (int)Mathf.Clamp01(Mathf.Sign(x - (parentOffsetX + childSize))),
                (int)Mathf.Clamp01(Mathf.Sign(y - (parentOffsetY + childSize))),
                (int)Mathf.Clamp01(Mathf.Sign(z - (parentOffsetZ + childSize)))                
            );

            if(parentSize > 1) {
                int childSiblingIndex = childLocalPos.x + 2 * (childLocalPos.y + 2 * childLocalPos.z);

                if(n.Children == null) {
                    if(addNewIfNull) {
                        n.Children = new Node[8];
                    }
                    else {
                        return null;
                    }
                }

                if(n.Children[childSiblingIndex] == null) {
                    if(addNewIfNull) {
                        n.Children[childSiblingIndex] = new Node(n, childSiblingIndex);
                    }
                    else {
                        return null;
                    }
                }

                n = n.Children[childSiblingIndex];
            }

            if(debugDraw) {
                DebugDrawNode(new Vector3Int(parentOffsetX, parentOffsetY, parentOffsetZ), parentSize, Size, 0.01f);
            }

            if(parentSize == 1) {
                return n;
            }

            parentOffsetX += childSize * childLocalPos.x;
            parentOffsetY += childSize * childLocalPos.y;
            parentOffsetZ += childSize * childLocalPos.z;
        }
    }

    private void Remove(int x, int y, int z) {
        Node descendant = Get(x, y, z);
        if(descendant == null) {
            Debug.LogWarningFormat("Failed to remove : {0}, {1}, {2}", x, y, z);
            return;
        }

        Node ancestor = descendant.Parent;
        
        while(ancestor != null) {
            ancestor.Children[descendant.SiblingIndex] = null;

            bool foundAliveRelatives = false;
            for(int i = 0; i < 8; i++) {
                if(ancestor.Children[i] != null) {
                    foundAliveRelatives = true;
                    break;
                }
                else if(i == 8) {
                    ancestor.Children = null;
                }
            }

            if(foundAliveRelatives) {
                break;
            }

            descendant = ancestor;
            ancestor = ancestor.Parent;
        }

        //Debug.LogFormat("Removed : {0}, {1}, {2}", x, y, z);
    }

    private void DebugDrawGrid() {
        for(int z = 0; z < Size; z++) {
            for(int y = 0; y < Size; y++) {
                for(int x = 0; x < Size; x++) {
                    Get(x, y, z, addNewIfNull: false, debugDraw: true);
                }
            }
        }

        //Get(0, 0, 0, addNewIfNull: false, debugDraw: true);

    }

    private void DebugDrawNode(Vector3Int nodeOffset, int nodeSize, int gridSize, float time) {
        Random.seed = nodeOffset.x + gridSize * (nodeOffset.y + gridSize * nodeOffset.z);
        Color color = new Color(Random.value, Random.value, Random.value, 1f);

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
