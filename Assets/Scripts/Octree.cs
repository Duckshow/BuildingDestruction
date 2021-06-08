using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("EditMode")]
[Serializable]
public partial class Octree<T> where T : IEquatable<T> {
   
    [Serializable]
    public class Node {
        public const int SIZE = 8;
        public const int WIDTH = 2;

        [SerializeField, HideInInspector] private Node parent;
        [SerializeField, HideInInspector] private Node[] children;
        [SerializeField, HideInInspector] private int siblingIndex;
        [SerializeField, HideInInspector] private int size;
        [SerializeField, HideInInspector] private Vector3Int fixedOffset; // TODO: can we remove this cached offset and keep it as something that's calculated by the octree? could save a lot of hassle when it comes to changing the octree offset
        [SerializeField, HideInInspector] private T value;

        public Node Parent { get { return parent; } }
        public Node[] Children { get { return children; } }
        public int SiblingIndex { get { return siblingIndex; } }
        public int Size { get { return size; } }


        public Node(Node parent, int siblingIndex, Vector3Int fixedOffset, int size, T value) {
            this.parent = parent;
            this.siblingIndex = siblingIndex;

            if(parent == null) {
                this.fixedOffset = Vector3Int.zero; // WARNING: never offset the root, it creates hard to find bugs!
            }
            else {
                this.fixedOffset = fixedOffset;
            }

            this.size = size;
            SetValue(value, informParent: false);

        }

        public bool HasValue() {
            return !EqualityComparer<T>.Default.Equals(value, default);
        }

        public T GetValue() {
            return value;
        }

        public void SetValue(T value, bool informParent) {
            this.value = value;
            children = null;

            if(!informParent) {
                return;
            }

            if(parent == null) {
                return;
            }

            if(CanSummarizeChildren(parent)) {
                parent.SetValue(value, informParent: true);
            }
        }

        public void SetupChildren() {
            children = new Node[SIZE];

            int childSize = size / 2;
            for(int i = 0; i < SIZE; i++) {
                children[i] = new Node(this, i, fixedOffset + childSize * VoxelGrid.IndexToCoords(i, 2), childSize, value);
            }

            value = default;
        }

        internal static bool CanSummarizeChildren(Node node) {
            Node firstChild = node.children[0];

            if(firstChild != null && firstChild.children != null) {
                return false;
            }

            for(int i = 1; i < SIZE; i++) {
                Node currentChild = node.children[i];

                if(currentChild.children != null) {
                    return false;
                }

                if(!EqualityComparer<T>.Default.Equals(firstChild.value, currentChild.value)) {
                    return false;
                }
            }

            return true;
        }

        public Vector3Int GetOffset(Vector3Int octreeOffset) {
            return GetOffset(octreeOffset, fixedOffset);            
        }

        internal static Vector3Int GetOffset(Vector3Int octreeOffset, Vector3Int fixedOffset) {
            if(octreeOffset.x < 0 || octreeOffset.y < 0 || octreeOffset.z < 0) {
                throw new Exception();
            }

            if(fixedOffset.x < 0 || fixedOffset.y < 0 || fixedOffset.z < 0) {
                throw new Exception();
            }

            return fixedOffset - octreeOffset;
        }

        public Vector3Int GetNeighborNodeOffset(Vector3Int octreeOffset, Direction direction) {
            Debug.Log(GetOffset(octreeOffset) + ", " + GetNeighborNodeOffset(octreeOffset, fixedOffset, size, direction));

            return GetNeighborNodeOffset(octreeOffset, fixedOffset, size, direction);
        }

        internal static Vector3Int GetNeighborNodeOffset(Vector3Int octreeOffset, Vector3Int fixedOffset, int nodeSize, Direction direction) {
            Vector3Int dirVec = Utils.GetDirectionVector(direction);

            int deltaX = dirVec.x;
            int deltaY = dirVec.y;
            int deltaZ = dirVec.z;

            if(dirVec.x > 0) { deltaX *= nodeSize; }
            if(dirVec.y > 0) { deltaY *= nodeSize; }
            if(dirVec.z > 0) { deltaZ *= nodeSize; }

            Vector3Int nodeOffset = GetOffset(octreeOffset, fixedOffset);

            if(nodeOffset.x < 0 || nodeOffset.y < 0 || nodeOffset.z < 0) {
                throw new Exception();
            }

            Vector3Int result = new Vector3Int(
                nodeOffset.x + deltaX,
                nodeOffset.y + deltaY,
                nodeOffset.z + deltaZ
            );


            return result;
        }

