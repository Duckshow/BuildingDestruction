using UnityEngine;
using System.Collections.Generic;

public static class VoxelMeshFactory
{
	private const float VOXEL_RADIUS = 0.5f;
	private static readonly Vector3 LEFT = Vector3.left * VOXEL_RADIUS;
	private static readonly Vector3 RIGHT = Vector3.right * VOXEL_RADIUS;
	private static readonly Vector3 FORE = Vector3.forward * VOXEL_RADIUS;
	private static readonly Vector3 BACK = Vector3.back * VOXEL_RADIUS;
	private static readonly Vector3 UP = Vector3.up * VOXEL_RADIUS;
	private static readonly Vector3 DOWN = Vector3.down * VOXEL_RADIUS;

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
			case Direction.Right: return VERTS_RIGHT;
			case Direction.Left: return VERTS_LEFT;
			case Direction.Up: return VERTS_UP;
			case Direction.Down: return VERTS_DOWN;
			case Direction.Fore: return VERTS_FORE;
			case Direction.Back: return VERTS_BACK;
			default: return null;
		}
	}

	private static Dictionary<ulong, Mesh> cachedMeshes = new Dictionary<ulong, Mesh>();

	public static bool TryGetMesh(Bin bin, out Mesh mesh) {
        if(bin == null) {
			mesh = null;
			return false;
        }

		ulong id = GetID(bin);

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
		for(int i = 0; i < Bin.SIZE; i++) {
			if(!bin.GetVoxelExists(i)) {
				continue;
            }

			if(!bin.GetVoxelHasNeighbor(i, Direction.Right))	{ faceCount++; }
			if(!bin.GetVoxelHasNeighbor(i, Direction.Left))		{ faceCount++; }
			if(!bin.GetVoxelHasNeighbor(i, Direction.Up))		{ faceCount++; }
			if(!bin.GetVoxelHasNeighbor(i, Direction.Down))		{ faceCount++; }
			if(!bin.GetVoxelHasNeighbor(i, Direction.Fore))		{ faceCount++; }
			if(!bin.GetVoxelHasNeighbor(i, Direction.Back))		{ faceCount++; }
		}

        if(faceCount == 0) {
			return null;
        }

		int vertCount = FaceCountToVertCount(faceCount);
		int triCount = FaceCountToTriCount(faceCount);

		Vector3[] verts = new Vector3[vertCount];
		Vector2[] uvs = new Vector2[vertCount];
		int[] tris = new int[triCount];

		int faceIndex = 0;
		int triIndex = 0;

		for(int i = 0; i < Bin.SIZE; i++) {
			if(!bin.GetVoxelExists(i)) {
				continue;
			}

			Vector3Int localCoords = Bin.GetVoxelLocalCoords(i);

			if(!bin.GetVoxelHasNeighbor(i, Direction.Right))	{ AddFace(localCoords, Direction.Right,	ref faceIndex, ref triIndex, verts, uvs, tris); }
			if(!bin.GetVoxelHasNeighbor(i, Direction.Left))		{ AddFace(localCoords, Direction.Left,	ref faceIndex, ref triIndex, verts, uvs, tris); }
			if(!bin.GetVoxelHasNeighbor(i, Direction.Up))		{ AddFace(localCoords, Direction.Up,	ref faceIndex, ref triIndex, verts, uvs, tris); }
			if(!bin.GetVoxelHasNeighbor(i, Direction.Down))		{ AddFace(localCoords, Direction.Down,	ref faceIndex, ref triIndex, verts, uvs, tris); }
			if(!bin.GetVoxelHasNeighbor(i, Direction.Fore))		{ AddFace(localCoords, Direction.Fore,	ref faceIndex, ref triIndex, verts, uvs, tris); }
			if(!bin.GetVoxelHasNeighbor(i, Direction.Back))		{ AddFace(localCoords, Direction.Back,	ref faceIndex, ref triIndex, verts, uvs, tris); }
		}

		return AssembleMesh(verts, uvs, tris);
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

	private static ulong GetID(Bin bin) {
		ulong id = 0;

		for(int i = 0; i < Bin.SIZE; i++) {
			const int SIDES = 6;
			int offset = i * SIDES;

			if(!bin.GetVoxelExists(i)) {
				id |= 0ul << offset + 0;
				id |= 0ul << offset + 1;
				id |= 0ul << offset + 2;
				id |= 0ul << offset + 3;
				id |= 0ul << offset + 4;
				id |= 0ul << offset + 5;
			}
			else {
				id |= (bin.GetVoxelHasNeighbor(i, Direction.Right)	?	1ul : 0ul) << offset + 0;
				id |= (bin.GetVoxelHasNeighbor(i, Direction.Left)	?	1ul : 0ul) << offset + 1;
				id |= (bin.GetVoxelHasNeighbor(i, Direction.Up)		?	1ul : 0ul) << offset + 2;
				id |= (bin.GetVoxelHasNeighbor(i, Direction.Down)	?	1ul : 0ul) << offset + 3;
				id |= (bin.GetVoxelHasNeighbor(i, Direction.Fore)	?	1ul : 0ul) << offset + 4;
				id |= (bin.GetVoxelHasNeighbor(i, Direction.Back)	?	1ul : 0ul) << offset + 5;
			}
		}

		return id;
	}

	private static int FaceCountToVertCount(int faceCount) { return faceCount * 4; }
	private static int FaceCountToTriCount(int faceCount) { return faceCount * 6; }

	public static void RunTests() {
		TestGetMesh();
		TestGetID();
		Debug.Log("Tests done.");
	}

	private static void TestGetMesh() {
		Bin bin = new Bin(0, Vector3Int.one);
		bin.SetAllVoxelExists(true);

		ulong id = GetID(bin);

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

	private static void TestGetID() {
		Bin GetNewBin(bool isFilled, bool hasNeighborRight, bool hasNeighborLeft, bool hasNeighborUp, bool hasNeighborDown, bool hasNeighborFore, bool hasNeighborBack) {
			Bin bin = new Bin(0, Vector3Int.one);
			bin.SetAllVoxelExists(isFilled);

			Bin binRight	= new Bin(0, Vector3Int.one);
			Bin binLeft		= new Bin(0, Vector3Int.one);
			Bin binUp		= new Bin(0, Vector3Int.one);
			Bin binDown		= new Bin(0, Vector3Int.one);
			Bin binFore		= new Bin(0, Vector3Int.one);
			Bin binBack		= new Bin(0, Vector3Int.one);

			binRight.SetAllVoxelExists(hasNeighborRight);
			binLeft.SetAllVoxelExists(hasNeighborLeft);
			binUp.SetAllVoxelExists(hasNeighborUp);
			binDown.SetAllVoxelExists(hasNeighborDown);
			binFore.SetAllVoxelExists(hasNeighborFore);
			binBack.SetAllVoxelExists(hasNeighborBack);

			bin.RefreshConnectivity(binRight, binLeft, binUp, binDown, binFore, binBack);
			return bin;
		}

		Bin bin;

		bin = GetNewBin(isFilled: true, hasNeighborRight: false, hasNeighborLeft: false, hasNeighborUp: false, hasNeighborDown: false, hasNeighborFore: false, hasNeighborBack: false);
		Debug.LogFormat("GetID Results, No neighbors: " + System.Convert.ToString((long)GetID(bin), 2));

		bin = GetNewBin(isFilled: true, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true);
		Debug.LogFormat("GetID Results, All neighbors: " + System.Convert.ToString((long)GetID(bin), 2));

		bin = GetNewBin(isFilled: true, hasNeighborRight: false, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true);
		Debug.LogFormat("GetID Results: No neighbors right" + System.Convert.ToString((long)GetID(bin), 2));

		bin = GetNewBin(isFilled: true, hasNeighborRight: true, hasNeighborLeft: false, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true);
		Debug.LogFormat("GetID Results, No neighbors left: " + System.Convert.ToString((long)GetID(bin), 2));

		bin = GetNewBin(isFilled: true, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: false, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true);
		Debug.LogFormat("GetID Results, No neighbors up: " + System.Convert.ToString((long)GetID(bin), 2));

		bin = GetNewBin(isFilled: true, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: false, hasNeighborFore: true, hasNeighborBack: true);
		Debug.LogFormat("GetID Results, No neighbors down: " + System.Convert.ToString((long)GetID(bin), 2));

		bin = GetNewBin(isFilled: true, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: false, hasNeighborBack: true);
		Debug.LogFormat("GetID Results, No neighbors fore: " + System.Convert.ToString((long)GetID(bin), 2));

		bin = GetNewBin(isFilled: true, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: false);
		Debug.LogFormat("GetID Results, No neighbors back: " + System.Convert.ToString((long)GetID(bin), 2));

		bin = GetNewBin(isFilled: false, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true);
		Debug.LogFormat("GetID Results, No filled: " + System.Convert.ToString((long)GetID(bin), 2));
	}
}
