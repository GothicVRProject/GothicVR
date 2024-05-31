using System;
using System.Linq;
using GVR.Caches;
using GVR.Debugging;
using GVR.Extensions;
using GVR.Globals;
using GVR.GothicVR.Scripts.Manager;
using GVR.Npc;
using GVR.Npc.Actions;
using GVR.Npc.Actions.AnimationActions;
using GVR.Npc.Routines;
using GVR.Properties;
using GVR.Vm;
using JetBrains.Annotations;
using UnityEngine;
using ZenKit.Daedalus;

namespace GVR.Manager
{
    public static class NpcHelper
    {
        private const float fpLookupDistance = 7f; // meter

        static NpcHelper()
        {
            GvrEvents.GeneralSceneLoaded.AddListener(GeneralSceneLoaded);
        }

        private static void GeneralSceneLoaded()
        {
            CacheHero();
        }

        public static void CacheHero()
        {
            var heroIndex = GameData.GothicVm.GlobalHero!.Index;
            var playerGo = GameObject.FindWithTag(Constants.PlayerTag);
            var playerProperties = playerGo.GetComponent<NpcProperties>();

            // Set data for NPC.
            playerProperties.npcInstance = (NpcInstance)GameData.GothicVm.GlobalHero;

            // Cache hero for future lookups.
            LookupCache.NpcCache[heroIndex] = playerProperties;
        }

        public static bool ExtIsMobAvailable(NpcInstance npcInstance, string vobName)
        {
            var npc = GetNpc(npcInstance);
            var vob = VobHelper.GetFreeInteractableWithin10M(npc.transform.position, vobName);

            return (vob != null);
        }
        
        public static bool ExtWldIsFPAvailable(NpcInstance npc, string fpNamePart)
        {
            var props = GetProperties(npc);
            var npcGo = props.go;
            var freePoints = WayNetHelper.FindFreePointsWithName(npcGo.transform.position, fpNamePart, fpLookupDistance);

            foreach (var fp in freePoints)
            {
                // Kind of: If we're already standing on a FreePoint, then there is one available.
                if (props.CurrentFreePoint == fp)
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
            else if (fp == props.CurrentFreePoint)
                return false;
            else if (fp.IsLocked)
                return false;
            else
                return true;
        }

        public static int ExtWldGetMobState(NpcInstance npcInstance, string scheme)
        {
            var npcGo = GetNpc(npcInstance);

            var props = GetProperties(npcInstance);

            VobProperties vob;

            if (props.currentInteractable != null)
                vob = props.currentInteractable.GetComponent<VobProperties>();
            else
                vob = VobHelper.GetFreeInteractableWithin10M(npcGo.transform.position, scheme);

            if (vob == null || vob.visualScheme != scheme)
                return -1;

            if (vob is InteractiveProperties interactiveVob)
            {
                return Math.Max(0, interactiveVob.Properties.State);
            }

            return -1;
        }

        public static ItemInstance ExtGetEquippedArmor(NpcInstance npc)
        {
            var armor = GetProperties(npc).EquippedItems
                .FirstOrDefault(i => i.MainFlag == (int)VmGothicEnums.ItemFlags.ITEM_KAT_ARMOR);

            return armor;
        }
        
        public static bool ExtNpcHasEquippedArmor(NpcInstance npc)
        {
            return ExtGetEquippedArmor(npc) != null;
        }

        public static ItemInstance ExtNpcGetEquippedMeleeWeapon(NpcInstance npc)
        {
            var meleeWeapon = GetProperties(npc).EquippedItems
                .FirstOrDefault(i => i.MainFlag == (int)VmGothicEnums.ItemFlags.ITEM_KAT_NF);

            return meleeWeapon;
        }

        public static bool ExtNpcHasEquippedMeleeWeapon(NpcInstance npc)
        {
            return ExtNpcGetEquippedMeleeWeapon(npc) != null;
        }

        public static ItemInstance ExtNpcGetEquippedRangedWeapon(NpcInstance npc)
        {
            var rangedWeapon = GetProperties(npc).EquippedItems
                .FirstOrDefault(i => i.MainFlag == (int)VmGothicEnums.ItemFlags.ITEM_KAT_FF);

            return rangedWeapon;
        }

        public static bool ExtNpcHasEquippedRangedWeapon(NpcInstance npc)
        {
            return ExtNpcGetEquippedRangedWeapon(npc) != null;
        }

        public static bool ExtIsNpcOnFp(NpcInstance npc, string vobNamePart)
        {
            var freePoint = GetProperties(npc).CurrentFreePoint;

            if (freePoint == null)
                return false;

            return freePoint.Name.ContainsIgnoreCase(vobNamePart);
        }

        /// <summary>
        /// As WldDetectNpc and WldDetectNpc seem to be the same logic except one parameter,
        /// we implement both in this function.
        /// </summary>
        public static bool ExtWldDetectNpcEx(NpcInstance npcInstance, int specificNpcIndex, int aiState, int guild,
            bool detectPlayer)
        {
            var npc = GetProperties(npcInstance);
            var npcPos = npc.transform.position;
            
            // FIXME - Add Guild check
            // FIXME - add range check based on perceiveAll's range (npc.sense_range)
            var foundNpc = LookupCache.NpcCache.Values
                .Where(i => i.go != null) // ignore empty (safe check)
                .Where(i => i.npcInstance.Index != npcInstance.Index) // ignore self
                .Where(i => detectPlayer || i.npcInstance.Index != GameData.GothicVm.GlobalHero!.Index) // if we don't detect player, then skip it
                .Where(i => specificNpcIndex < 0 || specificNpcIndex == i.npcInstance.Index) // Specific NPC is found right now?
                .Where(i => aiState < 0 || npc.state == i.state)
                .OrderBy(i => Vector3.Distance(i.transform.position, npcPos)) // get nearest
                .FirstOrDefault();

            // We need to set it, as there are calls where we immediately need _other_. e.g.:
            // if (Wld_DetectNpc(self, ...) && (Npc_GetDistToNpc(self, other)<HAI_DIST_SMALLTALK)
            if (foundNpc != null)
            {
                GameData.GothicVm.GlobalOther = foundNpc.npcInstance;
            }

            return foundNpc != null;
        }

        public static int ExtNpcHasItems(NpcInstance npc, uint itemId)
        {
            if (GetProperties(npc).Items.TryGetValue(itemId, out var amount))
                return amount;
            else
                return 0;
        }
        
        public static int ExtNpcGetDistToWp(NpcInstance npc, string waypointName)
        {
            var npcGo = GetNpc(npc);
            var npcPos = npcGo.transform.position;

            var waypoint = WayNetHelper.GetWayNetPoint(waypointName);

            if (waypoint == null || !npcGo)
                return int.MaxValue;

            return (int)Vector3.Distance(npcPos, waypoint.Position);
        }

        public static bool ExtNpcCanSeeNpc(NpcInstance npc, NpcInstance other)
        {
            var npcGo = GetNpc(npc);
            var otherGo = GetNpc(other);

            if (npcGo == null || otherGo == null)
                return false;

            var headBone = npcGo.FindChildRecursively("BIP01 HEAD").transform;

            var inSightRange = Vector3.Distance(npcGo.transform.position, otherGo.transform.position) <=
                               npc.SensesRange;

            Vector3 directionToTarget = (otherGo.transform.position - headBone.position).normalized;
            float angleToTarget = Vector3.Angle(headBone.forward, directionToTarget);

            var inFov = angleToTarget <= 50.0f; // OpenGothic assumes 100 fov for NPCs

            var inLineOfSight = Physics.Linecast(headBone.position, directionToTarget);

            return inSightRange && inFov && inLineOfSight;
        }

        public static void ExtNpcClearAiQueue(NpcInstance npc)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Clear();
        }

