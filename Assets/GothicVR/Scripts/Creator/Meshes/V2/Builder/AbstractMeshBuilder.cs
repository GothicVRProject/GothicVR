using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using GVR.Globals;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using ZenKit;
using Material = UnityEngine.Material;
using TextureFormat = UnityEngine.TextureFormat;
using Mesh = UnityEngine.Mesh;

namespace GVR.Creator.Meshes.V2.Builder
{
    public abstract class AbstractMeshBuilder
    {
        protected GameObject RootGo;
        protected GameObject ParentGo;
        protected bool HasMeshCollider = true;
        protected bool UseTextureArray;

        protected IMultiResolutionMesh Mrm;
        protected IModelHierarchy Mdh;
        protected IModelMesh Mdm;
        protected IMorphMesh Mmb;

        protected Vector3 RootPosition;
        protected Quaternion RootRotation;
        
        protected bool isMorphMeshMappingAlreadyCached;


        public abstract GameObject Build();


#region Setter
        public void SetGameObject([CanBeNull] GameObject rootGo, string name = null)
        {
            RootGo = rootGo == null ? new GameObject() : rootGo;

            if (name != null)
            {
                RootGo.name = name;
            }
        }

        public void SetParent([CanBeNull] GameObject parentGo, bool resetPosition = false, bool resetRotation = false)
        {
            if (parentGo == null)
            {
                return;
            }

            ParentGo = parentGo;
            RootGo.SetParent(parentGo, resetPosition, resetRotation);
        }

        public void SetRootPosAndRot(Vector3 position = default, Quaternion rotation = default)
        {
            RootPosition = position;
            RootRotation = rotation;
        }

        public void SetMdl(IModel mdl)
        {
            SetMdh(mdl.Hierarchy);
            SetMdm(mdl.Mesh);
        }

        public void SetMdh(string mdhName)
        {
            Mdh = AssetCache.TryGetMdh(mdhName);

            if (Mdh == null)
            {
                Debug.LogError($"MDH from name >{mdhName}< for object >{RootGo.name}< not found.");
            }
        }

        public void SetMdh(IModelHierarchy mdh)
        {
            this.Mdh = mdh;
        }

        public void SetMdm(string mdmName)
        {
            Mdm = AssetCache.TryGetMdm(mdmName);

            if (Mdm == null)
            {
                Debug.LogError($"MDH from name >{mdmName}< for object >{RootGo.name}< not found.");
            }
        }

        public void SetMdm(IModelMesh mdm)
        {
            this.Mdm = mdm;
        }

        public void SetMrm(IMultiResolutionMesh mrm)
        {
            this.Mrm = mrm;
        }

        public void SetMmb(IMorphMesh mmb)
        {
            this.Mmb = mmb;
        }

        public void SetMrm(string mrmName)
        {
            Mrm = AssetCache.TryGetMrm(mrmName);

            if (Mrm == null)
            {
                Debug.LogError($"MDH from name >{mrmName}< for object >{RootGo.name}< not found.");
            }
        }

        /// <summary>
        /// MorphMesh
        /// </summary>
        public void SetMmb(string mmbName)
        {
            Mmb = AssetCache.TryGetMmb(mmbName);

            if (Mmb == null)
            {
                Debug.LogError($"MDH from name >{mmbName}< for object >{RootGo.name}< not found.");
            }
        }

        /// <summary>
        /// Only a few objects (vobObjects) have disabled MeshColliders.
        /// </summary>
        public void DisableMeshCollider()
        {
            HasMeshCollider = false;
        }

        /// <summary>
        /// e.g. Vobs and world will use Texture array, but no NPCs or created Vobs after world loading.
        /// </summary>
        public void SetUseTextureArray(bool use)
        {
            UseTextureArray = use;
        }
#endregion


        protected void BuildViaMrm()
        {
            MeshFilter meshFilter = RootGo.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = RootGo.AddComponent<MeshRenderer>();
            meshRenderer.material = Constants.LoadingMaterial;
            PrepareMeshFilter(meshFilter, Mrm, meshRenderer);

            // If we use TextureArray, we apply textures later.
            // But if we want to create the mrm based texture now, we do it now. ;-)
            if (!UseTextureArray)
            {
                PrepareMeshRenderer(meshRenderer, Mrm);
            }

            if (HasMeshCollider)
            {
                PrepareMeshCollider(RootGo, meshFilter.sharedMesh, Mrm.Materials);
            }

            SetPosAndRot(RootGo, RootPosition, RootRotation);
        }

