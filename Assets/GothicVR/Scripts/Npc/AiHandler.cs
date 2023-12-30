using GVR.Globals;
using GVR.Npc.Actions;
using GVR.Npc.Actions.AnimationActions;
using GVR.Properties;
using PxCs.Data.Event;
using UnityEngine;
using ZenKit;

namespace GVR.Npc
{
    public class AiHandler : MonoBehaviour, IAnimationCallbacks
    {
        public NpcProperties properties;

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

            if (!properties.currentAction.IsFinished())
                return;
            
            // Queue is empty. Check if we want to start Looping
            if (properties.AnimationQueue.Count == 0)
            {
                switch (properties.currentLoopState)
                {
                    case NpcProperties.LoopState.Start:
                        if (properties.stateLoop == 0)
                            return;
                        properties.currentLoopState = NpcProperties.LoopState.Loop;
                        GameData.GothicVm.Call((int)properties.stateLoop, properties.npcInstance);
                        break;
                    case NpcProperties.LoopState.Loop:
                        GameData.GothicVm.Call((int)properties.stateLoop, properties.npcInstance);
                        break;
                }
            }
            // Go on
            else
            {
                Debug.Log($"Start playing {properties.AnimationQueue.Peek().GetType()}");
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
                properties.stateLoop = (int)symbolLoop.Index;
            
            var symbolEnd = vm.GetSymbolByName($"{routineSymbol.Name}_End");
            if (symbolEnd != null)
                properties.stateEnd = (int)symbolEnd.Index;
            
            properties.currentLoopState = NpcProperties.LoopState.Start;
            vm.Call(action, properties.npcInstance);
        }

        /// <summary>
        /// Clear ZS functions. If stopCurrentState=true, then stop current animation and don't execute with ZS_*_End()
        /// </summary>
        public void ClearState(bool stopCurrentState)
        {
            properties.AnimationQueue.Clear();

            if (stopCurrentState)
            {
                properties.currentLoopState = NpcProperties.LoopState.None;
                // FIXME - Also stop current animation immediately!
            }
            else
            {
                properties.currentLoopState = NpcProperties.LoopState.End;
                
                if (properties.stateEnd != 0)
                    vm.Call(properties.stateEnd, properties.npcInstance);
            }
        }
        
        private void PlayNextAnimation(AbstractAnimationAction action)
        {
            properties.currentAction = action;
            action.Start();
        }

        public void AnimationCallback(string pxEventTagDataParam)
        {
            var eventData = JsonUtility.FromJson<PxEventTagData>(pxEventTagDataParam);
            properties.currentAction.AnimationEventCallback(eventData);
        }

        public void AnimationSfxCallback(string pxEventSfxDataParam)
        {
            var eventData = JsonUtility.FromJson<PxEventSfxData>(pxEventSfxDataParam);
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
