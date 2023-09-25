using System;
using System.Collections.Generic;
using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes;
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

        // State time is activated within AI_StartState()
        // e.g. used to handle random wait loops for idle eating animations (eat a cheese only every n-m seconds)
        private bool isStateTimeActive;
        private float stateTime;
        
        private State currentState = State.None;
        private Action.Type curAnimationAction = Action.Type.AINone;
        private bool isAiWait;
        private float aiWaitSeconds;

        private enum State
        {
            None,
            Start,
            Loop,
            End
        }
        
        private void Update()
        {
            // Add new milliseconds when stateTime shall be measured.
            if (isStateTimeActive)
                stateTime += Time.deltaTime;

            // If state is AiWait, then we need to check if time is over and "stop" fake animation flag.
            if (isAiWait)
            {
                aiWaitSeconds -= Time.deltaTime;
                if (aiWaitSeconds <= 0f)
                {
                    isAiWait = false;
                    isPlayingAnimation = false;
                }
            }
            
            if (isPlayingAnimation)
                return;

            // Queue is empty. Check if we want to start Looping
            if (Queue.Count == 0)
            {
                switch (currentState)
                {
                    case State.Start:
                        if (stateLoop == 0)
                            return;
                        currentState = State.Loop;
                        PxVm.CallFunction(GameData.I.VmGothicPtr, stateLoop, GetComponent<Properties>().npcPtr);
                        break;
                    case State.Loop:
                        PxVm.CallFunction(GameData.I.VmGothicPtr, stateLoop, GetComponent<Properties>().npcPtr);
                        break;
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

        public static void ExtAiWait(IntPtr npcPtr, float seconds)
        {
            GetAi(npcPtr).Queue.Enqueue(new(Action.Type.AIWait, f0: seconds));
        }
        
        public static void ExtAiStandUp(IntPtr npcPtr)
        {
            // FIXME - from docu:
            // * Ist der Nsc in einem Animatinsstate, wird die passende Rücktransition abgespielt.
            // * Benutzt der NSC gerade ein MOBSI, poppt er ins stehen.
            GetAi(npcPtr).Queue.Enqueue(new(Action.Type.AIStandUp));
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

        public static void ExtAiStartState(IntPtr npcPtr, uint action, bool stopCurrentState, string wayPointName)
        {
            var self = GetAi(npcPtr);

            if (stopCurrentState)
                self.ClearState(stopCurrentState);
            
            if (wayPointName != "")
                Debug.LogError("FIXME - Waypoint unused so far.");
            
            self.isStateTimeActive = true;
            self.stateTime = 0;
            
            self.StartRoutine(action);
        }

        public static float ExtNpcGetStateTime(IntPtr npcPtr)
        {
            return GetAi(npcPtr).stateTime;
        }

        public static void ExtNpcSetStateTime(IntPtr npcPtr, int seconds)
        {
            GetAi(npcPtr).stateTime = seconds;
        }
        
        public static void ExtAiUseItemToState(IntPtr npcPtr, uint itemId, int expectedInventoryCount)
        {
            // FIXME - Hier weitermachen!
            // GetAi(npcPtr).Queue.Enqueue(new(Action.Type.AIUseItemToState, ui0: itemId, i0: expectedInventoryCount));
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
            var npc = props.gameObject;

            curAnimationAction = action.ActionType;
            switch (curAnimationAction)
            {
                case Action.Type.AIPlayAnim:
                    var mdh = AssetCache.I.TryGetMdh(props.overlayMdhName);
                    // FIXME - We need to handle both mds and mdh options! (base vs overlay)
                    AnimationCreator.I.PlayAnimation(props.baseMdsName, action.str0, mdh, gameObject);
                    isPlayingAnimation = true;
                    break;
                case Action.Type.AIUseItemToState:
                    var slotGo = NpcMeshCreator.I.GetSlot(npc, NpcMeshCreator.ItemSlot.RightHand);
                    VobCreator.I.CreateItem(action.ui0, slotGo);
                    break;
                case Action.Type.AIWait:
                    isPlayingAnimation = true;
                    isAiWait = true;
                    aiWaitSeconds = action.f0;
                    break;
                default:
                    Debug.LogError($"ActionType {action.ActionType} not yet handled.");
                    break;
            }
        }
        
        public void AnimationEndCallback(string name)
        {
            isPlayingAnimation = false;
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
        
                public Action(Type actionType, string str0 = null, int i0 = 0, uint ui0 = 0, float f0 = 0f)
                {
                    this.ActionType = actionType;
                    this.str0 = str0;
                    this.i0 = i0;
                    this.ui0 = ui0;
                    this.f0 = f0;
                }
                
                public readonly Type ActionType;
                public readonly string str0;
                public readonly int i0;
                public readonly uint ui0;
                public readonly float f0;
            }
    }
}