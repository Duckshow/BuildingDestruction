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

    public void Build(VoxelController.Voxel[,,] voxels) {
		List<Vector3> verts = new List<Vector3>();
		List<Color> color = new List<Color>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> tris = new List<int>();

		Vector3Int size = new Vector3Int(voxels.GetLength(0), voxels.GetLength(1), voxels.GetLength(2));

		for(int z = 0; z < size.z; z++) {
			for(int y = 0; y < size.y; y++) {
				for(int x = 0; x < size.x; x++) {
					VoxelController.Voxel voxel = voxels[x, y, z];

					if(!voxel.IsFilled) {
						continue;
					}

					VoxelController.Voxel neighbor;
					if(!VoxelController.TryGetVoxelAt(x, y + 1, z, voxels, out neighbor) || !neighbor.IsFilled) {
						AddFace(voxel, Direction.Up, verts, color, uvs, tris);
					}
					if(!VoxelController.TryGetVoxelAt(x, y - 1, z, voxels, out neighbor) || !neighbor.IsFilled) {
						AddFace(voxel, Direction.Down, verts, color, uvs, tris);
					}
					if(!VoxelController.TryGetVoxelAt(x + 1, y, z, voxels, out neighbor) || !neighbor.IsFilled) {
						AddFace(voxel, Direction.Right, verts, color, uvs, tris);
					}
					if(!VoxelController.TryGetVoxelAt(x - 1, y, z, voxels, out neighbor) || !neighbor.IsFilled) {
						AddFace(voxel, Direction.Left, verts, color, uvs, tris);
					}
					if(!VoxelController.TryGetVoxelAt(x, y, z + 1, voxels, out neighbor) || !neighbor.IsFilled) {
						AddFace(voxel, Direction.Fore, verts, color, uvs, tris);
					}
					if(!VoxelController.TryGetVoxelAt(x, y, z - 1, voxels, out neighbor) || !neighbor.IsFilled) {
						AddFace(voxel, Direction.Back, verts, color, uvs, tris);
					}
				}
			}
		}

		ApplyToMesh(meshFilter, meshCollider, verts, color, uvs, tris);
	}

	private void AddFace(VoxelController.Voxel voxel, Direction dir, List<Vector3> verts, List<Color> vertColors, List<Vector2> uvs, List<int> tris) {
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

		Vector3 localPos = voxel.GetLocalPos();
		verts.Add(localPos + v0);
		verts.Add(localPos + v1);
		verts.Add(localPos + v2);
		verts.Add(localPos + v3);

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
