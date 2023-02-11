using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UZVR
{
    public class Importer : MonoBehaviour
    {
        private Scene _Scene;
        private MeshFilter MeshTemplate = new MeshFilter();


        void Start()
        {
            _Scene = SceneManager.GetSceneByName("Importer");

            _ImportMap();
        }

        private void _ImportMap()
        {
            var srcPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\_work\\DATA\\Worlds\\_work\\OLDCAMP.3DS";
            var destPath = Application.dataPath + "/OLDCAMP.3DS";
            File.Copy(srcPath, destPath, true);

            _Scene.GetRootGameObjects().Append(new GameObject("Foo"));
        }


        bool done;
        public void Update()
        {
            if (done)
                return;

            var srcPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\_work\\DATA\\Worlds\\_work\\OLDCAMP.3DS";


            var root = new GameObject("Oldcamp");
            _Scene.GetRootGameObjects().Append(root);

            root.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);


            // https://intelligide.github.io/assimp-unity/
            var importer = new Assimp.AssimpContext();
            var scene = importer.ImportFile(srcPath);

            if (scene.MaterialCount != scene.MeshCount)
            {
                throw new Exception("Not yet handled.");
            }

            for (var i = 0; i < scene.MeshCount; i++)
            {
                var meshObj = new GameObject(string.Format("zenGin0.{0,3:000}", i));
                meshObj.transform.parent = root.transform;
                var meshFilter = meshObj.AddComponent<MeshFilter>();
                var meshRenderer = meshObj.AddComponent<MeshRenderer>();

                Assimp.Mesh importedMesh = scene.Meshes[i];
                Assimp.Material importedMaterial = scene.Materials[i];

                _SetMeshFilter(meshFilter, importedMesh);
                _SetMeshRenderer(meshRenderer, importedMaterial);
            }

            done = true;
        }


        private void _SetMeshFilter(MeshFilter meshFilter, Assimp.Mesh importedMesh)
        {
            var mesh = new Mesh();
            meshFilter.mesh = mesh;

            List<Vector3> vertices = new(importedMesh.Vertices.Count);
            foreach (Assimp.Vector3D vertex in importedMesh.Vertices)
            {
                vertices.Add(new Vector3(vertex.X, vertex.Y, vertex.Z));
            }
            mesh.vertices = vertices.ToArray();

            List<int> triangles = new();
            foreach (Assimp.Face face in importedMesh.Faces)
            {
                face.Indices.Reverse();
                triangles.AddRange(face.Indices);
            }
            mesh.triangles = triangles.ToArray();

            List<Vector3> normals = new(importedMesh.Normals.Count);
            foreach (Assimp.Vector3D vertex in importedMesh.Normals)
            {
                normals.Add(new Vector3(vertex.X, vertex.Y, vertex.Z));
            }
            mesh.normals = normals.ToArray();

            List<Vector4> tangents = new(importedMesh.Tangents.Count);
            foreach (Assimp.Vector3D tangent in importedMesh.Tangents)
            {
                tangents.Add(new Vector4(tangent.X, tangent.Y, tangent.Z, 1));
            }
            mesh.tangents = tangents.ToArray();

            if (importedMesh.HasTextureCoords(0))
            {
                List<Vector2> uv = new();
                foreach (Assimp.Vector3D tan in importedMesh.TextureCoordinateChannels[0])
                {
                    uv.Add(new Vector2(tan.X, tan.Y));
                }
                mesh.uv = uv.ToArray();
            }

            mesh.RecalculateBounds();
        }


        private void _SetMeshRenderer(MeshRenderer meshRenderer, Assimp.Material importedMaterial)
        {
            var material = new Material(Shader.Find("Standard"));
            meshRenderer.material = material;

            material.name = importedMaterial.Name;
        }
    }
}