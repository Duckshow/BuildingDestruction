using System.Collections.Generic;
using UnityEngine;

public class VoxelBuilder {
	private enum Direction { Up, Down, Left, Right, Fore, Back }

	private static GameObject meshObjectPrefab;

	private VoxelGrid voxelGrid;

	private Material material;

	private MeshObject[] meshObjects;

	public static void SetMeshObjectPrefab(GameObject prefab) {
		meshObjectPrefab = prefab;
	}

    public VoxelBuilder(VoxelGrid voxelGrid, Material material) {
		this.voxelGrid = voxelGrid;
		this.material = material;

        if(!PoolManager.HasPoolForPrefab(meshObjectPrefab)) {
			PoolManager.WarmPool(meshObjectPrefab, 5000); // TODO: let's make one pool for each combination of possible mesh - that way we never have to rebuild a mesh from scratch!
		}
	}

	public void BuildBinGridMeshes(Bin[] bins, Vector3Int offset) {
        if(meshObjects != null) {
            for(int i = 0; i < meshObjects.Length; i++) {
                if(meshObjects[i] != null) {
					PoolManager.ReleaseObject(meshObjects[i].gameObject);
					meshObjects[i] = null;
				}
			}
        }

		meshObjects = new MeshObject[bins.Length];

		Color clusterColor = new Color(Random.value, Random.value, Random.value, 1f);
		for(int i = 0; i < bins.Length; i++) {
			ApplyBinMesh(bins[i], clusterColor, offset);
        }
	}

	private void ApplyBinMesh(Bin bin, Color color, Vector3Int offset) {
		Vector3Int[] binContents = bin.GetContentCoords();

		List<Vector3> verts = new List<Vector3>();
		List<Color> colors = new List<Color>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> tris = new List<int>();

		for(int i = 0; i < binContents.Length; i++) {
			Vector3Int coords = binContents[i];

			Voxel v;
            if(!Voxel.TryGetVoxel(coords, voxelGrid, out v)) {
				continue;
            }

			if(!v.IsFilled) {
				continue;
			}

			Vector3Int localPos = coords;

			if(!v.HasNeighborRight) {
				AddFace(localPos, Direction.Right, color, verts, colors, uvs, tris);
			}
			if(!v.HasNeighborLeft) {
				AddFace(localPos, Direction.Left, color, verts, colors, uvs, tris);
			}
			if(!v.HasNeighborUp) {
				AddFace(localPos, Direction.Up, color, verts, colors, uvs, tris);
			}
			if(!v.HasNeighborDown) {
				AddFace(localPos, Direction.Down, color, verts, colors, uvs, tris);
			}
			if(!v.HasNeighborFore) {
				AddFace(localPos, Direction.Fore, color, verts, colors, uvs, tris);
			}
			if(!v.HasNeighborBack) {
				AddFace(localPos, Direction.Back, color, verts, colors, uvs, tris);
			}
		}

		Mesh mesh = ConstructMesh(verts, colors, uvs, tris);
		if(mesh == null) {
			return;
		}

		Debug.Assert(meshObjects[bin.Index] == null);

		meshObjects[bin.Index] = PoolManager.SpawnObject(meshObjectPrefab, voxelGrid.GetVoxelController().GetMeshTransform(), Vector3.zero, Quaternion.identity).GetComponent<MeshObject>();
		MeshObject meshObject = meshObjects[bin.Index];

		meshObject.transform.name = "MeshObject #" + bin.Index;
		meshObject.SetMesh(mesh);
		meshObject.SetMaterial(material);
	}

