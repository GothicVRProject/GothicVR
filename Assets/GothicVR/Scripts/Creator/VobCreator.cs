using PxCs;
using PxCs.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GVR.Demo;
using GVR.Phoenix.Data;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Mesh;

namespace GVR.Creator
{
    public class VobCreator : SingletonBehaviour<VobCreator>
    {
        // Cache helped speed up loading of G1 world textures from 870ms to 230 (~75% speedup)
        private Dictionary<string, Texture2D> cachedTextures = new();

        public void Create(GameObject root, WorldData world)
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().CreateVobs)
                return;

            var itemVobs = GetFlattenedVobsByType(world.vobs, PxWorld.PxVobType.PxVob_oCItem);
            var vobRootObj = new GameObject("Vobs");
            vobRootObj.transform.parent = root.transform;

            foreach (var vob in itemVobs)
            {
                // FIXME: Add caching of MRM as object will be created multiple times inside a scene.
                var mrm = PxMultiResolutionMesh.GetMRMFromVdf(PhoenixBridge.VdfsPtr, $"{vob.vobName}.MRM");

                if (mrm == null)
                {
                    Debug.LogError($"MultiResolutionModel (MRM) >{vob.vobName}.MRM< not found.");
                    continue;
                }

                
                SingletonBehaviour<MultiResolutionMeshCreator>.GetOrCreate()
                    .Create(mrm, root, $"vob-{vob.vobName}", vob.position.ToUnityVector(), vob.rotation.Value);
            }

            // Currently we don't need to store cachedTextures once the world is loaded.
            cachedTextures.Clear();
        }

        /// <summary>
        /// Convenient method to return specific vob elements in recursive list of PxVobData.childVobs...
        /// </summary>
        private List<PxVobData> GetFlattenedVobsByType(PxVobData[] vobsToFilter, PxWorld.PxVobType type)
        {
            var returnVobs = new List<PxVobData>();
            for (var i = 0; i < vobsToFilter.Length; i++)
            {
                var curVob = vobsToFilter[i];
                if (curVob.type == type)
                    returnVobs.Add(curVob);

                returnVobs.AddRange(GetFlattenedVobsByType(curVob.childVobs, type));
            }

            return returnVobs;
        }
    }
}
