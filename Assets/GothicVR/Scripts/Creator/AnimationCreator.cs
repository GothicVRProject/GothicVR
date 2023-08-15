using System.Collections.Generic;
using System.IO;
using System.Linq;
using GVR.Caches;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Animation;
using PxCs.Data.Model;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GVR.Creator
{
    public class AnimationCreator : SingletonBehaviour<AnimationCreator>
    {
        public void PlayAnimation(string mdsName, string animationName, PxModelHierarchyData mdh, GameObject go)
        {
            var rootBone = go.transform.GetChild(0).gameObject;
            var animationKeyName = GetPreparedAnimationKey(mdsName, animationName);
            var pxAnimation = AssetCache.I.TryGetAnimation(mdsName, animationName);

            // Try to load from cache
            if (!LookupCache.I.animClipCache.TryGetValue(animationKeyName, out var clip))
            {
                clip = LoadAnimationClip(pxAnimation, mdh, rootBone);
                LookupCache.I.animClipCache[animationKeyName] = clip;
            }
            
            var animator = rootBone.AddComponent<Animator>();
            var playableGraph = PlayableGraph.Create(go.name);

            playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            
            var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, pxAnimation.name, animator);

            playableOutput.SetSourcePlayable(clipPlayable);
            clipPlayable.SetDuration(pxAnimation.frameCount / pxAnimation.fps);
            // clipPlayable.SetSpeed(0.1);

            GraphVisualizerClient.Show(playableGraph);
            
            playableGraph.Play();
        }

        private AnimationClip LoadAnimationClip(PxAnimationData pxAnimation, PxModelHierarchyData mdh, GameObject rootBone)
        {
            var clip = new AnimationClip();
            
            var curves = new Dictionary<string, List<AnimationCurve>>((int)pxAnimation.nodeCount);
            var boneNames = pxAnimation.node_indices!.Select(nodeIndex => mdh.nodes![nodeIndex].name).ToArray();

            // Initialize array
            for (var boneId = 0; boneId < boneNames.Length; boneId++)
            {
                var boneName = boneNames[boneId];
                curves.Add(boneName, new List<AnimationCurve>(7));

                // Initialize 7 dimensions. (3x position, 4x rotation)
                curves[boneName].AddRange(Enumerable.Range(0, 7).Select(i => new AnimationCurve()).ToArray());
            }

            // Add KeyFrames from PxSamples
            for (var i = 0; i < pxAnimation.samples!.Length; i++)
            {
                // We want to know what time it is for the animation.
                // Therefore we need to know fps multiplied with current sample. As there are nodeCount samples before a new time starts,
                // we need to add this to the calculation.
                var time = (1 / pxAnimation.fps) * (int)(i / pxAnimation.nodeCount);
                var sample = pxAnimation.samples[i];
                var boneId = i % pxAnimation.nodeCount;
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

            foreach (var entry in curves)
            {
                var path = GetChildPathRecursively(rootBone.transform, entry.Key, "");

                clip.SetCurve(path, typeof(Transform), "m_LocalPosition.x", entry.Value[0]);
                clip.SetCurve(path, typeof(Transform), "m_LocalPosition.y", entry.Value[1]);
                clip.SetCurve(path, typeof(Transform), "m_LocalPosition.z", entry.Value[2]);
                clip.SetCurve(path, typeof(Transform), "m_LocalRotation.w", entry.Value[3]);
                clip.SetCurve(path, typeof(Transform), "m_LocalRotation.x", entry.Value[4]);
                clip.SetCurve(path, typeof(Transform), "m_LocalRotation.y", entry.Value[5]);
                clip.SetCurve(path, typeof(Transform), "m_LocalRotation.z", entry.Value[6]);
            }

            // Add some final settings
            clip.EnsureQuaternionContinuity();
            clip.wrapMode = WrapMode.Loop;
            
            return clip;
        }
    
        
        // TODO - If we have a performance bottleneck while loading animations, then we could cache these results.
        private string GetChildPathRecursively(Transform parent, string curName, string currentPath)
        {
            var result = parent.Find(curName);

            if (result != null)
            {
                // The child object was found, return the current path
                if (currentPath != "")
                    return currentPath + "/" + curName;
                else
                    return curName;
            }
            else
            {
                // Search recursively in the children of the current object
                foreach (Transform child in parent)
                {
                    var childPath = currentPath + "/" + child.name;
                    var resultPath = GetChildPathRecursively(child, curName, childPath);

                    if (resultPath != null)
                        // The child object was found in a recursive call, return the result path
                        return resultPath.TrimStart('/');
                }

                // The child object was not found
                return null;
            }
        }
        
        private string GetPreparedAnimationKey(string mdsKey, string animKey)
        {
            var preparedMdsKey = GetPreparedKey(mdsKey);
            var preparedAnimKey = GetPreparedKey(animKey);
            
            return preparedMdsKey + "-" + preparedAnimKey;
        }
        
        private string GetPreparedKey(string key)
        {
            var lowerKey = key.ToLower();
            var extension = Path.GetExtension(lowerKey);

            if (extension == string.Empty)
                return lowerKey;
            else
                return lowerKey.Replace(extension, "");
        }
    }
}