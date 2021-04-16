using System;
using System.Collections.Generic;
using MonsterLove.Collections;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
	public bool logStatus;
	public Transform root;

	private Dictionary<GameObject, ObjectPool<GameObject>> prefabLookup;
	private Dictionary<GameObject, ObjectPool<GameObject>> instanceLookup; 
	
	private bool dirty = false;
	
	void Awake () 
	{
		prefabLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
		instanceLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
	}

	void Update()
	{
		if(logStatus && dirty)
		{
			PrintStatus();
			dirty = false;
		}
	}

	public void warmPool(GameObject prefab, int size)
	{
		if(hasPoolForPrefab(prefab))
		{
			throw new Exception("Pool for prefab " + prefab.name + " has already been created");
		}
		var pool = new ObjectPool<GameObject>(() => { 
			GameObject go = InstantiatePrefab(prefab);
			go.SetActive(false);
			go.hideFlags = HideFlags.HideInHierarchy;
			return go;
		}, size);

		prefabLookup[prefab] = pool;

		dirty = true;
	}

	public bool hasPoolForPrefab(GameObject prefab) {
		return prefabLookup.ContainsKey(prefab);
	}

	public GameObject spawnObject(GameObject prefab)
	{
		return spawnObject(prefab, null, Vector3.zero, Quaternion.identity);
	}

	public GameObject spawnObject(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
	{
		if (!hasPoolForPrefab(prefab))
		{
			WarmPool(prefab, 1);
		}

		var pool = prefabLookup[prefab];

		var clone = pool.GetItem();
		
		clone.transform.parent = parent;
        
		if(parent != null) {
			clone.transform.localPosition = position;
			clone.transform.localRotation = rotation;
		}
        else {
			clone.transform.position = position;
			clone.transform.rotation = rotation;
		}

		clone.SetActive(true);
		clone.hideFlags = HideFlags.None;

		instanceLookup.Add(clone, pool);
		dirty = true;
		return clone;
	}

	public void releaseObject(GameObject clone)
	{
		clone.SetActive(false);
		clone.transform.parent = null;
		clone.hideFlags = HideFlags.HideInHierarchy;

		if(instanceLookup.ContainsKey(clone))
		{
			instanceLookup[clone].ReleaseItem(clone);
			instanceLookup.Remove(clone);
			dirty = true;
		}
		else
		{
			Destroy(clone);
			//Debug.LogWarning("No pool contains the object: " + clone.name);
		}
	}


	private GameObject InstantiatePrefab(GameObject prefab)
	{
		var go = Instantiate(prefab) as GameObject;
		if (root != null) go.transform.parent = root;
		return go;
	}

	public void PrintStatus()
	{
		foreach (KeyValuePair<GameObject, ObjectPool<GameObject>> keyVal in prefabLookup)
		{
			Debug.Log(string.Format("Object Pool for Prefab: {0} In Use: {1} Total {2}", keyVal.Key.name, keyVal.Value.CountUsedItems, keyVal.Value.Count));
		}
	}

	#region Static API

	public static void WarmPool(GameObject prefab, int size)
	{
		Instance.warmPool(prefab, size);
	}

	public static bool HasPoolForPrefab(GameObject prefab) {
		return Instance.hasPoolForPrefab(prefab);
	}

	public static GameObject SpawnObject(GameObject prefab)
	{
		return Instance.spawnObject(prefab);
	}

	public static GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		return Instance.spawnObject(prefab, null, position, rotation);
	}

	public static GameObject SpawnObject(GameObject prefab, Transform parent, Vector3 localPosition, Quaternion localRotation) {
		return Instance.spawnObject(prefab, parent, localPosition, localRotation);
	}

	public static void ReleaseObject(GameObject clone)
	{
		Instance.releaseObject(clone);
	}

	#endregion
}


