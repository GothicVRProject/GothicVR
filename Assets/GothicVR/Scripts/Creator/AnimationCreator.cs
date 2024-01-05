using System.Collections.Generic;
using System.IO;
using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using GVR.Npc.Actions;
using UnityEngine;
using ZenKit;
using Animation = UnityEngine.Animation;

namespace GVR.Creator
{
    public static class AnimationCreator
    {
        public static void PlayAnimation(string mdsName, string animationName, IModelHierarchy mdh, GameObject go, bool repeat = false)
        {
            var mdsAnimationKeyName = GetCombinedAnimationKey(mdsName, animationName);
            var animationComp = go.GetComponent<Animation>();
            
            var mds = AssetCache.TryGetMds(mdsName);
            var modelAnimation = AssetCache.TryGetAnimation(mdsName, animationName);
            var anim = mds.Animations.First(i => i.Name.EqualsIgnoreCase(animationName));

            if (anim.Direction == AnimationDirection.Backward)
            {
                Debug.LogError($"Backwards animations not yet handled. Called for >{animationName}< from >{mdsName}<");
                return;
            }

            // Try to load from cache
            if (!LookupCache.AnimationClipCache.TryGetValue(mdsAnimationKeyName, out var clip))
            {
                clip = LoadAnimationClip(modelAnimation, mdh, go, repeat, mdsAnimationKeyName);
                LookupCache.AnimationClipCache[mdsAnimationKeyName] = clip;
            }

            if (animationComp[mdsAnimationKeyName] == null)
            {
                AddClipEvents(clip, modelAnimation, anim);
                AddClipEndEvent(clip);
                animationComp.AddClip(clip, mdsAnimationKeyName);
            }

            animationComp.Stop();
            animationComp.Play(mdsAnimationKeyName);
        }

        public static void StopAnimation(GameObject go)
        {
            var animationComp = go.GetComponent<Animation>();

            // FIXME - Not perfect as the NPC will freeze where it stopped. We need to find a way to set NPC to a state before the animation.
            animationComp.Stop();
        }

