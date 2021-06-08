using UnityEngine;
using UnityEditor;
using System;

using Object = UnityEngine.Object;

public class FixedScriptableSingleton<T> : ScriptableObject where T : Object { // TODO: replace all instances of AssetDatabase with something that works in builds!

    private static T instance;
    public static T Instance {
        get {
            if(instance == null) {
                string[] assets = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).Name), new string[] { "Assets/ScriptableObjects" });

                if(assets == null || assets.Length == 0) {
                    throw new Exception("Tried to access scriptable singleton, but failed to find any!");
                }
                if(assets.Length > 1) {
                    throw new Exception("Tried to access scriptable singleton, but found multiple singletons of the same type!");
                }

                string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                instance = (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
            }

            return instance; 
        }
    }

}
