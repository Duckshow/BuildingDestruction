using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VoxelGrid))]
public class VoxelController : MonoBehaviour {
	private VoxelGrid voxelGrid;
	private List<Vector3Int> hitVoxels = new List<Vector3Int>();
	private List<Vector3> hitVoxelsWorldPositions = new List<Vector3>();

	private void Awake() {
		voxelGrid = GetComponent<VoxelGrid>();
    }

	private void Update() {
		if(voxelGrid == null) {
			return;
		}

        if(Input.GetKeyDown(KeyCode.Space)) {
			Vector3Int voxelGridDimensions = voxelGrid.GetVoxelGridDimensions();

            for(int z = 0; z < voxelGridDimensions.z; z++) {
                for(int y = 0; y < voxelGridDimensions.y; y++) {
                    for(int x = 0; x < voxelGridDimensions.x; x++) {
                        if(x != voxelGridDimensions.x / 2) {
							continue;
                        }

						voxelGrid.SetVoxelExists(new Vector3Int(x, y, z), exists: false);
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
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		hitVoxels.Clear();
		hitVoxelsWorldPositions.Clear();;

		Vector3Int binGridDimensions = voxelGrid.GetBinGridDimensions();

		Bounds b = new Bounds(Vector3.zero, Vector3.one);
		for(int binIndex = 0; binIndex < voxelGrid.GetBinCount(); binIndex++) {
			Bin bin = voxelGrid.GetBin(binIndex);
            if(bin.IsWholeBinEmpty() && bin.IsExterior) {
				continue;
            }

            for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {
				if(!bin.IsWholeBinEmpty() && !bin.GetVoxelExists(localVoxelIndex)) {
					continue;
				}

				Vector3 voxelWorldPos = Bin.GetVoxelWorldPos(binIndex, localVoxelIndex, binGridDimensions, voxelGrid.GetMeshTransform());
				b.center = voxelWorldPos;

				if(b.IntersectRay(ray)) {
					hitVoxels.Add(Bin.GetVoxelGlobalCoords(binIndex, localVoxelIndex, binGridDimensions));
					hitVoxelsWorldPositions.Add(voxelWorldPos);
				}
			}
		}

        if(isInstant) {
			for(int i = 0; i < hitVoxels.Count; i++) {
				voxelGrid.SetVoxelExists(hitVoxels[i], exists: false);
			}
		}
        else {
			float closestHit = Mathf.Infinity;
			int closestHitIndex = -1;
			for(int i = 0; i < hitVoxels.Count; i++) {
				float dist = Vector3.Distance(Camera.main.transform.position, hitVoxelsWorldPositions[i]);

				if(dist < closestHit) {
					closestHit = dist;
					closestHitIndex = i;
				}
			}

			if(closestHitIndex >= 0) {
				voxelGrid.SetVoxelExists(hitVoxels[closestHitIndex], exists: false);
			}
		}
    }
}
