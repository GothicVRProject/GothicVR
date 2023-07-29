using GVR.Caches;
using GVR.Demo;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Animation;
using PxCs.Data.Mesh;
using PxCs.Data.Model;
using PxCs.Data.Struct;
using PxCs.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Creator.Meshes;
using GVR.Debugging;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace GVR.Creator
{
    public class DebugAnimationCreator : SingletonBehaviour<DebugAnimationCreator>
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

            CreateChest(worldName);
            CreateGrindstone(worldName);
            //CreateBloodfly();
        }

        #region Chest
        private void CreateChest(string worldName)
        {
            var name = "DebugChest";
            var mdsName = "ChestSmall_OCCratesmall.MDS";
            var mdlName = "ChestSmall_OCCratesmall.mdl";
            var animationName = "T_S0_2_S1";

            var debugObj = new GameObject("DebugChest");
            SceneManager.GetSceneByName(worldName).GetRootGameObjects().Append(debugObj);

            var mds = PxModelScript.GetModelScriptFromVfs(GameData.I.VfsPtr, mdsName);
            var mdl = PxModel.LoadModelFromVfs(GameData.I.VfsPtr, mdlName);

            var obj = CreateAttachmentObj(name, mdl, debugObj);
            debugObj.transform.localPosition = new(-15f, 10f, 0);

            PlayAnimationChest(obj, mds, mdl, mdsName, animationName);
        }

        private GameObject CreateAttachmentObj(string objectName, PxModelData mdl, GameObject parent)
        {
            var mdh = mdl.hierarchy;
            var mdm = mdl.mesh;

            var rootObj = new GameObject(objectName);
            rootObj.SetParent(parent);

            var bones = new GameObject[mdh.nodes.Length];

            // Create empty GameObjects from hierarchy
            {
                for (var i = 0; i < mdh.nodes.Length; i++)
                {
                    bones[i] = new GameObject(mdh.nodes[i].name);
                }

                // Now set parents
                for (var i = 0; i < mdh.nodes.Length; i++)
                {
                    if (mdh.nodes[i].parentIndex == -1)
                        bones[i].SetParent(rootObj);
                    else
                        bones[i].SetParent(bones[mdh.nodes[i].parentIndex]);
                }
            }

            // Fill GameObjects with Meshes
            foreach (var subMesh in mdm.attachments)
            {
                var boneObj = bones.First(bone => bone.name == subMesh.Key);
                var node = mdh.nodes.First(i => i.name == boneObj.name);

                SetPosAndRot(boneObj, node.transform);


                var meshFilter = boneObj.AddComponent<MeshFilter>();
                var meshRenderer = boneObj.AddComponent<MeshRenderer>();

                PrepareMeshRenderer(meshRenderer, subMesh.Value);
                PrepareMeshFilter(meshFilter, subMesh.Value);
            }

            return rootObj;
        }

        private void SetPosAndRot(GameObject obj, PxMatrix4x4Data matrix)
        {
            var unityMatrix = matrix.ToUnityMatrix();
            SetPosAndRot(obj, unityMatrix.GetPosition() / 100, unityMatrix.rotation);
        }
        private void SetPosAndRot(GameObject obj, Vector3 position, Quaternion rotation)
        {
            // FIXME - This isn't working
            if (position.Equals(default) && rotation.Equals(default))
                return;

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

        private void PlayAnimationChest(GameObject rootObj, PxModelScriptData mds, PxModelData mdl, string mdsName, string animationName)
        {
            var mdh = mdl.hierarchy;

            PxAnimationData[] animations = new PxAnimationData[mds.animations.Length];
            for (int i = 0; i < animations.Length; i++)
            {
                var animName = mdsName.Replace(".MDS", $"-{mds.animations[i].name}.MAN", StringComparison.OrdinalIgnoreCase);
                animations[i] = PxAnimation.LoadFromVfs(GameData.I.VfsPtr, animName);
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
                boneList[3].AddKey(time, -sample.rotation.w);
                boneList[4].AddKey(time, sample.rotation.x); // It's important to have this value with a -1. Otherwise it's inversed.
                boneList[5].AddKey(time, sample.rotation.y); // TODO: Does it need to be -1 as well?
                boneList[6].AddKey(time, sample.rotation.z); // TODO: Does it need to be -1 as well?
            }

            // FIXME - Hard coded test for Chest!
            Dictionary<string, string> relativeClipPathMap = new()
            {
                {"BIP01 CHEST_SMALL_0", "BIP01 CHEST_SMALL_0"},
                {"BIP01 CHEST_SMALL_1", "BIP01 CHEST_SMALL_0/BIP01 CHEST_SMALL_1"},
                {"ZS_POS0", "BIP01 CHEST_SMALL_0/ZS_POS0"}
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
        #endregion

        #region Grindstone
        private void CreateGrindstone(string worldName)
        {
            var name = "DebugGrindstone";
            var mdsName = "BSSHARP_OC.MDS";
            var mdlName = "BSSHARP_OC.mdl";
            var animationName = "S_S1";

            var debugObj = new GameObject("DebugGrindstone");
            SceneManager.GetSceneByName(worldName).GetRootGameObjects().Append(debugObj);

            var mds = PxModelScript.GetModelScriptFromVfs(GameData.I.VfsPtr, mdsName);
            var mdl = PxModel.LoadModelFromVfs(GameData.I.VfsPtr, mdlName);

            var obj = CreateAttachmentObj(name, mdl, debugObj);
            debugObj.transform.localPosition = new(-20f, 10f, 0);

            PlayAnimationGrindstone(obj, mds, mdl, mdsName, animationName);
        }

        private void PlayAnimationGrindstone(GameObject rootObj, PxModelScriptData mds, PxModelData mdl, string mdsName, string animationName)
        {
            var mdh = mdl.hierarchy;

            PxAnimationData[] animations = new PxAnimationData[mds.animations.Length];
            for (int i = 0; i < animations.Length; i++)
            {
                var animName = mdsName.Replace(".MDS", $"-{mds.animations[i].name}.MAN", StringComparison.OrdinalIgnoreCase);
                animations[i] = PxAnimation.LoadFromVfs(GameData.I.VfsPtr, animName);
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

            // FIXME - Hard coded test for Chest!
            Dictionary<string, string> relativeClipPathMap = new()
            {
                {"BIP01 SCHLEIFSTEIN", "BIP01 SCHLEIFSTEIN"},
                {"BIP01 SCHLEIFER1", "BIP01 SCHLEIFSTEIN/BIP01 SCHLEIFER1"},
                {"BIP01 SCHLEIFER2", "BIP01 SCHLEIFSTEIN/BIP01 SCHLEIFER1/BIP01 SCHLEIFER2"},
                {"BIP01 SCHLEIFER3", "BIP01 SCHLEIFSTEIN/BIP01 SCHLEIFER1/BIP01 SCHLEIFER2/BIP01 SCHLEIFER3"},
                {"BIP01 SCHLEIFER4", "BIP01 SCHLEIFSTEIN/BIP01 SCHLEIFER1/BIP01 SCHLEIFER2/BIP01 SCHLEIFER3/BIP01 SCHLEIFER4"},

                {"BIP01 PFX_WHEEL", "BIP01 SCHLEIFSTEIN/BIP01 PFX_WHEEL"},
                {"ZS_POS0", "BIP01 SCHLEIFSTEIN/ZS_POS0"},

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
        #endregion

        private void CreateBloodfly()
        {
            ///
            /// File names to change
            ///
            var name = "Bloodfly";
            var mdsName = "Bloodfly.MDS"; // Bloodfly: "Bloodfly.mds"; Velaya: "BABE.MDS"
            var mdhName = "Bloodfly.mdh"; // Bloodfly: "Bloodfly.mdh"; Velaya: "BABE.mdh"
            var mdlName = "Bloodfly.mdl"; // Bloodfly: "Bloodfly.mdl"; Velaya: "BABE.mdl"
            var mdmName = "Blo_Body.mdm"; // Bloodfly: "Blo_Body.mdm"; Velaya: "Bab_body_Naked0.mdm"
            var mrmName = "BABE.mrm"; // Bloodfly: "Snapper.mrm"; Velaya: "BABE.mrm"
            var animationName = "T_WARN"; // Bloodfly: "S_FISTATTACK"; Velaya: "S_DANCE1"


            var debugObj = new GameObject("DebugAnimationObject");
            SceneManager.GetSceneByName("SampleScene").GetRootGameObjects().Append(debugObj);

            var mds = PxModelScript.GetModelScriptFromVfs(GameData.I.VfsPtr, mdsName);
            var mdl = PxModel.LoadModelFromVfs(GameData.I.VfsPtr, mdlName);
            var mdh = PxModelHierarchy.LoadFromVfs(GameData.I.VfsPtr, mdhName);
            var mdm = PxModelMesh.LoadModelMeshFromVfs(GameData.I.VfsPtr, mdmName);
            var mrm = PxMultiResolutionMesh.GetMRMFromVfs(GameData.I.VfsPtr, mrmName);
            var obj = MeshCreator.I.Create(name, mdm, mdh, default, default, debugObj);



            PxAnimationData[] animations = new PxAnimationData[mds.animations.Length];
            for (int i = 0; i < animations.Length; i++)
            {
                var animName = mdsName.Replace(".MDS", $"-{mds.animations[i].name}.MAN", System.StringComparison.OrdinalIgnoreCase);
                animations[i] = PxAnimation.LoadFromVfs(GameData.I.VfsPtr, animName);
            }
            var debugAnimationNames = animations.Select(i => i.name).ToArray();
            var anim = animations.First(i => i.name == animationName);


            /**
             * Check for "Bip01 Hold Sting"
             * compare rotation and animation with BloodflyWarn.FBX -> .anim file from PoC
             * 
             * mdh.nodes[44] == "Bip01 Hold Sting"
             * animation.node_indices.length == mdh.nodes.length - No node rewiring in this
             */


            ///
            /// Animations
            ///

            var animator = obj.transform.GetChild(0).gameObject.AddComponent<Animator>();
            AnimationClip clip = new AnimationClip();
            var playableGraph = PlayableGraph.Create("foobar");

            playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var curves = new Dictionary<string, List<AnimationCurve>>((int)anim.nodeCount);
            var boneNames = anim.node_indices.Select(nodeIndex => mdh.nodes[nodeIndex].name).ToArray();

            // Initialize array
            for (var boneId = 0; boneId < boneNames.Length; boneId++)
            {
                var boneName = boneNames[boneId];
                curves.Add(boneName, new List<AnimationCurve>(7));

                // Initialize 7 dimensions. (3x position, 4x rotation)
                curves[boneName].AddRange(Enumerable.Range(0, 7).Select(i => new AnimationCurve()).ToArray());
            }

            // Add KeyFrames from PxSamples
            for (var i = 0; i < anim.samples.Length; i++)
            {
                // We want to know what time it is for the animation.
                // Therefore we need to know fps multiplied with current sample. As there are nodeCount samples before a new time starts,
                // we need to add this to the calculation.
                var time = (1 / anim.fps) * (int)(i / anim.nodeCount);
                var sample = anim.samples[i];
                var boneId = i % anim.nodeCount;
                var boneName = boneNames[boneId];

                var boneList = curves[boneName];
                var uPosition = sample.position.ToUnityVector();

                // We add 6 properties for location and rotation.
                boneList[0].AddKey(time, uPosition.x);
                boneList[1].AddKey(time, uPosition.y);
                boneList[2].AddKey(time, uPosition.z);
                boneList[3].AddKey(time, sample.rotation.w);
                boneList[4].AddKey(time, sample.rotation.x);
                boneList[5].AddKey(time, sample.rotation.y);
                boneList[6].AddKey(time, sample.rotation.z);
            }

            // Hard coded test for Bloodfly
            Dictionary<string, string> relativeClipPathMap = new()
            {
                {"BIP01 CENTER", "BIP01 CENTER"},

                {"BIP01 L-LEG BACK01", "BIP01 CENTER/BIP01 L-LEG BACK01"},
                {"BIP01 L-LEG BACK02", "BIP01 CENTER/BIP01 L-LEG BACK01/BIP01 L-LEG BACK02"},
                {"BIP01 L-LEG BACK03", "BIP01 CENTER/BIP01 L-LEG BACK01/BIP01 L-LEG BACK02/BIP01 L-LEG BACK03"},
                {"BIP01 L-LEG BACK04", "BIP01 CENTER/BIP01 L-LEG BACK01/BIP01 L-LEG BACK02/BIP01 L-LEG BACK03/BIP01 L-LEG BACK04"},
                {"BIP01 L-LEG BACK05", "BIP01 CENTER/BIP01 L-LEG BACK01/BIP01 L-LEG BACK02/BIP01 L-LEG BACK03/BIP01 L-LEG BACK04/BIP01 L-LEG BACK05"},

                {"BIP01 L-LEG MIDDLE01", "BIP01 CENTER/BIP01 L-LEG MIDDLE01"},
                {"BIP01 L-LEG MIDDLE02", "BIP01 CENTER/BIP01 L-LEG MIDDLE01/BIP01 L-LEG MIDDLE02"},
                {"BIP01 L-LEG MIDDLE03", "BIP01 CENTER/BIP01 L-LEG MIDDLE01/BIP01 L-LEG MIDDLE02/BIP01 L-LEG MIDDLE03"},
                {"BIP01 L-LEG MIDDLE04", "BIP01 CENTER/BIP01 L-LEG MIDDLE01/BIP01 L-LEG MIDDLE02/BIP01 L-LEG MIDDLE03/BIP01 L-LEG MIDDLE04"},
                {"BIP01 L-LEG MIDDLE05", "BIP01 CENTER/BIP01 L-LEG MIDDLE01/BIP01 L-LEG MIDDLE02/BIP01 L-LEG MIDDLE03/BIP01 L-LEG MIDDLE04/BIP01 L-LEG MIDDLE05"},

                {"BIP01 L-LEG FRONT01", "BIP01 CENTER/BIP01 L-LEG FRONT01"},
                {"BIP01 L-LEG FRONT02", "BIP01 CENTER/BIP01 L-LEG FRONT01/BIP01 L-LEG FRONT02"},
                {"BIP01 L-LEG FRONT03", "BIP01 CENTER/BIP01 L-LEG FRONT01/BIP01 L-LEG FRONT02/BIP01 L-LEG FRONT03"},
                {"BIP01 L-LEG FRONT04", "BIP01 CENTER/BIP01 L-LEG FRONT01/BIP01 L-LEG FRONT02/BIP01 L-LEG FRONT03/BIP01 L-LEG FRONT04"},
                {"BIP01 L-LEG FRONT05", "BIP01 CENTER/BIP01 L-LEG FRONT01/BIP01 L-LEG FRONT02/BIP01 L-LEG FRONT03/BIP01 L-LEG FRONT04/BIP01 L-LEG FRONT05"},

                {"BIP01 R-LEG FRONT01", "BIP01 CENTER/BIP01 R-LEG FRONT01"},
                {"BIP01 R-LEG FRONT02", "BIP01 CENTER/BIP01 R-LEG FRONT01/BIP01 R-LEG FRONT02"},
                {"BIP01 R-LEG FRONT03", "BIP01 CENTER/BIP01 R-LEG FRONT01/BIP01 R-LEG FRONT02/BIP01 R-LEG FRONT03"},
                {"BIP01 R-LEG FRONT04", "BIP01 CENTER/BIP01 R-LEG FRONT01/BIP01 R-LEG FRONT02/BIP01 R-LEG FRONT03/BIP01 R-LEG FRONT04"},
                {"BIP01 R-LEG FRONT05", "BIP01 CENTER/BIP01 R-LEG FRONT01/BIP01 R-LEG FRONT02/BIP01 R-LEG FRONT03/BIP01 R-LEG FRONT04/BIP01 R-LEG FRONT05"},

                {"BIP01 R-LEG MIDDLE01", "BIP01 CENTER/BIP01 R-LEG MIDDLE01"},
                {"BIP01 R-LEG MIDDLE02", "BIP01 CENTER/BIP01 R-LEG MIDDLE01/BIP01 R-LEG MIDDLE02"},
                {"BIP01 R-LEG MIDDLE03", "BIP01 CENTER/BIP01 R-LEG MIDDLE01/BIP01 R-LEG MIDDLE02/BIP01 R-LEG MIDDLE03"},
                {"BIP01 R-LEG MIDDLE04", "BIP01 CENTER/BIP01 R-LEG MIDDLE01/BIP01 R-LEG MIDDLE02/BIP01 R-LEG MIDDLE03/BIP01 R-LEG MIDDLE04"},
                {"BIP01 R-LEG MIDDLE05", "BIP01 CENTER/BIP01 R-LEG MIDDLE01/BIP01 R-LEG MIDDLE02/BIP01 R-LEG MIDDLE03/BIP01 R-LEG MIDDLE04/BIP01 R-LEG MIDDLE05"},

                {"BIP01 R-LEG BACK01", "BIP01 CENTER/BIP01 R-LEG BACK01"},
                {"BIP01 R-LEG BACK02", "BIP01 CENTER/BIP01 R-LEG BACK01/BIP01 R-LEG BACK02"},
                {"BIP01 R-LEG BACK03", "BIP01 CENTER/BIP01 R-LEG BACK01/BIP01 R-LEG BACK02/BIP01 R-LEG BACK03"},
                {"BIP01 R-LEG BACK04", "BIP01 CENTER/BIP01 R-LEG BACK01/BIP01 R-LEG BACK02/BIP01 R-LEG BACK03/BIP01 R-LEG BACK04"},
                {"BIP01 R-LEG BACK05", "BIP01 CENTER/BIP01 R-LEG BACK01/BIP01 R-LEG BACK02/BIP01 R-LEG BACK03/BIP01 R-LEG BACK04/BIP01 R-LEG BACK05"},

                {"BIP01 R-WING01", "BIP01 CENTER/BIP01 R-WING01"},
                {"BIP01 R-WING02", "BIP01 CENTER/BIP01 R-WING02"},
                {"BIP01 L-WING01", "BIP01 CENTER/BIP01 L-WING01"},
                {"BIP01 L-WING02", "BIP01 CENTER/BIP01 L-WING02"},
                {"BIP01 L-PINCER", "BIP01 CENTER/BIP01 L-PINCER"},
                {"BIP01 R-PINCER", "BIP01 CENTER/BIP01 R-PINCER"},

                {"BIP01 TAIL01", "BIP01 CENTER/BIP01 TAIL01"},
                {"BIP01 TAIL02", "BIP01 CENTER/BIP01 TAIL01/BIP01 TAIL02"},
                {"BIP01 TAIL03", "BIP01 CENTER/BIP01 TAIL01/BIP01 TAIL02/BIP01 TAIL03"},
                {"BIP01 TAIL04", "BIP01 CENTER/BIP01 TAIL01/BIP01 TAIL02/BIP01 TAIL03/BIP01 TAIL04"},
                {"BIP01 TAIL05", "BIP01 CENTER/BIP01 TAIL01/BIP01 TAIL02/BIP01 TAIL03/BIP01 TAIL04/BIP01 TAIL05"},
                {"BIP01 TAIL06", "BIP01 CENTER/BIP01 TAIL01/BIP01 TAIL02/BIP01 TAIL03/BIP01 TAIL04/BIP01 TAIL05/BIP01 TAIL06"},
                {"BIP01 TAIL07", "BIP01 CENTER/BIP01 TAIL01/BIP01 TAIL02/BIP01 TAIL03/BIP01 TAIL04/BIP01 TAIL05/BIP01 TAIL06/BIP01 TAIL07"},
                {"BIP01 HOLD STING", "BIP01 CENTER/BIP01 TAIL01/BIP01 TAIL02/BIP01 TAIL03/BIP01 TAIL04/BIP01 TAIL05/BIP01 HOLD STING"}

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

            // sting test
            var stingEntries = curves["BIP01 HOLD STING"];
            var stringLocationX = stingEntries[0];
            var stringLocationY = stingEntries[1];
            var stringLocationZ = stingEntries[2];
            var stingRotationW = stingEntries[3];
            var stingRotationX = stingEntries[4];
            var stingRotationY = stingEntries[5];
            var stingRotationZ = stingEntries[6];

            clip.wrapMode = WrapMode.Loop;

            var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, anim.name, animator);

            playableOutput.SetSourcePlayable(clipPlayable);
            clipPlayable.SetDuration(anim.frameCount / anim.fps);
            clipPlayable.SetSpeed(0.5);

            GraphVisualizerClient.Show(playableGraph);

            playableGraph.Play();





            debugObj.transform.localPosition = new(-10f, 10f, 0);
        }
    }
}