        protected void BuildViaMdmAndMdh()
        {
            var nodeObjects = new GameObject[Mdh.Nodes.Count];

            // Create empty GameObjects from hierarchy
            {
                for (var i = 0; i < Mdh.Nodes.Count; i++)
                {
                    var node = Mdh.Nodes[i];
                    // We attached some Components to root of bones. Therefore reusing it.
                    if (node.Name.EqualsIgnoreCase("BIP01"))
                    {
                        var bip01 = RootGo.FindChildRecursively("BIP01");
                        if (bip01 != null)
                            nodeObjects[i] = RootGo.FindChildRecursively("BIP01");
                        else
                            nodeObjects[i] = new GameObject(Mdh.Nodes[i].Name);
                    }
                    else
                    {
                        nodeObjects[i] = new GameObject(Mdh.Nodes[i].Name);
                    }
                }

                // Now set parents
                for (var i = 0; i < Mdh.Nodes.Count; i++)
                {
                    var node = Mdh.Nodes[i];
                    var nodeObj = nodeObjects[i];

                    SetPosAndRot(nodeObj, node.Transform);

                    if (node.ParentIndex == -1)
                        nodeObj.SetParent(RootGo);
                    else
                        nodeObj.SetParent(nodeObjects[node.ParentIndex]);
                }

                for (var i = 0; i < nodeObjects.Length; i++)
                {
                    if (Mdh.Nodes[i].ParentIndex == -1)
                        nodeObjects[i].transform.localPosition = Mdh.RootTranslation.ToUnityVector();
                    else
                        SetPosAndRot(nodeObjects[i], Mdh.Nodes[i].Transform);
                }
            }

            //// Fill GameObjects with Meshes from "original" Mesh
            var meshCounter = 0;
            foreach (var softSkinMesh in Mdm.Meshes)
            {
                var mesh = softSkinMesh.Mesh;

                var meshObj = new GameObject($"ZM_{meshCounter++}");
                meshObj.SetParent(RootGo);

                var meshFilter = meshObj.AddComponent<MeshFilter>();
                var meshRenderer = meshObj.AddComponent<SkinnedMeshRenderer>();

                // FIXME - hard coded as it's the right value for BSFire. Need to be more dynamic by using element which has parent=-1.
                meshRenderer.rootBone = nodeObjects[0].transform;

                PrepareMeshRenderer(meshRenderer, mesh);
                PrepareMeshFilter(meshFilter, softSkinMesh);

                meshRenderer.sharedMesh = meshFilter.mesh;

                CreateBonesData(RootGo, nodeObjects, meshRenderer, softSkinMesh);
            }

            Dictionary<string, IMultiResolutionMesh> attachments = GetFilteredAttachments(Mdm.Attachments);

            // Fill GameObjects with Meshes from attachments
            foreach (KeyValuePair<string, IMultiResolutionMesh> subMesh in attachments)
            {
                GameObject meshObj = nodeObjects.First(bone => bone.name == subMesh.Key);
                MeshFilter meshFilter = meshObj.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = meshObj.AddComponent<MeshRenderer>();
                meshRenderer.material = Constants.LoadingMaterial;

                PrepareMeshFilter(meshFilter, subMesh.Value, meshRenderer);

                if (!UseTextureArray)
                {
                    PrepareMeshRenderer(meshRenderer, subMesh.Value);
                }
                
                PrepareMeshCollider(meshObj, meshFilter.sharedMesh, subMesh.Value.Materials);
            }

            SetPosAndRot(RootGo, RootPosition, RootRotation);

            // We need to set the root translation after we add children above. Otherwise the "additive" position/rotation will be broken.
            // We need to reset the rootBones position to zero. Otherwise Vobs won't be placed right.
            // Due to Unity's parent-child transformation magic, we need to do it at the end. ╰(*°▽°*)╯
            nodeObjects[0].transform.localPosition = Vector3.zero;
        }

        protected GameObject BuildViaMmb()
        {
            var meshFilter = RootGo.AddComponent<MeshFilter>();
            var meshRenderer = RootGo.AddComponent<MeshRenderer>();
            
            PrepareMeshFilter(meshFilter, Mmb.Mesh, meshRenderer);
            PrepareMeshRenderer(meshRenderer, Mmb.Mesh);
            
            SetPosAndRot(RootGo, RootPosition, RootRotation);
            
            return RootGo;
        }

