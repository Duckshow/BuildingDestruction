using UnityEngine;
using System.Collections.Generic;

public static class VoxelMeshFactory
{
	private enum Direction { Right, Left, Up, Down, Fore, Back }

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

	public static bool TryGetMesh(int index, Voxel?[] binVoxels, out Mesh mesh) {
		ulong id = GetID(binVoxels);

		if(ShouldUseCachedMesh(id)) {
			mesh = cachedMeshes[id];
			return mesh != null;
		}

		mesh = ConstructNewMesh(binVoxels);
		cachedMeshes.Add(id, mesh);

		return mesh != null;
	}

	private static bool ShouldUseCachedMesh(ulong id) {
		return cachedMeshes.ContainsKey(id);
	}

	private static Mesh ConstructNewMesh(Voxel?[] binVoxels) { // TODO: come up with a solution for not having two for-loops
		int faceCount = 0;
		for(int i = 0; i < binVoxels.Length; i++) {
			if(!binVoxels[i].HasValue) {
				continue;
            }

			Voxel v = binVoxels[i].Value;
			
			if(!v.IsFilled) {
				continue;
			}

			if(!v.HasNeighborRight) { faceCount++; }
			if(!v.HasNeighborLeft)	{ faceCount++; }
			if(!v.HasNeighborUp)	{ faceCount++; }
			if(!v.HasNeighborDown)	{ faceCount++; }
			if(!v.HasNeighborFore)	{ faceCount++; }
			if(!v.HasNeighborBack)	{ faceCount++; }
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

		Vector3Int[] voxelLocalCoords = Bin.GetContentsLocalCoords();

		for(int i = 0; i < binVoxels.Length; i++) {
			if(!binVoxels[i].HasValue) {
				continue;
			}

			Voxel v = binVoxels[i].Value;

			if(!v.IsFilled) {
				continue;
			}

			if(!v.HasNeighborRight) { AddFace(voxelLocalCoords[i], Direction.Right,	ref faceIndex, ref triIndex, verts, uvs, tris); }
			if(!v.HasNeighborLeft)	{ AddFace(voxelLocalCoords[i], Direction.Left,	ref faceIndex, ref triIndex, verts, uvs, tris); }
			if(!v.HasNeighborUp)	{ AddFace(voxelLocalCoords[i], Direction.Up,	ref faceIndex, ref triIndex, verts, uvs, tris); }
			if(!v.HasNeighborDown)	{ AddFace(voxelLocalCoords[i], Direction.Down,	ref faceIndex, ref triIndex, verts, uvs, tris); }
			if(!v.HasNeighborFore)	{ AddFace(voxelLocalCoords[i], Direction.Fore,	ref faceIndex, ref triIndex, verts, uvs, tris); }
			if(!v.HasNeighborBack)	{ AddFace(voxelLocalCoords[i], Direction.Back,	ref faceIndex, ref triIndex, verts, uvs, tris); }
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

	private static ulong GetID(Voxel?[] binVoxels) {
		Debug.Assert(binVoxels.Length == Bin.SIZE);

		ulong id = 0;

		for(int i = 0; i < Bin.SIZE; i++) {
			Voxel? v = binVoxels[i];

			const int SIDES = 6;
			int offset = i * SIDES;

			if(!v.HasValue || !v.Value.IsFilled) {
				id |= 0ul << offset + 0;
				id |= 0ul << offset + 1;
				id |= 0ul << offset + 2;
				id |= 0ul << offset + 3;
				id |= 0ul << offset + 4;
				id |= 0ul << offset + 5;
			}
			else {
				id |= (v.Value.HasNeighborRight ?	1ul : 0ul) << offset + 0;
				id |= (v.Value.HasNeighborLeft ?	1ul : 0ul) << offset + 1;
				id |= (v.Value.HasNeighborUp ?		1ul : 0ul) << offset + 2;
				id |= (v.Value.HasNeighborDown ?	1ul : 0ul) << offset + 3;
				id |= (v.Value.HasNeighborFore ?	1ul : 0ul) << offset + 4;
				id |= (v.Value.HasNeighborBack ?	1ul : 0ul) << offset + 5;
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
		Voxel?[] voxels = new Voxel?[Bin.SIZE];
		Vector3Int dimensions = new Vector3Int(Bin.WIDTH, Bin.WIDTH, Bin.WIDTH);

        for(int z = 0; z < Bin.WIDTH; z++) {
            for(int y = 0; y < Bin.WIDTH; y++) {
                for(int x = 0; x < Bin.WIDTH; x++) {
					Vector3Int coords = new Vector3Int(x, y, z);
					int i = VoxelGrid.CoordsToIndex(coords, dimensions);

					voxels[i] = new Voxel(
						i,
						coords,
						isFilled: true, 
						hasNeighborRight: x < Bin.WIDTH - 1,
						hasNeighborLeft: x > 0,
						hasNeighborUp: y < Bin.WIDTH - 1,
						hasNeighborDown: y > 0,
						hasNeighborFore: z < Bin.WIDTH - 1,
						hasNeighborBack: z > 0
						);
				}
            }
        }

		ulong id = GetID(voxels);

		cachedMeshes.Clear();

		Debug.Assert(ShouldUseCachedMesh(id) == false);
		TryGetMesh(0, voxels, out Mesh mesh1);
		Debug.Assert(ShouldUseCachedMesh(id) == true);
		TryGetMesh(0, voxels, out Mesh mesh2);
		Debug.Assert(ShouldUseCachedMesh(id) == true);

		Debug.Assert(mesh1 != null);
		Debug.Assert(mesh2 != null);
		Debug.Assert(mesh1 == mesh2);

		cachedMeshes.Clear();
	}

	private static void TestGetID() {
		Voxel?[] voxels;

		Voxel?[] GetVoxels(bool isFilled, bool hasNeighborRight, bool hasNeighborLeft, bool hasNeighborUp, bool hasNeighborDown, bool hasNeighborFore, bool hasNeighborBack) { 
			return new Voxel?[] {
				new Voxel(0, new Vector3Int(0, 0, 0), isFilled, hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack),
				new Voxel(1, new Vector3Int(1, 0, 0), isFilled, hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack),
				new Voxel(2, new Vector3Int(0, 1, 0), isFilled, hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack),
				new Voxel(3, new Vector3Int(1, 1, 0), isFilled, hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack),
				new Voxel(4, new Vector3Int(0, 0, 1), isFilled, hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack),
				new Voxel(5, new Vector3Int(1, 0, 1), isFilled, hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack),
				new Voxel(6, new Vector3Int(0, 1, 1), isFilled, hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack),
				new Voxel(7, new Vector3Int(1, 1, 1), isFilled, hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack)
			};
		}

		voxels = GetVoxels(isFilled: true, hasNeighborRight: false, hasNeighborLeft: false, hasNeighborUp: false, hasNeighborDown: false, hasNeighborFore: false, hasNeighborBack: false);
		Debug.LogFormat("GetID Results, No neighbors: " + System.Convert.ToString((long)GetID(voxels), 2));

		voxels = GetVoxels(isFilled: true, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true);
		Debug.LogFormat("GetID Results, All neighbors: " + System.Convert.ToString((long)GetID(voxels), 2));

		voxels = GetVoxels(isFilled: true, hasNeighborRight: false, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true);
		Debug.LogFormat("GetID Results: No neighbors right" + System.Convert.ToString((long)GetID(voxels), 2));

		voxels = GetVoxels(isFilled: true, hasNeighborRight: true, hasNeighborLeft: false, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true);
		Debug.LogFormat("GetID Results, No neighbors left: " + System.Convert.ToString((long)GetID(voxels), 2));

		voxels = GetVoxels(isFilled: true, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: false, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true);
		Debug.LogFormat("GetID Results, No neighbors up: " + System.Convert.ToString((long)GetID(voxels), 2));

		voxels = GetVoxels(isFilled: true, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: false, hasNeighborFore: true, hasNeighborBack: true);
		Debug.LogFormat("GetID Results, No neighbors down: " + System.Convert.ToString((long)GetID(voxels), 2));

		voxels = GetVoxels(isFilled: true, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: false, hasNeighborBack: true);
		Debug.LogFormat("GetID Results, No neighbors fore: " + System.Convert.ToString((long)GetID(voxels), 2));

		voxels = GetVoxels(isFilled: true, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: false);
		Debug.LogFormat("GetID Results, No neighbors back: " + System.Convert.ToString((long)GetID(voxels), 2));

		voxels = GetVoxels(isFilled: false, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true);
		Debug.LogFormat("GetID Results, No filled: " + System.Convert.ToString((long)GetID(voxels), 2));

		voxels = new Voxel?[] {
			new Voxel(0, new Vector3Int(0, 0, 0), isFilled: false,	hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true),
			new Voxel(1, new Vector3Int(1, 0, 0), isFilled: true,	hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true),
			new Voxel(2, new Vector3Int(0, 1, 0), isFilled: false,	hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true),
			new Voxel(3, new Vector3Int(1, 1, 0), isFilled: true,	hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true),
			new Voxel(4, new Vector3Int(0, 0, 1), isFilled: false,	hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true),
			new Voxel(5, new Vector3Int(1, 0, 1), isFilled: true,	hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true),
			new Voxel(6, new Vector3Int(0, 1, 1), isFilled: false,	hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true),
			new Voxel(7, new Vector3Int(1, 1, 1), isFilled: true,	hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true)
		};
		Debug.LogFormat("GetID Results, Every second filled: " + System.Convert.ToString((long)GetID(voxels), 2));

		voxels = new Voxel?[] {
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
		};
		Debug.LogFormat("GetID Results, All null: " + System.Convert.ToString((long)GetID(voxels), 2));

		voxels = new Voxel?[] {
			null,
			new Voxel(1, new Vector3Int(1, 0, 0), isFilled: true, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true),
			null,
			new Voxel(3, new Vector3Int(1, 1, 0), isFilled: true, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true),
			null,
			new Voxel(5, new Vector3Int(1, 0, 1), isFilled: true, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true),
			null,
			new Voxel(7, new Vector3Int(1, 1, 1), isFilled: true, hasNeighborRight: true, hasNeighborLeft: true, hasNeighborUp: true, hasNeighborDown: true, hasNeighborFore: true, hasNeighborBack: true)
		};
		Debug.LogFormat("GetID Results, Every second null: " + System.Convert.ToString((long)GetID(voxels), 2));
	}
}
