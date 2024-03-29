using System;
using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using GVR.Misc;

namespace GVR.Npc
{
    public class HeadMorph : AbstractMorphAnimation
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
        
        public void StartAnimation(string headName, HeadMorphType type)
        {
            var animationName = type switch
            {
                HeadMorphType.Viseme => "VISEME",
                _ => throw new Exception($"AnimationType >{type}< not yet handled for head morphing.")
            };
            morphMetadata = AssetCache.TryGetMmb(headName);
            morphAnimationMetadata = morphMetadata.Animations.First(anim => anim.Name.EqualsIgnoreCase(animationName));
            morphFrameData = MorphMeshCache.TryGetMorphData(headName, animationName);
            
            isAnimationRunning = true;
        }
    }
}
