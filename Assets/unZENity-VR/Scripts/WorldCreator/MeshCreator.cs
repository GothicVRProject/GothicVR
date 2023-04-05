using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZVR.Phoenix.Bridge;
using UZVR.Phoenix.World;
using UZVR.Util;
using static UZVR.Phoenix.World.BWorld;

namespace UZVR.WorldCreator
{
    public class MeshCreator: SingletonBehaviour<MeshCreator>
    {
        public void Create(GameObject root, BWorld world)
        {
            var meshObj = new GameObject("Mesh");
            meshObj.transform.parent = root.transform;


            // DEBUG - values are there in the right order...
            var vValue = world.featureTextures.First(i => i == new Vector2(1.91805f, 1.07236f));
            var vIndex = world.featureTextures[281449];

            // DEBUG Textures
            {
                var debug = new GameObject("DEBUG");
                var meshFilter = debug.AddComponent<MeshFilter>();
                var meshRenderer = debug.AddComponent<MeshRenderer>();

                var standardShader = Shader.Find("Standard");
                var material = new Material(standardShader);
                var bTexture = TextureBridge.LoadTexture(PhoenixBridge.VdfsPtr, "OWODWAT_A0.TGA");

                meshRenderer.material = material;

                var texture = new Texture2D((int)bTexture.width, (int)bTexture.height, bTexture.GetUnityTextureFormat(), false);
                texture.name = "OWODWAT_A0.TGA";
                texture.LoadRawTextureData(bTexture.data.ToArray());

                texture.Apply();

                material.mainTexture = texture;
                debug.transform.parent = meshObj.transform;


                var mesh = new Mesh();
                meshFilter.mesh = mesh;
                mesh.SetVertices(new Vector3[]
                {
                    new(76.33f, -2.28f, -10.25f),
                    new(67.82f, 18.91f, -10.25f),
                    new(61.26f, -8.07f, -10.25f),
                    new(52.76f, 13.12f, -10.25f),
                    //new(76.33f, -2.28f, -10.25f),
                    //new(61.26f, -8.07f, -10.25f),
                    //new(67.82f, 18.91f, -10.25f),
                    //new(52.76f, 13.12f, -10.25f)
                });
                mesh.SetTriangles(new int[]
                {
                    0, 1, 2, 3, 2, 1, //4, 5, 6, 7, 6, 5
                }, 0);
                mesh.SetUVs(0, new Vector2[]
                {
                    new(2.31f, -11.92f),
                    new(5.90f, -10.48f),
                    new(1.33f, -9.37f) ,
                    new(4.92f, -7.93f) ,

                    //new(2.31f, 13.92f) ,
                    //new(1.33f, 11.37f) ,
                    //new(5.90f, 12.48f) ,
                    //new(4.92f, 9.93f)  ,
                });
//                mesh.SetNormals(newNormals);
            }



            foreach (var subMesh in world.subMeshes.Values)
            {
                var subMeshObj = new GameObject(string.Format("submesh-{0}", subMesh.material.name));
                var meshFilter = subMeshObj.AddComponent<MeshFilter>();
                var meshRenderer = subMeshObj.AddComponent<MeshRenderer>();
                var meshCollider = subMeshObj.AddComponent<MeshCollider>();

                PrepareMeshRenderer2(meshRenderer, subMesh);
                PrepareMeshFilter2(meshFilter, subMesh);
                meshCollider.sharedMesh = meshFilter.mesh;

                subMeshObj.transform.parent = meshObj.transform;
            }
        }

        private void PrepareMeshRenderer2(MeshRenderer meshRenderer, BSubMesh subMesh)
        {
            var standardShader = Shader.Find("Standard");
            var material = new Material(standardShader);
            var bMaterial = subMesh.material;

            //material.color = Color.red; // bMaterial.color;
            meshRenderer.material = material;

            var bTexture = TextureBridge.LoadTexture(PhoenixBridge.VdfsPtr, bMaterial.textureName);

            if (bTexture == null)
                return;

            var texture = new Texture2D((int)bTexture.width, (int)bTexture.height, bTexture.GetUnityTextureFormat(), false);
            texture.name = bMaterial.textureName;
            texture.LoadRawTextureData(bTexture.data.ToArray());

            texture.Apply();

            material.mainTexture = texture;
        }

        private void PrepareMeshFilter2(MeshFilter meshFilter, BSubMesh subMesh)
        {
            var mesh = new Mesh();
            meshFilter.mesh = mesh;

            mesh.SetVertices(subMesh.vertices);
            mesh.SetTriangles(subMesh.triangles, 0);
            mesh.SetUVs(0, subMesh.uvs);
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

            var texture = new Texture2D((int)bTexture.width, (int)bTexture.height, bTexture.GetUnityTextureFormat(), false);
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
            var featureIndices = world.featureIndices;
            var textures = world.featureTextures;
            var normals = world.featureNormals;
            var matTriangles = world.materializedTriangles[materialIndex];
            var triangles = world.vertexIndices;

            var mesh = new Mesh();
            meshFilter.mesh = mesh;

            // Distinct -> We only want to check vertex-indices once for adding to the new mapping
            // Ordered -> We need to check from lowest used vertex-index to highest for the new mapping
            List<uint> distinctOrderedTriangles = matTriangles.Distinct().OrderBy(val => val).ToList();
            Dictionary<int, int> newVertexTriangleMapping = new();
            List<Vector3> newVertices = new();
            List<Vector2> newTextures = new();
            List<Vector3> newNormals = new();

            // Loop through all the distinctOrderedTriangles
            for (int i = 0; i < distinctOrderedTriangles.Count; i++)
            {
                // curVertexIndex == currently lowest vertex index in this loop
                int curVertexIndex = (int)distinctOrderedTriangles[i];
                Vector3 vertexAtIndex = vertices[(int)curVertexIndex];
                uint curFeatureIndex = featureIndices[(int)curVertexIndex * 3];
                Vector2 textureAtIndex = textures[(int)curFeatureIndex];
                Vector3 normalAtIndex = normals[(int)curFeatureIndex];

                // Previously index of vertex is now the new index of loop's >i<.
                // This Dictionary will be used to >replace< the original triangle values later.
                // e.g. previous Vertex-index=5 (key) is now the Vertex-index=0 (value)
                newVertexTriangleMapping.Add((int)curVertexIndex, i);

                // Add the vertex which was found as new lowest entry
                // e.g. Vertex-index=5 is now Vertex-index=0
                newVertices.Add(vertexAtIndex);
                newTextures.Add(textureAtIndex);
                newNormals.Add(normalAtIndex);
            }

            // Now we replace the triangle values. aka the vertex-indices (value old) with new >mapping< from Dictionary.
            var newTriangles = matTriangles.Select(originalVertexIndex => newVertexTriangleMapping[(int)originalVertexIndex]).ToArray();
            var newFlippedTriangles = new List<int>(newTriangles.Count());

            // We need to flip valueA with valueC to:
            // 1/ have the mesh elements shown (flipped surface) and
            // 2/ world mirrored right way.
            for (var i = 0; i < newTriangles.Count(); i+=3)
            {
                newFlippedTriangles.Add(newTriangles[i+2]);
                newFlippedTriangles.Add(newTriangles[i+1]);
                newFlippedTriangles.Add(newTriangles[i]);
            }

            mesh.SetVertices(newVertices);
            mesh.SetTriangles(newFlippedTriangles, 0);
            //mesh.SetUVs(0, newTextures);
            //mesh.SetNormals(newNormals);
        }
    }
}
