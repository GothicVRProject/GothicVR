using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using GVR.GothicVR.Scripts.Manager;
using GVR.Npc.Actions;
using GVR.Npc.Actions.AnimationActions;
using GVR.Phoenix.Interface.Vm;
using GVR.Properties;
using UnityEngine;
using ZenKit.Daedalus;

namespace GVR.Manager
{
    public static class NpcHelper
    {
        private const float fpLookupDistance = 20f; // meter

        public static bool ExtIsMobAvailable(NpcInstance npcInstance, string vobName)
        {
            var npc = GetNpc(npcInstance);
            var vob = VobHelper.GetFreeInteractableWithin10M(npc.transform.position, vobName);

            return (vob != null);
        }
        
        public static bool ExtWldIsFPAvailable(NpcInstance npc, string fpNamePart)
        {
            var props = GetProperties(npc);
            var npcGo = props.gameObject;
            var freePoints = WayNetHelper.FindFreePointsWithName(npcGo.transform.position, fpNamePart, fpLookupDistance);

            foreach (var fp in freePoints)
            {
                // Kind of: If we're already standing on a FreePoint, then there is one available.
                if (props.currentFreePoint == fp)
                    return true;
                // Alternatively, we found a free one within range.
                if (!fp.IsLocked)
                    return true;
            }

            return false;
        }

        public static string ExtGetNearestWayPoint(NpcInstance npc)
        {
            var pos = GetProperties(npc).transform.position;

            return WayNetHelper.FindNearestWayPoint(pos).Name;
        }

        public static bool ExtIsNextFpAvailable(NpcInstance npc, string fpNamePart)
        {
            var props = GetProperties(npc);
            var pos = props.transform.position;
            var fp = WayNetHelper.FindNearestFreePoint(pos, fpNamePart);

            if (fp == null)
                return false;
            // Ignore if we're already on this FP.
            else if (fp == props.currentFreePoint)
                return false;
            else if (fp.IsLocked)
                return false;
            else
                return true;
        }

        public static ItemInstance ExtGetEquippedArmor(NpcInstance npc)
        {
            var armor = GetProperties(npc).EquippedItems
                .FirstOrDefault(i => i.MainFlag == (int)VmGothicEnums.ItemFlags.ITEM_KAT_ARMOR);

            return armor;
        }
        
        public static bool ExtIsNpcOnFp(NpcInstance npc, string vobNamePart)
        {
            var freePoint = GetProperties(npc).currentFreePoint;

            if (freePoint == null)
                return false;

            return freePoint.Name.ContainsIgnoreCase(vobNamePart);
        }

        public static bool ExtWldDetectNpcEx(NpcInstance npc, int npcInstance, int aiState, int guild, bool ignorePlayer)
        {
            var npcGo = GetNpc(npc);
            var npcPos = npcGo.transform.position;
            
            // FIXME - currently hard coded with 20m, but needs to be imported from Phoenix: daedalus_classes.h::c_npc::senses and senses_range
            float distance = 20f; // 20m
            
            // FIXME - Add Guild check
            // FIXME - Add Hero check
            // FIXME - Add AiState check
            // FIXME - Add NpcCinstance check (only look for specific NPC)
            
            var foundNpc = LookupCache.NpcCache.Values
                .Where(i => Vector3.Distance(i.gameObject.transform.position, npcPos) <= distance)
                .Where(i => i.gameObject != npcGo)
                .OrderBy(i => Vector3.Distance(i.gameObject.transform.position, npcPos))
                .FirstOrDefault();

            return (foundNpc != null);
        }

        public static int ExtNpcHasItems(NpcInstance npc, uint itemId)
        {
            if (GetProperties(npc).Items.TryGetValue(itemId, out var amount))
                return amount;
            else
                return 0;
        }
        

        private static GameObject GetNpc(NpcInstance npc)
        {
            return GetProperties(npc).gameObject;
        }

        private static NpcProperties GetProperties(NpcInstance npc)
        {
            return LookupCache.NpcCache[npc.Index];
        }
        
