using System.IO;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Utilities.Geodesics
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TorusMeshGenerator : MonoBehaviour
    {
#if UNITY_EDITOR
        public float majorRadius = 5f; // R
        public float minorRadius = 2f; // r

        [Range(3, 128)] public int majorSegments = 48; // Around donut
        [Range(3, 64)] public int minorSegments = 24; // Around tube

        [SerializeField, JamStarter.Utilities.Attributes.ReadOnly]
        private Mesh mesh;


        [Button("Generate Torus")]
        void GenerateTorus()
        {
            if(mesh)
                DestroyImmediate(mesh);
        
            mesh = new Mesh();
            Vector3[] vertices = new Vector3[(majorSegments + 1) * (minorSegments + 1)];
            Vector3[] normals = new Vector3[vertices.Length];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[majorSegments * minorSegments * 6];

            int vert = 0;
            int tri = 0;

            for (int i = 0; i <= majorSegments; i++)
            {
                float u = (float)i / majorSegments * Mathf.PI * 2;
                Vector3 center = new Vector3(Mathf.Cos(u), 0, Mathf.Sin(u)) * majorRadius;

                for (int j = 0; j <= minorSegments; j++)
                {
                    float v = (float)j / minorSegments * Mathf.PI * 2;
                    Vector3 normal = new Vector3(Mathf.Cos(u) * Mathf.Cos(v), Mathf.Sin(v), Mathf.Sin(u) * Mathf.Cos(v));
                    Vector3 point = center + normal * minorRadius;

                    vertices[vert] = point;
                    normals[vert] = normal;
                    uv[vert] = new Vector2((float)i / majorSegments, (float)j / minorSegments);
                    vert++;
                }
            }

            for (int i = 0; i < majorSegments; i++)
            {
                for (int j = 0; j < minorSegments; j++)
                {
                    int current = i * (minorSegments + 1) + j;
                    int next = current + minorSegments + 1;

                    triangles[tri++] = current;
                    triangles[tri++] = current + 1;
                    triangles[tri++] = next;

                    triangles[tri++] = current + 1;
                    triangles[tri++] = next + 1;
                    triangles[tri++] = next;
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.name = "Generated Torus";

            mesh.RecalculateBounds();

            var path = EditorUtility.SaveFilePanel("Save Torus Mesh", Application.dataPath, "Torus", "asset");

            if (string.IsNullOrEmpty(path))
                return;
            
            SaveMesh(mesh, path);

            GetComponent<MeshFilter>().mesh = mesh;
        }
    
        private static void SaveMesh(Mesh mesh, string path = "Assets/GeneratedMeshes")
        {
            var relativePath = Path.Join("Assets", path.Replace(Application.dataPath, ""));
            
            AssetDatabase.CreateAsset(mesh, relativePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Saved mesh to: {relativePath}");
        }
#endif
    }
}