using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using GVR.Vm;
using JetBrains.Annotations;
using UnityEngine;
using Mesh = UnityEngine.Mesh;

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
        protected List<MorphAnimationData> RunningMorphs = new();

        // Time and looping information
        private bool _isAnimationRunning;

        private Mesh _mesh;

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
            if (RunningMorphs.Any(i => i.MeshName == newMorph.MeshName))
                StopAnimation(newMorph.AnimationMetadata.Name);

            var animationFlags = newMorph.AnimationMetadata.Flags.ToMorphAnimationFlags();

            // Not yet implemented warning.
            if (animationFlags.HasFlag(VmGothicEnums.MorphAnimationFlags.DISTRIBUTE_FRAMES_RANDOMLY)
                || animationFlags.HasFlag(VmGothicEnums.MorphAnimationFlags.SHIFT_VERTEX_POSITION)
                || animationFlags.HasFlag(VmGothicEnums.MorphAnimationFlags.REFERENCE_ANIMATION)
               )
            {
                Debug.LogWarning($"MorphMesh animation with flags {animationFlags} not yet supported!");
            }

            if (animationFlags.HasFlag(VmGothicEnums.MorphAnimationFlags.LOOP_INFINITELY))
                newMorph.IsLooping = true;
            else
                newMorph.AnimationDuration = (float)newMorph.AnimationMetadata.Duration.TotalMilliseconds / 1000; // /1k to normalize to seconds.

            RunningMorphs.Add(newMorph);
        }

        public void StopAnimation(string animationName)
        {
            var morphToStop = RunningMorphs.FirstOrDefault(i => i.AnimationName.EqualsIgnoreCase(animationName));

            if (morphToStop == null)
            {
                Debug.LogWarning($"MorphAnimation {animationName} not found on {gameObject.name}", gameObject);
                return;
            }

            // Reset to a stable value.
            _mesh.vertices = MorphMeshCache.GetOriginalUnityVertices(morphToStop.MeshName);

            RunningMorphs.Remove(morphToStop);
        }
        
        private void Update()
        {
            if (RunningMorphs.IsEmpty())
                return;

            foreach (var morph in RunningMorphs)
            {

                morph.Time += Time.deltaTime;

                if (!morph.IsLooping && morph.Time > morph.AnimationDuration)
                {
                    StopAnimation(morph.AnimationMetadata.Name);
                    return;
                }

                CalculateMorphWeights();

                // IMorphAnimation.Speed is in milliseconds. We therefore multiply current time by 1000.
                var newFrame = (morph.Time * 1000 * morph.AnimationMetadata.Speed % morph.AnimationMetadata.FrameCount);

                var currentMorph = morph.AnimationFrameData[(int)newFrame];
                var nextMorph =
                    morph.AnimationFrameData[(int)newFrame == morph.AnimationMetadata.FrameCount - 1 ? 0 : (int)newFrame + 1];

                // FIXME - We currently calculate this morph interpolation every time the morph is played. We could also cache it.
                // FIXME - e.g. cache it with 60, 30, 15 frames in mind (for each distance culling).
                // FIXME - This would mean more memory is needed, but less CPU cycles.
                var interpolatedMorph = new Vector3[currentMorph.Length];
                for (var i = 0; i < currentMorph.Length; i++)
                {
                    interpolatedMorph[i] =
                        Vector3.Lerp(currentMorph[i], nextMorph[i], newFrame - MathF.Truncate(newFrame));
                }

                _mesh.vertices = interpolatedMorph;
            }
        }

        private void CalculateMorphWeights()
        {
            // FIXME - Not yet implemented. But will provide smoother animations for e.g. viseme. Keep in mind there are blendIn and blendOut parameters.
        }
    }
}
