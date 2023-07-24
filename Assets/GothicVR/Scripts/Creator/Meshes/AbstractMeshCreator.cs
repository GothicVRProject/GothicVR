using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GVR.Caches;
using GVR.Phoenix.Data;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Mesh;
using PxCs.Data.Model;
using PxCs.Data.Struct;
using PxCs.Data.Vob;
using PxCs.Interface;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GVR.Creator.Meshes
{
    public abstract class AbstractMeshCreator<T> : SingletonBehaviour<T> where T : AbstractMeshCreator<T>
    {
        // Decals work only on URP shaders. We therefore temporarily change everything to this
        // until we know how to change specifics to the cutout only. (e.g. bushes)
        protected const string defaultShader = "Universal Render Pipeline/Unlit"; // "Unlit/Transparent Cutout";
        protected const float decalOpacity = 0.75f;

        
        public GameObject Create(WorldData world, GameObject parent)
        {
            var meshObj = new GameObject("Mesh");
            meshObj.isStatic = true;
            meshObj.SetParent(parent);

            foreach (var subMesh in world.subMeshes.Values)
            {
                var subMeshObj = new GameObject(subMesh.material.name);
                subMeshObj.isStatic = true;

                var meshFilter = subMeshObj.AddComponent<MeshFilter>();
                var meshRenderer = subMeshObj.AddComponent<MeshRenderer>();

                PrepareMeshRenderer(meshRenderer, subMesh);
                PrepareMeshFilter(meshFilter, subMesh);
                PrepareMeshCollider(subMeshObj, meshFilter.mesh, subMesh.material);

                subMeshObj.SetParent(meshObj);
            }

            return meshObj;
        }

        public GameObject Create(string objectName, PxModelData mdl, Vector3 position, Quaternion rotation, GameObject parent = null)
        {
            return Create(objectName, mdl.mesh, mdl.hierarchy, position, rotation, parent);
        }

        public GameObject Create(string objectName, PxModelMeshData mdm, PxModelHierarchyData mdh, Vector3 position, Quaternion rotation, GameObject parent = null)
        {
            var rootGo = new GameObject(objectName);
            rootGo.SetParent(parent);

            var nodeObjects = new GameObject[mdh.nodes!.Length];

            // Create empty GameObjects from hierarchy
            {
                for (var i = 0; i < mdh.nodes.Length; i++)
                {
                    nodeObjects[i] = new GameObject(mdh.nodes[i].name);
                }

                // Now set parents
                for (var i = 0; i < mdh.nodes.Length; i++)
                {
                    var node = mdh.nodes[i];
                    var nodeObj = nodeObjects[i];

                    SetPosAndRot(nodeObj, node.transform);

                    if (node.parentIndex == -1)
                        nodeObj.SetParent(rootGo);
                    else
                        nodeObj.SetParent(nodeObjects[node.parentIndex]);
                }
                
                for (var i=0; i<nodeObjects.Length; i++)
                {
                    if (mdh.nodes[i].parentIndex == -1)
                        nodeObjects[i].transform.localPosition = mdh.rootTranslation.ToUnityVector();
                    else
                        SetPosAndRot(nodeObjects[i], mdh.nodes[i].transform);
                }
            }
            
            //// Fill GameObjects with Meshes from "original" Mesh
            foreach (var softSkinMesh in mdm.meshes!)
            {
                var mesh = softSkinMesh.mesh;

                var meshObj = new GameObject("JaX_ZM_0");
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

            // Fill GameObjects with Meshes from attachments
            foreach (var subMesh in mdm.attachments!)
            {
                var meshObj = nodeObjects.First(bone => bone.name == subMesh.Key);
                var meshFilter = meshObj.AddComponent<MeshFilter>();
                var meshRenderer = meshObj.AddComponent<MeshRenderer>();

                PrepareMeshRenderer(meshRenderer, subMesh.Value);
                PrepareMeshFilter(meshFilter, subMesh.Value);
            }

            SetPosAndRot(rootGo, position, rotation);

            // We need to set the root translation after we add children above. Otherwise the "additive" position/rotation will be broken.
            // We need to reset the rootBones position to zero. Otherwise Vobs won't be placed right.
            // Due to Unity's parent-child transformation magic, we need to do it at the end. ╰(*°▽°*)╯
            nodeObjects[0].transform.localPosition = Vector3.zero;
            
            return rootGo;
        }

        public GameObject Create(string objectName, PxMultiResolutionMeshData mrm, Vector3 position, PxMatrix3x3Data rotation, bool withCollider, GameObject parent = null, GameObject rootGo = null)
        {
            if (mrm == null)
            {
                Debug.LogError("No mesh data was found for: " + objectName);
                return null;
            }

            rootGo ??= new GameObject();
            rootGo.name = objectName;
            rootGo.SetParent(parent);
            SetPosAndRot(rootGo, position, rotation);

            var meshFilter = rootGo.AddComponent<MeshFilter>();
            var meshRenderer = rootGo.AddComponent<MeshRenderer>();

            PrepareMeshRenderer(meshRenderer, mrm);
            PrepareMeshFilter(meshFilter, mrm);

            if (withCollider)
                PrepareMeshCollider(rootGo, meshFilter.mesh, mrm.materials);

            return rootGo;
        }

        protected void SetPosAndRot(GameObject obj, PxMatrix4x4Data matrix)
        {
            SetPosAndRot(obj, matrix.ToUnityMatrix());
        }

        protected void SetPosAndRot(GameObject obj, Matrix4x4 matrix)
        {
            SetPosAndRot(obj, matrix.GetPosition() / 100, matrix.rotation);
        }

        protected void SetPosAndRot(GameObject obj, Vector3 position, PxMatrix3x3Data rotation)
        {
            SetPosAndRot(obj, position, rotation.ToUnityMatrix().rotation);
        }

        protected void SetPosAndRot(GameObject obj, Vector3 position, Quaternion rotation)
        {
            obj.transform.localRotation = rotation;
            obj.transform.localPosition = position;
        }

        protected void PrepareMeshRenderer(Renderer rend, WorldData.SubMeshData subMesh)
        {
            var material = GetEmptyMaterial();
            var bMaterial = subMesh.material;

            rend.material = material;

            // No texture to add.
            if (bMaterial.texture == "")
            {
                Debug.LogWarning("No texture was set for: " + bMaterial.name);
                return;
            }

            var texture = GetTexture(bMaterial.texture);

            if (null == texture)
            {
                if (bMaterial.texture.EndsWith(".TGA"))
                    Debug.LogError("This is supposed to be a decal: " + bMaterial.texture);
                else
                    Debug.LogError("Couldn't get texture from name: " + bMaterial.texture);
            }

            material.mainTexture = texture;
        }

        protected void PrepareMeshFilter(MeshFilter meshFilter, WorldData.SubMeshData subMesh)
        {
            var mesh = new Mesh();
            meshFilter.mesh = mesh;

            mesh.SetVertices(subMesh.vertices);
            mesh.SetTriangles(subMesh.triangles, 0);
            mesh.SetUVs(0, subMesh.uvs);
        }

        protected void PrepareMeshRenderer(Renderer rend, PxMultiResolutionMeshData mrmData)
        {
            // check if mrmData.subMeshes is null

            if (null == mrmData)
            {
                Debug.LogError("No mesh data could be added to renderer: " + rend.transform.parent.name);
                return;
            }

            var finalMaterials = new List<Material>(mrmData.subMeshes.Length);

            foreach (var subMesh in mrmData.subMeshes)
            {
                var material = GetEmptyMaterial();
                var materialData = subMesh.material;

                rend.material = material;

                // No texture to add.
                if (materialData.texture == "")
                {
                    Debug.LogWarning("No texture was set for: " + materialData.name);
                    return;
                }

                var texture = GetTexture(materialData.texture);

                if (null == texture)
                    if (materialData.texture.EndsWith(".TGA"))
                        Debug.LogError("This is supposed to be a decal: " + materialData.texture);
                    else
                        Debug.LogError("Couldn't get texture from name: " + materialData.texture);



                material.mainTexture = texture;

                finalMaterials.Add(material);
            }

            rend.SetMaterials(finalMaterials);
        }

        protected void PrepareMeshFilter(MeshFilter meshFilter, PxMultiResolutionMeshData mrmData)
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
            if (null == mrmData)
            {
                Debug.LogError("No mesh data could be added to filter: " + meshFilter.transform.parent.name);
                return;
            }
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


        protected void PrepareMeshFilter(MeshFilter meshFilter, PxSoftSkinMeshData soft)
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
            var pxMesh = soft.mesh;
            var weights = soft.weights;
            
            meshFilter.mesh = mesh;
            mesh.subMeshCount = soft!.mesh!.subMeshes!.Length;

            var verticesAndUvSize = pxMesh!.subMeshes!.Sum(i => i.triangles!.Length) * 3;
            var preparedVertices = new List<Vector3>(verticesAndUvSize);
            var preparedUVs = new List<Vector2>(verticesAndUvSize);
            var preparedBoneWeights = new List<BoneWeight>(verticesAndUvSize);

            // 2-dimensional arrays (as there are segregated by submeshes)
            var preparedTriangles = new List<List<int>>(pxMesh.subMeshes.Length);

            foreach (var subMesh in pxMesh.subMeshes)
            {
                var vertices = pxMesh.positions;
                var triangles = subMesh.triangles;
                var wedges = subMesh.wedges;

                // every triangle is attached to a new vertex.
                // Therefore new submesh triangles start referencing their vertices with an offset from previous runs.
                var verticesIndexOffset = preparedVertices.Count;

                var subMeshTriangles = new List<int>(triangles!.Length * 3);
                for (var i = 0; i < triangles.Length; i++)
                {
                    // One triangle is made of 3 elements for Unity. We therefore need to prepare 3 elements within one loop.
                    var preparedIndex = i * 3 + verticesIndexOffset;

                    var index1 = wedges![triangles[i].c];
                    var index2 = wedges[triangles[i].b];
                    var index3 = wedges[triangles[i].a];

                    preparedVertices.Add(vertices![index1.index].ToUnityVector());
                    preparedVertices.Add(vertices[index2.index].ToUnityVector());
                    preparedVertices.Add(vertices[index3.index].ToUnityVector());

                    subMeshTriangles.Add(preparedIndex);
                    subMeshTriangles.Add(preparedIndex + 1);
                    subMeshTriangles.Add(preparedIndex + 2);

                    preparedUVs.Add(index1.texture.ToUnityVector());
                    preparedUVs.Add(index2.texture.ToUnityVector());
                    preparedUVs.Add(index3.texture.ToUnityVector());
                    
                    preparedBoneWeights.Add(weights[index1.index].ToBoneWeight(soft.nodes));
                    preparedBoneWeights.Add(weights[index2.index].ToBoneWeight(soft.nodes));
                    preparedBoneWeights.Add(weights[index3.index].ToBoneWeight(soft.nodes));
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
            for (var i = 0; i < pxMesh.subMeshes.Length; i++)
            {
                mesh.SetTriangles(preparedTriangles[i], i);
            }
        }

        private Collider PrepareMeshCollider(GameObject obj, Mesh mesh)
        {
            var meshCollider = obj.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            return meshCollider;
        }

        /// <summary>
        /// Check if Collider needs to be added.
        /// </summary>
        private Collider PrepareMeshCollider(GameObject obj, Mesh mesh, PxMaterialData materialData)
        {
            if (materialData.disableCollision ||
                materialData.group == PxMaterial.PxMaterialGroup.PxMaterialGroup_Water)
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
        private void PrepareMeshCollider(GameObject obj, Mesh mesh, PxMaterialData[] materialDatas)
        {
            var anythingDisableCollission = materialDatas.Any(i => i.disableCollision);
            var anythingWater = materialDatas.Any(i => i.group == PxMaterial.PxMaterialGroup.PxMaterialGroup_Water);

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
        private static void CreateBonesData(GameObject rootObj, GameObject[] nodeObjects, SkinnedMeshRenderer renderer, PxSoftSkinMeshData mesh)
        {
            var meshBones = new Transform[mesh.nodes!.Length];
            var bindPoses = new Matrix4x4[mesh.nodes!.Length];

            for (var i = 0; i < mesh.nodes.Length; i++)
            {
                var nodeIndex = mesh.nodes[i];

                meshBones[i] = nodeObjects[nodeIndex].transform;
                bindPoses[i] = meshBones[i].worldToLocalMatrix * rootObj.transform.localToWorldMatrix;
            }

            renderer.sharedMesh.bindposes = bindPoses;
            renderer.bones = meshBones;
        }

        protected virtual Texture2D GetTexture(string name)
        {
            return AssetCache.I.TryGetTexture(name);
        }

        protected Material GetEmptyMaterial()
        {
            var standardShader = Shader.Find(defaultShader);
            var material = new Material(standardShader);

            // Enable clipping of alpha values.
            material.EnableKeyword("_ALPHATEST_ON");

            return material;
        }
    }
}