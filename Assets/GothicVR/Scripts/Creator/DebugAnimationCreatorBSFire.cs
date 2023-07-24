using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Creator.Meshes;
using GVR.Debugging;
using GVR.Demo;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Animation;
using PxCs.Data.Mesh;
using PxCs.Data.Model;
using PxCs.Data.Struct;
using PxCs.Interface;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace GVR.Creator
{
    public class DebugAnimationCreatorBSFire : SingletonBehaviour<DebugAnimationCreatorBSFire>
    {
        private AssetCache assetCache;
        private const string DEFAULT_SHADER = "Universal Render Pipeline/Unlit";


        private void Start()
        {
            assetCache = AssetCache.I;
        }

        public void Create(string worldName)
        {
            if (!FeatureFlags.I.CreateExampleAnimation)
                return;

            // var blenderObject = GameObject.Find("Armature");
            // var blenderRenderer = blenderObject.GetComponent<SkinnedMeshRenderer>();

            var name = "DebugBSFire";
            var mdsName = "BSFIRE_OC.MDS";
            var mdlName = "BSFIRE_OC.mdl";
            var animationName = "S_S1";


            var mds = PxModelScript.GetModelScriptFromVdf(GameData.I.VdfsPtr, mdsName);
            var mdl = PxModel.LoadModelFromVdf(GameData.I.VdfsPtr, mdlName);


            var obj1 = CreateBSFireObj(name, mdl);
            var obj2 = MeshCreator.I.Create(name + "2", mdl, new(-26f, 10f, 0), Quaternion.identity);

            SceneManager.GetSceneByName(worldName).GetRootGameObjects().Append(obj1);
            SceneManager.GetSceneByName(worldName).GetRootGameObjects().Append(obj2);

            obj1.transform.localPosition = new(-25f, 10f, 0);

            PlayAnimationBSFire(obj1, mds, mdl, mdsName, animationName);
            PlayAnimationBSFire(obj2, mds, mdl, mdsName, animationName);
        }

        private GameObject CreateBSFireObj(string objectName, PxModelData mdl)
        {
            var mdh = mdl.hierarchy!;
            var mdm = mdl.mesh;

            var rootObj = new GameObject(objectName);

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
                        nodeObj.SetParent(rootObj);
                    else
                        nodeObj.SetParent(nodeObjects[node.parentIndex]);
                }
                
                for (var i=0; i<nodeObjects.Length; i++)
                {
                    if (mdh.nodes[i].parentIndex == -1)
                        nodeObjects[i].transform.position = mdh.rootTranslation.ToUnityVector();
                    else
                        SetPosAndRot(nodeObjects[i], mdh.nodes[i].transform.ToUnityMatrix());
                }
            }

            //// Fill GameObjects with Meshes from "original" Mesh
            foreach (var softSkinMesh in mdm.meshes)
            {
                var mesh = softSkinMesh.mesh;

                var meshObj = new GameObject("JaX_ZM_0");
                meshObj.SetParent(rootObj);

                var meshFilter = meshObj.AddComponent<MeshFilter>();
                var meshRenderer = meshObj.AddComponent<SkinnedMeshRenderer>();

                // FIXME - hard coded as it's the right value for BSFire. Need to be more dynamic by using element which has parent=-1.
                meshRenderer.rootBone = nodeObjects[0].transform;
                
                PrepareMeshRenderer(meshRenderer, mesh);
                PrepareMeshFilter(meshFilter, softSkinMesh);

                meshRenderer.sharedMesh = meshFilter.mesh;

                CreateBonesData(rootObj, nodeObjects, meshRenderer, softSkinMesh);
            }


            // Fill GameObjects with Meshes from attachments
            foreach (var subMesh in mdm.attachments)
            {
                var boneObj = nodeObjects.First(bone => bone.name == subMesh.Key);
                var node = mdh.nodes.First(i => i.name == boneObj.name);

                var meshObj = boneObj; // new GameObject("zm_" + boneObj.name);
                //meshObj.SetParent(boneObj, false, false);
                //SetPosAndRot(meshObj, node.transform);

                var meshFilter = meshObj.AddComponent<MeshFilter>();
                var meshRenderer = meshObj.AddComponent<MeshRenderer>();

                PrepareMeshRenderer(meshRenderer, subMesh.Value);
                PrepareMeshFilter(meshFilter, subMesh.Value);
            }

            return rootObj;
        }
        
        private void SetPosAndRot(GameObject obj, PxMatrix4x4Data matrix)
        {
            SetPosAndRot(obj, matrix.ToUnityMatrix());
        }
        
        private void SetPosAndRot(GameObject obj, Matrix4x4 unityMatrix)
        {
            SetPosAndRot(obj, unityMatrix.GetPosition() / 100, unityMatrix.rotation);
        }

        private void SetPosAndRot(GameObject obj, Vector3 position, Quaternion rotation)
        {
            obj.transform.localRotation = rotation;
            obj.transform.localPosition = position;
        }

        private void PrepareMeshRenderer(Renderer renderer, PxMultiResolutionMeshData mrmData)
        {
            var finalMaterials = new List<Material>(mrmData.subMeshes.Length);

            foreach (var subMesh in mrmData.subMeshes)
            {
                var standardShader = Shader.Find(DEFAULT_SHADER);
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

        private void PlayAnimationBSFire(GameObject rootObj, PxModelScriptData mds, PxModelData mdl, string mdsName, string animationName)
        {
            var mdh = mdl.hierarchy;

            PxAnimationData[] animations = new PxAnimationData[mds.animations.Length];
            for (int i = 0; i < animations.Length; i++)
            {
                var animName = mdsName.Replace(".MDS", $"-{mds.animations[i].name}.MAN", StringComparison.OrdinalIgnoreCase);
                animations[i] = PxAnimation.LoadFromVdf(GameData.I.VdfsPtr, animName);
            }
            var animation = animations.First(i => i.name == animationName);

            var animator = rootObj.gameObject.AddComponent<Animator>();
            var clip = new AnimationClip();
            var playableGraph = PlayableGraph.Create(rootObj.name);

            playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var curves = new Dictionary<string, List<AnimationCurve>>((int)animation.nodeCount);
            var boneNames = animation.node_indices.Select(nodeIndex => mdh.nodes[nodeIndex].name).ToArray();

            // Initialize array
            for (var boneId = 0; boneId < boneNames.Length; boneId++)
            {
                var boneName = boneNames[boneId];
                curves.Add(boneName, new List<AnimationCurve>(7));

                // Initialize 7 dimensions. (3x position, 4x rotation)
                curves[boneName].AddRange(Enumerable.Range(0, 7).Select(i => new AnimationCurve()).ToArray());
            }

            // Add KeyFrames from PxSamples
            for (var i = 0; i < animation.samples.Length; i++)
            {
                // We want to know what time it is for the animation.
                // Therefore we need to know fps multiplied with current sample. As there are nodeCount samples before a new time starts,
                // we need to add this to the calculation.
                var time = (1 / animation.fps) * (int)(i / animation.nodeCount);
                var sample = animation.samples[i];
                var boneId = i % animation.nodeCount;
                var boneName = boneNames[boneId];

                var boneList = curves[boneName];
                var uPosition = sample.position.ToUnityVector();

                // We add 6 properties for location and rotation.
                boneList[0].AddKey(time, uPosition.x);
                boneList[1].AddKey(time, uPosition.y);
                boneList[2].AddKey(time, uPosition.z);
                boneList[3].AddKey(time, -sample.rotation.w); // It's important to have this value with a -1. Otherwise animation is inversed.
                boneList[4].AddKey(time, sample.rotation.x);
                boneList[5].AddKey(time, sample.rotation.y);
                boneList[6].AddKey(time, sample.rotation.z);
            }

            // FIXME - Hard coded test for BSFire!
            Dictionary<string, string> relativeClipPathMap = new()
            {
                {"BIP01 PFX_FIRE", "BIP01 FPLACE/BIP01 PFX_FIRE"},
                {"BIP01 BELLOW2", "BIP01 FPLACE/BIP01 BELLOW3/BIP01 BELLOW2"},
                {"BIP01 BELLOW1", "BIP01 FPLACE/BIP01 BELLOW3/BIP01 BELLOW2/BIP01 BELLOW1"},
                {"BIP01 BELLOW4", "BIP01 FPLACE/BIP01 BELLOW3/BIP01 BELLOW2/BIP01 BELLOW1/BIP01 BELLOW4"},
            };

            foreach (var entry in curves)
            {
                clip.SetCurve(relativeClipPathMap[entry.Key], typeof(Transform), "m_LocalPosition.x", entry.Value[0]);
                clip.SetCurve(relativeClipPathMap[entry.Key], typeof(Transform), "m_LocalPosition.y", entry.Value[1]);
                clip.SetCurve(relativeClipPathMap[entry.Key], typeof(Transform), "m_LocalPosition.z", entry.Value[2]);
                clip.SetCurve(relativeClipPathMap[entry.Key], typeof(Transform), "m_LocalRotation.w", entry.Value[3]);
                clip.SetCurve(relativeClipPathMap[entry.Key], typeof(Transform), "m_LocalRotation.x", entry.Value[4]);
                clip.SetCurve(relativeClipPathMap[entry.Key], typeof(Transform), "m_LocalRotation.y", entry.Value[5]);
                clip.SetCurve(relativeClipPathMap[entry.Key], typeof(Transform), "m_LocalRotation.z", entry.Value[6]);
            }

            clip.wrapMode = WrapMode.Loop;

            var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, animation.name, animator);

            playableOutput.SetSourcePlayable(clipPlayable);
            clipPlayable.SetDuration(animation.frameCount / animation.fps);
            clipPlayable.SetSpeed(0.1);

            GraphVisualizerClient.Show(playableGraph);

            playableGraph.Play();
        }
    }
}