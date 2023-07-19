using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Manager;
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
using System.Threading.Tasks;

namespace GVR.Creator
{
    public class MeshCreator : SingletonBehaviour<MeshCreator>
    {
        private AssetCache assetCache;

        // Decals work only on URP shaders. We therefore temporarily change everything to this
        // until we know how to change specifics to the cutout only. (e.g. bushes)
        private const string defaultShader = "Universal Render Pipeline/Unlit"; // "Unlit/Transparent Cutout";
        private const float decalOpacity = 0.75f;


        private void Start()
        {
            assetCache = SingletonBehaviour<AssetCache>.GetOrCreate();
        }

        /// <summary>
        /// Inject singletons if we use this class from EditorMode.
        /// </summary>
        public void EditorInject(AssetCache assetCache)
        {
            this.assetCache = assetCache;
        }

        public async Task<GameObject> Create(WorldData world, GameObject parent)
        {
            var meshObj = new GameObject("Mesh");
            meshObj.isStatic = true;
            meshObj.SetParent(parent);

            // Track the progress of each sub-mesh creation separately
            float subMeshCreationProgress = 0f;
            int numSubMeshes = world.subMeshes.Values.Count;
            int subMeshCounter = 0;

            foreach (var subMesh in world.subMeshes.Values)
            {
                var subMeshObj = new GameObject(subMesh.material.name);
                subMeshObj.isStatic = true;

                var meshFilter = subMeshObj.AddComponent<MeshFilter>();
                var meshRenderer = subMeshObj.AddComponent<MeshRenderer>();

                await PrepareMeshRenderer(meshRenderer, subMesh);

                PrepareMeshFilter(meshFilter, subMesh);
                PrepareMeshCollider(subMeshObj, meshFilter.mesh, subMesh.material);

                subMeshObj.SetParent(meshObj);

                // Update the sub-mesh creation progress
                subMeshCounter++;
                subMeshCreationProgress = (float)subMeshCounter / numSubMeshes;

                // Invoke the progress callback with the subMeshCreationProgress
                // progressCallback?.Invoke(subMeshCreationProgress);
                LoadingManager.I.SetProgress(subMeshCreationProgress * 0.5f);
            }

            // Use the overall progress value as needed (e.g., update a loading progress UI)
            float overallProgress = subMeshCreationProgress;

            // Invoke the progress callback with the overallProgress
            // progressCallback?.Invoke(overallProgress);
            LoadingManager.I.SetProgress(overallProgress * 0.5f);

            // Return the meshObj
            return meshObj;
        }


        public async Task<GameObject> Create(string objectName, PxModelData mdl, Vector3 position, PxMatrix3x3Data rotation, GameObject parent = null, GameObject rootGo = null)
        {
            return await Create(objectName, mdl.mesh, mdl.hierarchy, position, rotation, parent, rootGo);
        }

