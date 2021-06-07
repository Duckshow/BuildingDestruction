using UnityEngine;
using UnityEditor;
using System;

[CreateAssetMenu(fileName = "Dependency Manager", menuName = "ScriptableObjects/Dependency Manager", order = 1)]
[FilePath("ScriptableObjects/Dependency Manager.asset", FilePathAttribute.Location.ProjectFolder)]
public class DependencyManager : FixedScriptableSingleton<DependencyManager> {
    [Serializable]
    public class GameObjectPrefabWrapper {
        public GameObject MeshObject;
    }

    [SerializeField] private GameObjectPrefabWrapper prefabs;
    public GameObjectPrefabWrapper Prefabs  { get { return prefabs; } }


    [Serializable]
    public class MaterialWrapper {
        public Material Voxel;
    }
    
    [SerializeField] private MaterialWrapper materials;
    public MaterialWrapper Materials        { get { return materials; } }
}
