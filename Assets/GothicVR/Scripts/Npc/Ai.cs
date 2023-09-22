using System;
using System.Collections.Generic;
using GVR.Caches;
using GVR.Creator;
using GVR.Phoenix.Interface.Vm;
using PxCs.Interface;
using UnityEngine;

namespace GVR.Npc
{
    public class Ai : MonoBehaviour, IAnimationCallbackEnd
    {
        public readonly Queue<Action> Queue = new();
        private VmGothicEnums.WalkMode walkMode;

        private bool isPlayingAnimation;


        private void Update()
        {
            if (Queue.Count == 0 || isPlayingAnimation)
                return;

            PlayNextAnimation(Queue.Dequeue());
        }

        public static void ExtAiStandUp(IntPtr npcPtr)
        {
            // FIXME - from docu:
            // * Ist der Nsc in einem Animatinsstate, wird die passende Rücktransition abgespielt.
            // * Benutzt der NSC gerade ein MOBSI, poppt er ins stehen.
            GetAi(npcPtr).Queue.Enqueue(new(Ai.Action.Type.AIStandUp));
        }
        
        public static void ExtAiSetWalkMode(IntPtr npcPtr, VmGothicEnums.WalkMode walkMode)
        {
            GetAi(npcPtr).walkMode = walkMode;
        }

        public static void ExtAiGotoWP(IntPtr npcPtr, string point)
        {
            // FIXME - e.g. for Thorus there's initially no string value for TA_Boss() self.wp - Intended or a bug on our side?
            GetAi(npcPtr).Queue.Enqueue(new(Action.Type.AIGoToWP, point));
        }

        public static void ExtAiAlignToWP(IntPtr npcPtr)
        {
            GetAi(npcPtr).Queue.Enqueue(new(Action.Type.AIAlignToWp));
        }
        
        public static void ExtAiPlayAni(IntPtr npcPtr, string name)
        {
            GetAi(npcPtr).Queue.Enqueue(new(Action.Type.AIPlayAnim, name));
        }
        
        private static Ai GetAi(IntPtr npcPtr)
        {
            var symbolIndex = PxVm.pxVmInstanceGetSymbolIndex(npcPtr);
            var props = LookupCache.I.NpcCache[symbolIndex];

            return props.GetComponent<Ai>();
        }
        
        private void PlayNextAnimation(Action action)
        {
            var props = GetComponent<Properties>();
            
            switch (action.ActionType)
            {
                case Action.Type.AIPlayAnim:
                    var mdh = AssetCache.I.TryGetMdh(props.overlayMdhName);
                    // FIXME - We need to handle both mds and mdh options! (base vs overlay)
                    AnimationCreator.I.PlayAnimation(props.baseMdsName, action.Data, mdh, gameObject);
                    isPlayingAnimation = true;
                    break;
                default:
                    break;
            }
        }
        
        
        
        public class Action
            {
                public enum Type
                {
                    AINone,
                    AILookAtNpc,
                    AIStopLookAt,
                    AIRemoveWeapon,
                    AITurnToNpc,
                    AIGoToNpc,
                    AIGoToNextFp,
                    AIGoToFP,
                    AIGoToWP,
                    AIStartState,
                    AIPlayAnim,
                    AIPlayAnimBs,
                    AIWait,
                    AIStandUp,
                    AIStandUpQuick,
                    AIEquipArmor,
                    AIEquipBestArmor,
                    AIEquipMelee,
                    AIEquipRange,
                    AIUseMob,
                    AIUseItem,
                    AIUseItemToState,
                    AITeleport,
                    AIDrawWeaponMelee,
                    AIDrawWeaponRange,
                    AIDrawSpell,
                    AIAttack,
                    AIFlee,
                    AIDodge,
                    AIUnEquipWeapons,
                    AIUnEquipArmor,
                    AIOutput,
                    AIOutputSvm,
                    AIOutputSvmOverlay,
                    AIProcessInfo,
                    AIStopProcessInfo,
                    AIContinueRoutine,
                    AIAlignToFp,
                    AIAlignToWp,
                    AISetNpcsToState,
                    AISetWalkMode,
                    AIFinishingMove,
                    AIDrawWeapon,
                    AITakeItem,
                    AIGotoItem,
                    AIPointAtNpc,
                    AIPointAt,
                    AIStopPointAt,
                    AIPrintScreen,
                    AILookAt
                }
        
                public Action(Type actionType, string data = null)
                {
                    this.ActionType = actionType;
                    this.Data = data;
                }
                
                public readonly Type ActionType;
                public readonly string Data;
            }

        public void AnimationEndCallback(string name)
        {
            isPlayingAnimation = false;
        }
    }
}