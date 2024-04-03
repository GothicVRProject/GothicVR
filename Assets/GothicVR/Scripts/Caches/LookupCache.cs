using System.Collections.Generic;
using GVR.Globals;
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
        /// Hint: Includes NPCs and Hero (Easier for lookups like "what is nearest enemy in range".)
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
                NpcCache.Clear();
                vobSoundsAndDayTime.Clear();
            });
        }

        public static void Dispose()
        {
            NpcCache.Clear();
            AnimationClipCache.Clear();
            fontCache.Clear();
            vobSoundsAndDayTime.Clear();
        }
    }
}
