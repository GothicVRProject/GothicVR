using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using UnityEngine;
using ZenKit;
using Mesh = UnityEngine.Mesh;

namespace GVR.Npc
{
    public class HeadMorph : MonoBehaviour
    {
        public enum HeadMorphType
        {
            Neutral,
            Friendly,
            Angry,
            Hostile,
            Firghtened,
            Eyesclosed,
            Eyesblink,
            Eat,
            Hurt,
            Viseme
        }

        public Mesh mesh;

        private IMorphMesh morphMetadata;
        private IMorphAnimation morphAnimationMetadata;
        private List<Vector3[]> morphFrameData;

        private bool isAnimationRunning;

        private void Start()
        {
            // As we don't set HeadMorph.component inside Prefab, we need to assign mesh later at runtime.
            if (mesh == null)
                mesh = GetComponent<MeshFilter>().mesh;
        }

        public void StartAnimation(string headName, HeadMorphType type)
        {
            var animationName = type switch
            {
                HeadMorph.HeadMorphType.Viseme => "VISEME",
                _ => throw new Exception($"AnimationType >{type}< not yet handled for head morphing.")
            };
            morphMetadata = AssetCache.TryGetMmb(headName);
            morphAnimationMetadata = morphMetadata.Animations.First(anim => anim.Name.EqualsIgnoreCase(animationName));
            morphFrameData = MorphMeshCache.TryGetHeadMorphData(headName, animationName);
            
            isAnimationRunning = true;
        }

        public void StopAnimation(string headName)
        {
            isAnimationRunning = false;

            mesh.vertices = MorphMeshCache.GetOriginalUnityVertices(headName);

            time = 0.0f;
            morphMetadata = null;
            morphAnimationMetadata = null;
            morphFrameData = null;
        }

        private float time;
        private void Update()
        {
            if (!isAnimationRunning)
                return;

            time += Time.deltaTime;

            var tickPerFrame = 1f / morphAnimationMetadata.Speed;
            
            // IMorphAnimation.Speed is in milliseconds. We therefore multiply current time by 1000.
            var newFrame = (time * 1000 * morphAnimationMetadata.Speed % morphAnimationMetadata.FrameCount);
            
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
            
            mesh.vertices = interpolatedMorph;
        }
    }
}
