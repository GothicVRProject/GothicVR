using GVR.Creator;
using GVR.Data.ZkEvents;
using GVR.Globals;
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

        private void Start()
        {
            properties.currentAction = new None(new(AnimationAction.Type.AINone), gameObject);
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
                        properties.currentLoopState = NpcProperties.LoopState.Loop;
                        vm.Call(properties.stateLoop, properties.npcInstance);
                        break;
                    case NpcProperties.LoopState.Loop:
                        vm.Call(properties.stateLoop, properties.npcInstance);
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

            // We always need to set "self" before executing any Daedalus function.
            vm.GlobalSelf = properties.npcInstance;
            vm.Call(action);
        }

        /// <summary>
        /// Clear ZS functions. If stopCurrentState=true, then stop current animation and don't execute with ZS_*_End()
        /// </summary>
        public void ClearState(bool stopCurrentStateImmediately)
        {
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
            
            // Whenever we change routine, we reset some data to "start" from scratch as if the NPC got spawned.
            properties.AnimationQueue.Clear();
            properties.currentAction = new None(new(AnimationAction.Type.AINone), gameObject);
            properties.stateTime = 0.0f;
        }

        private void PlayNextAnimation(AbstractAnimationAction action)
        {
            properties.currentAction = action;
            action.Start();
        }

        public void AnimationCallback(string pxEventTagDataParam)
        {
            var eventData = JsonUtility.FromJson<SerializableEventTag>(pxEventTagDataParam);
            properties.currentAction.AnimationEventCallback(eventData);
        }

        public void AnimationSfxCallback(string pxEventSfxDataParam)
        {
            var eventData = JsonUtility.FromJson<SerializableEventSoundEffect>(pxEventSfxDataParam);
            properties.currentAction.AnimationSfxEventCallback(eventData);
        }
        
        /// <summary>
        /// As all Components on a GameObject get called, we need to feed this information into current AnimationAction instance.
        /// </summary>
        public void AnimationEndCallback()
        {
            properties.currentAction.AnimationEndEventCallback();
        }
    }
}
