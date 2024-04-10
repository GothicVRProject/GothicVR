using System;
using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using GVR.Misc;
using UnityEngine;

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

        public HeadMorphType GetTypeByName(string name)
        {
            if (name.ContainsIgnoreCase("EAT"))
                return HeadMorphType.Eat;
            else
                Debug.LogError($"{name} as morphMeshType not yet mapped.");

            // If nothing found, we return the hurt face. Meme potential? ;-)
            return HeadMorphType.Hurt;
        }
        
        public void StartAnimation(string headName, HeadMorphType type)
        {
            var animationName = type switch
            {
                HeadMorphType.Viseme => "VISEME",
                HeadMorphType.Eat => "T_EAT",
                _ => throw new Exception($"AnimationType >{type}< not yet handled for head morphing.")
            };
            morphMetadata = AssetCache.TryGetMmb(headName);
            morphAnimationMetadata = morphMetadata.Animations.First(anim => anim.Name.EqualsIgnoreCase(animationName));
            morphFrameData = MorphMeshCache.TryGetMorphData(headName, animationName);
            
            isAnimationRunning = true;
        }
    }
}
