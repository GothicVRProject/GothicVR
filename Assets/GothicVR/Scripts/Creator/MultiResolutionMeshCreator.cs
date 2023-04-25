using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Mesh;
using PxCs.Data.Struct;
using PxCs.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GVR.Creator
{
    public class MultiResolutionMeshCreator : SingletonBehaviour<MultiResolutionMeshCreator>
    {
        // Cache helped speed up loading of G1 world textures from 870ms to 230 (~75% speedup)
        private Dictionary<string, Texture2D> cachedTextures = new();

        public GameObject Create(PxMultiResolutionMeshData mrm, GameObject? parent, string objectName, Vector3 position, PxMatrix3x3Data rotation)
        {
            var meshObj = new GameObject(objectName);

            var meshFilter = meshObj.AddComponent<MeshFilter>();
            var meshRenderer = meshObj.AddComponent<MeshRenderer>();
            var meshCollider = meshObj.AddComponent<MeshCollider>();

            try {
                PrepareMeshRenderer(meshRenderer, mrm);
                PrepareMeshFilter(meshFilter, mrm);
                meshCollider.sharedMesh = meshFilter.mesh;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogError(e.Message);
                Destroy(meshObj);
            }

            // Rotations from Gothic are a 3x3 matrix.
            // According to this blog post, we can leverage it to be used the right way automatically:
            // @see https://forum.unity.com/threads/convert-3x3-rotation-matrix-to-euler-angles.1086392/#post-7002275
            // Hint 1: The matrix is transposed, i.e. we needed to change e.g. m01=[0,1] to m01=[1,0]
            // Hint 2: m33 needs to be 1
            var matrix4x4 = new Matrix4x4();
            matrix4x4.m00 = rotation.m00;
            matrix4x4.m01 = rotation.m10;
            matrix4x4.m02 = rotation.m20;
            matrix4x4.m10 = rotation.m01;
            matrix4x4.m11 = rotation.m11;
            matrix4x4.m12 = rotation.m21;
            matrix4x4.m20 = rotation.m02;
            matrix4x4.m21 = rotation.m12;
            matrix4x4.m22 = rotation.m22;
            matrix4x4.m33 = 1;
            meshObj.transform.rotation = matrix4x4.rotation;
            meshObj.transform.position = position;

            if (null != parent)
                meshObj.transform.parent = parent.transform;

            return meshObj;
        }

        private void PrepareMeshRenderer(MeshRenderer meshRenderer, PxMultiResolutionMeshData mrmData)
        {
            var finalMaterials = new List<Material>(mrmData.subMeshes.Length);

            foreach (var subMesh in mrmData.subMeshes)
            {
                var standardShader = Shader.Find("Standard");
                var material = new Material(standardShader);
                var materialData = subMesh.material;

                meshRenderer.material = material;

                // No texture to add.
                if (materialData.texture == "")
                    return;

                // Load texture for the first time.
                if (!cachedTextures.TryGetValue(materialData.texture, out Texture2D cachedTexture))
                {
                    // FIXME - There might be more textures to load compressed. Please check for sake of performance!
                    var pxTexture = PxTexture.GetTextureFromVdf(
                        PhoenixBridge.VdfsPtr,
                        materialData.texture,
                        PxTexture.Format.tex_dxt1, PxTexture.Format.tex_dxt5);

                    // No texture found
                    if (pxTexture == null)
                    {
                        Debug.LogWarning($"Texture {materialData.texture} couldn't be found.");
                        return;
                    }

                    var format = pxTexture.format.AsUnityTextureFormat();
                    if (format == 0)
                    {
                        Debug.LogWarning($"Format >{pxTexture.format}< is not supported or not yet tested to work with Unity:");
                        return;
                    }

                    var texture = new Texture2D((int)pxTexture.width, (int)pxTexture.height, format, (int)pxTexture.mipmapCount, false);

                    texture.name = materialData.texture;

                    for (var i = 0u; i < pxTexture.mipmapCount; i++)
                    {
                        texture.SetPixelData(pxTexture.mipmaps[i].mipmap, (int)i);
                    }

                    texture.Apply();

                    cachedTextures.Add(materialData.texture, texture);
                    cachedTexture = texture;
                }

                material.mainTexture = cachedTexture;

                finalMaterials.Add(material);
            }

            meshRenderer.SetMaterials(finalMaterials);
        }

        private void PrepareMeshFilter(MeshFilter meshFilter, PxMultiResolutionMeshData mrmData)
        {
            /**
             * Ok, brace yourself:
             * There are three parameters of interest when it comes to creating meshes for items (etc.).
             * 1. positions - Unity: vertices (=Vector3)
             * 2. triangles - contains 3 indices to wedges.
             * 3. wedges - contains indices (Unity: triangles) to the positions (Unity: vertices) and textures (Unity: uvs (=Vector2)).
             * 
             * Data example:
             *  positions: 0=>[x1,x2,x3], 0=>[x2,y2,z2], 0=>[x3,y3,z3]
             *  submesh:
             *    triangles: [0, 2, 1], [1, 2, 3]
             *    wedges: 0=>[index=0, texture=...], 1=>[index=2, texture=...], 2=>[index=2, texture=...]
             *  
             *  If we now take first triangle and prepare it for Unity, we would get the following:
             *  vertices = 0[x0,...], 2[x2,...], 1[x1,...] --> as triangles point to a wedge and wedge index points to the position-index itself.
             *  triangles = 0, 2, 3 --> (indices for position items); ATTENTION: index 3 would normally be index 2, but! we can never reuse positions. We always need to create new ones. (Reason: uvs demand the same size as vertices.)
             *  uvs = [wedge[0].texture], [wedge[2].texture], [wedge[1].texture]
             */
            var mesh = new Mesh();
            meshFilter.mesh = mesh;
            mesh.subMeshCount = mrmData.subMeshes.Length;

            var verticesAndUvSize = mrmData.subMeshes.Sum(i => i.triangles.Length) * 3;
            var preparedVertices = new List<Vector3>(verticesAndUvSize);
            var preparedUVs = new List<Vector2>(verticesAndUvSize);

            // 2-dimensional arrays (as there are segregated by submeshes)
            var preparedTriangles = new List<List<int>>(mrmData.subMeshes.Length);

            foreach (var subMesh in mrmData.subMeshes)
            {
                var vertices = mrmData.positions;
                var triangles = subMesh.triangles;
                var wedges = subMesh.wedges;

                // every triangle is attached to a new vertex.
                // Therefore new submesh triangles start referencing their vertices with an offset from previous runs.
                var verticesIndexOffset = preparedVertices.Count;

                var subMeshTriangles = new List<int>(triangles.Length * 3);
                for (var i = 0; i < triangles.Length; i++)
                {
                    // One triangle is made of 3 elements for Unity. We therefore need to prepare 3 elements within one loop.
                    var preparedIndex = i * 3 + verticesIndexOffset;

                    var index1 = wedges[triangles[i].c];
                    var index2 = wedges[triangles[i].b];
                    var index3 = wedges[triangles[i].a];

                    preparedVertices.Add(vertices[index1.index].ToUnityVector());
                    preparedVertices.Add(vertices[index2.index].ToUnityVector());
                    preparedVertices.Add(vertices[index3.index].ToUnityVector());

                    subMeshTriangles.Add(preparedIndex);
                    subMeshTriangles.Add(preparedIndex + 1);
                    subMeshTriangles.Add(preparedIndex + 2);

                    preparedUVs.Add(index1.texture.ToUnityVector());
                    preparedUVs.Add(index2.texture.ToUnityVector());
                    preparedUVs.Add(index3.texture.ToUnityVector());
                }
                preparedTriangles.Add(subMeshTriangles);
            }

            // Unity 1/ handles vertices on mesh level, but triangles (aka vertex-indices) on submesh level.
            // and 2/ demands vertices to be stored before triangles/uvs.
            // Therefore we prepare the full data once and assign it afterwards.
            // @see: https://answers.unity.com/questions/531968/submesh-vertices.html
            mesh.SetVertices(preparedVertices);
            mesh.SetUVs(0, preparedUVs);
            for (var i = 0; i < mrmData.subMeshes.Length; i++)
            {
                mesh.SetTriangles(preparedTriangles[i], i);
            }
        }
    }
}