        public static void ExtAiWait(NpcInstance npc, float seconds)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new Wait(
                new(AnimationAction.Type.AIWait, float0: seconds),
                props.gameObject));
        }

        public static void ExtAiUseMob(NpcInstance npc, string target, int state)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new UseMob(
                new(AnimationAction.Type.AIUseMob, string0: target, int0: state),
                props.gameObject));
        }
        
        public static void ExtAiStandUp(NpcInstance npc)
        {
            // FIXME - Implement remaining tasks from G1 documentation:
            // * Ist der Nsc in einem Animatinsstate, wird die passende Rücktransition abgespielt.
            // * Benutzt der NSC gerade ein MOBSI, poppt er ins stehen.
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new StandUp(
                new(AnimationAction.Type.AIStandUp),
                props.gameObject));
        }
        
        public static void ExtAiSetWalkMode(NpcInstance npc, VmGothicEnums.WalkMode walkMode)
        {
            GetProperties(npc).walkMode = walkMode;
        }

        public static void ExtAiGotoWP(NpcInstance npc, string wayPointName)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new GoToWp(
                new(AnimationAction.Type.AIGoToWP, string0: wayPointName),
                props.gameObject));
        }

        public static void ExtAiGoToNextFp(NpcInstance npc, string fpNamePart)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new GoToNextFp(
                new(AnimationAction.Type.AIGoToNextFp, string0: fpNamePart),
                props.gameObject));
        }

        public static void ExtAiAlignToWP(NpcInstance npc)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new AlignToWp(
                new(AnimationAction.Type.AIAlignToWp),
                props.gameObject));
        }
        
        public static void ExtAiPlayAni(NpcInstance npc, string name)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new PlayAni(
                new(AnimationAction.Type.AIPlayAni, string0: name),
                props.gameObject));
        }

        public static void ExtAiStartState(NpcInstance npc, int action, bool stopCurrentState, string wayPointName)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new StartState(
                new(AnimationAction.Type.AIStartState, int0: action, bool0: stopCurrentState, string0: wayPointName),
                props.gameObject));
        }

        public static float ExtNpcGetStateTime(NpcInstance npc)
        {
            return GetProperties(npc).stateTime;
        }

        public static void ExtNpcSetStateTime(NpcInstance npc, int seconds)
        {
            GetProperties(npc).stateTime = seconds;
        }
        
        /// <summary>
        /// State means the final state where the animation shall go to.
        /// example:
        /// * itemId=xyz (ItFoBeer)
        /// * animationState = 0
        /// * ItFoBeer is of visual_scheme = Potion
        /// * expected state is t_Potion_Stand_2_S0 --> s_Potion_S0
        /// </summary>
        public static void ExtAiUseItemToState(NpcInstance npc, int itemId, int animationState)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new UseItemToState(
                new(AnimationAction.Type.AIUseItemToState, int0: itemId, int1: animationState),
                props.gameObject));
        }

        public static bool ExtNpcWasInState(NpcInstance npc, uint action)
        {
            var props = GetProperties(npc);
            return props.prevStateStart == action;
        }

        public static VmGothicEnums.BodyState ExtGetBodyState(NpcInstance npc)
        {
            return GetProperties(npc).bodyState;
        }
        
        /// <summary>
        /// Return position distance in cm.
        /// </summary>
        public static int ExtNpcGetDistToNpc(NpcInstance npc1, NpcInstance npc2)
        {
            var npc1Pos = LookupCache.NpcCache[npc1.Index].gameObject.transform.position;

            Vector3 npc2Pos;
            // If hero
            if (npc2.Id == 0)
                npc2Pos = Camera.main!.transform.position;
            else
                npc2Pos = LookupCache.NpcCache[npc2.Index].gameObject.transform.position;

            return (int)(Vector3.Distance(npc1Pos, npc2Pos) * 100);
        }

        public static void ExtAiDrawWeapon(NpcInstance npc)
        {
            var props = GetProperties(npc);

            props.AnimationQueue.Enqueue(new DrawWeapon(
                new(AnimationAction.Type.AIDrawWeapon),
                props.gameObject));
        }
    }
}
