using PxCs;
using PxCs.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZVR.Demo;
using UZVR.Phoenix.Data;
using UZVR.Phoenix.Interface;
using UZVR.Phoenix.Util;
using UZVR.Util;

namespace UZVR.Creator
{
    public class VobCreator: SingletonBehaviour<VobCreator>
    {
        // Cache helped speed up loading of G1 world textures from 870ms to 230 (~75% speedup)
        private Dictionary<string, Texture2D> cachedTextures = new();


        public void Create(GameObject root, WorldData world)
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().CreateVobs)
                return;

            var itemVobs = GetFlattenedVobsByType(world.vobs, PxWorld.PxVobType.PxVob_oCItem);
            var vobRootObj = new GameObject("Vobs");
            vobRootObj.transform.parent = root.transform;

//            var DEBUG_ELEMENTS = itemVobs.Take(5);

            foreach (var vob in itemVobs)
            {
                // FIXME: Add caching of MRM as object will be created multiple times inside a scene.
                var mrm = PxMRM.GetMRMFromVdf(PhoenixBridge.VdfsPtr, $"{vob.vobName}.MRM");

                if (mrm == null)
                {
                    Debug.LogError($"MultiResolutionModel (MRM) >{vob.vobName}.MRM< not found.");
                    continue;
                }

                var meshObj = new GameObject(string.Format("vob-{0}", vob.vobName));
                var meshFilter = meshObj.AddComponent<MeshFilter>();
                var meshRenderer = meshObj.AddComponent<MeshRenderer>();
                var meshCollider = meshObj.AddComponent<MeshCollider>();

                try
                {
                    PrepareMeshRenderer(meshRenderer, mrm);
                    PrepareMeshFilter(meshFilter, mrm);
                    meshCollider.sharedMesh = meshFilter.mesh;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Debug.LogError(e.Message);
                    Destroy(meshObj);
                    continue;
                }

                meshObj.transform.position = vob.position.ToUnityVector();
                meshObj.transform.parent = vobRootObj.transform;
            }

            // Currently we don't need to store cachedTextures once the world is loaded.
            cachedTextures.Clear();
        }

        /// <summary>
        /// Convenient method to return specific vob elements in recursive list of PxVobData.childVobs...
        /// </summary>
        private List<PxVobData> GetFlattenedVobsByType(PxVobData[] vobsToFilter, PxWorld.PxVobType type)
        {
            var returnVobs = new List<PxVobData>();
            for (var i = 0; i < vobsToFilter.Length; i++)
            {
                var curVob = vobsToFilter[i];
                if (curVob.type == type)
                    returnVobs.Add(curVob);

                returnVobs.AddRange(GetFlattenedVobsByType(curVob.childVobs, type));
            }

            return returnVobs;
        }

        private void PrepareMeshRenderer(MeshRenderer meshRenderer, PxMRMData mrmData)
        {
            if (mrmData.materials.Length != 1)
                throw new ArgumentOutOfRangeException("Currently it's only supported to have exact 1 material for VobMRMs.");

            var standardShader = Shader.Find("Standard");
            var material = new Material(standardShader);
            var materialData = mrmData.materials.First();

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
        }

        private void PrepareMeshFilter(MeshFilter meshFilter, PxMRMData mrmData)
        {
            var mesh = new Mesh();
            meshFilter.mesh = mesh;

            if (mrmData.subMeshes.Length != 1)
                throw new ArgumentOutOfRangeException("Currently it's only supported to have exact 1 subMesh for VobMRMs.");

            var vertices = mrmData.positions;
            var triangles = mrmData.subMeshes.First().triangles;
            var wedges = mrmData.subMeshes.First().wedges;


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
             *  triangles = 0, 2, 3 --> 3 would normally be 2, but! we can never reuse positions. We always need to create new ones. (Reason: uvs demand the same size as vertices.)
             *  uvs = [wedge[0].texture], [wedge[2].texture], [wedge[1].texture]
             */

            // Size is predictable:
            // 1. vertices.count == triangleIndices.size == uv.size
            // 2. triangles from Phoenix are 3 indices. Therefore final Unity size is *3
            var preparedVertices = new Vector3[triangles.Length * 3];
            var preparedTriangles = new int[triangles.Length * 3];
            var preparedUVs = new Vector2[triangles.Length * 3];

            for (var i = 0; i < triangles.Length; i++)
            {
                // One triangle is made of 3 elements for Unity. We therefore need to prepare 3 elements within one loop.
                var preparedIndex = i * 3;

                var index1 = wedges[triangles[i].c];
                var index2 = wedges[triangles[i].b];
                var index3 = wedges[triangles[i].a];

                preparedVertices[preparedIndex] = vertices[index1.index].ToUnityVector();
                preparedVertices[preparedIndex + 1] = vertices[index2.index].ToUnityVector();
                preparedVertices[preparedIndex + 2] = vertices[index3.index].ToUnityVector();

                preparedTriangles[preparedIndex] = preparedIndex;
                preparedTriangles[preparedIndex + 1] = preparedIndex +1;
                preparedTriangles[preparedIndex + 2] = preparedIndex +2;

                preparedUVs[preparedIndex] = index1.texture.ToUnityVector();
                preparedUVs[preparedIndex + 1] = index2.texture.ToUnityVector();
                preparedUVs[preparedIndex + 2] = index3.texture.ToUnityVector();
            }

            mesh.SetVertices(preparedVertices);
            mesh.SetTriangles(preparedTriangles, 0);
            mesh.SetUVs(0, preparedUVs);
        }
    }
}
