using GVR.Creator;
using GVR.Data.ZkEvents;
using GVR.Extensions;
using GVR.Globals;
using GVR.Manager;
using GVR.Npc.Actions;
using GVR.Npc.Actions.AnimationActions;
using GVR.Properties;
using UnityEngine;
using ZenKit;

namespace GVR.Npc
{
    public class AiHandler : BasePlayerBehaviour, IAnimationCallbacks
    {
        private static DaedalusVm vm => GameData.GothicVm;
        private const int DaedalusLoopContinue = 0; // Id taken from a Daedalus constant.

        private void Start()
        {
            properties.currentAction = new None(new(), gameObject);
        }

        /// <summary>
        /// Basically:
        /// 1. Send Update (Tick) into current Animation to handle
        /// 2. If finished, then check, if we need to handle the new state. _Start() --> _Loop()
        ///
        /// Hint: The isStateTimeActive is only for AI_StartState() from Daedalus which calls sub-routine within routine.
        /// </summary>
        private void Update()
        {
            properties.currentAction.Tick(transform);
          
            // Add new milliseconds when stateTime shall be measured.
            if (properties.isStateTimeActive)
                properties.stateTime += Time.deltaTime;

            // If we're not yet done, we won't handle further tasks (like dequeuing another Action)
            if (!properties.currentAction.IsFinished())
                return;
            
            // Queue is empty. Check if we want to start Looping
            if (properties.AnimationQueue.Count == 0)
            {
                // We always need to set "self" before executing any Daedalus function.
                vm.GlobalSelf = properties.npcInstance;

                switch (properties.currentLoopState)
                {
                    case NpcProperties.LoopState.Start:
                        if (properties.stateLoop == 0)
                            return;
                        vm.Call(properties.stateStart);

                        properties.currentLoopState = NpcProperties.LoopState.Loop;
                        break;
                    case NpcProperties.LoopState.Loop:
                        // Check return type as not every loop returns int.
                        if (vm.GetSymbolByIndex(properties.stateLoop)!.ReturnType == DaedalusDataType.Int)
                        {
                            var loopResponse = vm.Call<int>(properties.stateLoop);
                            // Some ZS_*_Loop return !=0 when they want to quit.
                            if (loopResponse != DaedalusLoopContinue)
                                properties.currentLoopState = NpcProperties.LoopState.End;
                        }
                        else
                        {
                            vm.Call(properties.stateLoop);
                        }
                        break;
                }
            }
            // Go on
            else
            {
                Debug.Log($"Start playing >{properties.AnimationQueue.Peek().GetType()}< on >{properties.go.name}<");
                PlayNextAnimation(properties.AnimationQueue.Dequeue());
            }
        }

        public void StartRoutine(int action, string wayPointName)
        {
            // End original loop first
            if (properties.currentLoopState == NpcProperties.LoopState.Loop)
            {
                // We reuse this function as it is doing what we need.
                ClearState(false);
            }

            // We need to set WayPoint within Daedalus instance as it calls _self.wp_ during routine loops.
            properties.npcInstance.Wp = wayPointName;
            properties.stateStart = action;

            var routineSymbol = vm.GetSymbolByIndex(action);
            
            var symbolLoop = vm.GetSymbolByName($"{routineSymbol.Name}_Loop");
            if (symbolLoop != null)
                properties.stateLoop = symbolLoop.Index;
            
            var symbolEnd = vm.GetSymbolByName($"{routineSymbol.Name}_End");
            if (symbolEnd != null)
                properties.stateEnd = symbolEnd.Index;
            
            properties.currentLoopState = NpcProperties.LoopState.Start;

            // We need to properly start state time as e.g. ZS_Cook won't call AI_StartState() or Npc_SetStateTime()
            // But it's required as it checks immediately how long the Cauldron is already been whirled.
            properties.isStateTimeActive = true;
            properties.stateTime = 0;
        }

        /// <summary>
        /// Clear ZS functions. If stopCurrentState=true, then stop current animation and don't execute with ZS_*_End()
        /// </summary>
        public void ClearState(bool stopCurrentStateImmediately)
        {
            // Whenever we change routine, we reset some data to "start" from scratch as if the NPC got spawned.
            properties.AnimationQueue.Clear();
            properties.currentAction = new None(new(), gameObject);
            properties.stateTime = 0.0f;

            if (stopCurrentStateImmediately)
            {
                properties.currentLoopState = NpcProperties.LoopState.None;
                AnimationCreator.StopAnimation(properties.go);
            }
            else
            {
                properties.currentLoopState = NpcProperties.LoopState.End;

                if (properties.stateEnd != 0)
                {
                    // We always need to set "self" before executing any Daedalus function.
                    vm.GlobalSelf = properties.npcInstance;
                    vm.Call(properties.stateEnd);
                }
            }
        }

        private void PlayNextAnimation(AbstractAnimationAction action)
        {
            properties.currentAction = action;
            action.Start();
        }

        public void AnimationCallback(string eventTagDataParam)
        {
            var eventData = JsonUtility.FromJson<SerializableEventTag>(eventTagDataParam);
            properties.currentAction.AnimationEventCallback(eventData);
        }

        public void AnimationSfxCallback(string eventSfxDataParam)
        {
            var eventData = JsonUtility.FromJson<SerializableEventSoundEffect>(eventSfxDataParam);
            properties.currentAction.AnimationSfxEventCallback(eventData);
        }

        public void AnimationMorphCallback(string eventMorphDataParam)
        {
            var eventData = JsonUtility.FromJson<SerializableEventMorphAnimation>(eventMorphDataParam);
            properties.currentAction.AnimationMorphEventCallback(eventData);
        }
        
        /// <summary>
        /// As all Components on a GameObject get called, we need to feed this information into current AnimationAction instance.
        /// </summary>
        public void AnimationEndCallback(string eventEndSignalParam)
        {
            var eventData = JsonUtility.FromJson<SerializableEventEndSignal>(eventEndSignalParam);

            // FIXME ! We need to re-add physics when e.g. looping walk animation!
            properties.currentAction.AnimationEndEventCallback(eventData);
        }
    }
}
