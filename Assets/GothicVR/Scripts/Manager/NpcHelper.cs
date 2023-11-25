using System;
using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using GVR.GothicVR.Scripts.Manager;
using GVR.Npc;
using GVR.Npc.Actions;
using GVR.Npc.Actions.AnimationActions;
using GVR.Phoenix.Interface.Vm;
using GVR.Properties;
using PxCs.Interface;
using UnityEngine;

namespace GVR.Manager
{
    public static class NpcHelper
    {
        private const float fpLookupDistance = 20f; // meter

        public static bool ExtIsMobAvailable(IntPtr npcPtr, string vobName)
        {
            var npc = GetNpc(npcPtr);
            var vob = VobHelper.GetFreeInteractableWithin10M(npc.transform.position, vobName);

            return (vob != null);
        }
        
        public static bool ExtWldIsFPAvailable(IntPtr npcPtr, string fpNamePart)
        {
            var props = GetProperties(npcPtr);
            var npcGo = props.gameObject;
            var freePoints = WayNetHelper.FindFreePointsWithName(npcGo.transform.position, fpNamePart, fpLookupDistance);

            foreach (var fp in freePoints)
            {
                if (props.CurrentFreePoint == fp)
                    return true;
                if (!fp.IsLocked)
                    return true;
            }

            return false;
        }

        public static string ExtGetNearestWayPoint(IntPtr npcPtr)
        {
            var pos = GetProperties(npcPtr).transform.position;

            return WayNetHelper.FindNearestWayPoint(pos).Name;
        }

        public static bool ExtIsNextFpAvailable(IntPtr npcPtr, string fpNamePart)
        {
            var props = GetProperties(npcPtr);
            var pos = props.transform.position;
            var fp = WayNetHelper.FindNearestFreePoint(pos, fpNamePart);

            if (fp == null)
                return false;
            // Ignore if we're already on this FP.
            if (fp == props.CurrentFreePoint)
                return false;
            else if (fp.IsLocked)
                return false;
            else
                return true;
        }

        public static IntPtr ExtGetEquippedArmor(IntPtr npcPtr)
        {
            var armor = GetProperties(npcPtr).EquippedItems
                .FirstOrDefault(i => i.mainFlag == PxVm.PxVmItemFlags.ITEM_KAT_ARMOR);

            return armor?.instancePtr ?? IntPtr.Zero;
        }
        
        public static bool ExtIsNpcOnFp(IntPtr npcPtr, string vobNamePrefix)
        {
            var freePoint = GetProperties(npcPtr).CurrentFreePoint;

            if (freePoint == null)
                return false;

            return freePoint.Name.StartsWithIgnoreCase(vobNamePrefix);
        }

        public static bool ExtWldDetectNpcEx(IntPtr npcPtr, int npcInstance, int aiState, int guild, bool ignorePlayer)
        {
            var npc = GetNpc(npcPtr);
            var npcPos = npc.transform.position;
            
            // FIXME - currently hard coded with 20m, but needs to be imported from Phoenix: daedalus_classes.h::c_npc::senses and senses_range
            float distance = 20f; // 20m
            
            // FIXME - Add Guild check
            // FIXME - Add Hero check
            // FIXME - Add AiState check
            // FIXME - Add NpcCinstance check (only look for specific NPC)
            
            var foundNpc = LookupCache.NpcCache.Values
                .Where(i => Vector3.Distance(i.gameObject.transform.position, npcPos) <= distance)
                .Where(i => i.gameObject != npc)
                .OrderBy(i => Vector3.Distance(i.gameObject.transform.position, npcPos))
                .FirstOrDefault();

            return (foundNpc != null);
        }

        public static int ExtNpcHasItems(IntPtr npcPtr, uint itemId)
        {
            if (GetProperties(npcPtr).Items.TryGetValue(itemId, out var amount))
                return amount;
            else
                return 0;
        }
        
        
        private static GameObject GetNpc(IntPtr npcPtr)
        {
            return GetProperties(npcPtr).gameObject;
        }
        
        private static NpcProperties GetProperties(IntPtr npcPtr)
        {
            var symbolIndex = PxVm.pxVmInstanceGetSymbolIndex(npcPtr);
            var props = LookupCache.NpcCache[symbolIndex];

            // Workaround: When calling PxVm.InitializeNpc(), phoenix will start executing all of the INSTANCEs methods.
            // But some of them like Hlp_GetNpc() need the IntPtr before it's being returned by InitializeNpc().
            // But Phoenix gives us the Pointer via other External calls. We then set it asap.
            if (props.npcPtr == IntPtr.Zero)
                props.npcPtr = npcPtr;

            return props;
        }
        
