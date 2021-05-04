using System.Collections.Generic;
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

        if(Input.GetKeyDown(KeyCode.Space)) {
			Vector3Int binGridDimensions = voxelGrid.GetBinGridDimensions();

            for(int z = 0; z < binGridDimensions.z; z++) {
                for(int y = 0; y < binGridDimensions.y; y++) {
                    for(int x = 0; x < binGridDimensions.x; x++) {
                        if(x != binGridDimensions.x / 2) {
							continue;
                        }

						int binIndex = VoxelGrid.CoordsToIndex(x, y, z, binGridDimensions);

                        for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {
							Bin bin;
							voxelGrid.TryGetBin(binIndex, out bin);

							Vector3Int localCoords = bin.GetVoxelLocalCoords(localVoxelIndex);
							
							if(localCoords.x == 0) {
								voxelGrid.SetVoxelIsFilled(new VoxelAddress(binIndex, localVoxelIndex), false);
							}
						}
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
		List<VoxelAddress> hitVoxels = new List<VoxelAddress>();
		List<Vector3> hitVoxelsWorldPositions = new List<Vector3>();

		Bounds b = new Bounds(Vector3.zero, Vector3.one);
		for(int binIndex = 0; binIndex < voxelGrid.GetBinCount(); binIndex++) {
			Bin bin;
            if(!voxelGrid.TryGetBin(binIndex, out bin)) {
				continue;
            }

            for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {
				if(!bin.GetVoxelIsFilled(localVoxelIndex)) {
					continue;
				}

				Vector3 voxelWorldPos = bin.GetVoxelWorldPos(localVoxelIndex, voxelGrid.GetMeshTransform());
				b.center = voxelWorldPos;

				if(b.IntersectRay(ray)) {
					hitVoxels.Add(new VoxelAddress(binIndex, localVoxelIndex));
					hitVoxelsWorldPositions.Add(voxelWorldPos);
				}
			}
		}

        if(isInstant) {
			for(int i = 0; i < hitVoxels.Count; i++) {
				voxelGrid.SetVoxelIsFilled(hitVoxels[i], false);
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
				voxelGrid.SetVoxelIsFilled(hitVoxels[closestHitIndex], false);
			}
		}
    }
}
