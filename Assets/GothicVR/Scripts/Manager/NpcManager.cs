using System;
using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using GVR.GothicVR.Scripts.Manager;
using GVR.Npc;
using GVR.Util;
using PxCs.Interface;
using UnityEngine;

namespace GVR.Manager
{
    public class NpcManager : SingletonBehaviour<NpcManager>
    {
        private const float fpLookupDistance = 20f; // meter

        public bool ExtIsMobAvailable(IntPtr npcPtr, string vobName)
        {
            var npc = GetNpc(npcPtr);
            var vob = VobManager.I.GetFreeInteractableWithin10M(npc.transform.position, vobName);

            return (vob != null);
        }
        
        public bool ExtWldIsFPAvailable(IntPtr npcPtr, string fpNamePart)
        {
            var props = GetProperties(npcPtr);
            var npcGo = props.gameObject;
            var freePoints = WayNetManager.I.FindFreePointsWithName(npcGo.transform.position, fpNamePart, fpLookupDistance);

            foreach (var fp in freePoints)
            {
                if (props.CurrentFreePoint == fp)
                    return true;
                if (!fp.IsLocked)
                    return true;
            }

            return false;
        }

        public bool ExtIsNpcOnFp(IntPtr npcPtr, string vobNamePrefix)
        {
            var freePoint = GetProperties(npcPtr).CurrentFreePoint;

            if (freePoint == null)
                return false;

            return freePoint.Name.StartsWithIgnoreCase(vobNamePrefix);
        }

        public bool ExtWldDetectNpcEx(IntPtr npcPtr, int npcInstance, int aiState, int guild, bool ignorePlayer)
        {
            var npc = GetNpc(npcPtr);
            var npcPos = npc.transform.position;
            
            // FIXME - currently hard coded with 20m, but needs to be imported from Phoenix: daedalus_classes.h::c_npc::senses and senses_range
            float distance = 20f; // 20m
            
            // FIXME - Add Guild check
            // FIXME - Add Hero check
            // FIXME - Add AiState check
            // FIXME - Add NpcCinstance check (only look for specific NPC)
            
            var foundNpc = LookupCache.I.NpcCache.Values
                .Where(i => Vector3.Distance(i.gameObject.transform.position, npcPos) <= distance)
                .Where(i => i.gameObject != npc)
                .OrderBy(i => Vector3.Distance(i.gameObject.transform.position, npcPos))
                .FirstOrDefault();

            return (foundNpc != null);
        }

        public int ExtNpcHasItems(IntPtr npcPtr, uint itemId)
        {
            if (GetProperties(npcPtr).Items.TryGetValue(itemId, out var amount))
                return amount;
            else
                return 0;
        }
        
        
        private GameObject GetNpc(IntPtr npcPtr)
        {
            return GetProperties(npcPtr).gameObject;
        }
        
        private static Properties GetProperties(IntPtr npcPtr)
        {
            var symbolIndex = PxVm.pxVmInstanceGetSymbolIndex(npcPtr);
            var props = LookupCache.I.NpcCache[symbolIndex];

            // Workaround: When calling PxVm.InitializeNpc(), phoenix will start executing all of the INSTANCEs methods.
            // But some of them like Hlp_GetNpc() need the IntPtr before it's being returned by InitializeNpc().
            // But Phoenix gives us the Pointer via other External calls. We then set it asap.
            if (props.npcPtr == IntPtr.Zero)
                props.npcPtr = npcPtr;

            return props;
        }
    }
}