                public static void ExtAiWait(IntPtr npcPtr, float seconds)
        {
            var props = GetProperties(npcPtr);
            props.AnimationQueue.Enqueue(new Wait(
                new(AnimationAction.Type.AIWait, f0: seconds),
                props.gameObject));
        }

        public static void ExtAiUseMob(IntPtr npcPtr, string target, int state)
        {
            var props = GetProperties(npcPtr);
            props.AnimationQueue.Enqueue(new UseMob(
                new(AnimationAction.Type.AIUseMob, str0: target, i0: state),
                props.gameObject));
        }
        
        public static void ExtAiStandUp(IntPtr npcPtr)
        {
            // FIXME - from docu:
            // * Ist der Nsc in einem Animatinsstate, wird die passende Rücktransition abgespielt.
            // * Benutzt der NSC gerade ein MOBSI, poppt er ins stehen.
            var props = GetProperties(npcPtr);
            props.AnimationQueue.Enqueue(new StandUp(
                new(AnimationAction.Type.AIStandUp),
                props.gameObject));
        }
        
        public static void ExtAiSetWalkMode(IntPtr npcPtr, VmGothicEnums.WalkMode walkMode)
        {
            GetProperties(npcPtr).walkMode = walkMode;
        }

        public static void ExtAiGotoWP(IntPtr npcPtr, string point)
        {
            var props = GetProperties(npcPtr);
            props.AnimationQueue.Enqueue(new GoToWp(
                new(AnimationAction.Type.AIGoToWP, str0: point),
                props.gameObject));
        }

        public static void ExtAiGoToNextFp(IntPtr npcPtr, string fpNamePart)
        {
            var props = GetProperties(npcPtr);
            props.AnimationQueue.Enqueue(new GoToNextFp(
                new(AnimationAction.Type.AIGoToNextFp, str0: fpNamePart),
                props.gameObject));
        }

        public static void ExtAiAlignToWP(IntPtr npcPtr)
        {
            var props = GetProperties(npcPtr);
            props.AnimationQueue.Enqueue(new AlignToWp(
                new(AnimationAction.Type.AIAlignToWp),
                props.gameObject));
        }
        
        public static void ExtAiPlayAni(IntPtr npcPtr, string name)
        {
            var props = GetProperties(npcPtr);
            props.AnimationQueue.Enqueue(new PlayAni(
                new(AnimationAction.Type.AIPlayAni, str0: name),
                props.gameObject));
        }

        public static void ExtAiStartState(IntPtr npcPtr, uint action, bool stopCurrentState, string wayPointName)
        {
            var props = GetProperties(npcPtr);
            var ai = props.GetComponent<AiHandler>();

            ai.ClearState(stopCurrentState);
            
            if (wayPointName != "")
                Debug.LogError("FIXME - Waypoint unused so far.");
            
            props.isStateTimeActive = true;
            props.stateTime = 0;
            
            ai.StartRoutine(action);
        }

        public static float ExtNpcGetStateTime(IntPtr npcPtr)
        {
            return GetProperties(npcPtr).stateTime;
        }

        public static void ExtNpcSetStateTime(IntPtr npcPtr, int seconds)
        {
            GetProperties(npcPtr).stateTime = seconds;
        }
        
        /// <summary>
        /// State means the final state where the animation shall go to.
        /// example:
        /// * itemId=xyz (ItFoBeer)
        /// * animationState = 0
        /// * ItFoBeer is of visual_scheme = Potion
        /// * expected state is t_Potion_Stand_2_S0 --> s_Potion_S0
        /// </summary>
        public static void ExtAiUseItemToState(IntPtr npcPtr, uint itemId, int animationState)
        {
            var props = GetProperties(npcPtr);
            props.AnimationQueue.Enqueue(new UseItemToState(
                new(AnimationAction.Type.AIUseItemToState, ui0: itemId, i0: animationState),
                props.gameObject));
        }

        public static bool ExtNpcWasInState(IntPtr npcPtr, uint action)
        {
            var props = GetProperties(npcPtr);
            return props.prevStateStart == action;
        }

        public static VmGothicEnums.BodyState ExtGetBodyState(IntPtr npcPtr)
        {
            return GetProperties(npcPtr).bodyState;
        }
        
        private static AiHandler GetAi(IntPtr npcPtr)
        {
            var symbolIndex = PxVm.pxVmInstanceGetSymbolIndex(npcPtr);
            var props = LookupCache.NpcCache[symbolIndex];

            return props.GetComponent<AiHandler>();
        }
    }
}