using GVR.Demo;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Animation;
using PxCs.Interface;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace GVR.Creator
{
    public class DebugAnimationCreator : SingletonBehaviour<DebugAnimationCreator>
    {
        public void Create()
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().CreateExampleAnimation)
                return;

            ///
            /// File names to change
            ///
            var mdsName = "BABE.MDS"; // Snapper: "Snapper.mds"; Velaya: "BABE.MDS"
            var mdhName = "BABE.mdh"; // Snapper: "Snapper.mdh"; Velaya: "BABE.mdh"
            var mdlName = "BABE.mdl"; // Snapper: "Snapper.mdl"; Velaya: "BABE.mdl"
            var mdmName = "Bab_body_Naked0.mdm"; // Snapper: "Sna_Body.mdm"; Velaya: "Bab_body_Naked0.mdm"
            var mrmName = "BABE.mrm"; // Snapper: "Snapper.mrm"; Velaya: "BABE.mrm"
            var animationName = "S_DANCE1"; // Snapper: "R_CLEAN"; Velaya: "S_DANCE1"


            var debugObj = new GameObject("DebugAnimationObject");
            SceneManager.GetSceneByName("SampleScene").GetRootGameObjects().Append(debugObj);

            var mds = PxModelScript.GetModelScriptFromVdf(PhoenixBridge.VdfsPtr, mdsName);
            var mdl = PxModel.LoadModelFromVdf(PhoenixBridge.VdfsPtr, mdlName);
            var mdh = PxModelHierarchy.LoadFromVdf(PhoenixBridge.VdfsPtr, mdhName);
            var mdm = PxModelMesh.LoadModelMeshFromVdf(PhoenixBridge.VdfsPtr, mdmName);
            var mrm = PxMultiResolutionMesh.GetMRMFromVdf(PhoenixBridge.VdfsPtr, mrmName);
            var root = SingletonBehaviour<MeshCreator>.GetOrCreate().Create("DebugAnimationObject1", mdm, mdh, default, default, debugObj);



            PxAnimationData[] animations = new PxAnimationData[mds.animations.Length];
            for (int i = 0; i < animations.Length; i++)
            {
                var animName = mdsName.Replace(".MDS", $"-{mds.animations[i].name}.MAN", System.StringComparison.OrdinalIgnoreCase);
                animations[i] = PxAnimation.LoadFromVdf(PhoenixBridge.VdfsPtr, animName);
            }
            var debugAnimationNames = animations.Select(i => i.name).ToArray();
            var anim = animations.First(i => i.name == animationName);



            ///
            /// Animations
            ///

            var animator = root.transform.GetChild(0).gameObject.AddComponent<Animator>();
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
            clipPlayable.SetSpeed(0.5);

            GraphVisualizerClient.Show(playableGraph);

            playableGraph.Play();





            debugObj.transform.localPosition = new(-10f, 10f, 0);
        }
    }
}
