using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KollmannSoftware.Notes {
    public class SceneReferences : MonoBehaviour {
#if UNITY_EDITOR
        private static List<SceneReferences> instances = new List<SceneReferences>();
        private static SceneReferences instance;
        public static SceneReferences Instance {
            get {
                if (instances == null || instances.Count != UnityEditor.SceneManagement.EditorSceneManager.sceneCount) {
                    Refresh();
                }
                instance = instances[0];
                if (instance == null) {
                    Refresh();
                }
                return instance;
            }
        }
        public List<SceneReference> sceneReferences = new List<SceneReference>();
        [System.Serializable]
        public class SceneReference {
            public string guid;
            public GameObject gameObject;

            public SceneReference(GameObject gameObject, string guid) {
                this.gameObject = gameObject;
                this.guid = guid;
            }
        }

        public static void Refresh() {
            instances.Clear();
            var references = FindObjectsOfType<SceneReferences>();
            if (references != null && references.Count() > 0) {
                for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++) {
                    var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i);
                    var existingComponent = references.FirstOrDefault(q => q.gameObject.scene == scene);
                    if (existingComponent != null) {
                        instances.Add(existingComponent);
                    }
                    else {
                        UnityEditor.SceneManagement.EditorSceneManager.SetActiveScene(scene);
                        var sceneReferenceGo = new GameObject("Scene References");
                        sceneReferenceGo.hideFlags = HideFlags.HideInHierarchy;
                        sceneReferenceGo.tag = "EditorOnly";
                        instances.Add(sceneReferenceGo.AddComponent<SceneReferences>());
                    }
                }
            }
            else if (references == null || references.Length == 0) {
                for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++) {
                    var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i);
                    UnityEditor.SceneManagement.EditorSceneManager.SetActiveScene(scene);
                    var sceneReferenceGo = new GameObject("Scene References");
                    sceneReferenceGo.hideFlags = HideFlags.HideInHierarchy;
                    sceneReferenceGo.tag = "EditorOnly";
                    instances.Add(sceneReferenceGo.AddComponent<SceneReferences>());
                }
            }
            instance = instances[0];
            if (instance == null) {
                instance = FindObjectOfType<SceneReferences>();
                if (instance == null) {
                    var sceneReferenceGo = new GameObject("Scene References");
                    sceneReferenceGo.hideFlags = HideFlags.HideInHierarchy;
                    sceneReferenceGo.tag = "EditorOnly";
                    instance = sceneReferenceGo.AddComponent<SceneReferences>();
                }
            }
        }

        public string GetGUIDByGameObject(GameObject go) {
            var referencesComp = instances.FirstOrDefault(q => q.sceneReferences.Any(r => r.gameObject == go));
            if (referencesComp != null) {
                var reference = referencesComp.sceneReferences.FirstOrDefault(r => r.gameObject == go);
                return reference == null ? null : reference.guid;
            }
            return null;
        }

        public GameObject GetGameObjectByGUID(string guid) {
            var referencesComp = instances.FirstOrDefault(q => q.sceneReferences.Any(r => r.guid.Equals(guid)));
            if (referencesComp != null) {
                var reference = referencesComp.sceneReferences.FirstOrDefault(r => r.guid.Equals(guid));
                return reference == null ? null : reference.gameObject;
            }
            return null;
        }

        public bool HasReference(string guid) {
            return instances.Any(q => q.sceneReferences.Any(r => r.guid.Equals(guid)));
        }

        public bool HasReference(GameObject go) {
            return instances.Any(q => q.sceneReferences.Any(r => r.gameObject == go));
        }

        public void RemoveReference(GameObject go) {
            instances.ForEach(q => q.sceneReferences.RemoveAll(r => r.gameObject == go));
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }

        public void RemoveReference(string guid) {
            instances.ForEach(q => q.sceneReferences.RemoveAll(r => r.guid.Equals(guid)));
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }

        public void AddReference(GameObject go, string guid) {
            var reference = new SceneReference(go, guid);
            instances.First(q => q.gameObject.scene == go.scene).sceneReferences.Add(reference);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
        }
#endif
    }
}