        public static void ExtNpcClearInventory(NpcInstance npc)
        {
            var props = GetProperties(npc);
            props.Items.Clear();
        }

        public static string ExtNpcGetNextWp(NpcInstance npc)
        {
            var pos = GetProperties(npc).transform.position;

            return WayNetHelper.FindNearestWayPoint(pos, true).Name;
        }

        public static int ExtNpcGetTalentSkill(NpcInstance npc, int skillId)
        {
            var props = GetProperties(npc);

            // FIXME - this is related to overlays for the npc's
            return 0;
        }

        public static int ExtNpcGetTalentValue(NpcInstance npc, int skillId)
        {
            return GetProperties(npc).Talents[(VmGothicEnums.Talent)skillId];
        }

        [CanBeNull]
        private static GameObject GetNpc([CanBeNull] NpcInstance npc)
        {
            return GetProperties(npc)?.go;
        }

        private static NpcProperties GetProperties([CanBeNull] NpcInstance npc)
        {
            return npc == null ? null : LookupCache.NpcCache[npc.Index];
        }
        
        public static void ExtAiWait(NpcInstance npc, float seconds)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new Wait(
                new(float0: seconds),
                props.go));
        }

        public static void ExtAiUseMob(NpcInstance npc, string target, int state)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new UseMob(
                new(string0: target, int0: state),
                props.go));
        }
        
        public static void ExtAiStandUp(NpcInstance npc)
        {
            // FIXME - Implement remaining tasks from G1 documentation:
            // * Ist der Nsc in einem Animatinsstate, wird die passende Rücktransition abgespielt.
            // * Benutzt der NSC gerade ein MOBSI, poppt er ins stehen.
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new StandUp(new(), props.go));
        }
        
        public static void ExtAiSetWalkMode(NpcInstance npc, VmGothicEnums.WalkMode walkMode)
        {
            GetProperties(npc).walkMode = walkMode;
        }

        public static void ExtAiGoToFp(NpcInstance npc, string freePointName)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new GoToFp(
                new(string0: freePointName),
                props.go));
        }
        
        public static void ExtAiGoToNextFp(NpcInstance npc, string fpNamePart)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new GoToNextFp(
                new(string0: fpNamePart),
                props.go));
        }
        
        public static void ExtAiGoToWp(NpcInstance npc, string wayPointName)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new GoToWp(
                new(string0: wayPointName),
                props.go));
        }

        public static void ExtAiGoToNpc(NpcInstance self, NpcInstance other)
        {
            if (other == null)
                return;
            
            var props = GetProperties(self);
            props.AnimationQueue.Enqueue(new GoToNpc(
                new(int0: other.Id, int1: other.Index),
                props.go));
        }
        
        public static void ExtAiAlignToFp(NpcInstance npc)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new AlignToFp(new(), props.go));
        }

        public static void ExtAiAlignToWp(NpcInstance npc)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new AlignToWp(new(), props.go));
        }
        
        public static void ExtAiPlayAni(NpcInstance npc, string name)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new PlayAni(new(string0: name), props.go));
        }

        public static void ExtAiStartState(NpcInstance npc, int action, bool stopCurrentState, string wayPointName)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new StartState(
                new(int0: action, bool0: stopCurrentState, string0: wayPointName),
                props.go));
        }

        public static void ExtAiLookAt(NpcInstance npc, string wayPointName)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new LookAt(new(string0: wayPointName), props.go));
        }

        public static void ExtAiLookAtNpc(NpcInstance npc, NpcInstance other)
        {
            if (other == null)
                return;

            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new LookAtNpc(
                new(int0: other.Id, int1: other.Index),
                props.go));
        }

        public static void ExtAiContinueRoutine(NpcInstance npc)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new ContinueRoutine(new(), props.go));
        }

        public static void ExtAiTurnToNpc(NpcInstance npc, NpcInstance other)
        {
            if (other == null)
                return;

            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new TurnToNpc(
                new(int0: other.Id, int1: other.Index),
                props.go));
        }

        public static void ExtAiPlayAniBS(NpcInstance npc, string name, int bodyState)
        {
            var props = GetProperties(npc);
            props.AnimationQueue.Enqueue(new PlayAniBS(new(string0: name, int0: bodyState), props.go));
        }
        
        public static void ExtAiUnequipArmor(NpcInstance npc)
        {
            var props = GetProperties(npc);
            props.BodyData.Armor = 0;
        }

        /// <summary>
        /// Daedalus needs an int value.
        /// </summary>
        public static int ExtNpcGetStateTime(NpcInstance npc)
        {
            var props = GetProperties(npc);

            // If there is no active running state, we immediately assume the current routine is running since the start of all beings.
            if (!props.isStateTimeActive)
                return int.MaxValue;
            else
                return (int)props.stateTime;
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
                new(int0: itemId, int1: animationState),
                props.go));
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
            if (npc1 == null || npc2 == null)
                return int.MaxValue;

            var npc1Pos = LookupCache.NpcCache[npc1.Index].go.transform.position;

            Vector3 npc2Pos;
            // If hero
            if (npc2.Id == 0)
                npc2Pos = Camera.main!.transform.position;
            else
                npc2Pos = LookupCache.NpcCache[npc2.Index].go.transform.position;

            return (int)(Vector3.Distance(npc1Pos, npc2Pos) * 100);
        }

        public static void ExtAiDrawWeapon(NpcInstance npc)
        {
            var props = GetProperties(npc);

            props.AnimationQueue.Enqueue(new DrawWeapon(new(), props.go));
        }

        public static void ExtNpcExchangeRoutine(NpcInstance npcInstance, string routineName)
        {
            var formattedRoutineName = $"Rtn_{routineName}_{npcInstance.Id}";
            var newRoutine = GameData.GothicVm.GetSymbolByName(formattedRoutineName);

            if (newRoutine == null)
            {
                Debug.LogError($"Routine {formattedRoutineName} couldn't be found.");
                return;
            }

            var npcGo = LookupCache.NpcCache[npcInstance.Index];
            ExchangeRoutine(npcGo.go, npcInstance, newRoutine.Index);
        }

        public static void ExchangeRoutine(GameObject go, NpcInstance npcInstance, int routineIndex)
        {
            // e.g. Monsters have no routine and therefore no further routine handling needed.
            if (routineIndex == 0)
                return;

            var routineComp = go.GetComponent<Routine>();
            routineComp.Routines.Clear();
            
            // We always need to set "self" before executing any Daedalus function.
            GameData.GothicVm.GlobalSelf = npcInstance;
            GameData.GothicVm.Call(routineIndex);
            
            if (!FeatureFlags.I.enableNpcRoutines)
                return;

            routineComp.CalculateCurrentRoutine();

            var startRoutine = routineComp.CurrentRoutine;
            go.GetComponent<AiHandler>().StartRoutine(startRoutine.action, startRoutine.waypoint);
        }

        public static void LoadHero()
        {
            var hero = GameData.GothicVm.InitInstance<NpcInstance>("hero");
            GameData.GothicVm.GlobalHero = hero;
        }

        public static GameObject GetHeroGameObject()
        {
            var heroIndex = GameData.GothicVm.GlobalHero!.Index;

            return LookupCache.NpcCache[heroIndex].go;
        }
    }
}
