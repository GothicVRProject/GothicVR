using System;
using GVR.Caches;
using GVR.GothicVR.Scripts.Manager;
using GVR.Npc;
using GVR.Util;
using PxCs.Interface;
using UnityEngine;

namespace GVR.Manager
{
    public class NpcManager : SingletonBehaviour<NpcManager>
    {
        public bool ExtIsMobAvailable(IntPtr npcPtr, string vobName)
        {
            var npc = GetNpc(npcPtr);
            var vob = VobManager.I.GetFreeInteractableWithin10m(npc.transform.position, vobName);

            return (vob != null);
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