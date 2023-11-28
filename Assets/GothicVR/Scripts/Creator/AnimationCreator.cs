using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using GVR.Npc.Actions;
using GVR.Npc.Data;
using PxCs.Data.Animation;
using PxCs.Data.Model;
using UnityEngine;

namespace GVR.Creator
{
    public static class AnimationCreator
    {
        public static AnimationData PlayAnimation(string mdsName, string animationName, PxModelHierarchyData mdh, GameObject go, bool repeat = false)
        {
            var mdsAnimationKeyName = GetCombinedAnimationKey(mdsName, animationName);
            var animationComp = go.GetComponent<Animation>();
            
            var mds = AssetCache.TryGetMds(mdsName);
            var pxAnimation = AssetCache.TryGetAnimation(mdsName, animationName);

            // Try to load from cache
            if (!LookupCache.AnimationCache.TryGetValue(mdsAnimationKeyName, out var animationData))
            {
                animationData = LoadAnimationClip(pxAnimation, mdh, go, repeat, mdsAnimationKeyName);
                LookupCache.AnimationCache[mdsAnimationKeyName] = animationData;
            }

            if (animationComp[mdsAnimationKeyName] == null)
            {
                AddClipEvents(animationData.clip, mds, pxAnimation, animationName);
                AddClipEndEvent(animationData.clip);
                animationComp.AddClip(animationData.clip, mdsAnimationKeyName);
            }

            animationComp.Play(mdsAnimationKeyName);

            return animationData;
        }

        private static AnimationData LoadAnimationClip(PxAnimationData pxAnimation, PxModelHierarchyData mdh, GameObject rootBone, bool repeat, string clipName)
        {
            var clip = new AnimationClip
            {
                legacy = true,
                name = clipName,
                wrapMode = repeat ? WrapMode.Loop : WrapMode.Once
            };

            var animationData = new AnimationData()
            {
                clip = clip
            };
            
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

                // NPCs animation starts higher than on the ground. Adjust it for all bones to come.
                if (boneName.EqualsIgnoreCase("BIP01"))
                    boneList[1].AddKey(time, 0.0f);
                else
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
                
                if (path.EqualsIgnoreCase("BIP01!!!"))
                {
                    // We reorganize RootMotion entries to get more performance during execution each frame.
                    // Takes slightly more time to create during first load, but will benefit within Update().
                    for (var i = 0; i < entry.Value[0].length; i++)
                    {
                        animationData.rootMotions.Add(new()
                        {
                            time = entry.Value[0].keys[i].time,
                            position = new Vector3(
                                    entry.Value[0].keys[i].value,
                                    entry.Value[1].keys[i].value,
                                    entry.Value[2].keys[i].value),
                            rotation = new Quaternion(
                                entry.Value[3].keys[i].value,
                                entry.Value[4].keys[i].value,
                                entry.Value[5].keys[i].value,
                                entry.Value[6].keys[i].value
                                )
                        });
                    }
                }
                else
                {
                    clip.SetCurve(path, typeof(Transform), "m_LocalPosition.x", entry.Value[0]);
                    clip.SetCurve(path, typeof(Transform), "m_LocalPosition.y", entry.Value[1]);
                    clip.SetCurve(path, typeof(Transform), "m_LocalPosition.z", entry.Value[2]);
                    clip.SetCurve(path, typeof(Transform), "m_LocalRotation.w", entry.Value[3]);
                    clip.SetCurve(path, typeof(Transform), "m_LocalRotation.x", entry.Value[4]);
                    clip.SetCurve(path, typeof(Transform), "m_LocalRotation.y", entry.Value[5]);
                    clip.SetCurve(path, typeof(Transform), "m_LocalRotation.z", entry.Value[6]);
                }
            }

            // Add some final settings
            clip.EnsureQuaternionContinuity();
            clip.frameRate = pxAnimation.fps;

            return animationData;
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

        private static void AddClipEvents(AnimationClip clip, PxModelScriptData mds, PxAnimationData pxAnimation, string animationName)
        {
            var anim = mds.animations.First(i => i.name.EqualsIgnoreCase(animationName));

            foreach (var pxEvent in anim.events)
            {
                var clampedFrame = ClampFrame(pxEvent.frame, anim.firstFrame, (int)pxAnimation.frameCount, anim.lastFrame);
                
                AnimationEvent animEvent = new()
                {
                    time = clampedFrame / clip.frameRate,
                    functionName = nameof(IAnimationCallbacks.AnimationCallback),
                    stringParameter = JsonUtility.ToJson(pxEvent) // As we can't add a custom object, we serialize data.
                 };
                
                clip.AddEvent(animEvent);
            }

            foreach (var sfxEvent in anim.sfx)
            {
                var clampedFrame = ClampFrame(sfxEvent.frame, anim.firstFrame, (int)pxAnimation.frameCount, anim.lastFrame);
                AnimationEvent animEvent = new()
                {
                    time = clampedFrame / clip.frameRate,
                    functionName = nameof(IAnimationCallbacks.AnimationSfxCallback),
                    stringParameter = JsonUtility.ToJson(sfxEvent) // As we can't add a custom object, we serialize data.
                };
                
                clip.AddEvent(animEvent);
            }

            foreach (var sfxEvent in anim.sfx)
            {
                Debug.LogWarning($"SFX events not yet implemented: {sfxEvent.name}");
            }
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
