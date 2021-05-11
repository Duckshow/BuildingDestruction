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
		Vector3Int newDimensions = voxelGrid.GetBinGridDimensions();

		if(dimensions == Vector3Int.zero) {
			dimensions = newDimensions;
		}

		int newLength = newDimensions.x * newDimensions.y * newDimensions.z;

		if(meshObjects == null) {
			meshObjects = new MeshObject[newLength];
		}

		Debug.AssertFormat(newDimensions.x <= dimensions.x && newDimensions.y <= dimensions.y && newDimensions.z <= dimensions.z, "VoxelBuilder size: {0} > {1}!", newDimensions, dimensions);

		dimensions = newDimensions;

		Mesh[] meshes = new Mesh[newLength];
		for(int i = 0; i < newLength; i++) {
			Bin bin = voxelGrid.GetBin(i);

			if(VoxelMeshFactory.TryGetMesh(bin, out meshes[i]) && meshObjects[i] == null) {
				meshObjects[i] = Instantiate(meshObjectPrefab, meshTransform).GetComponent<MeshObject>();
			}
		}

		for(int i = newLength; i < meshObjects.Length; i++) {
            if(meshObjects[i] != null) {
				Destroy(meshObjects[i].gameObject);
			}
		}

 		StartCoroutine(WaitThenFinishRefresh(meshes, meshObjects, newLength, dimensions, material));
	}

	private static IEnumerator WaitThenFinishRefresh(Mesh[] meshes, MeshObject[] meshObjects, int newMeshObjectCount, Vector3Int dimensions, Material material) {
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
