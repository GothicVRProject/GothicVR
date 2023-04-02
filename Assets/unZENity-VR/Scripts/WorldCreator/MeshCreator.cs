using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZVR.Phoenix.Bridge;
using UZVR.Phoenix.World;
using UZVR.Util;

namespace UZVR.WorldCreator
{
    public class MeshCreator: SingletonBehaviour<MeshCreator>
    {
        public void Create(GameObject root, BWorld world)
        {
            var meshObj = new GameObject("Mesh");
            meshObj.transform.parent = root.transform;

            for (var materialIndex=0; materialIndex < world.materials.Count; materialIndex++)
            {
                // Material isn't used in this world
                if (world.triangles[materialIndex].Count == 0)
                    continue;

                var subMeshObj = new GameObject(string.Format("submesh-{0}", world.materials[materialIndex].name));
                var meshFilter = subMeshObj.AddComponent<MeshFilter>();
                var meshRenderer = subMeshObj.AddComponent<MeshRenderer>();
                var meshCollider = subMeshObj.AddComponent<MeshCollider>();

                PrepareMeshRenderer(meshRenderer, world, materialIndex);
                PrepareMeshFilter(meshFilter, world, materialIndex);
                meshCollider.sharedMesh = meshFilter.mesh;

                subMeshObj.transform.parent = meshObj.transform;
            }
        }

        private void PrepareMeshRenderer(MeshRenderer meshRenderer, BWorld world, int materialIndex)
        {
            var standardShader = Shader.Find("Standard");
            var material = new Material(standardShader);
            var bMaterial = world.materials[materialIndex];

            //material.color = Color.red; // bMaterial.color;
            meshRenderer.material = material;

            var bTexture = TextureBridge.LoadTexture(PhoenixBridge.VdfsPtr, bMaterial.textureName);

            if (bTexture == null)
                return;

            var texture = new Texture2D((int)bTexture.width, (int)bTexture.height, TextureFormat.RGBA32, false);
            texture.name = bMaterial.textureName;
            texture.LoadRawTextureData(bTexture.data.ToArray());

            texture.Apply();

            material.mainTexture = texture;
        }

        /// <summary>
        /// This method is quite complex. What we do is:
        /// We use all the vertices from every mesh and check which ones are used in our submesh (aka are triangles using a vertex?)
        /// We create a new Vertices list and Triangles list based on our changes.
        /// Check the code for technical details.
        /// 
        /// Example:
        ///     vertices  => 0=[...], 1=[...], 2=[...]
        ///     triangles => 0=4, 1=5, 2=8, 3=1, 4=1
        ///     
        ///     distinctOrderedTriangles => 0=1, 1=4, 2=5, 3=8
        ///     
        ///     newVertexTriangleMapping => 1=0, 4=1, 5=2, 8=3
        ///     
        ///     newVertices  => 0=[...], 1=[...], 2=[...]
        ///     newTriangles => 0=1, 1=0, 2=3, 3=0, 4=0 <-- values are replaced with new mapping
        /// </summary>
        private void PrepareMeshFilter(MeshFilter meshFilter, BWorld world, int materialIndex)
        {
            var vertices = world.vertices;
            var textures = world.textures;
            var normals = world.normals;
            var triangles = world.triangles[materialIndex];

            var mesh = new Mesh();
            meshFilter.mesh = mesh;

            // Distinct -> We only want to check vertex-indices once for adding to the new mapping
            // Ordered -> We need to check from lowest used vertex-index to highest for the new mapping
            List<int> distinctOrderedTriangles = triangles.Distinct().OrderBy(val => val).ToList();
            Dictionary<int, int> newVertexTriangleMapping = new();
            List<Vector3> newVertices = new();
            List<Vector2> newTextures = new();
            List<Vector3> newNormals = new();

            // Loop through all the distinctOrderedTriangles
            for (int i = 0; i < distinctOrderedTriangles.Count; i++)
            {
                // curVertexIndex == currently lowest vertex index in this loop
                int curVertexIndex = distinctOrderedTriangles[i];
                Vector3 vertexAtIndex = vertices[curVertexIndex];
                Vector2 textureAtIndex = textures[curVertexIndex];
                Vector3 normalAtIndex = normals[curVertexIndex];

                // Previously index of vertex is now the new index of loop's >i<.
                // This Dictionary will be used to >replace< the original triangle values later.
                // e.g. previous Vertex-index=5 (key) is now the Vertex-index=0 (value)
                newVertexTriangleMapping.Add(curVertexIndex, i);

                // Add the vertex which was found as new lowest entry
                // e.g. Vertex-index=5 is now Vertex-index=0
                newVertices.Add(vertexAtIndex);
                newTextures.Add(textureAtIndex);
                newNormals.Add(normalAtIndex);
            }

            // Now we replace the triangle values. aka the vertex-indices (value old) with new >mapping< from Dictionary.
            var newTriangles = triangles.Select(originalVertexIndex => newVertexTriangleMapping[originalVertexIndex]);

            mesh.SetVertices(newVertices);
            mesh.SetTriangles(newTriangles.ToArray(), 0);
            mesh.SetUVs(0, newTextures);
            mesh.SetNormals(newNormals);
        }
    }
}
