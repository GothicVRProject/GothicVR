using GVR.Npc;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Animation;
using PxCs.Data.Mesh;
using PxCs.Data.Struct;
using PxCs.Interface;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
            // Example: visualname = HUMANS.MDS
            // FIXME - add cache to ModelScript (E.g. HUMANS.MDS are called multiple times)

            var modelScript = PxModelScript.GetModelScriptFromVdf(PhoenixBridge.VdfsPtr, data.visual); // Declaration of animations in a model.
            var skeletonName = modelScript.skeleton.name.Replace(".ASC", ".MDM");

            //var anim = PxAnimation.LoadFromVdf(PhoenixBridge.VdfsPtr, )

            if (null == tempHumanAnimations)
            {
                tempHumanAnimations = new PxAnimationData[modelScript.animations.Length];
                for (int i = 0; i < tempHumanAnimations.Length; i++)
                {
                    var animName = data.visual.Replace(".MDS", $"-{modelScript.animations[i].name}.MAN");

                    //// FIXME - cache
                    tempHumanAnimations[i] = PxAnimation.LoadFromVdf(PhoenixBridge.VdfsPtr, animName);
                }
            }

            object mdl = null; // Model.h --> if null
            object mdh = null; // --> if null
            var mdm = PxModelMesh.LoadModelMeshFromVdf(PhoenixBridge.VdfsPtr, skeletonName); // --> if null

            var mrm = PxMultiResolutionMesh.GetMRMFromVdf(PhoenixBridge.VdfsPtr, skeletonName); // Keyframes of animation.

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

            var gameObject = CreateNpcMesh(mrm);

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

            AddAnimation(gameObject);


            var animator = gameObject.AddComponent<Animator>();


            var clip1 = new AnimationClip();

            AnimationPlayableUtilities.PlayClip(animator, clip1, out PlayableGraph playableGraph);
        }

        private static void Mdl_ApplyOverlayMds(VmGothicBridge.Mdl_ApplyOverlayMdsData data)
        {

        }

        private static void Mdl_SetVisualBody(VmGothicBridge.Mdl_SetVisualBodyData data)
        {
            // TBD
        }


        // FIXME - Logic is copy&pasted from VobCreator.cs - We need to create a proper class where we can reuse functionality
        // FIXME - instead of duplicating it!

        private static GameObject CreateNpcMesh(PxMultiResolutionMeshData mrm)
        {
            return SingletonBehaviour<MultiResolutionMeshCreator>.GetOrCreate().Create(mrm, null, "foobar", Vector3.zero, new PxMatrix3x3Data());
        }

        private static void AddAnimation(GameObject gameObject)
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
