using UnityEngine;

[RequireComponent(typeof(VoxelGrid))]
public class VoxelController : MonoBehaviour {
	private VoxelGrid voxelGrid;

	private void Awake() {
		voxelGrid = GetComponent<VoxelGrid>();
    }

	private void Update() {
		if(voxelGrid == null) {
			return;
		}
        if(voxelGrid.GetVoxelCluster().State == VoxelCluster.UpdateState.WaitingForUpdate) {
			return;
        }

        if(Input.GetKeyDown(KeyCode.Space)) {
			Vector3Int voxelGridDimensions = voxelGrid.GetVoxelCluster().VoxelDimensions;

            for(int z = 0; z < voxelGridDimensions.z; z++) {
                for(int y = 0; y < voxelGridDimensions.y; y++) {
                    for(int x = 0; x < voxelGridDimensions.x; x++) {
                        if(x != voxelGridDimensions.x / 2) {
							continue;
                        }

						voxelGrid.GetVoxelCluster().RemoveVoxel(new Vector3Int(x, y, z));
					}
				}
			}
        }

        if(Input.GetMouseButton(0)) {
			FireBeam(isInstant: true);
        }
        else if(Input.GetMouseButton(1)) {
			FireBeam(isInstant: false);
        }
	}

	private void FireBeam(bool isInstant) {
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector3Int lineStartCoords = voxelGrid.GetVoxelCoordsFromWorldPos(mouseWorldPos);
		Vector3Int lineEndCoords = voxelGrid.GetVoxelCoordsFromWorldPos(mouseWorldPos + Camera.main.transform.forward * 100);

		Vector3Int[] line = Bresenhams3D.GetLine(lineStartCoords, lineEndCoords);

		for(int i = 0; i < line.Length; i++) {
			Vector3Int lineVoxelCoords = line[i];
            
			if(!Utils.AreCoordsWithinDimensions(lineVoxelCoords, voxelGrid.GetVoxelCluster().VoxelDimensions)) {
				continue;
            }

			voxelGrid.GetVoxelCluster().RemoveVoxel(lineVoxelCoords);
            if(!isInstant) {
				break;
            }
		}
    }
}
