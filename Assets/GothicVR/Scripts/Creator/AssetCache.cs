using GVR.Util;
using PxCs.Data.Animation;
using PxCs.Data.Model;
using System.Collections.Generic;

namespace GVR.Creator
{
    public class AssetCache : SingletonBehaviour<AssetCache>
    {
        public Dictionary<string, string> materialCache;
        public Dictionary<string, string> textureCache;

        public Dictionary<string, PxModelMeshData> mdmCache;
        public Dictionary<string, PxAnimationData> animationCache;
    }
}
