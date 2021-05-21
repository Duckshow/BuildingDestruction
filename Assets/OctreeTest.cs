using UnityEngine;

public class OctreeTest : MonoBehaviour
{
    const float UPDATE_FREQ = 0.05f;

    public int Size;
    private Octree<Color> octree;
    private Vector3Int coords = new Vector3Int();

    private void Start() {
        octree = new Octree<Color>(Size);
    }

    float timeToUpdate;
    private void Update() {
        if(timeToUpdate - Time.time > 0f) {
            return;
        }

        timeToUpdate = Time.time + UPDATE_FREQ;

        DebugDrawGrid();
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
        octree.SetValue(coords.x, coords.y, coords.z, value);

        coords.x++;
        if(coords.x == Size) {
            coords.x = 0;
            coords.y++;
            if(coords.y == Size) {
                coords.y = 0;
                coords.z++;
            }
        }
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

        octree.SetValue(coords.x, coords.y, coords.z, default);
    }

    private void DebugDrawGrid() {
        for(int z = 0; z < Size; z++) {
            for(int y = 0; y < Size; y++) {
                for(int x = 0; x < Size; x++) {
                    octree.TryGetValue(x, y, z, out Color value, DebugDrawNode);
                }
            }
        }
    }

    private static void DebugDrawNode(int nodeOffsetX, int nodeOffsetY, int nodeOffsetZ, int nodeSize, int gridSize, Color value) {
        Random.seed = nodeOffsetX + gridSize * (nodeOffsetY + gridSize * nodeOffsetZ);

        float halfSize = nodeSize * 0.5f;
        Vector3 drawPos = new Vector3(nodeOffsetX + halfSize, nodeOffsetY + halfSize, nodeOffsetZ + halfSize);

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

        Debug.DrawLine(leftDownBack,        leftUpBack,         value, UPDATE_FREQ);
        Debug.DrawLine(leftUpBack,          rightUpBack,        value, UPDATE_FREQ);
        Debug.DrawLine(rightUpBack,         rightDownBack,      value, UPDATE_FREQ);
        Debug.DrawLine(rightDownBack,       leftDownBack,       value, UPDATE_FREQ);

        Debug.DrawLine(leftDownForward,     leftUpForward,      value, UPDATE_FREQ);
        Debug.DrawLine(leftUpForward,       rightUpForward,     value, UPDATE_FREQ);
        Debug.DrawLine(rightUpForward,      rightDownForward,   value, UPDATE_FREQ);
        Debug.DrawLine(rightDownForward,    leftDownForward,    value, UPDATE_FREQ);

        Debug.DrawLine(leftDownBack,        leftDownForward,    value, UPDATE_FREQ);
        Debug.DrawLine(leftDownForward,     leftUpForward,      value, UPDATE_FREQ);
        Debug.DrawLine(leftUpForward,       leftUpBack,         value, UPDATE_FREQ);
        Debug.DrawLine(leftUpBack,          leftDownBack,       value, UPDATE_FREQ);

        Debug.DrawLine(rightDownBack,       rightDownForward,   value, UPDATE_FREQ);
        Debug.DrawLine(rightDownForward,    rightUpForward,     value, UPDATE_FREQ);
        Debug.DrawLine(rightUpForward,      rightUpBack,        value, UPDATE_FREQ);
        Debug.DrawLine(rightUpBack,         rightDownBack,      value, UPDATE_FREQ);

        Debug.DrawLine(leftUpBack,          leftUpForward,      value, UPDATE_FREQ);
        Debug.DrawLine(leftUpForward,       rightUpForward,     value, UPDATE_FREQ);
        Debug.DrawLine(rightUpForward,      rightUpBack,        value, UPDATE_FREQ);
        Debug.DrawLine(rightUpBack,         leftUpBack,         value, UPDATE_FREQ);

        Debug.DrawLine(leftDownBack,        leftDownForward,    value, UPDATE_FREQ);
        Debug.DrawLine(leftDownForward,     rightDownForward,   value, UPDATE_FREQ);
        Debug.DrawLine(rightDownForward,    rightDownBack,      value, UPDATE_FREQ);
        Debug.DrawLine(rightDownBack,       leftDownBack,       value, UPDATE_FREQ);
    }
}