        public static void GetChildIndexesFacingDirection(Direction direction, out int childIndex0, out int childIndex1, out int childIndex2, out int childIndex3) {
            childIndex0 = -1;
            childIndex1 = -1;
            childIndex2 = -1;
            childIndex3 = -1;

            switch(direction) {
                case Direction.Right: {
                    childIndex0 = 1;
                    childIndex1 = 3;
                    childIndex2 = 5;
                    childIndex3 = 7;
                    break;
                }
                case Direction.Left: {
                    childIndex0 = 0;
                    childIndex1 = 2;
                    childIndex2 = 4;
                    childIndex3 = 6;
                    break;
                }
                case Direction.Up: {
                    childIndex0 = 2;
                    childIndex1 = 3;
                    childIndex2 = 6;
                    childIndex3 = 7;
                    break;
                }
                case Direction.Down: {
                    childIndex0 = 0;
                    childIndex1 = 1;
                    childIndex2 = 4;
                    childIndex3 = 5;
                    break;
                }
                case Direction.Fore: {
                    childIndex0 = 4;
                    childIndex1 = 5;
                    childIndex2 = 6;
                    childIndex3 = 7;
                    break;
                }
                case Direction.Back: {
                    childIndex0 = 0;
                    childIndex1 = 1;
                    childIndex2 = 2;
                    childIndex3 = 3;
                    break;
                }
            }
        }
    }

