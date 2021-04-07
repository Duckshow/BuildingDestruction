using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VoxelBuilder : MonoBehaviour {
	private enum Direction { Up, Down, Left, Right, Fore, Back }

	private MeshFilter meshFilter;
	private MeshCollider meshCollider;

    private void Awake() {
		meshFilter = GetComponent<MeshFilter>();
		meshCollider = GetComponent<MeshCollider>();
    }

    public void Build(VoxelGrid voxelGrid) {
		List<Vector3> verts = new List<Vector3>();
		List<Color> color = new List<Color>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> tris = new List<int>();

        for(int i = 0; i < voxelGrid.GetVoxelCount(); i++) {
			Voxel v = voxelGrid.GetVoxel(i);
            if(!v.IsFilled) {
				continue;
            }

			Vector3Int coords = VoxelGrid.IndexToCoords(i, voxelGrid.GetDimensions());

			if(!v.HasNeighborRight) {
				AddFace(v, coords, Direction.Right, verts, color, uvs, tris);
			}
			if(!v.HasNeighborLeft) {
				AddFace(v, coords, Direction.Left, verts, color, uvs, tris);
			}
			if(!v.HasNeighborUp) {
				AddFace(v, coords, Direction.Up, verts, color, uvs, tris);
			}
			if(!v.HasNeighborDown) {
				AddFace(v, coords, Direction.Down, verts, color, uvs, tris);
			}
			if(!v.HasNeighborFore) {
				AddFace(v, coords, Direction.Fore, verts, color, uvs, tris);
			}
			if(!v.HasNeighborBack) {
				AddFace(v, coords, Direction.Back, verts, color, uvs, tris);
			}
		}

		ApplyToMesh(meshFilter, meshCollider, verts, color, uvs, tris);
	}

	private static void AddFace(Voxel voxel, Vector3Int coords, Direction dir, List<Vector3> verts, List<Color> vertColors, List<Vector2> uvs, List<int> tris) {
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

		vertColors.Add(voxel.Color);
		vertColors.Add(voxel.Color);
		vertColors.Add(voxel.Color);
		vertColors.Add(voxel.Color);

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

	private static void ApplyToMesh(MeshFilter meshFilter, MeshCollider meshCollider, List<Vector3> verts, List<Color> colors, List<Vector2> uvs, List<int> tris) {
		Mesh mesh = meshFilter.mesh;

		mesh.Clear();
		mesh.vertices = verts.ToArray();
		mesh.colors = colors.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = tris.ToArray();
		mesh.Optimize();
		mesh.RecalculateNormals();

		meshFilter.mesh = mesh;
		meshCollider.sharedMesh = mesh;

		verts.Clear();
		uvs.Clear();
		tris.Clear();
	}
}