        private static AnimationClip LoadAnimationClip(IModelAnimation pxAnimation, IModelHierarchy mdh, GameObject rootBone, bool repeat, string clipName)
        {
            var clip = new AnimationClip
            {
                legacy = true,
                name = clipName,
                wrapMode = repeat ? WrapMode.Loop : WrapMode.Once
            };

            var curves = new Dictionary<string, List<AnimationCurve>>((int)pxAnimation.NodeCount);
            var boneNames = pxAnimation.NodeIndices.Select(nodeIndex => mdh.Nodes[(int)nodeIndex].Name).ToArray();

            // Initialize array
            for (var boneId = 0; boneId < boneNames.Length; boneId++)
            {
                var boneName = boneNames[boneId];
                curves.Add(boneName, new List<AnimationCurve>(7));

                // Initialize 7 dimensions. (3x position, 4x rotation)
                curves[boneName].AddRange(Enumerable.Range(0, 7).Select(i => new AnimationCurve()).ToArray());
            }

            Vector3 rootBoneStartCorrection = Vector3.zero;

            // Add KeyFrames from PxSamples
            for (var i = 0; i < pxAnimation.Samples.Count; i++)
            {
                // We want to know what time it is for the animation.
                // Therefore we need to know fps multiplied with current sample. As there are nodeCount samples before a new time starts,
                // we need to add this to the calculation.
                var time = (1 / pxAnimation.Fps) * (int)(i / pxAnimation.NodeCount);
                var sample = pxAnimation.Samples[i];
                var boneId = i % pxAnimation.NodeCount;
                var boneName = boneNames[boneId];
                var boneList = curves[boneName];
                var isRootBone = boneName.EqualsIgnoreCase("BIP01");

                // Some animations don't start with BIP01=(0,0,0).
                // Therefore we need to calculate the offset.
                // Otherwise e.g. walking will hick up as NPC will _spawn_ slightly in front of last animation loop.
                if (time == 0.0f && isRootBone)
                    rootBoneStartCorrection = sample.Position.ToUnityVector();

                Vector3 uPosition;
                if (isRootBone)
                    uPosition = sample.Position.ToUnityVector() - rootBoneStartCorrection;
                else
                    uPosition = sample.Position.ToUnityVector();

                // We add 7 properties for location and rotation.
                boneList[0].AddKey(time, uPosition.x);
                boneList[1].AddKey(time, uPosition.y);
                boneList[2].AddKey(time, uPosition.z);
                boneList[3].AddKey(time, -sample.Rotation.W); // It's important to have this value with a -1. Otherwise animation is inversed.
                boneList[4].AddKey(time, sample.Rotation.X);
                boneList[5].AddKey(time, sample.Rotation.Y);
                boneList[6].AddKey(time, sample.Rotation.Z);
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
            clip.frameRate = pxAnimation.Fps;

            return clip;
        }
        
        // TODO - If we have a performance bottleneck while loading animations, then we could cache these results.
        private static string GetChildPathRecursively(Transform parent, string curName, string currentPath)
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

        private static void AddClipEvents(AnimationClip clip, IModelAnimation modelAnimation, IAnimation anim)
        {
            foreach (var zkEvent in anim.EventTags)
            {
                var clampedFrame = ClampFrame(zkEvent.Frame, anim.FirstFrame, modelAnimation.FrameCount, anim.LastFrame);

                AnimationEvent animEvent = new()
                {
                    time = clampedFrame / clip.frameRate,
                    functionName = nameof(IAnimationCallbacks.AnimationCallback),
                    stringParameter = JsonUtility.ToJson(zkEvent) // As we can't add a custom object, we serialize data.
                 };

                clip.AddEvent(animEvent);
            }

            foreach (var sfxEvent in anim.SoundEffects)
            {
                var clampedFrame = ClampFrame(sfxEvent.Frame, anim.FirstFrame, (int)modelAnimation.FrameCount, anim.LastFrame);
                AnimationEvent animEvent = new()
                {
                    time = clampedFrame / clip.frameRate,
                    functionName = nameof(IAnimationCallbacks.AnimationSfxCallback),
                    stringParameter = JsonUtility.ToJson(sfxEvent) // As we can't add a custom object, we serialize data.
                };
                
                clip.AddEvent(animEvent);
            }

            if (anim.ParticleEffects.Any())
                Debug.LogWarning($"SFX events not yet implemented. Tried to use for {anim.Name}");
        }

        /// <summary>
        /// Bugfix: There are events which would happen after the animation is done.
        /// </summary>
        private static float ClampFrame(int expectedFrame, int firstFrame, int frameCount, int lastFrame)
        {
            if (expectedFrame < firstFrame)
                return 0;
            // e.g. beer-in-hand destroy animation would be triggered after animation itself.
            if (expectedFrame >= (firstFrame + frameCount))
                return frameCount - 1;
            else
                return expectedFrame - firstFrame;
        }
        
        /// <summary>
        /// Adds event at the end of animation.
        /// The event is called on every MonoBehaviour on GameObject where Clip is played.
        /// @see: https://docs.unity3d.com/ScriptReference/AnimationEvent.html
        /// @see: https://docs.unity3d.com/ScriptReference/GameObject.SendMessage.html
        /// </summary>
        private static void AddClipEndEvent(AnimationClip clip)
        {
            AnimationEvent finalEvent = new()
            {
                time = clip.length,
                functionName = nameof(IAnimationCallbacks.AnimationEndCallback),
            };
            
            clip.AddEvent(finalEvent);
        }
        
        /// <summary>
        /// .man files are combined of MDSNAME-ANIMATIONNAME.man
        /// </summary>
        private static string GetCombinedAnimationKey(string mdsKey, string animKey)
        {
            var preparedMdsKey = GetPreparedKey(mdsKey);
            var preparedAnimKey = GetPreparedKey(animKey);
            
            return preparedMdsKey + "-" + preparedAnimKey;
        }
        
        /// <summary>
        /// Basically extract file ending and lower names.
        /// </summary>
        private static string GetPreparedKey(string key)
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
