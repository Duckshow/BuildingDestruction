using UnityEngine;
using System.Collections;

public class VoxelBuilder {

	private VoxelGrid owner;

	private Vector3Int dimensions;
	private MeshObject[] meshObjects;

	public VoxelBuilder(VoxelGrid owner) {
		this.owner = owner;
	}

    public void Refresh() {
		Vector3Int newDimensions = owner.GetDimensions();

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
			Vector3Int voxelCoords = VoxelGrid.IndexToCoords(i, newDimensions);
            if(!owner.TryGetVoxel(voxelCoords, out bool doesVoxelExist)) {
				throw new System.Exception();
            }
            if(!doesVoxelExist) {
				continue;
            }

			owner.GetVoxelNeighbors(voxelCoords, out bool hasNeighborRight, out bool hasNeighborLeft, out bool hasNeighborUp, out bool hasNeighborDown, out bool hasNeighborFore, out bool hasNeighborBack);

			if(VoxelMeshFactory.TryGetMesh(hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack, out meshes[i]) && meshObjects[i] == null) {
				meshObjects[i] = Object.Instantiate(DependencyManager.Instance.Prefabs.MeshObject, owner.GetMeshTransform()).GetComponent<MeshObject>(); // TODO: make a global meshobject pool
			}
		}

		for(int i = newLength; i < meshObjects.Length; i++) {
            if(meshObjects[i] != null) {
				Object.Destroy(meshObjects[i].gameObject);
			}
		}

 		owner.StartCoroutine(WaitThenFinishRefresh(meshes, meshObjects, newLength, dimensions, DependencyManager.Instance.Materials.Voxel));
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
			meshObject.transform.localPosition = VoxelGrid.IndexToCoords(i, dimensions);
			meshObject.SetMesh(meshes[i]);
			meshObject.SetMaterial(material);
		}
	}
}
