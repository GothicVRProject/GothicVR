using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using ZenKit;
using Mesh = UnityEngine.Mesh;
using Random = UnityEngine.Random;

namespace GVR.Morph
{
    /// <summary>
    ///
    /// </summary>
    public abstract class AbstractMorphAnimation : MonoBehaviour
    {
        /// <summary>
        /// Multiple morphs can run at the same time. e.g. viseme and eyesblink.
        /// We therefore add animations to a list to calculate their positions together.
        /// </summary>
        private readonly List<MorphAnimationData> _runningMorphs = new();
        private Mesh _mesh;

        /// <summary>
        /// RandomMorphs are blinking eyes in G1.
        /// The settings for it contains two options of blinking firstTime and secondTime.
        /// </summary>
        protected List<(string morphMeshName, string animationName, float firstTimeAverage, float firstTimeVariable, float secondTimeAverage, float secondTimeVariable, float probabilityOfFirst, float timer)> randomAnimations = new ();
        protected List<float> randomAnimationTimers = new(); // It's faster to alter a list entry than rewriting the whole Tuple struct above.

        protected virtual void Start()
        {
            // As we don't set component inside Prefab, we need to assign mesh later at runtime.
            if (_mesh == null)
                _mesh = GetComponent<MeshFilter>().mesh;
        }

        protected void StartAnimation(string morphMeshName, [CanBeNull] string animationName)
        {
            var newMorph = new MorphAnimationData();

            newMorph.MeshMetadata = AssetCache.TryGetMmb(morphMeshName);
            newMorph.AnimationMetadata = animationName == null
                ? newMorph.MeshMetadata.Animations.First()
                : newMorph.MeshMetadata.Animations.First(anim => anim.Name.EqualsIgnoreCase(animationName));
            newMorph.AnimationFrameData = MorphMeshCache.TryGetMorphData(morphMeshName, newMorph.AnimationMetadata.Name);

            // Reset if already added and playing
            if (_runningMorphs.Any(i => i.MeshName == newMorph.MeshName))
                StopAnimation(newMorph.AnimationMetadata.Name);

            var animationFlags = newMorph.AnimationMetadata.Flags;

            // Not yet implemented warning.
            if (animationFlags.HasFlag(MorphAnimationFlags.Random)
                || animationFlags.HasFlag(MorphAnimationFlags.Shape)
                || animationFlags.HasFlag(MorphAnimationFlags.ShapeReference)
               )
            {
                Debug.LogWarning($"MorphMesh animation with flags {animationFlags} not yet supported!");
            }

            // TODO/HINT - Is also handled via -1 second duration value of morphAnimation.Duration
            if (animationFlags.HasFlag(MorphAnimationFlags.Loop))
                newMorph.IsLooping = true;

            _runningMorphs.Add(newMorph);
        }

        public void StopAnimation(string animationName)
        {
            var morphToStop = _runningMorphs.FirstOrDefault(i => i.AnimationName.EqualsIgnoreCase(animationName));

            if (morphToStop == null)
            {
                Debug.LogWarning($"MorphAnimation {animationName} not found on {gameObject.name}", gameObject);
                return;
            }

            // Reset to a stable value.
            _mesh.vertices = MorphMeshCache.GetOriginalUnityVertices(morphToStop.MeshName);

            _runningMorphs.Remove(morphToStop);
        }

        private void Update()
        {
            UpdateRunningMorphs();
            CheckIfRandomAnimationShouldBePlayed();
        }

        // FIXME - We currently calculate morphs every frame but could lower it's value with 60, 30, 15 frames in mind (e.g. for each distance culling).
        // FIXME - Means less CPU cycles to calculate morphs.
        private void UpdateRunningMorphs()
        {
            // ToList() -> As we might call .Remove() during looping, we need to clone the list to allow it (otherwise we get a CompilerIssue).
            foreach (var morph in _runningMorphs.ToList())
            {
                morph.Time += Time.deltaTime;

                CalculateMorphWeights();

                // IMorphAnimation.Speed is in milliseconds. We therefore multiply current time by 1000.
                var newFrameFloat = (morph.Time * 1000 * morph.AnimationMetadata.Speed);
                var newFrameInt = (int)newFrameFloat;

                // We can't use animationtime as (e.g.) for R_EYESBLINK we have only one frame which is a time of 0.0f,
                // but instead we need to say frame 0 is 0...0.999 of first frame.
                if (newFrameInt >= morph.AnimationFrameCount)
                {
                    // We just assume we're exactly at point 0.0f when we reached the end. Not 100% perfect but we're in milliseconds area of error.
                    if (morph.IsLooping)
                    {
                        morph.Time = 0;
                        newFrameFloat = 0.0f;
                        newFrameInt = 0;
                    }
                    else
                    {
                        StopAnimation(morph.AnimationName);
                        continue;
                    }
                }

                var currentMorph = morph.AnimationFrameData[newFrameInt];

                // e.g. R_EYESBLINK
                if (morph.AnimationFrameCount == 1)
                {
                    // FIXME - We need blendin/blendout otherwise this will be a on-off only.
                    var calculatedMorph = new Vector3[currentMorph.Length];
                    for (var i = 0; i < currentMorph.Length; i++)
                    {
                        calculatedMorph[i] = currentMorph[i];
                    }
                    _mesh.vertices = calculatedMorph;
                }
                else
                {
                    var nextMorph =
                        morph.AnimationFrameData[newFrameInt == morph.AnimationMetadata.FrameCount - 1 ? 0 : newFrameInt + 1];

                    var interpolatedMorph = new Vector3[currentMorph.Length];
                    for (var i = 0; i < currentMorph.Length; i++)
                    {
                        interpolatedMorph[i] =
                            Vector3.Lerp(currentMorph[i], nextMorph[i], newFrameFloat - MathF.Truncate(newFrameFloat));
                    }
                    _mesh.vertices = interpolatedMorph;
                }
            }
        }

        private void CalculateMorphWeights()
        {
            // FIXME - Not yet implemented. But will provide smoother animations for e.g. viseme. Keep in mind there are blendIn and blendOut parameters.
        }

        private void CheckIfRandomAnimationShouldBePlayed()
        {
            for (var i = 0; i < randomAnimations.Count; i++)
            {
                randomAnimationTimers[i] -= Time.deltaTime;

                if (randomAnimationTimers[i] > 0)
                    continue;

                var anim = randomAnimations[i];

                // FIXME - Set maxWeight for animation based on random value.
                // FIXME - var weight = Random.value * 0.4f + 0.6f
                StartAnimation(anim.morphMeshName, anim.animationName);

                if (Random.value < anim.probabilityOfFirst)
                    randomAnimationTimers[i] = (Random.value * 2 - 1) * anim.firstTimeVariable + anim.firstTimeAverage;
                else
                    randomAnimationTimers[i] = (Random.value * 2 - 1) * anim.secondTimeVariable + anim.secondTimeAverage;
            }
        }
    }
}
