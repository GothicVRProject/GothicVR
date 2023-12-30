using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using UnityEngine;
using UnityEngine.Rendering;
using ZenKit;
using Material = UnityEngine.Material;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Mesh = UnityEngine.Mesh;
using TextureFormat = UnityEngine.TextureFormat;

namespace GVR.Creator.Meshes
{
    public abstract class AbstractMeshCreator
    {
        // Decals work only on URP shaders. We therefore temporarily change everything to this
        // until we know how to change specifics to the cutout only. (e.g. bushes)
        protected const string DefaultShader = "Universal Render Pipeline/Unlit"; // "Unlit/Transparent Cutout";
        protected const string WaterShader = "Shader Graphs/Unlit_Both_ScrollY"; //Vinces moving texture water shader
        protected const string AlphaToCoverageShaderName = "Unlit/Unlit-AlphaToCoverage";
        protected const float DecalOpacity = 0.75f;
        
        protected GameObject Create(string objectName, IModelMesh mdm, IModelHierarchy mdh, Vector3 position, Quaternion rotation, GameObject parent = null, GameObject rootGo = null)
        {
            rootGo ??= new GameObject(objectName); // Create new object if it is a null-parameter until now.
            rootGo.SetParent(parent, true, true);

            var nodeObjects = new GameObject[mdh.Nodes.Count];

            // Create empty GameObjects from hierarchy
            {
                for (var i = 0; i < mdh.Nodes.Count ; i++)
                {
                    var node = mdh.Nodes[i];
                    // We attached some Components to root of bones. Therefore reusing it.
                    if (node.Name.EqualsIgnoreCase("BIP01"))
                    {
                        var bip01 = rootGo.FindChildRecursively("BIP01");
                        if (bip01 != null)
                            nodeObjects[i] = rootGo.FindChildRecursively("BIP01");
                        else
                            nodeObjects[i] = new GameObject(mdh.Nodes[i].Name);
                    }
                    else
                    {
                        nodeObjects[i] = new GameObject(mdh.Nodes[i].Name);
                    }
                }

                // Now set parents
                for (var i = 0; i < mdh.Nodes.Count; i++)
                {
                    var node = mdh.Nodes[i];
                    var nodeObj = nodeObjects[i];

                    SetPosAndRot(nodeObj, node.Transform);

                    if (node.ParentIndex == -1)
                        nodeObj.SetParent(rootGo);
                    else
                        nodeObj.SetParent(nodeObjects[node.ParentIndex]);
                }

                for (var i = 0; i < nodeObjects.Length; i++)
                {
                    if (mdh.Nodes[i].ParentIndex == -1)
                        nodeObjects[i].transform.localPosition = mdh.RootTranslation.ToUnityVector();
                    else
                        SetPosAndRot(nodeObjects[i], mdh.Nodes[i].Transform);
                }
            }

            //// Fill GameObjects with Meshes from "original" Mesh
            int meshCounter = 0;
            foreach (var softSkinMesh in mdm.Meshes)
            {
                var mesh = softSkinMesh.Mesh;

                var meshObj = new GameObject($"ZM_{meshCounter}");
                meshObj.SetParent(rootGo);

                var meshFilter = meshObj.AddComponent<MeshFilter>();
                var meshRenderer = meshObj.AddComponent<SkinnedMeshRenderer>();

                // FIXME - hard coded as it's the right value for BSFire. Need to be more dynamic by using element which has parent=-1.
                meshRenderer.rootBone = nodeObjects[0].transform;

                PrepareMeshRenderer(meshRenderer, mesh);
                PrepareMeshFilter(meshFilter, softSkinMesh);

                meshRenderer.sharedMesh = meshFilter.mesh;
                
                CreateBonesData(rootGo, nodeObjects, meshRenderer, softSkinMesh);
            }

            var attachments = GetFilteredAttachments(mdm.Attachments);

            // Fill GameObjects with Meshes from attachments
            foreach (var subMesh in attachments)
            {
                var meshObj = nodeObjects.First(bone => bone.name == subMesh.Key);
                var meshFilter = meshObj.AddComponent<MeshFilter>();
                var meshRenderer = meshObj.AddComponent<MeshRenderer>();

                PrepareMeshRenderer(meshRenderer, subMesh.Value);
                PrepareMeshFilter(meshFilter, subMesh.Value);
                PrepareMeshCollider(meshObj, meshFilter.mesh, subMesh.Value.Materials);
            }

            SetPosAndRot(rootGo, position, rotation);

            // We need to set the root translation after we add children above. Otherwise the "additive" position/rotation will be broken.
            // We need to reset the rootBones position to zero. Otherwise Vobs won't be placed right.
            // Due to Unity's parent-child transformation magic, we need to do it at the end. ╰(*°▽°*)╯
            nodeObjects[0].transform.localPosition = Vector3.zero;

            return rootGo;
        }

