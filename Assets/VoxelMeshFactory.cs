using System.Collections.Generic;
using UnityEngine;
using MonsterLove.Collections;

public static class VoxelMeshFactory {
    private const float VOXEL_RADIUS = 0.5f;
    private static readonly Vector3 LEFT = Vector3.left * VOXEL_RADIUS;
    private static readonly Vector3 RIGHT = Vector3.right * VOXEL_RADIUS;
    private static readonly Vector3 FORE = Vector3.forward * VOXEL_RADIUS;
    private static readonly Vector3 BACK = Vector3.back * VOXEL_RADIUS;
    private static readonly Vector3 UP = Vector3.up * VOXEL_RADIUS;
    private static readonly Vector3 DOWN = Vector3.down * VOXEL_RADIUS;

    private const int MAX_ESTIMATED_FACE_COUNT = 24;
    private const int MAX_ESTIMATED_VERT_COUNT = MAX_ESTIMATED_FACE_COUNT * 4;
    private const int MAX_ESTIMATED_TRI_COUNT = MAX_ESTIMATED_FACE_COUNT * 6;
    private static ObjectPool<Vector3[]> vertsPool   = new ObjectPool<Vector3[]>(() => new Vector3[MAX_ESTIMATED_VERT_COUNT], 1);
    private static ObjectPool<Vector2[]> uvsPool     = new ObjectPool<Vector2[]>(() => new Vector2[MAX_ESTIMATED_VERT_COUNT], 1);
    private static ObjectPool<int[]> trisPool        = new ObjectPool<int[]>(() => new int[MAX_ESTIMATED_TRI_COUNT], 1);

    private static readonly Vector3[] VERTS_UP = new Vector3[] {
        LEFT + BACK + UP,
        LEFT + FORE + UP,
        RIGHT + FORE + UP,
        RIGHT + BACK + UP
    };

    private static readonly Vector3[] VERTS_DOWN = new Vector3[] {
        LEFT + FORE + DOWN,
        LEFT + BACK + DOWN,
        RIGHT + BACK + DOWN,
        RIGHT + FORE + DOWN
    };

    private static readonly Vector3[] VERTS_LEFT = new Vector3[] {
        LEFT + FORE + DOWN,
        LEFT + FORE + UP,
        LEFT + BACK + UP,
        LEFT + BACK + DOWN
    };

    private static readonly Vector3[] VERTS_RIGHT = new Vector3[] {
        RIGHT + BACK + DOWN,
        RIGHT + BACK + UP,
        RIGHT + FORE + UP,
        RIGHT + FORE + DOWN
    };

    private static readonly Vector3[] VERTS_FORE = new Vector3[] {
        RIGHT + FORE + DOWN,
        RIGHT + FORE + UP,
        LEFT + FORE + UP,
        LEFT + FORE + DOWN
    };

    private static readonly Vector3[] VERTS_BACK = new Vector3[] {
        LEFT + BACK + DOWN,
        LEFT + BACK + UP,
        RIGHT + BACK + UP,
        RIGHT + BACK + DOWN
    };

    private static Vector3[] GetVertsForFace(Direction dir) {
        switch(dir) {
            case Direction.Right:
                return VERTS_RIGHT;
            case Direction.Left:
                return VERTS_LEFT;
            case Direction.Up:
                return VERTS_UP;
            case Direction.Down:
                return VERTS_DOWN;
            case Direction.Fore:
                return VERTS_FORE;
            case Direction.Back:
                return VERTS_BACK;
            default:
                return null;
        }
    }

    private static Dictionary<ulong, Mesh> cachedMeshes = new Dictionary<ulong, Mesh>();

    public static bool TryGetMesh(Bin bin, out Mesh mesh) {
        if(bin.IsWholeBinEmpty()) {
            mesh = null;
            return false;
        }

        ulong id = Bin.GetVisualID(bin);

        if(ShouldUseCachedMesh(id)) {
            mesh = cachedMeshes[id];
            return mesh != null;
        }

        mesh = ConstructNewMesh(bin);
        cachedMeshes.Add(id, mesh);

        return mesh != null;
    }

    private static bool ShouldUseCachedMesh(ulong id) {
        return cachedMeshes.ContainsKey(id);
    }

