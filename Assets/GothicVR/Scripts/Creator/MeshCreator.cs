using GVR.Phoenix.Data;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Mesh;
using PxCs.Data.Model;
using PxCs.Data.Struct;
using PxCs.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GVR.Creator
{
    public class MeshCreator : SingletonBehaviour<MeshCreator>
    {
        private static AssetCache assetCache;

        void Start()
        {
            assetCache = SingletonBehaviour<AssetCache>.GetOrCreate();
        }
        
        public GameObject Create(WorldData world, GameObject parent = null)
        {
            var meshObj = new GameObject("Mesh");
            meshObj.SetParent(parent);

            foreach (var subMesh in world.subMeshes.Values)
            {
                var subMeshObj = new GameObject(string.Format("submesh-{0}", subMesh.material.name));
                var meshFilter = subMeshObj.AddComponent<MeshFilter>();
                var meshRenderer = subMeshObj.AddComponent<MeshRenderer>();
                var meshCollider = subMeshObj.AddComponent<MeshCollider>();

                PrepareMeshRenderer(meshRenderer, subMesh);
                PrepareMeshFilter(meshFilter, subMesh);
                meshCollider.sharedMesh = meshFilter.mesh;

                subMeshObj.transform.parent = meshObj.transform;
            }

            return meshObj;
        }

        public GameObject Create(string objectName, PxMultiResolutionMeshData mrm, Vector3 position, PxMatrix3x3Data rotation, GameObject parent = null)
        {
            var meshObj = new GameObject(objectName);
            meshObj.SetParent(parent);

            var meshFilter = meshObj.AddComponent<MeshFilter>();
            var meshRenderer = meshObj.AddComponent<MeshRenderer>();
            var meshCollider = meshObj.AddComponent<MeshCollider>();

            SetPosAndRot(meshObj, position, rotation);

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
            }

            return meshObj;
        }

        public GameObject Create(string objectName, PxModelMeshData mdm, PxModelHierarchyData mdh, Vector3 position = default, PxMatrix3x3Data rotation = default, GameObject parent = null)
        {
            var meshRootObject = new GameObject(objectName);
            SetPosAndRot(meshRootObject, position, rotation);

            try
            {
                foreach (var mesh in mdm.meshes)
                {
                    var subMesh = new GameObject(mesh.mesh.materials.First().name);
                    var meshFilter = subMesh.AddComponent<MeshFilter>();
                    var meshRenderer = subMesh.AddComponent<SkinnedMeshRenderer>();
                    var meshCollider = subMesh.AddComponent<MeshCollider>();

                    PrepareMeshRenderer(meshRenderer, mesh.mesh);
                    PrepareMeshFilter(meshFilter, mesh);
                    meshRenderer.sharedMesh = meshFilter.mesh; // FIXME - We could get rid of meshFilter as the same mesh is needed on SkinnedMeshRenderer. Need to test...
                    meshCollider.sharedMesh = meshFilter.mesh;

                    CreateBonesData(subMesh, meshRenderer, mdh);

                    //meshRenderer.rootBone = meshRootObject.transform;

                    subMesh.SetParent(meshRootObject);
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogError(e.Message);
                Destroy(meshRootObject);
            }

            meshRootObject.SetParent(parent);

            return meshRootObject;
        }


        private void SetPosAndRot(GameObject obj, Vector3 position, PxMatrix3x3Data rotation)
        {
            if (position.Equals(default) && rotation.Equals(default))
                return;

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
            obj.transform.rotation = matrix4x4.rotation;
            obj.transform.position = position;
        }

        private void PrepareMeshRenderer(Renderer renderer, WorldData.SubMeshData subMesh)
        {
            var standardShader = Shader.Find("Standard");
            var material = new Material(standardShader);
            var bMaterial = subMesh.material;

            renderer.material = material;

            // No texture to add.
            if (bMaterial.texture == "")
                return;

            var texture = assetCache.TryGetTexture(bMaterial.texture);

            if (null == texture)
                throw new Exception("Couldn't get texture from name: " + bMaterial.texture);

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

        private void PrepareMeshRenderer(Renderer renderer, PxMultiResolutionMeshData mrmData)
        {
            var finalMaterials = new List<Material>(mrmData.subMeshes.Length);

            foreach (var subMesh in mrmData.subMeshes)
            {
                var standardShader = Shader.Find("Standard");
                var material = new Material(standardShader);
                var materialData = subMesh.material;

                renderer.material = material;

                // No texture to add.
                if (materialData.texture == "")
                    return;

                var texture = assetCache.TryGetTexture(materialData.texture);

                if (null == texture)
                    throw new Exception("Couldn't get texture from name: " + materialData.texture);

                material.mainTexture = texture;

                finalMaterials.Add(material);
            }

            renderer.SetMaterials(finalMaterials);
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

                    preparedBoneWeights.Add(weights[index1.index].ToBoneWeight());
                    preparedBoneWeights.Add(weights[index2.index].ToBoneWeight());
                    preparedBoneWeights.Add(weights[index3.index].ToBoneWeight());
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
    }
}