        public async Task<GameObject> Create(string objectName, PxModelMeshData mdm, PxModelHierarchyData mdh, Vector3 position, PxMatrix3x3Data rotation, GameObject parent = null, GameObject rootGo = null)
        {
            rootGo ??= new GameObject();
            rootGo.name = objectName;
            rootGo.SetParent(parent);
            SetPosAndRot(rootGo, position, rotation);

            // There are MDMs where there is no mesh, but meshes are in the attachment fields.
            if (mdm.meshes.Length == 0)
            {
                if (mdm.attachments.Values.Count == 0)
                {
                    Debug.LogWarning($"Object >{objectName}< has neither mdm.meshes nor mdm.attachments. Create mesh aborted.");
                    return null;
                }

                foreach (var subMesh in mdm.attachments)
                {
                    var subMeshName = subMesh.Key;
                    var subMeshObj = new GameObject(subMeshName);
                    subMeshObj.SetParent(rootGo);

                    var matrix = mdh.nodes.First(i => i.name == subMeshName).transform;

                    SetPosAndRot(subMeshObj, matrix);

                    var meshFilter = subMeshObj.AddComponent<MeshFilter>();
                    var meshRenderer = subMeshObj.AddComponent<MeshRenderer>();

                    await PrepareMeshRenderer(meshRenderer, subMesh.Value);
                    PrepareMeshFilter(meshFilter, subMesh.Value);
                    PrepareMeshCollider(subMeshObj, meshFilter.mesh, subMesh.Value.materials);
                }
            }
            else
            {
                foreach (var mesh in mdm.meshes)
                {
                    var subMeshObj = new GameObject(mesh.mesh.materials.First().name);
                    subMeshObj.SetParent(rootGo, true, false);

                    var meshFilter = subMeshObj.AddComponent<MeshFilter>();
                    // Changed SkinnedMeshRenderer to MeshRenderer for now bones seems to crash the game on PICO/Quest2
                    // var meshRenderer = subMeshObj.AddComponent<SkinnedMeshRenderer>();
                    var meshRenderer = subMeshObj.AddComponent<MeshRenderer>();

                    await PrepareMeshRenderer(meshRenderer, mesh.mesh);
                    PrepareMeshFilter(meshFilter, mesh);

                    //this is needed only for skinnedmeshrenderer
                    // meshRenderer.sharedMesh = meshFilter.mesh; // FIXME - We could get rid of meshFilter as the same mesh is needed on SkinnedMeshRenderer. Need to test...
                    PrepareMeshCollider(subMeshObj, meshFilter.mesh, mesh.mesh.materials);

                    // bones commented since we don't use for now skinnedmeshrenderer
                    // CreateBonesData(subMeshObj, meshRenderer, mdh);

                    // FIXME - needed?
                    //meshRenderer.rootBone = meshRootObject.transform;
                }
            }

            return rootGo;
        }

        public async Task<GameObject> Create(string objectName, PxMultiResolutionMeshData mrm, Vector3 position, PxMatrix3x3Data rotation, bool withCollider, GameObject parent = null, GameObject rootGo = null)
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

            await PrepareMeshRenderer(meshRenderer, mrm);
            PrepareMeshFilter(meshFilter, mrm);

            if (withCollider)
                PrepareMeshCollider(rootGo, meshFilter.mesh, mrm.materials);

            return rootGo;
        }

        public async Task<GameObject> CreateDecal(PxVobData vob, GameObject parent)
        {
            if (!vob.vobDecal.HasValue)
            {
                Debug.LogWarning("No decalData was set for: " + vob.visualName);
                return null;
            }

            var decalData = vob.vobDecal.Value;

            var decalProjectorGo = new GameObject(decalData.name);
            var decalProj = decalProjectorGo.AddComponent<DecalProjector>();
            var texture = await assetCache.TryGetTextureAsync(vob.visualName);

            // x/y needs to be made twice the size and transformed from cm in m.
            // z - value is close to what we see in Gothic spacer.
            decalProj.size = new(decalData.dimension.X * 2 / 100, decalData.dimension.Y * 2 / 100, 0.5f);
            decalProjectorGo.SetParent(parent);
            SetPosAndRot(decalProjectorGo, vob.position.ToUnityVector(), vob.rotation!.Value);

            decalProj.pivot = Vector3.zero;
            decalProj.fadeFactor = decalOpacity;

            // FIXME use Prefab!
            // https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@12.0/manual/creating-a-decal-projector-at-runtime.html
            var standardShader = Shader.Find("Shader Graphs/Decal");
            var material = new Material(standardShader);
            material.SetTexture(Shader.PropertyToID("Base_Map"), texture);

            decalProj.material = material;

            return decalProjectorGo;
        }

        private void SetPosAndRot(GameObject obj, PxMatrix4x4Data matrix)
        {
            var unityMatrix = matrix.ToUnityMatrix();
            SetPosAndRot(obj, unityMatrix.GetPosition() / 100, unityMatrix.rotation);
        }

        private void SetPosAndRot(GameObject obj, Vector3 position, PxMatrix3x3Data rotation)
        {
            SetPosAndRot(obj, position, rotation.ToUnityMatrix().rotation);
        }

        private void SetPosAndRot(GameObject obj, Vector3 position, Quaternion rotation)
        {
            // FIXME - This isn't working
            if (position.Equals(default) && rotation.Equals(default))
                return;

            obj.transform.localRotation = rotation;
            obj.transform.localPosition = position;
        }