        /// <summary>
        /// There are some objects (e.g. NPCs) where we want to skip specific attachments. This method can be overridden for this feature.
        /// </summary>
        protected virtual Dictionary<string, IMultiResolutionMesh> GetFilteredAttachments(Dictionary<string, IMultiResolutionMesh> attachments)
        {
            return attachments;
        }

        protected GameObject Create(string objectName, IMultiResolutionMesh mrm, Vector3 position, Quaternion rotation, bool withCollider, GameObject parent = null, GameObject rootGo = null)
        {
            if (mrm == null)
            {
                Debug.LogError("No mesh data was found for: " + objectName);
                return null;
            }
            
            // If there is no texture for any of the meshes, just skip this item.
            // G1: Some skull decorations are without texture.
            if (mrm.Materials.All(m => m.Texture.IsEmpty()))
                return null;

            rootGo ??= new GameObject();
            rootGo.name = objectName;
            rootGo.SetParent(parent);
            SetPosAndRot(rootGo, position, rotation);

            var meshFilter = rootGo.AddComponent<MeshFilter>();
            var meshRenderer = rootGo.AddComponent<MeshRenderer>();

            PrepareMeshRenderer(meshRenderer, mrm);
            PrepareMeshFilter(meshFilter, mrm);

            if (withCollider)
                PrepareMeshCollider(rootGo, meshFilter.mesh, mrm.Materials);

            return rootGo;
        }

        protected void SetPosAndRot(GameObject obj, Matrix4x4 matrix)
        {
            SetPosAndRot(obj, matrix.ToUnityMatrix());
        }

        protected void SetPosAndRot(GameObject obj, UnityEngine.Matrix4x4 matrix)
        {
            SetPosAndRot(obj, matrix.GetPosition() / 100, matrix.rotation);
        }

        protected void SetPosAndRot(GameObject obj, Vector3 position, Quaternion rotation)
        {
            obj.transform.SetLocalPositionAndRotation(position, rotation);
        }

        protected void PrepareMeshRenderer(Renderer rend, IMultiResolutionMesh mrmData)
        {
            if (null == mrmData)
            {
                Debug.LogError("No mesh data could be added to renderer: " + rend.transform.parent.name);
                return;
            }

            var finalMaterials = new List<Material>(mrmData.SubMeshes.Count);

            foreach (var subMesh in mrmData.SubMeshes)
            {
                var materialData = subMesh.Material;

                var texture = GetTexture(materialData.Texture);
                if (null == texture)
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

                var material = GetDefaultMaterial(texture != null && texture.format == TextureFormat.RGBA32);

                rend.material = material;

                // No texture to add.
                if (materialData.Texture.IsEmpty())
                {
                    Debug.LogWarning("No texture was set for: " + materialData.Name);
                    return;
                }

                material.mainTexture = texture;

                finalMaterials.Add(material);
            }

            rend.SetMaterials(finalMaterials);
        }

        protected void PrepareMeshFilter(MeshFilter meshFilter, IMultiResolutionMesh mrmData)
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
            meshFilter.mesh = mesh;
            if (null == mrmData)
            {
                Debug.LogError("No mesh data could be added to filter: " + meshFilter.transform.parent.name);
                return;
            }
            mesh.subMeshCount = mrmData.SubMeshes.Count;

            var verticesAndUvSize = mrmData.SubMeshes.Sum(i => i.Triangles.Count) * 3;
            var preparedVertices = new List<Vector3>(verticesAndUvSize);
            var preparedUVs = new List<Vector2>(verticesAndUvSize);

