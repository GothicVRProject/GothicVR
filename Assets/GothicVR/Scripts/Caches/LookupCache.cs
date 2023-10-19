using System;
using System.Collections.Generic;
using GVR.Manager;
using GVR.Properties;
using GVR.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace GVR.Caches
{
    /// <summary>
    /// Contains lookup caches for GameObjects for faster use.
    /// </summary>
    public class LookupCache : SingletonBehaviour<LookupCache>
    {
        /// <summary>
        /// [symbolIndex] = Properties-Component
        /// </summary>
        public readonly Dictionary<uint, NpcProperties> NpcCache = new();

        /// <summary>
        /// Already created AnimationClips can be reused.
        ///
        /// For creation of AnimationClip and it's curves, we need to have GameObject
        /// </summary>
        public Dictionary<string, AnimationClip> AnimClipCache = new();


        /// <summary>
        /// This dictionary caches the sprite assets for fonts.
        /// </summary>
        public Dictionary<string, TMP_SpriteAsset> fontCache = new();

        /// <summary>
        /// VobSounds and VobSoundsDayTime GOs.
        /// </summary>
        public List<GameObject> vobSoundsAndDayTime = new();
        
        
        private void Start()
        {
            GvrSceneManager.I.sceneGeneralUnloaded.AddListener(PreWorldCreate);
        }

        private void PreWorldCreate()
        {
            vobSoundsAndDayTime.Clear();
        }
    }
}
