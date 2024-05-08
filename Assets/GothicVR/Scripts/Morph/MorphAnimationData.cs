using System.Collections.Generic;
using UnityEngine;
using ZenKit;

namespace GVR.Morph
{
    public class MorphAnimationData
    {
        // Time data
        public float Time;
        public bool IsLooping;

        // Mesh data
        public IMorphMesh MeshMetadata;
        public string MeshName => MeshMetadata.Name;

        // Animation data
        public IMorphAnimation AnimationMetadata;
        public List<Vector3[]> AnimationFrameData;
        public string AnimationName => AnimationMetadata.Name;
        public int AnimationFrameCount => AnimationMetadata.FrameCount;
    }
}
