using System.Collections.Generic;
using GVR.Globals;
using GVR.Manager;
using GVR.Properties;
using TMPro;
using UnityEngine;

namespace GVR.Caches
{
    /// <summary>
    /// Contains lookup caches for GameObjects for faster use.
    /// </summary>
    public static class LookupCache
    {
        /// <summary>
        /// [symbolIndex] = Properties-Component
        /// </summary>
        public static readonly Dictionary<int, NpcProperties> NpcCache = new();

        /// <summary>
        /// Already created AnimationData (Clips + RootMotions) can be reused.
        /// </summary>
        public static readonly Dictionary<string, AnimationClip> AnimationClipCache = new();
        
        /// <summary>
        /// This dictionary caches the sprite assets for fonts.
        /// </summary>
        public static Dictionary<string, TMP_SpriteAsset> fontCache = new();
        
        /// <summary>
        /// VobSounds and VobSoundsDayTime GOs.
        /// </summary>
        public static List<GameObject> vobSoundsAndDayTime = new();
        
        
        static LookupCache()
        {
            GvrEvents.GeneralSceneUnloaded.AddListener(delegate
            {
                vobSoundsAndDayTime.Clear();
            });
        }
    }
}
