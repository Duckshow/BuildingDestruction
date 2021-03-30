using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VoxelController : MonoBehaviour {

	public readonly struct Voxel {
		public readonly int Index;
		public readonly bool IsFilled;
		public readonly Color Color;
		public readonly bool HasNeighborRight;
		public readonly bool HasNeighborLeft;
		public readonly bool HasNeighborUp;
		public readonly bool HasNeighborDown;
		public readonly bool HasNeighborFore;
		public readonly bool HasNeighborBack;

		public Voxel(int index) {
			Index = index;

			IsFilled = false;
			Color = Color.clear;
			HasNeighborRight = false;
			HasNeighborLeft = false;
			HasNeighborUp = false;
			HasNeighborDown = false;
			HasNeighborFore = false;
			HasNeighborBack = false;
		}

		public Voxel(int index, bool isFilled) {
			Index = index;
			IsFilled = isFilled;

			Color = Color.clear;
			HasNeighborRight = false;
			HasNeighborLeft = false;
			HasNeighborUp = false;
			HasNeighborDown = false;
			HasNeighborFore = false;
			HasNeighborBack = false;
		}

		public Voxel(int index, bool isFilled, Color color, bool hasNeighborRight, bool hasNeighborLeft, bool hasNeighborUp, bool hasNeighborDown, bool hasNeighborFore, bool hasNeighborBack) {
			Index = index;
			IsFilled = isFilled;
            Color = color;

			HasNeighborRight = hasNeighborRight;
			HasNeighborLeft = hasNeighborLeft;
			HasNeighborUp = hasNeighborUp;
            HasNeighborDown = hasNeighborDown;
            HasNeighborFore = hasNeighborFore;
            HasNeighborBack = hasNeighborBack;
        }

		public static Voxel GetChangedVoxel(Voxel v, int index) {
			return new Voxel(index, v.IsFilled, v.Color, v.HasNeighborUp, v.HasNeighborDown, v.HasNeighborRight, v.HasNeighborLeft, v.HasNeighborFore, v.HasNeighborBack);
		}

		public static Voxel GetChangedVoxel(Voxel v, bool isFilled) {
            return new Voxel(v.Index, isFilled, v.Color, v.HasNeighborUp, v.HasNeighborDown, v.HasNeighborRight, v.HasNeighborLeft, v.HasNeighborFore, v.HasNeighborBack);
        }

        public static Voxel GetChangedVoxel(Voxel v, Color color) {
            return new Voxel(v.Index, v.IsFilled, color, v.HasNeighborUp, v.HasNeighborDown, v.HasNeighborRight, v.HasNeighborLeft, v.HasNeighborFore, v.HasNeighborBack);
        }

        public static Voxel GetChangedVoxel(Voxel v, bool hasNeighborRight, bool hasNeighborLeft, bool hasNeighborUp, bool hasNeighborDown, bool hasNeighborFore, bool hasNeighborBack) {
            return new Voxel(v.Index, v.IsFilled, v.Color, hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack);
        }

		public static int GetIndex(Vector3Int coords, Vector3Int dimensions) {
			return GetIndex(coords.x, coords.y, coords.z, dimensions);
		}

		public static int GetIndex(int x, int y, int z, Vector3Int dimensions) {
			return x + dimensions.x * (y + dimensions.y * z);
		}

		public static Vector3Int GetCoordinates(int index, Vector3Int dimensions) {
			return new Vector3Int(index % dimensions.x, (index / dimensions.x) % dimensions.y, index / (dimensions.x * dimensions.y));
		}

		public static bool TryGet(Vector3Int coords, Voxel[] voxels, Vector3Int dimensions, out Voxel voxel) {
			return TryGet(coords.x, coords.y, coords.z, voxels, dimensions, out voxel);
		}

		public static bool TryGet(int x, int y, int z, Voxel[] voxels, Vector3Int dimensions, out Voxel voxel) {
			if(x < 0 || y < 0 || z < 0 || x >= dimensions.x || y >= dimensions.y || z >= dimensions.z) {
				voxel = new Voxel();
				return false;
			}

			voxel = voxels[GetIndex(x, y, z, dimensions)];
			return true;
		}
	}
	 
	private class VoxelCluster {
		private Voxel[] newGrid;
		private Voxel[] newVoxels;
		private int newVoxelCount;
		private Vector3Int minCoord, maxCoord;
		private Vector3 pivot;
		private bool isStatic;
		private Vector3Int newDimensions;
		public Color color;

		public void Fill(Voxel startVoxel, Voxel[] grid, Vector3Int gridDimensions, bool wasStatic, bool[] visitedVoxels) {
			newVoxels = FindVoxels(startVoxel, grid, gridDimensions, visitedVoxels, out newVoxelCount);
			//TryRecursiveAdd(startVoxel, newVoxelsList, oldVoxels, oldDimensions, bucketFillMap);

			color = new Color(Random.value, Random.value, Random.value, 1);
            for(int i = 0; i < newVoxelCount; i++) {
				newVoxels[i] = Voxel.GetChangedVoxel(newVoxels[i], color);
			}

			minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
			maxCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

            for(int i = 0; i < newVoxelCount; i++) {
				Vector3Int coords = Voxel.GetCoordinates(newVoxels[i].Index, gridDimensions);

				minCoord.x = Mathf.Min(minCoord.x, coords.x);
				minCoord.y = Mathf.Min(minCoord.y, coords.y);
				minCoord.z = Mathf.Min(minCoord.z, coords.z);

				maxCoord.x = Mathf.Max(maxCoord.x, coords.x);
				maxCoord.y = Mathf.Max(maxCoord.y, coords.y);
				maxCoord.z = Mathf.Max(maxCoord.z, coords.z);
			}

			newDimensions = maxCoord - minCoord + Vector3Int.one;
			newGrid = new Voxel[newDimensions.x * newDimensions.y * newDimensions.z];
		
            for(int i = 0; i < newGrid.Length; i++) {
				newGrid[i] = new Voxel(i);
			}

			isStatic = wasStatic && minCoord.y == 0;
			pivot = Vector3.zero;
			float divisor = 0f;

            for(int i = 0; i < newVoxelCount; i++) {
				Voxel v = newVoxels[i];
				int newIndex = Voxel.GetIndex(Voxel.GetCoordinates(v.Index, gridDimensions) - minCoord, newDimensions);
				v = Voxel.GetChangedVoxel(v, newIndex);

				newGrid[newIndex] = v;
				newVoxels[i] = v;

				Vector3Int coords = Voxel.GetCoordinates(newIndex, newDimensions);
				if(!isStatic || coords.y == 0) {
					pivot += new Vector3(coords.x, coords.y, coords.z);
					divisor++;
				}
			}

			pivot /= divisor;
            if(isStatic) {
				pivot.y = -0.5f;
            }
		}

		//private static void TryRecursiveAdd(Voxel voxel, List<Voxel> foundVoxels, Voxel[] grid, Vector3Int dimensions, bool[] bucketFillMap) {
  //          void TryGoToNeighbor(int x, int y, int z, out bool doesNeighborExist) {
  //              Voxel neighbor;

  //              doesNeighborExist = Voxel.TryGet(x, y, z, grid, dimensions, out neighbor) && neighbor.IsFilled;
                
		//		if(!doesNeighborExist || bucketFillMap[neighbor.Index]) {
  //                  return;
  //              }

  //              TryRecursiveAdd(neighbor, foundVoxels, grid, dimensions, bucketFillMap);
  //          }

  //          bucketFillMap[voxel.Index] = true;

		//	Vector3Int coords = Voxel.GetCoordinates(voxel.Index, dimensions);

		//	bool hasNeighborUp, hasNeighborDown, hasNeighborRight, hasNeighborLeft, hasNeighborFore, hasNeighborBack;
  //          TryGoToNeighbor(coords.x, coords.y + 1, coords.z, out hasNeighborUp);
  //          TryGoToNeighbor(coords.x, coords.y - 1, coords.z, out hasNeighborDown);
  //          TryGoToNeighbor(coords.x + 1, coords.y, coords.z, out hasNeighborRight);
  //          TryGoToNeighbor(coords.x - 1, coords.y, coords.z, out hasNeighborLeft);
  //          TryGoToNeighbor(coords.x, coords.y, coords.z + 1, out hasNeighborFore);
  //          TryGoToNeighbor(coords.x, coords.y, coords.z - 1, out hasNeighborBack);

  //          voxel = Voxel.GetChangedVoxel(voxel, hasNeighborUp, hasNeighborDown, hasNeighborRight, hasNeighborLeft, hasNeighborFore, hasNeighborBack);

		//	// OPTIMIZATION: this could save a lot of iterating, but might not be necessary after we Job-ify this
  //          //if(!voxel.HasNeighborRight || !voxel.HasNeighborLeft || !voxel.HasNeighborUp || !voxel.HasNeighborDown || !voxel.HasNeighborFore || !voxel.HasNeighborBack) {
  //              foundVoxels.Add(voxel);
  //          //}
  //      }

		private static Voxel[] FindVoxels(Voxel startVoxel, Voxel[] grid, Vector3Int dimension, bool[] visitedVoxels, out int foundVoxelCount) {
			Voxel[] foundVoxels = new Voxel[grid.Length];
			foundVoxelCount = 0;

			Queue<Voxel> q = new Queue<Voxel>();
			q.Enqueue(startVoxel);

            while(q.Count > 0) {
				Voxel v = q.Dequeue();

				if(!v.IsFilled) {
					continue;
				}

				if(visitedVoxels[v.Index]) {
					continue;
                }

				visitedVoxels[v.Index] = true;

				Vector3Int coords = Voxel.GetCoordinates(v.Index, dimension);
				Vector3Int coordsRight =	coords + Vector3Int.right;
				Vector3Int coordsLeft =		coords + Vector3Int.left;
				Vector3Int coordsUp =		coords + Vector3Int.up;
				Vector3Int coordsDown =		coords + Vector3Int.down;
				Vector3Int coordsFore =		coords + Vector3Int.forward;
				Vector3Int coordsBack =		coords + Vector3Int.back;

				Voxel vRight, vLeft, vUp, vDown, vFore, vBack;
				bool hasNeighborRight = Voxel.TryGet(coordsRight, grid, dimension, out vRight) && vRight.IsFilled;
				bool hasNeighborLeft =	Voxel.TryGet(coordsLeft, grid, dimension, out vLeft) && vLeft.IsFilled;
				bool hasNeighborUp =	Voxel.TryGet(coordsUp, grid, dimension, out vUp) && vUp.IsFilled;
				bool hasNeighborDown =	Voxel.TryGet(coordsDown, grid, dimension, out vDown) && vDown.IsFilled;
				bool hasNeighborFore =	Voxel.TryGet(coordsFore, grid, dimension, out vFore) && vFore.IsFilled;
				bool hasNeighborBack =	Voxel.TryGet(coordsBack, grid, dimension, out vBack) && vBack.IsFilled;

				v = Voxel.GetChangedVoxel(v, hasNeighborRight, hasNeighborLeft, hasNeighborUp, hasNeighborDown, hasNeighborFore, hasNeighborBack);
				foundVoxels[foundVoxelCount] = v;
				foundVoxelCount++;

				if(hasNeighborRight)	q.Enqueue(vRight);
				if(hasNeighborLeft)		q.Enqueue(vLeft);
				if(hasNeighborUp)		q.Enqueue(vUp);
				if(hasNeighborDown)		q.Enqueue(vDown);
				if(hasNeighborFore)		q.Enqueue(vFore);
				if(hasNeighborBack)		q.Enqueue(vBack);
			}

			return foundVoxels;
		}

		public Voxel[] GetNewGrid() {
			return newGrid;
		}

		public Voxel[] GetVoxels() {
			return newVoxels;
		}

		public int GetVoxelCount() {
			return newVoxelCount;
		}

		public Vector3Int GetDimensions() {
			return newDimensions;
		}

		public Vector3 GetPivot() {
			return pivot;
		}

		public Vector3Int GetOffset() {
			return minCoord;
		}

		public bool GetIsStatic() {
			return isStatic;
		}
	}


	[SerializeField] private VoxelBuilder voxelBuilder;
	[SerializeField] private Texture2D[] textures;

	[SerializeField, HideInInspector] private bool isStatic;
	[SerializeField, HideInInspector] private bool isDescendant;
	[SerializeField, HideInInspector] private Vector3Int dimensions;

	private Voxel[] voxels;
	private new Rigidbody rigidbody;

	private bool isDirty;
	private const float UPDATE_LATENCY = 0.1f;
	private float timeToUpdate;

	private void OnValidate() {
		voxelBuilder = GetComponentInChildren<VoxelBuilder>();
        if(voxelBuilder == null) {
			GameObject child = new GameObject();
			child.transform.name = "VoxelBuilder";
			child.transform.parent = transform;
			child.transform.localPosition = Vector3.zero;
			voxelBuilder = child.AddComponent<VoxelBuilder>();
        }
    }

    private void Awake() {
		rigidbody = GetComponent<Rigidbody>();
	}

    private void Start() {
        if(!isDescendant) {
			dimensions = new Vector3Int(50, 50, 50);
			voxels = new Voxel[dimensions.x * dimensions.y * dimensions.z];

            for(int i = 0; i < voxels.Length; i++) {
				voxels[i] = new Voxel(i, true);
			}


			voxelBuilder.transform.localPosition = new Vector3(-(dimensions.x / 2), 0.5f, -(dimensions.z / 2));

			isDirty = true;
		}
    }

    private void Update() {
		rigidbody.isKinematic = isStatic;

		if(voxels == null) {
			return;
		}

        if(Input.GetKeyDown(KeyCode.Space)) {
            for(int z = 0; z < dimensions.z; z++) {
                for(int y = 0; y < dimensions.y; y++) {
					int i = Voxel.GetIndex(dimensions.x / 2, y, z, dimensions);
					voxels[i] = Voxel.GetChangedVoxel(voxels[i], isFilled: false);
                }
            }

			isDirty = true;
        }

		return;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		List<int> hitIndexes = new List<int>();

		//RaycastHit hit;
  //      if(Physics.Raycast(ray, out hit)) {
		//	Debug.Log(hit.point);
  //      }

		Bounds b = new Bounds(Vector3.zero, Vector3.one);
		for(int i = 0; i < voxels.Length; i++) {
			Voxel voxel = voxels[i];

			if(!voxel.IsFilled) {
				continue;
			}

            if(voxel.HasNeighborRight && voxel.HasNeighborLeft && voxel.HasNeighborUp && voxel.HasNeighborDown && voxel.HasNeighborFore && voxel.HasNeighborBack) {
				continue;
            }

			b.center = GetVoxelWorldPos(voxel.Index);
			if(b.IntersectRay(ray)) {
				hitIndexes.Add(voxel.Index);
			}
		}

		float closestHit = Mathf.Infinity;
		int closestHitIndex = -1;
		for(int i = 0; i < hitIndexes.Count; i++) {
			int hitIndex = hitIndexes[i];
			float dist = Vector3.Distance(Camera.main.transform.position, GetVoxelWorldPos(hitIndex));

			if(dist < closestHit) {
				closestHit = dist;
				closestHitIndex = i;
			}
		}

		if(closestHitIndex >= 0) {
			int hitIndex = hitIndexes[closestHitIndex];
			voxels[hitIndex] = Voxel.GetChangedVoxel(voxels[hitIndex], isFilled: false);
			isDirty = true;
		}
	}

	private void LateUpdate() {
		if(Time.time < timeToUpdate) {
			return;
		}
		else {
			timeToUpdate = Time.time + UPDATE_LATENCY;
		}

		if(!isDirty) {
			return;
		}
		else {
			isDirty = false;
		}

		List<VoxelCluster> voxelClusters = new List<VoxelCluster>();
		bool[] visitedVoxels = new bool[voxels.Length];

        for(int i = 0; i < voxels.Length; i++) {
			if(visitedVoxels[i]) {
				continue;
			}

			Voxel voxel = voxels[i];
			if(!voxel.IsFilled) {
				continue;
			}

			VoxelCluster newCluster = new VoxelCluster();
			newCluster.Fill(voxel, voxels, dimensions, isStatic, visitedVoxels);
			voxelClusters.Add(newCluster);
		}

        //for(int i = 0; i < bucketFillMap.Length; i++) {
        //    Debug.LogFormat("{0}: {1}", i, bucketFillMap[i]);
        //}

		//Debug.Log("pew");

		for(int i = voxelClusters.Count - 1; i >= 0; i--) {
			VoxelController voxelController;
			if(i == 0) {
				voxelController = this;
			}
			else {
				GameObject go = Instantiate(gameObject, transform.parent);
				voxelController = go.GetComponent<VoxelController>();
				voxelController.isDescendant = true;
			}

			VoxelCluster cluster = voxelClusters[i];

			voxelController.isStatic = cluster.GetIsStatic();

			Vector3 GetLocalPosWithWorldRotation(Vector3 localPos, Transform t) {
				return (t.TransformPoint(localPos) - t.position);
			}

			voxelController.voxelBuilder.transform.position += GetLocalPosWithWorldRotation(cluster.GetOffset(), voxelController.voxelBuilder.transform);

			if(voxelController.isDescendant) {
				voxelController.voxelBuilder.transform.parent = null;
				voxelController.transform.position = voxelController.voxelBuilder.transform.position + GetLocalPosWithWorldRotation(cluster.GetPivot(), voxelController.voxelBuilder.transform);
				voxelController.voxelBuilder.transform.parent = voxelController.transform;
			}

			voxelController.ApplyNewVoxels(cluster);
        }
	}

	private void ApplyNewVoxels(VoxelCluster cluster) {
		voxels = cluster.GetNewGrid();
		dimensions = cluster.GetDimensions();
		voxelBuilder.Build(cluster.GetVoxels(), cluster.GetVoxelCount(), dimensions);
	}

	//private Voxel GetVoxel(int x, int y, int z) {
	//	return voxels[Voxel.GetIndex(x, y, z, dimensions)];
	//}

	public Vector3 GetVoxelLocalPos(int index) {
		return Voxel.GetCoordinates(index, dimensions);
	}

	public Vector3 GetVoxelWorldPos(int index) {
		return voxelBuilder.transform.TransformPoint(GetVoxelLocalPos(index));
	}
}
