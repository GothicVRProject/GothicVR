using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Mesh;
using PxCs.Data.Model;
using PxCs.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace GVR.Caches
{
    /// <summary>
    /// Contains lookup caches for GameObjects for faster use.
    /// </summary>
    public class LookupCache : SingletonBehaviour<LookupCache>
    {
        /// <summary>
        /// [symbolIndex] = GameObject
        /// </summary>
        public Dictionary<uint, GameObject> npcCache = new();

        /// <summary>
        /// Already created AnimationClips can be reused.
        ///
        /// For creation of AnimationClip and it's curves, we need to have GameObject
        /// </summary>
        public Dictionary<string, AnimationClip> animClipCache = new();


        /// <summary>
        /// This dictionary caches the sprite assets for fonts.
        /// </summary>
        public Dictionary<string, TMP_SpriteAsset> fontCache = new();

    }
}