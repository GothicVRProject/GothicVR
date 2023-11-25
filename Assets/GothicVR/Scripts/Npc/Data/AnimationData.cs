using System.Collections.Generic;
using UnityEngine;

namespace GVR.Npc.Data
{
    public class AnimationData
    {
        public AnimationClip clip;
        public List<RootMotionData> rootMotions = new();
        
        
        public struct RootMotionData
        {
            public float time;
            public Vector3 position;
            public Quaternion rotation;
        }
    }
}
