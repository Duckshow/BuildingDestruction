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

		for(int i0 = 0; i0 < newMeshObjectCount; i0++) {
			int[] voxelIndexes = voxelGrid.GetBinUnsafe(i0).VoxelIndexes;
			Voxel?[] voxels = new Voxel?[voxelIndexes.Length];

            for(int i1 = 0; i1 < voxelIndexes.Length; i1++) {
				voxels[i1] = voxelGrid.TryGetVoxelUnsafe(i1);
			}

			if(VoxelMeshFactory.TryGetMesh(i0, voxels, out meshes[i0]) && meshObjects[i0] == null) {
				meshObjects[i0] = Instantiate(meshObjectPrefab, meshTransform).GetComponent<MeshObject>();
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
