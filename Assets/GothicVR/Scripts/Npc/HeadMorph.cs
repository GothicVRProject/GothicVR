using System;
using System.Linq;
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
            Frightened,
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
            StartAnimation(headName, GetAnimationNameByType(type));
        }

        /// <summary>
        /// We need to wrap StopAnimation by fetching string name of animation based on HeadMorphType
        /// </summary>
        public void StopAnimation(HeadMorphType type)
        {
            var animationName = GetAnimationNameByType(type);
            var animationData = MorphMetadata.Animations.First(anim => anim.Name.EqualsIgnoreCase(animationName));

            StopAnimation(animationData.Name);
        }

        private string GetAnimationNameByType(HeadMorphType type)
        {
            return type switch
            {
                HeadMorphType.Viseme => "VISEME",
                HeadMorphType.Eat => "T_EAT",
                _ => throw new Exception($"AnimationType >{type}< not yet handled for head morphing.")
            };
        }
    }
}
