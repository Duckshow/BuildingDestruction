using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DestructibleObject : MonoBehaviour
{

    [SerializeField, Min(1)] private Vector3Int size;

    private bool[][][] voxels;
	private MeshFilter meshFilter;

    private void Awake() {
		meshFilter = GetComponent<MeshFilter>();
    }

    void Start()
    {
        voxels = new bool[size.x][][];
        for (int x = 0; x < size.x; x++)
        {
            voxels[x] = new bool[size.y][];

            for (int y = 0; y < size.y; y++)
            {
                voxels[x][y] = new bool[size.z];
            }
        }

		CreateMesh();
    }

    void Update()
    {
        
    }

    private void CreateMesh() {

		List<Vector3Int> exteriorVoxels = new List<Vector3Int>();
        int x = 0, y = 0, z = 0;

        for(x = 0; x < size.x; x++) {
			for(y = 0; y < size.y; y++) {
				for(z = 0; z < size.z; z++) {
                    if(IsVoxelOnExterior(x, y, z, voxels)) {
						exteriorVoxels.Add(new Vector3Int(x, y, z));
                    }
				}
			}
		}

		meshFilter.mesh = MeshFromPolygon(exteriorVoxels);
    }

	private static bool IsVoxelOnExterior(int x, int y, int z, bool[][][] voxels) {
        if(x == 0) {
			return true;
        }

        if(y == 0) {
			return true;
        }

        if(z == 0) {
			return true;
        }

		if(x == voxels.Length - 1) {
			return true;
		}

		if(y == voxels[x].Length - 1) {
			return true;
		}

		if(z == voxels[x][y].Length - 1) {
			return true;
		}

        if(!voxels[x + 1][y][z]) {
			return true;
        }

		if(!voxels[x - 1][y][z]) {
			return true;
		}

		if(!voxels[x][y + 1][z]) {
			return true;
		}

		if(!voxels[x][y - 1][z]) {
			return true;
		}

		if(!voxels[x][y][z + 1]) {
			return true;
		}
		
		if(!voxels[x][y][z - 1]) {
			return true;
		}

		return false;
	}

	private static Mesh MeshFromPolygon(List<Vector3Int> polygon) {
		var count = polygon.Count;
		// TODO: cache these things to avoid garbage
		var verts = new Vector3[count];
		var norms = new Vector3[count];
		var tris = new int[count * 3];
		// TODO: add UVs

		var vi = 0;
		var ni = 0;
		var ti = 0;

		// Top
		for(int i = 0; i < count; i++) {
			verts[vi++] = new Vector3(polygon[i].x, polygon[i].y, polygon[i].z);
			norms[ni++] = Vector3.forward;
		}

        for(int vert = 0; vert < count - 2; vert++) {
            tris[ti++] = vert;
            tris[ti++] = vert + 1;
            tris[ti++] = vert + 2;
        }

        //for(int vert = 2; vert < count; vert++) {
        //	tris[ti++] = count;
        //	tris[ti++] = count + vert;
        //	tris[ti++] = count + vert - 1;
        //}

        //for(int vert = 0; vert < count; vert++) {
        //	var si = 2 * count + 4 * vert;

        //	tris[ti++] = si;
        //	tris[ti++] = si + 1;
        //	tris[ti++] = si + 2;

        //	tris[ti++] = si;
        //	tris[ti++] = si + 2;
        //	tris[ti++] = si + 3;
        //}

        Debug.Assert(ti == tris.Length);
		Debug.Assert(vi == verts.Length);
		Debug.Log(ti + ", " + tris.Length);

		var mesh = new Mesh();


		mesh.vertices = verts;
		mesh.triangles = tris;
		mesh.normals = norms;

		return mesh;
	}
}
