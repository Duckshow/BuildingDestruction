using UnityEngine;
using System.Collections;

public class VoxelBuilder {

	[SerializeField, HideInInspector] private Vector3Int dimensions;

	private VoxelGrid owner;
	private GameObject meshObjectPrefab;
	private Material material;
	private MeshObject[] meshObjects;

    public VoxelBuilder(VoxelGrid owner) {
		this.owner = owner;

		meshObjectPrefab = DependencyManager.Instance.Prefabs.MeshObject;
		material = DependencyManager.Instance.Materials.Voxel;
	}

    public void Refresh() {
		Debug.Assert(ReferenceEquals(this, owner.GetVoxelBuilder()));

		Vector3Int newDimensions = owner.GetVoxelCluster().Dimensions;

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
            if(!owner.GetVoxelCluster().TryGetVoxelBlock(i, out Bin voxelBlock)) {
				continue;
            }

			if(VoxelMeshFactory.TryGetMesh(voxelBlock, out meshes[i]) && meshObjects[i] == null) {
				meshObjects[i] = Object.Instantiate(meshObjectPrefab, owner.GetMeshTransform()).GetComponent<MeshObject>(); // TODO: make a global meshobject pool
			}
		}

		for(int i = newLength; i < meshObjects.Length; i++) {
            if(meshObjects[i] != null) {
				Object.Destroy(meshObjects[i].gameObject);
			}
		}

 		owner.StartCoroutine(WaitThenFinishRefresh(meshes, meshObjects, newLength, dimensions, material));
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
				Object.Destroy(meshObjects[i].gameObject);
				continue;
            }

			meshObject.transform.name = "MeshObject #" + i;
			meshObject.transform.localPosition = Utils.IndexToCoords(i, dimensions) * Bin.WIDTH + new Vector3(0.5f, 0.5f, 0.5f);
			meshObject.SetMesh(meshes[i]);
			meshObject.SetMaterial(material);
		}
	}
}
