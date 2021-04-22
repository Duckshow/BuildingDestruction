using UnityEngine;
using System.Collections;

[RequireComponent(typeof(VoxelGrid))]
public class VoxelBuilder : MonoBehaviour {

	[SerializeField] private GameObject meshObjectPrefab; // TODO: handle dependencies better
	[SerializeField] private Material material;
	[SerializeField] private Transform meshTransform;

	[SerializeField, HideInInspector] private Vector3Int dimensions;
	private MeshObject[] meshObjects;
	
	private VoxelGrid voxelGrid;

    private void Awake() {
		voxelGrid = GetComponent<VoxelGrid>();
    }

    public void Refresh() {
		Vector3Int voxelGridDimensions = voxelGrid.GetVoxelGridDimensions();

        if(dimensions == Vector3Int.zero) {
			dimensions = VoxelGrid.CalculateBinGridDimensions(voxelGridDimensions);
		}
		if(meshObjects == null) {
			meshObjects = new MeshObject[voxelGridDimensions.x * voxelGridDimensions.y * voxelGridDimensions.z];
		}

		Vector3Int newDimensions = VoxelGrid.CalculateBinGridDimensions(voxelGridDimensions);
		Debug.AssertFormat(newDimensions.x <= dimensions.x && newDimensions.y <= dimensions.y && newDimensions.z <= dimensions.z, "VoxelBuilder size: {0} > {1}!", newDimensions, dimensions);

		dimensions = newDimensions;

		int newMeshObjectCount = newDimensions.x * newDimensions.y * newDimensions.z;
		Mesh[] meshes = new Mesh[newMeshObjectCount];

		for(int i = 0; i < newMeshObjectCount; i++) {
			Voxel?[] binVoxels = Bin.GetBinVoxels(i, voxelGrid);

			if(VoxelMeshFactory.TryGetMesh(i, binVoxels, out meshes[i]) && meshObjects[i] == null) {
				meshObjects[i] = Instantiate(meshObjectPrefab, meshTransform).GetComponent<MeshObject>();
			}
		}

		for(int i = newMeshObjectCount; i < meshObjects.Length; i++) {
            if(meshObjects[i] != null) {
				Destroy(meshObjects[i].gameObject);
			}
		}

		StartCoroutine(WaitThenFinishRefresh(meshes, newMeshObjectCount));
	}

	private IEnumerator WaitThenFinishRefresh(Mesh[] meshes, int newMeshObjectCount) {
		yield return null;

		for(int i = 0; i < newMeshObjectCount; i++) {
			MeshObject meshObject = meshObjects[i];
            if(meshObject == null) {
				continue;
            }

			Mesh mesh = meshes[i];
            if(mesh == null) {
				Destroy(meshObjects[i].gameObject);
				continue;
            }

			meshObject.transform.name = "MeshObject #" + i;
			meshObject.transform.localPosition = VoxelGrid.IndexToCoords(i, dimensions) * Bin.WIDTH;
			meshObject.SetMesh(meshes[i]);
			meshObject.SetMaterial(material);
		}
	}
}