        protected void PrepareMeshRenderer(Renderer rend, IMultiResolutionMesh mrmData)
        {
            if (null == mrmData)
            {
                Debug.LogError("No mesh data could be added to renderer: " + rend.transform.parent.name);
                return;
            }

            if (rend is MeshRenderer && !rend.GetComponent<MeshFilter>().sharedMesh)
            {
                Debug.LogError($"Null mesh on {rend.gameObject.name}");
                return;
            }

            List<Material> finalMaterials = new List<Material>(mrmData.SubMeshes.Count);
            int submeshCount = rend is MeshRenderer ? rend.GetComponent<MeshFilter>().sharedMesh.subMeshCount : mrmData.SubMeshCount;

            for (int i = 0; i < submeshCount; i++)
            {
                IMaterial materialData = mrmData.SubMeshes[i].Material;
                if (materialData.Texture.IsEmpty()) // No texture to add.
                {
                    Debug.LogWarning("No texture was set for: " + materialData.Name);
                    return;
                }

                UnityEngine.Texture texture = GetTexture(materialData.Texture);
                if (!texture)
                {
                    if (materialData.Texture.EndsWithIgnoreCase(".TGA"))
                    {
                        Debug.LogError("This is supposed to be a decal: " + materialData.Texture);
                    }
                    else
                    {
                        Debug.LogError("Couldn't get texture from name: " + materialData.Texture);
                    }
                }

                Material material = GetDefaultMaterial(texture && ((Texture2D)texture).format == TextureFormat.RGBA32);

                material.mainTexture = texture;
                rend.material = material;
                finalMaterials.Add(material);
            }

            rend.SetMaterials(finalMaterials);
        }

        protected void PrepareMeshFilter(MeshFilter meshFilter, IMultiResolutionMesh mrmData, MeshRenderer meshRenderer)
        {
            Mesh mesh = new Mesh();
            meshFilter.mesh = mesh;

            if (null == mrmData)
            {
                Debug.LogError("No mesh data could be added to filter: " + meshFilter.transform.parent.name);
                return;
            }

            CreateMorphMeshBegin(mrmData, mesh);

            int triangleCount = mrmData.SubMeshes.Sum(i => i.Triangles.Count);
            int vertexCount = triangleCount * 3;
            List<Vector3> preparedVertices = new List<Vector3>(vertexCount);
            List<Vector4> preparedUVs = new List<Vector4>(vertexCount);
            List<Vector3> normals = new List<Vector3>(vertexCount);
            List<List<int>> preparedTriangles = new List<List<int>>();
            int index = 0;
            Dictionary<TextureCache.TextureArrayTypes, int> submeshPerTextureFormat = new Dictionary<TextureCache.TextureArrayTypes, int>();

            foreach (var subMesh in mrmData.SubMeshes)
            {
                // When using the texture array, get the index of the array of the matching texture format. Build submeshes for each texture format, i.e. separating opaque and alpha cutout textures.
                int textureArrayIndex = 0, maxMipLevel = 0;
                Vector2 textureScale = Vector2.one;
                TextureCache.TextureArrayTypes textureArrayType = TextureCache.TextureArrayTypes.Opaque;
                if (UseTextureArray)
                {
                    TextureCache.GetTextureArrayIndex(TextureCache.TextureTypes.Vob, subMesh.Material, out textureArrayType, out textureArrayIndex, out textureScale, out maxMipLevel);
                    if (!submeshPerTextureFormat.ContainsKey(textureArrayType))
                    {
                        submeshPerTextureFormat.Add(textureArrayType, preparedTriangles.Count);
                        preparedTriangles.Add(new List<int>());
                    }
                }
                else
                {
                    preparedTriangles.Add(new List<int>());
                }

                for (int i = 0; i < subMesh.Triangles.Count; i++)
                {
                    // One triangle is made of 3 elements for Unity. We therefore need to prepare 3 elements within one loop.
                    MeshWedge[] wedges = { subMesh.Wedges[subMesh.Triangles[i].Wedge2], subMesh.Wedges[subMesh.Triangles[i].Wedge1], subMesh.Wedges[subMesh.Triangles[i].Wedge0] };

                    for (int w = 0; w < wedges.Length; w++)
                    {
                        preparedVertices.Add(mrmData.Positions[wedges[w].Index].ToUnityVector());
                        if (UseTextureArray)
                        {
                            preparedTriangles[submeshPerTextureFormat[textureArrayType]].Add(index++);
                        }
                        else
                        {
                            preparedTriangles[preparedTriangles.Count - 1].Add(index++);
                        }
                        normals.Add(wedges[w].Normal.ToUnityVector());
                        Vector2 uv = Vector2.Scale(textureScale, wedges[w].Texture.ToUnityVector());
                        preparedUVs.Add(new Vector4(uv.x, uv.y, textureArrayIndex, maxMipLevel));

                        CreateMorphMeshEntry(wedges[w].Index, preparedVertices.Count);
                    }
                }
            }

            // Unity 1/ handles vertices on mesh level, but triangles (aka vertex-indices) on submesh level.
            // and 2/ demands vertices to be stored before triangles/uvs.
            // Therefore we prepare the full data once and assign it afterwards.
            // @see: https://answers.unity.com/questions/531968/submesh-vertices.html
            mesh.subMeshCount = preparedTriangles.Count;
            mesh.SetVertices(preparedVertices);
            mesh.SetUVs(0, preparedUVs);
            mesh.SetNormals(normals);
            for (int i = 0; i < preparedTriangles.Count; i++)
            {
                mesh.SetTriangles(preparedTriangles[i], i);
            }

            CreateMorphMeshEnd(preparedVertices);

            if (UseTextureArray)
            {
                TextureCache.VobMeshRenderersForTextureArray.Add((meshRenderer, (mrmData, submeshPerTextureFormat.Keys.ToList())));
            }
        }

