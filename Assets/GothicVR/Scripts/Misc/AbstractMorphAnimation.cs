using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using GVR.Vm;
using JetBrains.Annotations;
using UnityEngine;
using ZenKit;
using Mesh = UnityEngine.Mesh;

namespace GVR.Misc
{
    // Placed inside namespace Misc, as it's used by Vob and NPCs simultaneously.
    public abstract class AbstractMorphAnimation : MonoBehaviour
    {
        // Time and looping information
        private bool _isAnimationRunning;
        private float _time;
        private float _animationDuration;
        private bool _isLooping;

        // Mesh information
        private string _morphMeshName;
        private Mesh _mesh;
        private string _morphName;

        // Cached morph data
        protected IMorphMesh MorphMetadata;
        private IMorphAnimation _morphAnimationMetadata;
        private List<Vector3[]> _morphFrameData;

        protected virtual void Start()
        {
            // As we don't set component inside Prefab, we need to assign mesh later at runtime.
            if (_mesh == null)
                _mesh = GetComponent<MeshFilter>().mesh;
        }
        
        protected void StartAnimation(string morphMeshName, [CanBeNull] string animationName)
        {
            MorphMetadata = AssetCache.TryGetMmb(morphMeshName);
            _morphAnimationMetadata = animationName == null
                ? MorphMetadata.Animations.First()
                : MorphMetadata.Animations.First(anim => anim.Name.EqualsIgnoreCase(animationName));
            _morphFrameData = MorphMeshCache.TryGetMorphData(morphMeshName, _morphAnimationMetadata.Name);

            // Reset
            if (_isAnimationRunning)
                StopAnimation(_morphAnimationMetadata.Name);

            var animationFlags = _morphAnimationMetadata.Flags.ToMorphAnimationFlags();

            // Not yet implemented warning.
            if (animationFlags.HasFlag(VmGothicEnums.MorphAnimationFlags.DISTRIBUTE_FRAMES_RANDOMLY)
                || animationFlags.HasFlag(VmGothicEnums.MorphAnimationFlags.SHIFT_VERTEX_POSITION)
                || animationFlags.HasFlag(VmGothicEnums.MorphAnimationFlags.REFERENCE_ANIMATION)
               )
            {
                Debug.LogWarning($"MorphMesh animation with flags {animationFlags} not yet supported!");
            }

            if (animationFlags.HasFlag(VmGothicEnums.MorphAnimationFlags.LOOP_INFINITELY))
                _isLooping = true;
            else
                _animationDuration = (float)_morphAnimationMetadata.Duration.TotalMilliseconds / 1000; // /1k to normalize to seconds.

            _morphMeshName = morphMeshName;
            _isAnimationRunning = true;
        }

        public void StopAnimation(string animationName)
        {
            _isAnimationRunning = false;

            _mesh.vertices = MorphMeshCache.GetOriginalUnityVertices(_morphMeshName);

            _time = 0.0f;
            _animationDuration = 0.0f;
            _isLooping = false;
            _morphMeshName = null;
            MorphMetadata = null;
            _morphAnimationMetadata = null;
            _morphFrameData = null;
        }
        
        private void Update()
        {
            if (!_isAnimationRunning)
            {
                return;
            }
            
            _time += Time.deltaTime;

            if (!_isLooping && _time > _animationDuration)
            {
                StopAnimation(_morphAnimationMetadata.Name);
                return;
            }

            CalculateMorphWeights();

            // IMorphAnimation.Speed is in milliseconds. We therefore multiply current time by 1000.
            var newFrame = (_time * 1000 * _morphAnimationMetadata.Speed % _morphAnimationMetadata.FrameCount);
            
            var currentMorph = _morphFrameData[(int)newFrame];
            var nextMorph =
                _morphFrameData[(int)newFrame == _morphAnimationMetadata.FrameCount - 1 ? 0 : (int)newFrame + 1];
            
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

        private void CalculateMorphWeights()
        {
            // FIXME - Not yet implemented. But will provide smoother animations for e.g. viseme. Keep in mind there are blendIn and blendOut parameters.
        }
    }
}