	private static void AddFace(Vector3Int coords, Direction dir, Color color, List<Vector3> verts, List<Color> vertColors, List<Vector2> uvs, List<int> tris) {
		const float VOXEL_RADIUS = 0.5f;
		Vector3 left = Vector3.left * VOXEL_RADIUS;
		Vector3 right = Vector3.right * VOXEL_RADIUS;
		Vector3 fore = Vector3.forward * VOXEL_RADIUS;
		Vector3 back = Vector3.back * VOXEL_RADIUS;
		Vector3 up = Vector3.up * VOXEL_RADIUS;
		Vector3 down = Vector3.down * VOXEL_RADIUS;

		Vector3 v0, v1, v2, v3;

		switch(dir) {
			case Direction.Up: {
				v0 = left + back + up;
				v1 = left + fore + up;
				v2 = right + fore + up;
				v3 = right + back + up;
				break;
			}
			case Direction.Down: {
				v0 = left + fore + down;
				v1 = left + back + down;
				v2 = right + back + down;
				v3 = right + fore + down;
				break;
			}
			case Direction.Left: {
				v0 = left + fore + down;
				v1 = left + fore + up;
				v2 = left + back + up;
				v3 = left + back + down;
				break;
			}
			case Direction.Right: {
				v0 = right + back + down;
				v1 = right + back + up;
				v2 = right + fore + up;
				v3 = right + fore + down;
				break;
			}
			case Direction.Fore: {
				v0 = right + fore + down;
				v1 = right + fore + up;
				v2 = left + fore + up;
				v3 = left + fore + down;
				break;
			}
			case Direction.Back: {
				v0 = left + back + down;
				v1 = left + back + up;
				v2 = right + back + up;
				v3 = right + back + down;
				break;
			}
			default: {
				v0 = Vector3.zero;
				v1 = Vector3.zero;
				v2 = Vector3.zero;
				v3 = Vector3.zero;
				break;
			}
		}

		int oldVertCount = verts.Count;

		verts.Add(coords + v0);
		verts.Add(coords + v1);
		verts.Add(coords + v2);
		verts.Add(coords + v3);

		vertColors.Add(color);
		vertColors.Add(color);
		vertColors.Add(color);
		vertColors.Add(color);

		uvs.Add(new Vector2(0, 0));
		uvs.Add(new Vector2(0, 1));
		uvs.Add(new Vector2(1, 1));
		uvs.Add(new Vector2(1, 0));

		tris.Add(oldVertCount + 0);
		tris.Add(oldVertCount + 1);
		tris.Add(oldVertCount + 2);
		tris.Add(oldVertCount + 0);
		tris.Add(oldVertCount + 2);
		tris.Add(oldVertCount + 3);
	}

	private static Mesh ConstructMesh(List<Vector3> verts, List<Color> colors, List<Vector2> uvs, List<int> tris) {
        if(verts.Count == 0) {
			return null;
        }

		Mesh mesh = new Mesh();
		mesh.vertices = verts.ToArray();
		mesh.colors = colors.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = tris.ToArray();
		mesh.Optimize();
		mesh.RecalculateNormals();

		verts.Clear();
		uvs.Clear();
		tris.Clear();

		return mesh;
	}

	private static bool AreCoordsWithinDimensions(Vector3Int coords, Vector3Int dimensions) {
		return coords.x >= 0 && coords.y >= 0 && coords.z >= 0 && coords.x < dimensions.x && coords.y < dimensions.y && coords.z < dimensions.z;
	}

    public static void RunTests() {
		TestAddFace();
		TestAreCoordsWithinDimension();
    }

	private static void TestAddFace() {
		void TestAddFaceDirection(Direction dir) {
			List<Vector3> verts = new List<Vector3>();
			AddFace(Vector3Int.zero, dir, Color.clear, verts, new List<Color>(), new List<Vector2>(), new List<int>());
			Debug.LogFormat("AddFace Results = {0}: {1}, {2}, {3}, {4}", dir, verts[0], verts[1], verts[2], verts[3]);
		}

		TestAddFaceDirection(Direction.Right);
		TestAddFaceDirection(Direction.Left);
		TestAddFaceDirection(Direction.Up);
		TestAddFaceDirection(Direction.Down);
		TestAddFaceDirection(Direction.Fore);
		TestAddFaceDirection(Direction.Back);
	}

	private static void TestAreCoordsWithinDimension() {
		Vector3Int dimensions = new Vector3Int(8, 8, 8);

		Debug.Assert(AreCoordsWithinDimensions(new Vector3Int(0, 0, 0), dimensions) == true);
		Debug.Assert(AreCoordsWithinDimensions(new Vector3Int(1, 2, 3), dimensions) == true);
		Debug.Assert(AreCoordsWithinDimensions(dimensions - Vector3Int.one, dimensions) == true);

		Debug.Assert(AreCoordsWithinDimensions(new Vector3Int(0, 0, -1), dimensions) == false);
		Debug.Assert(AreCoordsWithinDimensions(new Vector3Int(1000, 1000, 1000), dimensions) == false);
		Debug.Assert(AreCoordsWithinDimensions(dimensions, dimensions) == false);
	}
}
