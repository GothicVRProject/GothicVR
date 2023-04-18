using PxCs;
using PxCs.Data;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UZVR.Phoenix.Data;
using UZVR.Util;

namespace UZVR.Creator
{
    public class VobCreator: SingletonBehaviour<VobCreator>
    {
        // Cache helped speed up loading of G1 world textures from 870ms to 230 (~75% speedup)
        private Dictionary<string, Texture2D> cachedTextures = new();

        public void Create(GameObject root, WorldData world)
        {
            var vobs = world.vobs;

            var itemVobs = GetFlattenedVobsByType(vobs, PxWorld.PxVobType.PxVob_oCItem);


            itemVobs.ForEach(item => Debug.Log(item.vobName));
        }

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
