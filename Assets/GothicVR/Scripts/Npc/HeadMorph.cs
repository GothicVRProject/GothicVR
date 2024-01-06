using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Creator.Meshes;
using GVR.Extensions;
using UnityEditor.VersionControl;
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

        public void StopAnimation()
        {
            isAnimationRunning = false;

            time = 0.0f;
            previousFrame = -1;
            morphMetadata = null;
            morphAnimationMetadata = null;
            morphFrameData = null;
        }

        private float time;
        private int previousFrame = -1;
        private void Update()
        {
            if (!isAnimationRunning)
                return;

            time += Time.deltaTime;

            var newFrame = (int)(time * (morphAnimationMetadata.FrameCount / morphAnimationMetadata.Speed) % morphAnimationMetadata.FrameCount);
            
            if (newFrame == previousFrame)
                return;
            
            mesh.vertices = morphFrameData[newFrame];
            
            previousFrame = newFrame;
            
            Debug.Log($"AnimateMorph Frame {newFrame}");
            //
            // frame += Time.deltaTime;
            //
            // // Do not show a frame twice.
            // if (lastFrame == (int)frame)
            //     return;
            //
            // // It's a hack to show only one new frame each second.
            // lastFrame = (int)frame;
            //
            // var frameIndexOffset = (int)frame * anim.Vertices.Count;
            //
            // foreach (var vertexId in anim.Vertices)
            // {
            //     var vertexElementsFromMapping = vertexMapping[vertexId];
            //     var vertexValue = anim.Samples[vertexId + frameIndexOffset];
            //
            //     foreach (var vertexMappingId in vertexElementsFromMapping)
            //     {
            //         // Test: Just check if Morph mesh is working after all - Big head mode
            //         // vertices[vertexMappingId] = vertices[vertexMappingId] * 1.1f;
            //
            //         // Test: Do the animations "additive" - Open mouth mode
            //         vertices[vertexMappingId] += vertexValue.ToUnityVector();
            //
            //         // Test: Set the head vertex to new MorphMesh value - Tiny head mode
            //         // vertices[vertexMappingId] = vertexValue.ToUnityVector();
            //     }
            // }
            //
            // mesh.vertices = vertices;
            //
            // // This Vignette has only 16 frames.
            // if (frame > 15)
            // {
            //     Debug.Log("Last frame reached.");
            //     frame = 0;
            // }
        }
    }
}