            // 2-dimensional arrays (as there are segregated by submeshes)
            var preparedTriangles = new List<List<int>>(mrmData.SubMeshes.Count);

            foreach (var subMesh in mrmData.SubMeshes)
            {
                var vertices = mrmData.Positions;
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

                    var index1 = wedges[triangles[i].Wedge2];
                    var index2 = wedges[triangles[i].Wedge1];
                    var index3 = wedges[triangles[i].Wedge0];

                    preparedVertices.Add(vertices[index1.Index].ToUnityVector());
                    preparedVertices.Add(vertices[index2.Index].ToUnityVector());
                    preparedVertices.Add(vertices[index3.Index].ToUnityVector());

                    subMeshTriangles.Add(preparedIndex);
                    subMeshTriangles.Add(preparedIndex + 1);
                    subMeshTriangles.Add(preparedIndex + 2);

                    preparedUVs.Add(index1.Texture.ToUnityVector());
                    preparedUVs.Add(index2.Texture.ToUnityVector());
                    preparedUVs.Add(index3.Texture.ToUnityVector());
                }
                preparedTriangles.Add(subMeshTriangles);
            }

            // Unity 1/ handles vertices on mesh level, but triangles (aka vertex-indices) on submesh level.
            // and 2/ demands vertices to be stored before triangles/uvs.
            // Therefore we prepare the full data once and assign it afterwards.
            // @see: https://answers.unity.com/questions/531968/submesh-vertices.html
            mesh.SetVertices(preparedVertices);
            mesh.SetUVs(0, preparedUVs);
            for (var i = 0; i < mrmData.SubMeshes.Count; i++)
            {
                mesh.SetTriangles(preparedTriangles[i], i);
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

            foreach (var subMesh in zkMesh.SubMeshes)
            {
                var vertices = zkMesh.Positions;
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
            var anythingDisableCollission = materialDatas.Any(i => i.DisableCollision);
            var anythingWater = materialDatas.Any(i => i.Group == MaterialGroup.Water);

            if (anythingDisableCollission || anythingWater)
            {
                // Do not add colliders
            }
            else
            {
                PrepareMeshCollider(obj, mesh);
            }
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

        protected virtual Texture2D GetTexture(string name)
        {
            return AssetCache.TryGetTexture(name);
        }

        protected Material GetDefaultMaterial(bool isAlphaTest)
        {
            var shader = Shader.Find(DefaultShader);
            if (isAlphaTest)
            {
                shader = Shader.Find(AlphaToCoverageShaderName);
            }
            var material = new Material(shader);
            if (isAlphaTest)
            {
                // Manually correct the render queue for alpha test, as Unity doesn't want to do it from the shader's render queue tag.
                material.renderQueue = (int)RenderQueue.AlphaTest;
            }
            return material;
        }

        protected Material GetWaterMaterial(IMaterial materialData)
        {
            var shader = Shader.Find(WaterShader);
            Material material = new Material(shader);

            // FIXME - Running water speed and direction is hardcoded based on material names
            // Needs to be improved by a better shader and the implementation of proper water material parameters

            //JaXt0r's suggestion for a not so hardcoded running water implementation
            //material.SetFloat("_ScrollSpeed", -900000 * materialData.animMapDir.ToUnityVector().SqrMagnitude());

            switch (materialData.Name)
            {
                case "OWODSEA2SWAMP": material.SetFloat("_ScrollSpeed", 0f); break;
                case "NCWASSER": material.SetFloat("_ScrollSpeed", 0f); break;
                case "OWODWATSTOP": material.SetFloat("_ScrollSpeed", (materialData.TextureAnimationFps / 75f)); break;
                case "OWODWFALL": material.SetFloat("_ScrollSpeed", -(materialData.TextureAnimationFps / 10f)); break;
                default: material.SetFloat("_ScrollSpeed", -(materialData.TextureAnimationFps / 75f)); break;
            }

            material.SetFloat("_Surface", 0);
            material.SetInt("_ZWrite", 0);
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);

            return material;
        }
    }
}
