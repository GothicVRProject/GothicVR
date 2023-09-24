using System;
using System.Collections.Generic;
using GVR.Caches;
using GVR.Creator;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using PxCs.Interface;
using UnityEngine;

namespace GVR.Npc
{
    public class Ai : MonoBehaviour, IAnimationCallbackEnd
    {
        public readonly Queue<Action> Queue = new();
        private VmGothicEnums.WalkMode walkMode;
        
        // HINT: These information aren't set within Daedalus. We need to define them manually.
        // HINT: i.e. every animation might have a BS. E.g. when AI_TakeItem() is called, we set BS.BS_TAKEITEM
        private VmGothicEnums.BodyState bodyState;
        
        private bool isPlayingAnimation;

        private uint prevStateStart;
        private uint stateStart;
        private uint stateLoop;
        private uint stateEnd;
        
        private State currentState = State.None;
        private enum State
        {
            None,
            Start,
            Loop,
            End
        }
        
        private void Update()
        {
            if (isPlayingAnimation)
                return;

            // Queue is empty. Check if we want to start Looping
            if (Queue.Count == 0)
            {
                if (currentState == State.Start && stateLoop != 0)
                {
                    currentState = State.Loop;
                    PxVm.CallFunction(GameData.I.VmGothicPtr, stateLoop, GetComponent<Properties>().npcPtr);
                }
            }
            // Go on
            else
            {
                PlayNextAnimation(Queue.Dequeue());
            }
        }

        public void StartRoutine(uint action)
        {
            stateStart = action;

            var routineSymbol = PxDaedalusScript.GetSymbol(GameData.I.VmGothicPtr, action);
            Debug.Log($"Starting Routine {routineSymbol!.name}");
            
            var symbolLoop = PxDaedalusScript.GetSymbol(GameData.I.VmGothicPtr, $"{routineSymbol.name}_Loop");
            if (symbolLoop != null)
                stateLoop = symbolLoop.id;
            
            var symbolEnd = PxDaedalusScript.GetSymbol(GameData.I.VmGothicPtr, $"{routineSymbol.name}_End");
            if (symbolEnd != null)
                stateEnd = symbolEnd.id;
            
            currentState = State.Start;
            PxVm.CallFunction(GameData.I.VmGothicPtr, action, GetComponent<Properties>().npcPtr);
        }

        /// <summary>
        /// Clear ZS functions. If stopCurrentState=true, then stop current animation and don't execute with ZS_*_End()
        /// </summary>
        private void ClearState(bool stopCurrentState)
        {
            Queue.Clear();

            if (stopCurrentState)
            {
                currentState = State.None;
                // FIXME - Also stop current animation immediately!
            }
            else
            {
                currentState = State.End;
                
                if (stateEnd != 0)
                    PxVm.CallFunction(GameData.I.VmGothicPtr, stateEnd, GetComponent<Properties>().npcPtr);
            }
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

        public static void ExtStartState(IntPtr npcPtr, uint action, bool stopCurrentState, string wayPointName)
        {
            var self = GetAi(npcPtr);

            if (stopCurrentState)
                self.ClearState(stopCurrentState);
            
            if (wayPointName != "")
                Debug.LogError("FIXME - Waypoint unused so far.");
            
            self.StartRoutine(action);
        }

        public static bool ExtNpcWasInState(IntPtr npcPtr, uint action)
        {
            var self = GetAi(npcPtr);

            return self.prevStateStart == action;
        }

        public static VmGothicEnums.BodyState ExtGetBodyState(IntPtr npcPtr)
        {
            return GetAi(npcPtr).bodyState;
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