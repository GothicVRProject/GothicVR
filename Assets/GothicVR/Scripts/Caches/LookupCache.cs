using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Mesh;
using PxCs.Data.Model;
using PxCs.Interface;
using System;
using System.Collections.Generic;
using System.IO;
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
    }
}