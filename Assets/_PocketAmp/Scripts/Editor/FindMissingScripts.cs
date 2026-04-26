using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoftAware.Editor
{
    public class FindMissingScripts : EditorWindow
    {
        [MenuItem("Tools/PocketAmp/Find Missing Scripts in Scene")]
        public static void FindInScene()
        {
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            var count = 0;
            
            foreach (var root in rootObjects)
            {
                count += FindInGameObject(root);
            }
            
            Debug.Log($"[FindMissingScripts] Finished scanning. Found {count} missing scripts.");
        }
        
        private static int FindInGameObject(GameObject go)
        {
            var count = 0;
            var components = go.GetComponents<Component>();
            
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogError($"Missing script found on GameObject: {GetFullPath(go)}", go);
                    count++;
                }
            }
            
            foreach (Transform child in go.transform)
            {
                count += FindInGameObject(child.gameObject);
            }
            
            return count;
        }
        
        private static string GetFullPath(GameObject go)
        {
            var path = go.name;
            while (go.transform.parent != null)
            {
                go = go.transform.parent.gameObject;
                path = go.name + "/" + path;
            }
            return path;
        }
    }
}
