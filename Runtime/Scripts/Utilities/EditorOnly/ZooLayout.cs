#if UNITY_EDITOR && NAUGHTY
using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using Utilities.Debugging;

namespace GGJ.EditorUtility
{
    public class ZooLayout : MonoBehaviour
    {
        private readonly string[] COLORS = new[]
        {
            "#dc6250",
            "#deada5",
            "#dad4c9",
            "#ffd183",
            "#eeb24a",
            "#55927f",
            "#21525a",
            "#272a32",
            "#2152a5",
            "#5a8bde",
            "#b89ce9",
            "#844790",
        
        };
        [SerializeField]
        private DefaultAsset[] prefabFolders;

        [SerializeField, Min(1f)]
        private float spacing;
        [SerializeField, Min(1)]
        private int maxColumns;

        [SerializeField, HideInInspector]
        private List<GameObject> generatedPrefabs;
        [SerializeField, HideInInspector]
        private List<GameObject> floors;

        [SerializeField, Header("Pedestal")]
        private SpriteRenderer floorSpriteRendererPrefab;
        [SerializeField, Range(-1f, 1f)] 
        private float verticalOffset;

        [SerializeField, HideInInspector]
        private Transform prefabContainer;
        [SerializeField, HideInInspector]
        private Transform floorContainer;
    
        [Button]
        private void GenerateZoo()
        {
            if (prefabFolders == null || prefabFolders.Length == 0)
                return;

            if (prefabContainer == null)
                SetupContainers();
            
            if(generatedPrefabs == null)
                generatedPrefabs = new List<GameObject>();
            
            if(floors == null)
                floors = new List<GameObject>();
            
            ClearZoo();

            PrepareFoldersCollection(ref prefabFolders);
        
            var row = 0;
            var column = 0;
            var colorIndex = 0;
            foreach (var prefabFolder in prefabFolders)
            {
                var folderName = prefabFolder.name;
                var prefabs = GetAllPrefabsIncludingModels(prefabFolder);

                ColorUtility.TryParseHtmlString(COLORS[colorIndex], out var collectionColor);
            
                var folderTransform = new GameObject($"-- {folderName} Folder --").transform;
                folderTransform.SetParent(prefabContainer, false);
                generatedPrefabs.Add(folderTransform.gameObject);
                
                foreach (var prefab in prefabs)
                {
                    var position = new Vector3(column * spacing, 0f, row * spacing);
                    var floor = Instantiate(floorSpriteRendererPrefab, 
                        position + (Vector3.up * verticalOffset), 
                        floorSpriteRendererPrefab.transform.rotation, 
                        floorContainer);
                    floor.gameObject.name = $"{folderName} - Floor[{column}, {row}]";
                    floor.enabled = true;
                    floor.transform.localScale = Vector3.one * spacing;
                    floor.color = collectionColor;
                
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, folderTransform);
                    instance.transform.position = position;
                
                    if (column + 1 >= maxColumns)
                    {
                        column = 0;
                        row++;
                    }
                    else column++;

                    floors.Add(floor.gameObject);
                    generatedPrefabs.Add(instance);
                }

                colorIndex++;
                column = 0;
                row+=2;
            }
        }

        [Button]
        private void ClearZoo()
        {
            for (var i = generatedPrefabs.Count - 1; i >= 0; i--)
            {
                DestroyImmediate(generatedPrefabs[i]);
            }
            generatedPrefabs.Clear();
        
            for (var i = floors.Count - 1; i >= 0; i--)
            {
                DestroyImmediate(floors[i]);
            }
            floors.Clear();
        }

        private static List<GameObject> GetAllPrefabsIncludingModels(DefaultAsset folder)
        {
            var results = new List<GameObject>();

            if (folder == null)
                throw new NullReferenceException();

            var folderPath = AssetDatabase.GetAssetPath(folder);

            var guids = AssetDatabase.FindAssets("t:Prefab t:model", new[] { folderPath });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                    continue;

                var type = PrefabUtility.GetPrefabAssetType(prefab);

                if (type == PrefabAssetType.Regular ||
                    type == PrefabAssetType.Variant ||
                    type == PrefabAssetType.Model)
                {
                    results.Add(prefab);
                }
            }

            return results;
        }

        private void SetupContainers()
        {
            CreateContainer("--- PrefabContainer ---", gameObject.transform, ref prefabContainer);
            CreateContainer("--- FloorContainer ---", gameObject.transform, ref floorContainer);
            
            UnityEditor.EditorUtility.SetDirty(this);

            void CreateContainer(string containerName, Transform parent, ref Transform targetTransform)
            {
                if (targetTransform != null)
                    throw new Exception();
                
                targetTransform = new GameObject(containerName).transform;
                targetTransform.SetParent(parent, false);
            }
        }

        private static void PrepareFoldersCollection(ref DefaultAsset[] folders)
        {
            if(folders == null)
                return;
            
            var cleanedList = folders
                .Where(asset => asset != null)
                .Where(IsFolder)
                .Distinct()
                .ToArray();

            if (cleanedList.Length == folders.Length)
                return;
            
            folders = cleanedList;
            return;
            
            bool IsFolder(DefaultAsset asset)
            {
                var assetPath = AssetDatabase.GetAssetPath(asset);
                return AssetDatabase.IsValidFolder(assetPath);
            }
        }
        
        //================================================================================================================//


        private void OnDrawGizmos()
        {
            if (generatedPrefabs == null)
                return; 
        
            foreach (var generatedPrefab in generatedPrefabs)
            {
                var pos = generatedPrefab.transform.position;
                Draw.Label(pos, generatedPrefab.name);
            }
        }
    }
}
#endif