        private async Task PrepareMeshRenderer(Renderer rend, WorldData.SubMeshData subMesh)
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

            var texture = await assetCache.TryGetTextureAsync(bMaterial.texture);

            if (null == texture)
            {
                if (bMaterial.texture.EndsWith(".TGA"))
                    Debug.LogError("This is supposed to be a decal: " + bMaterial.texture);
                else
                    Debug.LogError("Couldn't get texture from name: " + bMaterial.texture);
            }

            material.mainTexture = texture;
        }

        private void PrepareMeshFilter(MeshFilter meshFilter, WorldData.SubMeshData subMesh)
        {
            var mesh = new Mesh();
            meshFilter.mesh = mesh;

            mesh.SetVertices(subMesh.vertices);
            mesh.SetTriangles(subMesh.triangles, 0);
            mesh.SetUVs(0, subMesh.uvs);
        }

        private async Task PrepareMeshRenderer(Renderer rend, PxMultiResolutionMeshData mrmData)
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

                var texture = await assetCache.TryGetTextureAsync(materialData.texture);

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


        private void PrepareMeshFilter(MeshFilter meshFilter, PxSoftSkinMeshData soft)
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

            var DebugWeightIndices = weights.SelectMany(i => i).Select(i => i.nodeIndex).GroupBy(i => i).ToArray();

            meshFilter.mesh = mesh;
            mesh.subMeshCount = soft.mesh.subMeshes.Length;

            var verticesAndUvSize = pxMesh.subMeshes.Sum(i => i.triangles.Length) * 3;
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

                    // remove bones to avoid crash on Quest and Pico

                    // preparedBoneWeights.Add(weights[index1.index].ToBoneWeight());
                    // preparedBoneWeights.Add(weights[index2.index].ToBoneWeight());
                    // preparedBoneWeights.Add(weights[index3.index].ToBoneWeight());
                }
                preparedTriangles.Add(subMeshTriangles);
            }

            // Unity 1/ handles vertices on mesh level, but triangles (aka vertex-indices) on submesh level.
            // and 2/ demands vertices to be stored before triangles/uvs.
            // Therefore we prepare the full data once and assign it afterwards.
            // @see: https://answers.unity.com/questions/531968/submesh-vertices.html
            mesh.SetVertices(preparedVertices);
            mesh.SetUVs(0, preparedUVs);

            // same here for the bones
            // mesh.boneWeights = preparedBoneWeights.ToArray();
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


        private static void CreateBonesData(GameObject root, SkinnedMeshRenderer renderer, PxModelHierarchyData mdh)
        {
            Transform[] bones = new Transform[mdh.nodes.Length];
            Matrix4x4[] bindPoses = new Matrix4x4[mdh.nodes.Length];

            for (var i = 0; i < mdh.nodes.Length; i++)
            {
                var node = mdh.nodes[i];
                // HINT: We currently don't use the nodeMatrix. So is it really needed?
                var nodeMatrix = node.transform.ToUnityMatrix();
                var go = new GameObject(node.name);
                go.SetParent(root);

                // FIXME - used?
                //go.transform.rotation = nodeMatrix.rotation;
                //go.transform.localPosition = nodeMatrix.GetPosition(); // FIXME - needed? -> Unity positions are too big by factor 100.

                go.transform.localRotation = Quaternion.identity;
                go.transform.localPosition = Vector3.zero;

                //Debug.Log("rotation " + nodeMatrix.rotation);
                //Debug.Log("position " + nodeMatrix.GetPosition());

                bones[i] = go.transform;

                // FIXME - is this right or the other one?
                //bindPoses[i] = go.transform.worldToLocalMatrix;//.worldToLocalMatrix * go.transform.localToWorldMatrix;
                bindPoses[i] = bones[i].worldToLocalMatrix * root.transform.localToWorldMatrix;
            }

            renderer.sharedMesh.bindposes = bindPoses;
            renderer.bones = bones;
        }

        private Material GetEmptyMaterial()
        {
            var standardShader = Shader.Find(defaultShader);
            var material = new Material(standardShader);

            // Enable clipping of alpha values.
            material.EnableKeyword("_ALPHATEST_ON");

            return material;
        }
    }
}