    private static Mesh ConstructNewMesh(Bin bin) { // TODO: come up with a solution for not having two for-loops
        if(bin.IsWholeBinEmpty()) {
            return null;
        }

        int faceCount = 0;
        bool[] voxelHasNeighbors = new bool[Bin.SIZE * Bin.FACES];

        for(int i = 0; i < Bin.SIZE; i++) {
            if(!bin.GetVoxelExists(i)) {
                continue;
            }

            bool hasNeighborRight = bin.GetVoxelHasNeighbor(i, Direction.Right);
            bool hasNeighborLeft = bin.GetVoxelHasNeighbor(i, Direction.Left);
            bool hasNeighborUp = bin.GetVoxelHasNeighbor(i, Direction.Up);
            bool hasNeighborDown = bin.GetVoxelHasNeighbor(i, Direction.Down);
            bool hasNeighborFore = bin.GetVoxelHasNeighbor(i, Direction.Fore);
            bool hasNeighborBack = bin.GetVoxelHasNeighbor(i, Direction.Back);

            if(hasNeighborRight) { voxelHasNeighbors[i * Bin.FACES + 0] = true; }
            else { ++faceCount; }
            if(hasNeighborLeft) { voxelHasNeighbors[i * Bin.FACES + 1] = true; }
            else { ++faceCount; }
            if(hasNeighborUp) { voxelHasNeighbors[i * Bin.FACES + 2] = true; }
            else { ++faceCount; }
            if(hasNeighborDown) { voxelHasNeighbors[i * Bin.FACES + 3] = true; }
            else { ++faceCount; }
            if(hasNeighborFore) { voxelHasNeighbors[i * Bin.FACES + 4] = true; }
            else { ++faceCount; }
            if(hasNeighborBack) { voxelHasNeighbors[i * Bin.FACES + 5] = true; }
            else { ++faceCount; }
        }

        if(faceCount == 0) {
            return null;
        }

        Vector3[] verts = vertsPool.GetItem();
        Vector2[] uvs = uvsPool.GetItem();
        int[] tris = trisPool.GetItem();

        int faceIndex = 0;
        int triIndex = 0;
        for(int i = 0; i < Bin.SIZE; i++) {
            if(!bin.GetVoxelExists(i)) {
                continue;
            }

            Vector3Int localCoords = Bin.GetVoxelLocalCoords(i);

            if(!voxelHasNeighbors[i * Bin.FACES + 0]) { AddFace(localCoords, Direction.Right, ref faceIndex, ref triIndex, verts, uvs, tris); }
            if(!voxelHasNeighbors[i * Bin.FACES + 1]) { AddFace(localCoords, Direction.Left, ref faceIndex, ref triIndex, verts, uvs, tris); }
            if(!voxelHasNeighbors[i * Bin.FACES + 2]) { AddFace(localCoords, Direction.Up, ref faceIndex, ref triIndex, verts, uvs, tris); }
            if(!voxelHasNeighbors[i * Bin.FACES + 3]) { AddFace(localCoords, Direction.Down, ref faceIndex, ref triIndex, verts, uvs, tris); }
            if(!voxelHasNeighbors[i * Bin.FACES + 4]) { AddFace(localCoords, Direction.Fore, ref faceIndex, ref triIndex, verts, uvs, tris); }
            if(!voxelHasNeighbors[i * Bin.FACES + 5]) { AddFace(localCoords, Direction.Back, ref faceIndex, ref triIndex, verts, uvs, tris); }
        }


        int actualVertCount = FaceCountToVertCount(faceCount);
        for(int i = actualVertCount; i < verts.Length; i++) {
            verts[i] = new Vector3();
            uvs[i] = new Vector2();
        }

        int actualTriCount = FaceCountToTriCount(faceCount);
        for(int i = actualTriCount; i < tris.Length; i++) {
            tris[i] = 0;
        }

        Mesh mesh = AssembleMesh(verts, uvs, tris);

        vertsPool.ReleaseItem(verts);
        uvsPool.ReleaseItem(uvs);
        trisPool.ReleaseItem(tris);

        return mesh;
    }

    private static void AddFace(Vector3Int voxelLocalCoords, Direction dir, ref int faceIndex, ref int triIndex, Vector3[] verts, Vector2[] uvs, int[] tris) {
        Vector3[] faceVerts = GetVertsForFace(dir);

        verts[faceIndex + 0] = voxelLocalCoords + faceVerts[0];
        verts[faceIndex + 1] = voxelLocalCoords + faceVerts[1];
        verts[faceIndex + 2] = voxelLocalCoords + faceVerts[2];
        verts[faceIndex + 3] = voxelLocalCoords + faceVerts[3];

        uvs[faceIndex + 0] = new Vector2(0, 0);
        uvs[faceIndex + 1] = new Vector2(0, 1);
        uvs[faceIndex + 2] = new Vector2(1, 1);
        uvs[faceIndex + 3] = new Vector2(1, 0);

        tris[triIndex + 0] = faceIndex + 0;
        tris[triIndex + 1] = faceIndex + 1;
        tris[triIndex + 2] = faceIndex + 2;
        tris[triIndex + 3] = faceIndex + 0;
        tris[triIndex + 4] = faceIndex + 2;
        tris[triIndex + 5] = faceIndex + 3;

        faceIndex += 4;
        triIndex += 6;
    }

    private static Mesh AssembleMesh(Vector3[] verts, Vector2[] uvs, int[] tris) {
        if(verts.Length == 0) {
            return null;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.Optimize();
        mesh.RecalculateNormals();

        return mesh;
    }

    private static int FaceCountToVertCount(int faceCount) { return faceCount * 4; }
    private static int FaceCountToTriCount(int faceCount) { return faceCount * 6; }

    public static void RunTests() {
        TestGetMesh();
        Debug.Log("Tests done.");
    }

    private static void TestGetMesh() {
        Bin bin = new Bin(0, Vector3Int.one);
        bin = Bin.SetBinAllVoxelsExists(bin, true);

        ulong id = Bin.GetVisualID(bin);

        cachedMeshes.Clear();

        Debug.Assert(ShouldUseCachedMesh(id) == false);
        TryGetMesh(bin, out Mesh mesh1);
        Debug.Assert(ShouldUseCachedMesh(id) == true);
        TryGetMesh(bin, out Mesh mesh2);
        Debug.Assert(ShouldUseCachedMesh(id) == true);

        Debug.Assert(mesh1 != null);
        Debug.Assert(mesh2 != null);
        Debug.Assert(mesh1 == mesh2);

        cachedMeshes.Clear();
    }
}
