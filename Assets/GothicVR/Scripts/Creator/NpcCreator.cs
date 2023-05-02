using GVR.Npc;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Animation;
using PxCs.Data.Model;
using PxCs.Extensions;
using PxCs.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GVR.Creator
{
    public class NpcCreator : SingletonBehaviour<NpcCreator>
    {
        void Start()
        {
            VmGothicBridge.PhoenixWld_InsertNpc.AddListener(Wld_InsertNpc);
            VmGothicBridge.PhoenixTA_MIN.AddListener(TA_MIN);
            VmGothicBridge.PhoenixMdl_SetVisual.AddListener(Mdl_SetVisual);
            VmGothicBridge.PhoenixMdl_ApplyOverlayMds.AddListener(Mdl_ApplyOverlayMds);
            VmGothicBridge.PhoenixMdl_SetVisualBody.AddListener(Mdl_SetVisualBody);
        }

        /// <summary>
        /// Original Gothic uses this function to spawn an NPC instance into the world.
        /// 
        /// The startpoint to walk isn't neccessarily the spawnpoint mentioned here.
        /// It can also be the currently active routine point to walk to.
        /// We therefore execute the daily routines to collect current location and use this as spawn location.
        /// </summary>
        public static void Wld_InsertNpc(int npcInstance, string spawnpoint)
        {
            var npcContainer = GameObject.Find("NPCs");

            var initialSpawnpoint = PhoenixBridge.World.waypoints
                .FirstOrDefault(item => item.name.ToLower() == spawnpoint.ToLower());

            if (initialSpawnpoint == null)
            {
                Debug.LogWarning(string.Format("spawnpoint={0} couldn't be found.", spawnpoint));
                return;
            }

            var pxNpc = PxVm.InitializeNpc(PhoenixBridge.VmGothicPtr, (uint)npcInstance);

            var newNpc = Instantiate(Resources.Load<GameObject>("Prefabs/Npc"));
            newNpc.name = string.Format("{0}-{1}", string.Concat(pxNpc.names), spawnpoint);
            var npcRoutine = pxNpc.routine;

            PxVm.CallFunction(PhoenixBridge.VmGothicPtr, (uint)npcRoutine, pxNpc.npcPtr);

            newNpc.GetComponent<Properties>().npc = pxNpc;

            if (PhoenixBridge.npcRoutines.TryGetValue(pxNpc.npcPtr, out List<RoutineData> routines))
            {
                initialSpawnpoint = PhoenixBridge.World.waypoints
                    .FirstOrDefault(item => item.name.ToLower() == routines.First().waypoint.ToLower());
                newNpc.GetComponent<Routine>().routines = routines;
            }

            newNpc.transform.position = initialSpawnpoint.position.ToUnityVector();
            newNpc.transform.parent = npcContainer.transform;
        }

        private static void TA_MIN(VmGothicBridge.TA_MINData data)
        {
            // If we put h=24, DateTime will throw an error instead of rolling.
            var stop_hFormatted = data.stop_h == 24 ? 0 : data.stop_h;

            RoutineData routine = new()
            {
                start_h = data.start_h,
                start_m = data.start_m,
                start = new(1, 1, 1, data.start_h, data.start_m, 0),
                stop_h = data.stop_h,
                stop_m = data.stop_m,
                stop = new(1, 1, 1, stop_hFormatted, data.stop_m, 0),
                action = data.action,
                waypoint = data.waypoint
            };

            // Add element if key not yet exists.
            PhoenixBridge.npcRoutines.TryAdd(data.npc, new());
            PhoenixBridge.npcRoutines[data.npc].Add(routine);
        }


        // FIXME - Performance increased from 20sec load down to 5sec load.
        // FIXME - Need to be replaced with proper caching later.
        private static PxAnimationData[] tempHumanAnimations = null;

        private static void Mdl_SetVisual(VmGothicBridge.Mdl_SetVisualData data)
        {
            var name = PxVm.pxVmInstanceNpcGetName(data.npcPtr, 0).MarshalAsString();

            if (name != "Velaya")
                return;


            // Example: visualname = HUMANS.MDS
            // FIXME - add cache to ModelScript (E.g. HUMANS.MDS are called multiple times)
            var mds = PxModelScript.GetModelScriptFromVdf(PhoenixBridge.VdfsPtr, data.visual); // Declaration of animations in a model.

            velayaMds = mds;


            if (null == tempHumanAnimations)
            {
                tempHumanAnimations = new PxAnimationData[mds.animations.Length];
                for (int i = 0; i < tempHumanAnimations.Length; i++)
                {
                    var animName = data.visual.Replace(".MDS", $"-{mds.animations[i].name}.MAN");

                    //// FIXME - cache the right way. (Not in a hidden static temp variable)
                    tempHumanAnimations[i] = PxAnimation.LoadFromVdf(PhoenixBridge.VdfsPtr, animName);
                }
            }


            if (mds.skeleton.disableMesh)
            {
                var mdhName = data.visual.Replace(".MDS", ".MDH");

                var mdh = PxModelHierarchy.LoadFromVdf(PhoenixBridge.VdfsPtr, mdhName);
                velayaMdh = mdh;


                //var skeletonName = mds.skeleton.name.Replace(".ASC", ".MDM");
                //var mdm = PxModelMesh.LoadModelMeshFromVdf(PhoenixBridge.VdfsPtr, skeletonName); // --> if null

                //var gameObject = CreateNpcMesh(mdm, name);

                //AddAnimations(gameObject, mdh, mdm);

            }
            else
            {
                var skeletonName = mds.skeleton.name.Replace(".ASC", ".MDM");
                var mdm = PxModelMesh.LoadModelMeshFromVdf(PhoenixBridge.VdfsPtr, skeletonName); // --> if null

                //var gameObject = CreateNpcMesh(mdm, name);
            }

            return;

            /*
             * Walk animation load:
             * 1. load ModelScript (with array Animations[].name) (done)
             * 2. Use this name, add MAN and load it as PxAnimationData (done)
             * 3. Use the PxAnimationData.samples for animation
             * 4. PxAnimationData.nodeIndices --> Example: a skeleton with 10 bones (nodes) and 20 frames has 
             *    10 node_indices and 10*20samples (i.e. 10 samples == 10 frames for bone/node1, 11...20 samples == 10 frames for node 2)
             * 5. PxModelHierarchy (mdh) == skeleton/bones. 
             * 
             * Check checksums!
             */


            // https://forum.unity.com/threads/whats-the-difference-between-animation-and-animator.288962/
            // https://answers.unity.com/questions/1319072/how-to-change-animation-clips-of-an-animator-state.html
            // https://answers.unity.com/questions/911169/procudurally-generate-animationclip-at-runtime.html
            // Playable API: https://blog.unity.com/engine-platform/extending-timeline-practical-guide
            // https://docs.unity3d.com/Manual/Playables.html?_ga=2.213931281.7656710.1587191155-1588388095.1584548241
            // Playables API Demo: https://catlikecoding.com/unity/tutorials/tower-defense/animation/
            // Demo 2: https://delphic.me.uk/blog/seekers_animation_system
            // Demo 2 - source code: https://gist.github.com/delphic/42e7ade45edb60df418954ef09287337

            // Playable API open source project: https://forum.unity.com/threads/uplayableanimation-playableapi-animation-system.1353557/
            // github: https://github.com/EricHu33/uPlayableAnimation

            //AddAnimation(gameObject);


            //var animator = gameObject.AddComponent<Animator>();


            //var clip1 = new AnimationClip();

            //AnimationPlayableUtilities.PlayClip(animator, clip1, out PlayableGraph playableGraph);
        }

        private static void Mdl_ApplyOverlayMds(VmGothicBridge.Mdl_ApplyOverlayMdsData data)
        {
            // TBD

            var name = PxVm.pxVmInstanceNpcGetName(data.npcPtr, 0).MarshalAsString();

            if (name != "Velaya")
                return;

            var mds = PxModelScript.GetModelScriptFromVdf(PhoenixBridge.VdfsPtr, data.overlayname);
        }


        private static PxModelHierarchyData velayaMdh;
        private static PxModelScriptData velayaMds;


        private static void Mdl_SetVisualBody(VmGothicBridge.Mdl_SetVisualBodyData data)
        {
            // TBD
            var name = PxVm.pxVmInstanceNpcGetName(data.npcPtr, 0).MarshalAsString();

            if (name != "Velaya")
                return;

            var mdm = PxModelMesh.LoadModelMeshFromVdf(PhoenixBridge.VdfsPtr, $"{data.body}.MDM");

            var root = CreateNpcMesh(name, mdm, velayaMdh).transform.GetChild(0).gameObject;

            // DEBUG - change location for better visibility
            root.transform.position += root.transform.position + new Vector3(1, 5, 1);

            var debugMesh = root.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            CreateAnimations(root);


            //var tempAnimation = tempHumanAnimations[1];



            //Animation animation = root.AddComponent<Animation>();
            //AnimationCurve curve = new AnimationCurve();
            //curve.keys = new Keyframe[] { new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 3f, 0f, 0f), new Keyframe(2f, 0f, 0f, 0f) };
            //AnimationClip clip = new AnimationClip();
            //clip.legacy = true;
            //clip.SetCurve("BIP01", typeof(Transform), "m_LocalPosition.z", curve);
            //clip.wrapMode = WrapMode.Loop;
            //animation.AddClip(clip, "test");
            //animation.Play("test");

        }

        private static void CreateAnimations(GameObject root)
        {
            var animator = root.AddComponent<Animator>();
            AnimationClip clip = new AnimationClip();
            var playableGraph = PlayableGraph.Create("foobar");

            var cachedAnimations = tempHumanAnimations;
            var anim = cachedAnimations.First(i => i.name == "S_DANCE1");
            playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var curves = new Dictionary<string, List<AnimationCurve>>((int)anim.nodeCount);
            var boneNames = anim.node_indices.Select(nodeIndex => velayaMdh.nodes[nodeIndex].name).ToArray();

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
                var time = (1/anim.fps) * (int)(i / anim.nodeCount);
                var sample = anim.samples[i];
                var boneId = i % anim.nodeCount;
                var boneName = boneNames[boneId];

                var boneList = curves[boneName];
                var uPosition = sample.position / 100; // Gothic locations are too big by factor 100

                // We add 6 properties for location and rotation.
                boneList[0].AddKey(time, uPosition.X);
                boneList[1].AddKey(time, uPosition.Y);
                boneList[2].AddKey(time, uPosition.Z);
                boneList[3].AddKey(time, sample.rotation.w);
                boneList[4].AddKey(time, sample.rotation.x);
                boneList[5].AddKey(time, sample.rotation.y);
                boneList[6].AddKey(time, sample.rotation.z);
            }

            foreach (var entry in curves)
            {
                clip.SetCurve(entry.Key, typeof(Transform), "m_LocalPosition.x", entry.Value[0]);
                clip.SetCurve(entry.Key, typeof(Transform), "m_LocalPosition.y", entry.Value[1]);
                clip.SetCurve(entry.Key, typeof(Transform), "m_LocalPosition.z", entry.Value[2]);
                clip.SetCurve(entry.Key, typeof(Transform), "m_LocalRotation.w", entry.Value[3]);
                clip.SetCurve(entry.Key, typeof(Transform), "m_LocalRotation.x", entry.Value[4]);
                clip.SetCurve(entry.Key, typeof(Transform), "m_LocalRotation.y", entry.Value[5]);
                clip.SetCurve(entry.Key, typeof(Transform), "m_LocalRotation.z", entry.Value[6]);
            }

            clip.wrapMode = WrapMode.Loop;

            var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, anim.name, animator);

            playableOutput.SetSourcePlayable(clipPlayable);
            clipPlayable.SetDuration(anim.frameCount / anim.fps);

            GraphVisualizerClient.Show(playableGraph);

            playableGraph.Play();


            // 0 -> Start, 1 -> middle, 2 -> return to original
            //AnimationClip clip = new AnimationClip();
            //AnimationCurve curve = new AnimationCurve();
            //curve.keys = new Keyframe[] { new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f), new Keyframe(2f, 0f, 0f, 0f) };
            //clip.SetCurve("BIP01 L UPPERARM", typeof(Transform), "m_LocalPosition.z", curve);
            //clip.wrapMode = WrapMode.Loop;

            //var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
            //var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
            //playableOutput.SetSourcePlayable(clipPlayable);
            //clipPlayable.SetDuration(2f);

            //GraphVisualizerClient.Show(playableGraph);

            //playableGraph.Play();


            //var animator = root.AddComponent<Animator>();

            //var playableGraph = PlayableGraph.Create("foobar");
            //playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            //AnimationClip clip = new AnimationClip();
            //AnimationCurve curve = new AnimationCurve();
            //curve.keys = new Keyframe[] { new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 3f, 0f, 0f), new Keyframe(2f, 0f, 0f, 0f) };
            //clip.SetCurve("BIP01 L UPPERARM", typeof(Transform), "m_LocalPosition.z", curve);
            //clip.wrapMode = WrapMode.Loop;

            //var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
            //var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
            //playableOutput.SetSourcePlayable(clipPlayable);

            //GraphVisualizerClient.Show(playableGraph);

            //playableGraph.Play();
        }

        // FIXME - Logic is copy&pasted from VobCreator.cs - We need to create a proper class where we can reuse functionality
        // FIXME - instead of duplicating it!

        private static GameObject CreateNpcMesh(string name, PxModelMeshData mdm, PxModelHierarchyData mdh)
        {
            return SingletonBehaviour<MeshCreator>.GetOrCreate().Create(name, mdm, mdh, default, default, null);
        }

        private static void AddAnimations(GameObject gameObject, PxModelHierarchyData mdh, PxModelMeshData mdm)
        {
            if (mdm.meshes.Length != 1)
                throw new ArgumentOutOfRangeException("Only one material is supported for bones/animations as of now.");



            // Next: mdh.nodes == bones. Every bone need to be a separate GameObject below current one. (e.g. "Bip01 Head")


            //gameObject.AddComponent<SkinnedMeshRenderer>();

            /*
             * What we need:
             * 1. Bones
             * 2. Animations
             */
            // NEXT - Add bones (mdh...transform as bones!
            // NEXT - link node_indices (Aka animation information) to bones and start play!


            //var renderer = new SkinnedMeshRenderer();
            //Transform[] bones = new Transform[mdh.nodes.Length];

            //for (int i = 0; i < mdh.nodes.Length; i++)
            //{
            //    var transform = mdh.nodes[i].transform.ToUnityMatrix();

            //    bones[i] = new Transform()
            //    {
            //        name = mdh.nodes[i].name,
            //        position = transform.GetPosition(),
            //        rotation = transform.rotation,
            //        localScale = Vector3.one
            //    };
            //}
            //humanDescription.skeleton = skeletonBones;

            //var avatar = AvatarBuilder.BuildHumanAvatar(gameObject, humanDescription);

            //var animator = gameObject.AddComponent<Animator>();
            //animator.avatar = avatar;

        }


        private static void AddAnimationDemo(GameObject gameObject)
        {
            var anim = gameObject.AddComponent<Animation>();
            var rend = gameObject.AddComponent<SkinnedMeshRenderer>();
            var mesh = gameObject.GetComponent<MeshCollider>().sharedMesh;
            // Build basic mesh
            //Mesh mesh = new Mesh();
            //mesh.vertices = new Vector3[] { new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(-1, 5, 0), new Vector3(1, 5, 0) };
            //mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
            //mesh.triangles = new int[] { 2, 3, 1, 2, 1, 0 };
            //mesh.RecalculateNormals();
            //rend.material = new Material(Shader.Find("Diffuse"));

            // assign bone weights to mesh
            BoneWeight[] weights = new BoneWeight[mesh.vertices.Length];

            for (int i = 0; i < weights.Length; i++)
            {
                if (i < weights.Length / 2)
                {
                    weights[0].boneIndex0 = 0;
                    weights[0].weight0 = 1;
                }
                else
                {
                    weights[0].boneIndex0 = 1;
                    weights[0].weight0 = 1;
                }
            }
            mesh.boneWeights = weights;

            // Create Bone Transforms and Bind poses
            // One bone at the bottom and one at the top

            Transform[] bones = new Transform[2];
            Matrix4x4[] bindPoses = new Matrix4x4[2];
            bones[0] = gameObject.transform;// new GameObject("Lower").transform;
            //bones[0].parent = gameObject.transform;
            // Set the position relative to the parent
            bones[0].localRotation = Quaternion.identity;
            bones[0].localPosition = Vector3.zero;
            // The bind pose is bone's inverse transformation matrix
            // In this case the matrix we also make this matrix relative to the root
            // So that we can move the root game object around freely
            bindPoses[0] = bones[0].worldToLocalMatrix * gameObject.transform.localToWorldMatrix;

            bones[1] = new GameObject("Upper").transform;
            bones[1].parent = gameObject.transform;
            // Set the position relative to the parent
            bones[1].localRotation = Quaternion.identity;
            bones[1].localPosition = new Vector3(0, 5, 0);
            // The bind pose is bone's inverse transformation matrix
            // In this case the matrix we also make this matrix relative to the root
            // So that we can move the root game object around freely
            bindPoses[1] = bones[1].worldToLocalMatrix * gameObject.transform.localToWorldMatrix;

            // bindPoses was created earlier and was updated with the required matrix.
            // The bindPoses array will now be assigned to the bindposes in the Mesh.
            mesh.bindposes = bindPoses;

            // Assign bones and bind poses
            rend.bones = bones;
            rend.sharedMesh = mesh;

            // Assign a simple waving animation to the bottom bone
            AnimationCurve curve = new AnimationCurve();
            curve.keys = new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 3, 0, 0), new Keyframe(2, 0.0F, 0, 0) };

            // Create the clip with the curve
            AnimationClip clip = new AnimationClip();
            clip.legacy = true;
            clip.SetCurve("Lower", typeof(Transform), "m_LocalPosition.z", curve);

            // Add and play the clip
            clip.wrapMode = WrapMode.Loop;
            anim.AddClip(clip, "test");
            anim.Play("test");
        }
    }
}