        protected void PrepareMeshFilter(MeshFilter meshFilter, ISoftSkinMesh soft)
        {
            /*
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
            var zkMesh = soft.Mesh;
            var weights = soft.Weights;

            meshFilter.mesh = mesh;
            mesh.subMeshCount = soft!.Mesh.SubMeshes.Count;

            var verticesAndUvSize = zkMesh.SubMeshes.Sum(i => i.Triangles!.Count) * 3;
            var preparedVertices = new List<Vector3>(verticesAndUvSize);
            var preparedUVs = new List<Vector2>(verticesAndUvSize);
            var preparedBoneWeights = new List<BoneWeight>(verticesAndUvSize);

            // 2-dimensional arrays (as there are segregated by submeshes)
            var preparedTriangles = new List<List<int>>(zkMesh.SubMeshes.Count);
            var vertices = GetSoftSkinMeshPositions(soft);

            foreach (var subMesh in zkMesh.SubMeshes)
            {
                var triangles = subMesh.Triangles;
                var wedges = subMesh.Wedges;

                // every triangle is attached to a new vertex.
                // Therefore new submesh triangles start referencing their vertices with an offset from previous runs.
                var verticesIndexOffset = preparedVertices.Count;

                var subMeshTriangles = new List<int>(triangles.Count * 3);
                for (var i = 0; i < triangles.Count; i++)
                {
                    // One triangle is made of 3 elements for Unity. We therefore need to prepare 3 elements within one loop.
                    var preparedIndex = i * 3 + verticesIndexOffset;

                    var index1 = wedges![triangles[i].Wedge2];
                    var index2 = wedges[triangles[i].Wedge1];
                    var index3 = wedges[triangles[i].Wedge0];

                    preparedVertices.Add(vertices![index1.Index].ToUnityVector());
                    preparedVertices.Add(vertices[index2.Index].ToUnityVector());
                    preparedVertices.Add(vertices[index3.Index].ToUnityVector());

                    subMeshTriangles.Add(preparedIndex);
                    subMeshTriangles.Add(preparedIndex + 1);
                    subMeshTriangles.Add(preparedIndex + 2);

                    preparedUVs.Add(index1.Texture.ToUnityVector());
                    preparedUVs.Add(index2.Texture.ToUnityVector());
                    preparedUVs.Add(index3.Texture.ToUnityVector());

                    preparedBoneWeights.Add(weights[index1.Index].ToBoneWeight(soft.Nodes));
                    preparedBoneWeights.Add(weights[index2.Index].ToBoneWeight(soft.Nodes));
                    preparedBoneWeights.Add(weights[index3.Index].ToBoneWeight(soft.Nodes));
                }
                preparedTriangles.Add(subMeshTriangles);
            }

            // Unity 1/ handles vertices on mesh level, but triangles (aka vertex-indices) on submesh level.
            // and 2/ demands vertices to be stored before triangles/uvs.
            // Therefore we prepare the full data once and assign it afterwards.
            // @see: https://answers.unity.com/questions/531968/submesh-vertices.html
            mesh.SetVertices(preparedVertices);
            mesh.SetUVs(0, preparedUVs);

            mesh.boneWeights = preparedBoneWeights.ToArray();
            for (var i = 0; i < zkMesh.SubMeshes.Count; i++)
            {
                mesh.SetTriangles(preparedTriangles[i], i);
            }
        }

        protected virtual List<System.Numerics.Vector3> GetSoftSkinMeshPositions(ISoftSkinMesh softSkinMesh)
        {
            return softSkinMesh.Mesh.Positions;
        }

        /// <summary>
        /// We basically only set the values from official Unity documentation. No added sugar for the bingPoses.
        /// @see https://docs.unity3d.com/ScriptReference/Mesh-bindposes.html
        /// @see https://forum.unity.com/threads/some-explanations-on-bindposes.86185/
        /// </summary>
        private void CreateBonesData(GameObject rootObj, GameObject[] nodeObjects, SkinnedMeshRenderer renderer, ISoftSkinMesh mesh)
        {
            var meshBones = new Transform[mesh.Nodes.Count];
            var bindPoses = new UnityEngine.Matrix4x4[mesh.Nodes.Count];

            for (var i = 0; i < mesh.Nodes.Count; i++)
            {
                var nodeIndex = mesh.Nodes[i];

                meshBones[i] = nodeObjects[nodeIndex].transform;
                bindPoses[i] = meshBones[i].worldToLocalMatrix * rootObj.transform.localToWorldMatrix;
            }

            renderer.sharedMesh.bindposes = bindPoses;
            renderer.bones = meshBones;
        }

        protected Collider PrepareMeshCollider(GameObject obj, Mesh mesh)
        {
            var meshCollider = obj.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            return meshCollider;
        }

        /// <summary>
        /// Check if Collider needs to be added.
        /// </summary>
        protected Collider PrepareMeshCollider(GameObject obj, Mesh mesh, IMaterial materialData)
        {
            if (materialData.DisableCollision ||
                materialData.Group == MaterialGroup.Water)
            {
                // Do not add colliders
                return null;
            }
            else
            {
                return PrepareMeshCollider(obj, mesh);
            }
        }

        /// <summary>
        /// Check if Collider needs to be added.
        /// </summary>
        protected void PrepareMeshCollider(GameObject obj, Mesh mesh, List<IMaterial> materialDatas)
        {
            bool anythingDisableCollission = materialDatas.Any(i => i.DisableCollision);
            bool anythingWater = materialDatas.Any(i => i.Group == MaterialGroup.Water);

            if (!anythingDisableCollission && !anythingWater)
            {
                PrepareMeshCollider(obj, mesh);
            }
        }

        private void CreateMorphMeshBegin(IMultiResolutionMesh mrm, Mesh mesh)
        {
            if (Mmb == null)
            {
                return;
            }
            
            // MorphMeshes will change the vertices. This call optimizes performance.
            mesh.MarkDynamic();

            isMorphMeshMappingAlreadyCached = MorphMeshCache.IsMappingAlreadyCached(Mmb.Name);
            if (isMorphMeshMappingAlreadyCached)
            {
                return;
            }

            MorphMeshCache.AddVertexMapping(Mmb.Name, mrm.PositionCount);
        }

        private void CreateMorphMeshEntry(int index1, int preparedVerticesCount)
        {
            // We add mapping data to later reuse for IMorphAnimation samples
            if (Mmb == null || isMorphMeshMappingAlreadyCached)
            {
                return;
            }

            MorphMeshCache.AddVertexMappingEntry(Mmb.Name, index1, preparedVerticesCount - 1);
        }

        private void CreateMorphMeshEnd(List<Vector3> preparedVertices)
        {
            if (Mmb == null || isMorphMeshMappingAlreadyCached)
            {
                return;
            }

            MorphMeshCache.SetUnityVerticesForVertexMapping(Mmb.Name, preparedVertices.ToArray());
        }

        /// <summary>
        /// There are some objects (e.g. NPCs) where we want to skip specific attachments. This method can be overridden for this feature.
        /// </summary>
        protected virtual Dictionary<string, IMultiResolutionMesh> GetFilteredAttachments(Dictionary<string, IMultiResolutionMesh> attachments)
        {
            return attachments;
        }

        protected virtual Texture2D GetTexture(string name)
        {
            return TextureCache.TryGetTexture(name);
        }

        protected virtual Material GetDefaultMaterial(bool isAlphaTest)
        {
            return new Material(Constants.ShaderSingleMeshLit);
        }

        protected Material GetWaterMaterial()
        {
            Material material = new Material(Constants.ShaderWater);
            // Manually correct the render queue for alpha test, as Unity doesn't want to do it from the shader's render queue tag.
            material.renderQueue = (int)RenderQueue.Transparent;
            return material;
        }

        protected void SetPosAndRot(GameObject obj, System.Numerics.Matrix4x4 matrix)
        {
            SetPosAndRot(obj, matrix.ToUnityMatrix());
        }

        protected void SetPosAndRot(GameObject obj, Matrix4x4 matrix)
        {
            SetPosAndRot(obj, matrix.GetPosition() / 100, matrix.rotation);
        }

        protected void SetPosAndRot(GameObject obj, Vector3 position, Quaternion rotation)
        {
            obj.transform.SetLocalPositionAndRotation(position, rotation);
        }
    }
}