    public static Vector3Int[] LOCAL_COORDS_LOOKUP = new Vector3Int[] { 
        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 1, 1)
    };

    [SerializeField, HideInInspector] private int size;
    [SerializeField, HideInInspector] private Vector3Int offset;
    [SerializeField, HideInInspector] private Vector3Int dimensions;
    [SerializeField, HideInInspector] private readonly Node root;

    public int Size { get { return size; } }
    public Vector3Int Offset { get { return offset; } }
    public Vector3Int Dimensions { get { return dimensions; } }


    public Octree(Vector3Int offset, Vector3Int dimensions, T startValue) {
        size = Utils.RoundUpToPOT(dimensions.Max());
        this.offset = offset;
        this.dimensions = dimensions;

        root = new Node(null, -1, Vector3Int.zero, size, default);

        if(EqualityComparer<T>.Default.Equals(startValue, default)) {
            return;
        }

        for(int z = 0; z < dimensions.z; z++) {
            for(int y = 0; y < dimensions.y; y++) {
                for(int x = 0; x < dimensions.x; x++) {
                    SetValue(x, y, z, startValue);
                }
            }
        }
    }

    public void Resize(Vector3Int newOffset, Vector3Int newDimensions) {
        Debug.Assert(
            newOffset.x >= offset.x && 
            newOffset.y >= offset.y && 
            newOffset.z >= offset.z
            , string.Format("Tried to Resize from offset {0} to {1}!", offset, newOffset)
        );

        Debug.Assert(
            newDimensions.x <= dimensions.x && 
            newDimensions.y <= dimensions.y && 
            newDimensions.z <= dimensions.z
            , string.Format("Tried to Resize from dimensions {0} to {1}!", dimensions, newDimensions)
        );

        offset = newOffset;
        dimensions = newDimensions;
    }

    public bool IsEmpty() {
        return !root.HasValue() && (root.Children == null || root.Children.Length == 0);
    }

    public bool TryGetValue(Vector3Int coords, out T value) {
        return TryGetValue(coords, out value, debugDrawCallback: null);
    }

    public bool TryGetValue(Vector3Int coords, out T value, Action<Vector3Int, int, Vector3Int, T, float> debugDrawCallback) {
        if(!TryGetNode(coords.x, coords.y, coords.z, offset, dimensions, root, targetSize: 1, createChildrenIfMissing: false, debugDrawCallback, out Node node)) {
            value = default;
            return false;
        }

        value = node.HasValue() ? node.GetValue() : default;
        return true;
    }

    public bool TryGetNode(Vector3Int coords, out Node node) {
        return TryGetNode(coords.x, coords.y, coords.z, offset, dimensions, root, targetSize: 1, createChildrenIfMissing: false, null, out node);
    }

    public bool TryGetNode(Vector3Int coords, Node startNode, int targetSize, out Node node) {
        return TryGetNode(coords.x, coords.y, coords.z, offset, dimensions, startNode ?? root, targetSize, createChildrenIfMissing: false, null, out node);
    }

    public void SetValue(Vector3Int coords, T value, int size = 1) {
        SetValue(coords.x, coords.y, coords.z, value, size);
    }

    public void SetValue(int x, int y, int z, T value, int size = 1) {
        if(!TryGetNode(x, y, z, offset, dimensions, root, size, createChildrenIfMissing: true, null, out Node node)) {
            return;
        }

        Debug.Assert(node.Size == size);
        node.SetValue(value, informParent: true);
    }

    private static bool TryGetNode(int x, int y, int z, Vector3Int octreeOffset, Vector3Int octreeDimensions, Node startNode, int targetSize, bool createChildrenIfMissing, Action<Vector3Int, int, Vector3Int, T, float> debugDrawCallback, out Node node) {
        node = startNode;

        if(x < 0 || y < 0 || z < 0 || x >= octreeDimensions.x || y >= octreeDimensions.y || z >= octreeDimensions.z) {
            return false;
        }
        
        if(debugDrawCallback != null) {
            debugDrawCallback(node.GetOffset(octreeOffset), node.Size, octreeDimensions, node.GetValue(), 0.1f);
        }

        while(node.Size > targetSize) {
            if(node.HasValue() && !createChildrenIfMissing) {
                break;
            }

            Vector3Int nodeOffset = node.GetOffset(octreeOffset);

            int childSize = node.Size / 2;
            int childLocalCoordsX = x < nodeOffset.x + childSize ? 0 : 1;// (int)Mathf.Clamp01(Mathf.Sign(x - (fixedNodeOffset.x + childSize)));
            int childLocalCoordsY = y < nodeOffset.y + childSize ? 0 : 1;// (int)Mathf.Clamp01(Mathf.Sign(y - (fixedNodeOffset.y + childSize)));
            int childLocalCoordsZ = z < nodeOffset.z + childSize ? 0 : 1;// (int)Mathf.Clamp01(Mathf.Sign(z - (fixedNodeOffset.z + childSize)));

            bool hasChildren = node.Children != null;
            if(!hasChildren && !createChildrenIfMissing) {
                return true;
            }

            int childSiblingIndex = VoxelGrid.CoordsToIndex(childLocalCoordsX, childLocalCoordsY, childLocalCoordsZ, width: 2);
            if(!hasChildren) {
                node.SetupChildren();
            }

            node = node.Children[childSiblingIndex];

            if(debugDrawCallback != null) {
                debugDrawCallback(node.GetOffset(octreeOffset), node.Size, octreeDimensions, node.GetValue(), 0.1f);
            }
        }

        return true;
    }


    public bool TryGetAdjacentNodes(Node origin, Direction direction, out Node[] neighbors, out int neighborCount) {
        return TryGetAdjacentNodes(this, origin, direction, out neighbors, out neighborCount);
    }

    internal static bool TryGetAdjacentNodes(Octree<T> octree, Node origin, Direction direction, out Node[] neighbors, out int neighborCount) {
        Node searchStartNode = null;

        if(origin.Parent != null) {
            // TODO: test performance if we recurse upwards and run this on all the ancestors
            // TODO: also try just removing this, see what that does

            Vector3Int localCoords = LOCAL_COORDS_LOOKUP[origin.SiblingIndex];

            if(direction == Direction.Right && localCoords.x == 0 ||
               direction == Direction.Left && localCoords.x == 1 ||
               direction == Direction.Up && localCoords.y == 0 ||
               direction == Direction.Down && localCoords.y == 1 ||
               direction == Direction.Fore && localCoords.z == 0 ||
               direction == Direction.Back && localCoords.z == 1) {
                searchStartNode = origin.Parent;
            }
        }

        Vector3Int coords = origin.GetNeighborNodeOffset(octree.offset, direction);

        Node neighbor;
        bool success = octree.TryGetNode(
            coords,
            searchStartNode,
            targetSize: origin.Size,
            out neighbor
        );

        if(!success) {
            neighbors = null;
            neighborCount = 0;
            return false;
        }

        Debug.Assert(neighbor != origin);

        Direction face = Utils.GetOppositeDirection(direction);

        Queue<Node> nodesToVisit = new Queue<Node>();
        nodesToVisit.Enqueue(neighbor);

        neighbors = new Node[origin.Size * origin.Size];
        neighborCount = 0;

        while(nodesToVisit.Count > 0) {
            Node node = nodesToVisit.Dequeue();

            if(node.Children == null) {
                if(node.HasValue()) {
                    neighbors[neighborCount] = node;
                    ++neighborCount;
                }
                
                continue;
            }

            Node.GetChildIndexesFacingDirection(face, out int childIndex0, out int childIndex1, out int childIndex2, out int childIndex3);
            
            nodesToVisit.Enqueue(node.Children[childIndex0]);
            nodesToVisit.Enqueue(node.Children[childIndex1]);
            nodesToVisit.Enqueue(node.Children[childIndex2]);
            nodesToVisit.Enqueue(node.Children[childIndex3]);
        }

        return true;
    }
}
