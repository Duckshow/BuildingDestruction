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
			Vector3Int voxelGridDimensions = VoxelGrid.CalculateVoxelGridDimensions(voxelGrid.GetBinGridDimensions());

			int index = 0;
            for(int z = 0; z < voxelGridDimensions.z; z++) {
                for(int y = 0; y < voxelGridDimensions.y; y++) {
                    for(int x = 0; x < voxelGridDimensions.x; x++) {
                        if(x == voxelGridDimensions.x / 2) {
							voxelGrid.SetVoxelIsFilled(index, false);
						}

						index++;
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
		List<int> hitIndexes = new List<int>();

		Bounds b = new Bounds(Vector3.zero, Vector3.one);
		for(int binIndex = 0; binIndex < voxelGrid.GetBinCount(); binIndex++) {
			Bin bin = voxelGrid.GetBin(binIndex);
            if(bin == null) {
				continue;
            }

            for(int localVoxelIndex = 0; localVoxelIndex < Bin.SIZE; localVoxelIndex++) {
				if(!bin.GetVoxel(localVoxelIndex).IsFilled) {
					continue;
				}

				int globalVoxelIndex = bin.GetVoxel(localVoxelIndex).GlobalIndex;
				b.center = VoxelGrid.GetVoxelWorldPos(globalVoxelIndex, voxelGrid.GetBinGridDimensions(), voxelGrid.GetMeshTransform());

				if(b.IntersectRay(ray)) {
					hitIndexes.Add(globalVoxelIndex);
				}
			}
		}

        if(isInstant) {
			for(int i = 0; i < hitIndexes.Count; i++) {
				voxelGrid.SetVoxelIsFilled(hitIndexes[i], false);
			}
		}
        else {
			float closestHit = Mathf.Infinity;
			int closestHitIndex = -1;
			for(int i = 0; i < hitIndexes.Count; i++) {
				int hitIndex = hitIndexes[i];
				float dist = Vector3.Distance(Camera.main.transform.position, VoxelGrid.GetVoxelWorldPos(hitIndex, voxelGrid.GetBinGridDimensions(), voxelGrid.GetMeshTransform()));

				if(dist < closestHit) {
					closestHit = dist;
					closestHitIndex = i;
				}
			}

			if(closestHitIndex >= 0) {
				voxelGrid.SetVoxelIsFilled(hitIndexes[closestHitIndex], false);
			}
		}
    }
}
