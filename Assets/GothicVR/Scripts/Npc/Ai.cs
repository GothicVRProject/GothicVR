using System;
using System.Collections.Generic;
using GVR.Caches;
using GVR.Npc.Actions;
using GVR.Npc.Actions.AnimationActions;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Properties;
using PxCs.Data.Event;
using PxCs.Interface;
using UnityEngine;

namespace GVR.Npc
{
    public class Ai : MonoBehaviour, IAnimationCallbacks
    {
        public readonly Queue<AbstractAnimationAction> AnimationQueue = new();
        public VmGothicEnums.WalkMode walkMode;
        
        // HINT: These information aren't set within Daedalus. We need to define them manually.
        // HINT: i.e. every animation might have a BS. E.g. when AI_TakeItem() is called, we set BS.BS_TAKEITEM
        public VmGothicEnums.BodyState bodyState;
        
        private uint prevStateStart;
        private uint stateStart;
        private uint stateLoop;
        private uint stateEnd;

        // State time is activated within AI_StartState()
        // e.g. used to handle random wait loops for idle eating animations (eat a cheese only every n-m seconds)
        private bool isStateTimeActive;
        private float stateTime;
        
        private State currentState = State.None;
        private AbstractAnimationAction currentAction;

        public bool hasItemEquipped;
        public uint currentItem;
        public string usedItemSlot;
        public int itemAnimationState = -1; // We need to start with an "invalid" value as >0< is an allowed state value like in >t_Potion_Stand_2_S0<

        
        private enum State
        {
            None,
            Start,
            Loop,
            End
        }
        
        private void Start()
        {
            currentAction = new None(new(Action.Type.AINone), gameObject);
        }
        
        private void Update()
        {
            currentAction.Tick(transform);
          
            // Add new milliseconds when stateTime shall be measured.
            if (isStateTimeActive)
                stateTime += Time.deltaTime;

            if (!currentAction.IsFinished())
                return;
            
            // Queue is empty. Check if we want to start Looping
            if (AnimationQueue.Count == 0)
            {
                switch (currentState)
                {
                    case State.Start:
                        if (stateLoop == 0)
                            return;
                        currentState = State.Loop;
                        PxVm.CallFunction(GameData.I.VmGothicPtr, stateLoop, GetComponent<NpcProperties>().npcPtr);
                        break;
                    case State.Loop:
                        PxVm.CallFunction(GameData.I.VmGothicPtr, stateLoop, GetComponent<NpcProperties>().npcPtr);
                        break;
                }
            }
            // Go on
            else
            {
                Debug.Log($"Start playing {AnimationQueue.Peek().GetType()}");
                PlayNextAnimation(AnimationQueue.Dequeue());
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
            PxVm.CallFunction(GameData.I.VmGothicPtr, action, GetComponent<NpcProperties>().npcPtr);
        }

        /// <summary>
        /// Clear ZS functions. If stopCurrentState=true, then stop current animation and don't execute with ZS_*_End()
        /// </summary>
        private void ClearState(bool stopCurrentState)
        {
            AnimationQueue.Clear();

            if (stopCurrentState)
            {
                currentState = State.None;
                // FIXME - Also stop current animation immediately!
            }
            else
            {
                currentState = State.End;
                
                if (stateEnd != 0)
                    PxVm.CallFunction(GameData.I.VmGothicPtr, stateEnd, GetComponent<NpcProperties>().npcPtr);
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            currentAction?.OnCollisionEnter(collision);
        }

        public static void ExtAiWait(IntPtr npcPtr, float seconds)
        {
            var self = GetAi(npcPtr);
            self.AnimationQueue.Enqueue(new Wait(
                new(Action.Type.AIWait, f0: seconds),
                self.gameObject));
        }

        public static void ExtAiUseMob(IntPtr npcPtr, string target, int state)
        {
            var self = GetAi(npcPtr);
            self.AnimationQueue.Enqueue(new UseMob(
                new(Action.Type.AIUseMob, str0: target, i0: state),
                self.gameObject));
        }
        
        public static void ExtAiStandUp(IntPtr npcPtr)
        {
            // FIXME - from docu:
            // * Ist der Nsc in einem Animatinsstate, wird die passende Rücktransition abgespielt.
            // * Benutzt der NSC gerade ein MOBSI, poppt er ins stehen.
            var self = GetAi(npcPtr);
            self.AnimationQueue.Enqueue(new StandUp(
                new(Action.Type.AIStandUp),
                self.gameObject));
        }
        
        public static void ExtAiSetWalkMode(IntPtr npcPtr, VmGothicEnums.WalkMode walkMode)
        {
            GetAi(npcPtr).walkMode = walkMode;
        }

        public static void ExtAiGotoWP(IntPtr npcPtr, string point)
        {
            var self = GetAi(npcPtr);
            self.AnimationQueue.Enqueue(new GoToWp(
                new(Action.Type.AIGoToWP, str0: point),
                self.gameObject));
        }

        public static void ExtAiAlignToWP(IntPtr npcPtr)
        {
            var self = GetAi(npcPtr);
            self.AnimationQueue.Enqueue(new AlignToWp(
                new(Action.Type.AIAlignToWp),
                self.gameObject));
        }
        
        public static void ExtAiPlayAni(IntPtr npcPtr, string name)
        {
            var self = GetAi(npcPtr);
            self.AnimationQueue.Enqueue(new PlayAni(
                new(Action.Type.AIPlayAni, str0: name),
                self.gameObject));
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
            var self = GetAi(npcPtr);
            self.AnimationQueue.Enqueue(new UseItemToState(
                new(Action.Type.AIUseItemToState, ui0: itemId, i0: animationState),
                self.gameObject));
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
        
        private void PlayNextAnimation(AbstractAnimationAction action)
        {
            currentAction = action;
            action.Start();
        }

        public void AnimationCallback(string pxEventTagDataParam)
        {
            var eventData = JsonUtility.FromJson<PxEventTagData>(pxEventTagDataParam);
            currentAction.AnimationEventCallback(eventData);
        }

        public void AnimationSfxCallback(string pxEventSfxDataParam)
        {
            var eventData = JsonUtility.FromJson<PxEventSfxData>(pxEventSfxDataParam);
            currentAction.AnimationSfxEventCallback(eventData);
        }
        
        /// <summary>
        /// As all Components on a GameObject get called, we need to feed this information into current AnimationAction instance.
        /// </summary>
        public void AnimationEndCallback()
        {
            currentAction.AnimationEventEndCallback();
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
                    AIPlayAni,
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