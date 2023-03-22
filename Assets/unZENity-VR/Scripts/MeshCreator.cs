using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZVR.Phoenix;

namespace UZVR
{
    public class MeshCreator
    {
        public void Create(GameObject root, PCBridge_World world)
        {
            var meshObj = new GameObject("Mesh");
            meshObj.transform.parent = root.transform;

            for (var materialIndex=0; materialIndex < world.materials.Count; materialIndex++)
            {
                // Material isn't used in this map
                if (world.triangles[materialIndex].Count == 0)
                    continue;

                var subMeshObj = new GameObject(string.Format("submesh-{0}", world.materials[materialIndex].name));
                var meshFilter = subMeshObj.AddComponent<MeshFilter>();
                var meshRenderer = subMeshObj.AddComponent<MeshRenderer>();
                //var meshCollider = subMeshObj.AddComponent<MeshCollider>();

                _PrepareMeshRenderer(meshRenderer, world, materialIndex);
                _PrepareMeshFilter(meshFilter, world, materialIndex);
                //meshCollider.sharedMesh = meshFilter.mesh;

                subMeshObj.transform.localScale = Vector3.one / 100;
                subMeshObj.transform.parent = meshObj.transform;
            }
        }

        private void _PrepareMeshRenderer(MeshRenderer meshRenderer, PCBridge_World world, int materialIndex)
        {
            var standardShader = Shader.Find("Standard");
            var material = new Material(standardShader);

            material.color = world.materials[materialIndex].color;
            meshRenderer.material = material;
        }

        // TODO Put our solution into this post to help others: https://forum.unity.com/threads/remove-vertices-that-are-not-in-triangle-solved.342335/
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
        private void _PrepareMeshFilter(MeshFilter meshFilter, PCBridge_World world, int materialIndex)
        {
            var vertices = world.vertices;
            var triangles = world.triangles[materialIndex];

            var mesh = new Mesh();
            meshFilter.mesh = mesh;

            // Distinct -> We only want to check vertex-indices once for adding to the new mapping
            // Ordered -> We need to check from lowest used vertex-index to highest for the new mapping
            List<int> distinctOrderedTriangles = triangles.Distinct().OrderBy(val => val).ToList();
            Dictionary<int, int> newVertexTriangleMapping = new();
            List<Vector3> newVertices = new();

            // Loop through all the distinctOrderedTriangles
            for (int i=0; i<distinctOrderedTriangles.Count; i++)
            {
                // curVertexIndex == currently lowest vertex index in this loop
                int curVertexIndex = distinctOrderedTriangles[i];
                Vector3 vertexAtIndex = vertices[curVertexIndex];

                // Previously index of vertex is now the new index of loop's >i<.
                // This Dictionary will be used to >replace< the original triangle values later.
                // e.g. previous Vertex-index=5 (key) is now the Vertex-index=0 (value)
                newVertexTriangleMapping.Add(curVertexIndex, i);

                // Add the vertex which was found as new lowest entry
                // e.g. Vertex-index=5 is now Vertex-index=0
                newVertices.Add(vertexAtIndex);
            }

            // Now we replace the triangle values. aka the vertex-indices (value old) with new >mapping< from Dictionary.
            var newTriangles = triangles.Select(originalVertexIndex => newVertexTriangleMapping[originalVertexIndex]);

            mesh.vertices = newVertices.ToArray();
            mesh.triangles = newTriangles.ToArray();
        }

        private void _PrepareMeshRenderer(MeshRenderer meshRenderer, PCBridge_World world)
        {
            var standardShader = Shader.Find("Standard");

            Material[] materials = new Material[world.materials.Count];


            for (int i = 0; i < materials.Length; i++)
            {
                var m = new Material(standardShader);
                m.color = world.materials[i].color;
                materials[i] = m;
            }

            meshRenderer.materials = materials;

            //meshRenderer.materials[0].color = Color.red;
            //meshRenderer.materials[1].color = Color.yellow;
            //meshRenderer.materials[2].color = Color.green;

            //using (var imageFile = File.OpenRead("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\___backup-modding\\GOTHIC MOD Development Kit\\VDFS-Tool\\_WORK\\DATA\\TEXTURES\\DESKTOP\\NOMIP\\INV_SLOT.TGA"))
            //{
            //    var texture = _LoadTGA(imageFile);
            //    foreach (var material in meshRenderer.materials)
            //    {
            //        material.mainTexture = texture;
            //    }
            //}
        }
        private void _PrepareMeshFilter(MeshFilter meshFilter, PCBridge_World world)
        {
            var mesh = new Mesh();
            meshFilter.mesh = mesh;

            mesh.subMeshCount = world.materials.Count;
            mesh.vertices = world.vertices.ToArray();

            for (var materialIndex = 0; materialIndex < world.triangles.Count; materialIndex++)
            {
                mesh.SetTriangles(world.triangles[materialIndex], materialIndex);
            }
        }

        // Credits: https://gist.github.com/mikezila/10557162
        //private Texture2D _LoadTGA(Stream TGAStream)
        //{

        //    using (BinaryReader r = new BinaryReader(TGAStream))
        //    {
        //        // Skip some header info we don't care about.
        //        // Even if we did care, we have to move the stream seek point to the beginning,
        //        // as the previous method in the workflow left it at the end.
        //        r.BaseStream.Seek(12, SeekOrigin.Begin);

        //        short width = r.ReadInt16();
        //        short height = r.ReadInt16();
        //        int bitDepth = r.ReadByte();

        //        // Skip a byte of header information we don't care about.
        //        r.BaseStream.Seek(1, SeekOrigin.Current);

        //        Texture2D tex = new Texture2D(width, height);
        //        Color32[] pulledColors = new Color32[width * height];

        //        if (bitDepth == 32)
        //        {
        //            for (int i = 0; i < width * height; i++)
        //            {
        //                byte red = r.ReadByte();
        //                byte green = r.ReadByte();
        //                byte blue = r.ReadByte();
        //                byte alpha = r.ReadByte();

        //                pulledColors[i] = new Color32(blue, green, red, alpha);
        //            }
        //        }
        //        else if (bitDepth == 24)
        //        {
        //            for (int i = 0; i < width * height; i++)
        //            {
        //                byte red = r.ReadByte();
        //                byte green = r.ReadByte();
        //                byte blue = r.ReadByte();

        //                pulledColors[i] = new Color32(blue, green, red, 1);
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("TGA texture had non 32/24 bit depth.");
        //        }

        //        tex.SetPixels32(pulledColors);
        //        tex.Apply();
        //        return tex;

        //    }
        //}
    }
}
