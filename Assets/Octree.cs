using System;
using System.Collections.Generic;
using UnityEngine;

public partial class Octree<T> where T : IEquatable<T> {
    public class Node<U> where U : IEquatable<U> {
        public Node<U> Parent;
        public Node<U>[] Children;
        public int SiblingIndex = -1;

        private U value;

        public Node(Node<U> parent, int siblingIndex, U value) {
            Parent = parent;
            SiblingIndex = siblingIndex;
            SetValue(value, informParent: false);
        }

        public bool HasValue() {
            return !EqualityComparer<U>.Default.Equals(value, default);
        }

        public U GetValue() {
            return value;
        }

        public void SetValue(U value, bool informParent) {
            this.value = value;
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

        public void SetupChildren() {
            Children = new Node<U>[CHILDREN_PER_PARENT];

            for(int i = 0; i < CHILDREN_PER_PARENT; i++) {
                Children[i] = new Node<U>(this, i, value);
            }

            value = default;
        }

        public static bool CanSummarizeChildren(Node<U> parent) {
            Node<U> firstChild = parent.Children[0];

            if(firstChild != null && firstChild.Children != null) {
                return false;
            }

            for(int i = 1; i < CHILDREN_PER_PARENT; i++) {
                Node<U> currentChild = parent.Children[i];

                if(currentChild.Children != null) {
                    return false;
                }

                if(!EqualityComparer<U>.Default.Equals(firstChild.value, currentChild.value)) {
                    return false;
                }
            }

            return true;
        }
    }

    public const int CHILDREN_PER_PARENT = 8;

    private int size;
    private Node<T> root;

    public Octree(int size) {
        if(size == 0 || (size & (size - 1)) != 0) {
            throw new Exception("Octree can only use sizes that are a power-of-two! Input: " + size);
        }

        this.size = size;
        root = new Node<T>(null, -1, default);
    }

    public bool TryGetValue(int x, int y, int z, out T value, Action<int, int, int, int, int, T> debugDrawCallback) {
        Node<T> node = TryGetNode(x, y, z, size, root, createChildrenIfMissing: false, debugDrawCallback);

        bool success = node != null && node.HasValue();
        value = success ? node.GetValue() : default;
        return success;
    }

    public void SetValue(int x, int y, int z, T value) {
        Node<T> node = TryGetNode(x, y, z, size, root, createChildrenIfMissing: true, debugDrawCallback: null);
        if(node == null) {
            return;
        }

        node.SetValue(value, informParent: true);
    }

    private static Node<T> TryGetNode(int x, int y, int z, int treeSize, Node<T> root, bool createChildrenIfMissing, Action<int, int, int, int, int, T> debugDrawCallback) {
        if(x < 0 || y < 0 || z < 0 || x >= treeSize || y >= treeSize || z >= treeSize) {
            return null;
        }

        Node<T> node = root;

        int nodeSize = treeSize;
        int childSize = nodeSize / 2;

        int nodeOffsetX = 0;
        int nodeOffsetY = 0;
        int nodeOffsetZ = 0;

        if(debugDrawCallback != null) {
            debugDrawCallback(nodeOffsetX, nodeOffsetY, nodeOffsetZ, nodeSize, treeSize, node.GetValue());
        }

        while(nodeSize > 1) {
            if(node.HasValue() && !createChildrenIfMissing) {
                break;
            }

            int childLocalCoordsX = (int)Mathf.Clamp01(Mathf.Sign(x - (nodeOffsetX + childSize)));
            int childLocalCoordsY = (int)Mathf.Clamp01(Mathf.Sign(y - (nodeOffsetY + childSize)));
            int childLocalCoordsZ = (int)Mathf.Clamp01(Mathf.Sign(z - (nodeOffsetZ + childSize)));

            bool hasChildren = node.Children != null;
            if(!hasChildren && !createChildrenIfMissing) {
                return node;
            }

            int childSiblingIndex = VoxelGrid.CoordsToIndex(childLocalCoordsX, childLocalCoordsY, childLocalCoordsZ, width: 2);
            if(!hasChildren) {
                node.SetupChildren();
            }

            node = node.Children[childSiblingIndex];

            nodeOffsetX += childSize * childLocalCoordsX;
            nodeOffsetY += childSize * childLocalCoordsY;
            nodeOffsetZ += childSize * childLocalCoordsZ;

            nodeSize = childSize;
            childSize = nodeSize / 2;

            if(debugDrawCallback != null) {
                debugDrawCallback(nodeOffsetX, nodeOffsetY, nodeOffsetZ, nodeSize, treeSize, node.GetValue());
            }
        }

        return node;
    }
}
