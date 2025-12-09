#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utilities.EditorOnly
{
    /// <summary>
    /// This script will attempt to add a prefab by GUID if it exists, promising that it will become instantiated in the
    /// scene. Otherwise, if that prefab does not exist it will continue to check once every second. This GameObject will
    /// destroy itself in playmode & only runs in editor.
    /// </summary>
    [ExecuteInEditMode]
    public class AddPrefabIfExists : MonoBehaviour
    {
        [SerializeField]
        private string prefabGuid;

        private double m_counter;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Update()
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
                return;
            }

            if (EditorApplication.timeSinceStartup - m_counter < 1.0)
                return;

            m_counter = EditorApplication.timeSinceStartup;
        
            if (string.IsNullOrWhiteSpace(prefabGuid))
                return;

            var prefabAsset = AssetDatabase.LoadAssetByGUID<GameObject>(new GUID(prefabGuid));
            if (prefabAsset == null)
                return;

            PrefabUtility.InstantiatePrefab(prefabAsset, gameObject.scene);
        
            DestroyImmediate(gameObject);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); 
        }
    }
}
#endif
