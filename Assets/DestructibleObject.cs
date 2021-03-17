using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DestructibleObject : MonoBehaviour
{

    [SerializeField, Min(1)] private Vector3Int size;
	[SerializeField, Min(0.001f)] private float scale;
    [SerializeField] private Material material;

    //private bool[][][] voxels;
    private bool[][][] v;
    private MeshFilter meshFilter;

    private void Awake() {
		meshFilter = GetComponent<MeshFilter>();
    }

    void Start()
    {
        //voxels = new bool[size.x][][];
        //for(int x = 0; x < size.x; x++) {
        //    voxels[x] = new bool[size.y][];

        //    for(int y = 0; y < size.y; y++) {
        //        voxels[x][y] = new bool[size.z];
        //    }
        //}

        v = new bool[size.x][][];
        for(int x = 0; x < size.x; x++) {
            v[x] = new bool[size.y][];

            for(int y = 0; y < size.y; y++) {
                v[x][y] = new bool[size.z];
            }
        }

        CreateMesh();
    }

    private void Update() {
        if(meshFilter.mesh != null) {
            for(int i = 0; i < meshFilter.mesh.vertices.Length; i++) {
				Vector3 v = meshFilter.mesh.vertices[i];

                Debug.DrawLine(v + Vector3.left * 0.1f, v + Vector3.forward * 0.1f, Color.red, 0.05f);
				Debug.DrawLine(v + Vector3.forward * 0.1f, v + Vector3.right * 0.1f, Color.red, 0.05f);
				Debug.DrawLine(v + Vector3.right * 0.1f, v + Vector3.back * 0.1f, Color.red, 0.05f);
				Debug.DrawLine(v + Vector3.back * 0.1f, v + Vector3.left * 0.1f, Color.red, 0.05f);

				Debug.DrawLine(v + Vector3.left * 0.1f, v + Vector3.up * 0.1f, Color.red, 0.05f);
				Debug.DrawLine(v + Vector3.up * 0.1f, v + Vector3.right * 0.1f, Color.red, 0.05f);
				Debug.DrawLine(v + Vector3.right * 0.1f, v + Vector3.down * 0.1f, Color.red, 0.05f);
				Debug.DrawLine(v + Vector3.down * 0.1f, v + Vector3.left * 0.1f, Color.red, 0.05f);

				Debug.DrawLine(v + Vector3.forward * 0.1f, v + Vector3.up * 0.1f, Color.red, 0.05f);
				Debug.DrawLine(v + Vector3.up * 0.1f, v + Vector3.back * 0.1f, Color.red, 0.05f);
				Debug.DrawLine(v + Vector3.back * 0.1f, v + Vector3.down * 0.1f, Color.red, 0.05f);
				Debug.DrawLine(v + Vector3.down * 0.1f, v + Vector3.forward * 0.1f, Color.red, 0.05f);
			}
		}
    }

    private void CreateMesh() {

        //List<Vector3> vertices = new List<Vector3>();
        //List<Vector3> normals = new List<Vector3>();
        Vector3 normal;

        //int x = 0, y = 0, z = 0;



        //for(z = 0; z < size.z; z++) {
        //	for(y = 0; y < size.y; y++) {
        //		for(x = 0; x < size.x; x++) {
        //			if(IsVoxelOnExterior(x, y, z, voxels, out normal)) {
        //				vertices.Add(transform.position + new Vector3(x * scale, y * scale, z * scale));
        //				normals.Add(normal);
        //			}
        //		}
        //	}
        //}



        //Mesh mesh = new Mesh();
        //mesh.vertices = vertices.ToArray();
        //mesh.normals = normals.ToArray();
        //meshFilter.mesh = mesh;






        //Set the mode used to create the mesh.
        //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
        //Marching marching = new MarchingCubes();
        Marching marching = new MarchingTertrahedron();

        //Surface is the value that represents the surface of mesh
        //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
        //The target value does not have to be the mid point it can be any value with in the range.
        marching.Surface = 0f;

        //Fill voxels with values. Im using perlin noise but any method to create voxels will work.
        float[] voxels = new float[size.x * size.y * size.z];
        for(int z = 0; z < size.z; z++) {
            for(int y = 0; y < size.y; y++) {
                for(int x = 0; x < size.x; x++) {
                    if(IsVoxelOnExterior(x, y, z, v, out normal) || x > size.x / 2 && y > size.y / 2) {
                        voxels[x + y * size.x + z * size.y * size.z] = 0f;// (Random.value - 0.5f) * 2f;
                    }
                    else {
                        voxels[x + y * size.x + z * size.y * size.z] = 1f;// (Random.value - 0.5f) * 2f;
                    }

                    if(x > 5 && x < 10 && y > 5 && y < 10 && z > 5 && z < 10) {
                        voxels[x + y * size.x + z * size.y * size.z] = 1f;
                    }
                    else {
                        voxels[x + y * size.x + z * size.y * size.z] = 0f;
                    }
                }
            }
        }
        
        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();

        //The mesh produced is not optimal. There is one vert for each index.
        //Would need to weld vertices for better quality mesh.
        marching.Generate(voxels, size.x, size.y, size.z, verts, indices);

        //A mesh in unity can only be made up of 65000 verts.
        //Need to split the verts between multiple meshes.

        int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
        int numMeshes = verts.Count / maxVertsPerMesh + 1;

        for(int i = 0; i < numMeshes; i++) {

            List<Vector3> splitVerts = new List<Vector3>();
            List<int> splitIndices = new List<int>();

            for(int j = 0; j < maxVertsPerMesh; j++) {
                int idx = i * maxVertsPerMesh + j;

                if(idx < verts.Count) {
                    splitVerts.Add(verts[idx]);
                    splitIndices.Add(j);
                }
            }

            if(splitVerts.Count == 0)
                continue;

            Mesh mesh = new Mesh();
            mesh.SetVertices(splitVerts);
            mesh.SetTriangles(splitIndices, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.transform.localPosition = new Vector3(-size.x / 2, -size.y / 2, -size.z / 2);
            
            //meshes.Add(go);
        }
    }

	private static bool IsVoxelOnExterior(int x, int y, int z, bool[][][] voxels, out Vector3 normal) {
        if(x == 1) {// || !voxels[x - 1][y][z]) {
			normal = Vector3.left;
			return true;
        }

        if(y == 1) {// || !voxels[x][y - 1][z]) {
			normal = Vector3.down;
			return true;
        }

        if(z == 1) {// || !voxels[x][y][z - 1]) {
			normal = Vector3.back;
			return true;
        }

		if(x == voxels.Length - 2) {//|| !voxels[x + 1][y][z]) {
			normal = Vector3.right;
			return true;
		}

		if(y == voxels[x].Length - 2) {//|| !voxels[x][y + 1][z]) {
			normal = Vector3.up;
			return true;
		}

		if(z == voxels[x][y].Length - 2) {//|| !voxels[x][y][z + 1]) {
			normal = Vector3.forward;
			return true;
		}

		normal = Vector3.zero;
		return false;
	}
}
