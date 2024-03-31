using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using UnityEngine;
using ZenKit;
using Mesh = UnityEngine.Mesh;

namespace GVR.Misc
{
    // Placed inside namespace Misc, as it's used by Vob and NPCs simultaneously.
    public abstract class AbstractMorphAnimation : MonoBehaviour
    {
        protected IMorphMesh morphMetadata;
        protected IMorphAnimation morphAnimationMetadata;
        protected List<Vector3[]> morphFrameData;
        protected bool isAnimationRunning;
        
        private float _time;
        private Mesh _mesh;
        
        protected virtual void Start()
        {
            // As we don't set component inside Prefab, we need to assign mesh later at runtime.
            if (_mesh == null)
                _mesh = GetComponent<MeshFilter>().mesh;
        }
        
        public void StartAnimation(string morphMeshName)
        {
            morphMetadata = AssetCache.TryGetMmb(morphMeshName);
            morphAnimationMetadata = morphMetadata.Animations.First();
            morphFrameData = MorphMeshCache.TryGetMorphData(morphMeshName, morphAnimationMetadata.Name);
            
            isAnimationRunning = true;
        }
        
        public void StopAnimation(string morphMeshName)
        {
            isAnimationRunning = false;

            _mesh.vertices = MorphMeshCache.GetOriginalUnityVertices(morphMeshName);

            _time = 0.0f;
            morphMetadata = null;
            morphAnimationMetadata = null;
            morphFrameData = null;
        }
        
        private void Update()
        {
            if (!isAnimationRunning)
            {
                return;
            }
            
            _time += Time.deltaTime;
            
            var tickPerFrame = 1f / morphAnimationMetadata.Speed;
            
            // IMorphAnimation.Speed is in milliseconds. We therefore multiply current time by 1000.
            var newFrame = (_time * 1000 * morphAnimationMetadata.Speed % morphAnimationMetadata.FrameCount);
            
            var currentMorph = morphFrameData[(int)newFrame];
            var nextMorph =
                morphFrameData[(int)newFrame == morphAnimationMetadata.FrameCount - 1 ? 0 : (int)newFrame + 1];
            
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
}
