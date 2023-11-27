using GVR.Extensions;
using GVR.Npc.Actions;
using GVR.Npc.Actions.AnimationActions;
using GVR.Phoenix.Interface;
using GVR.Properties;
using PxCs.Data.Event;
using PxCs.Interface;
using UnityEngine;

namespace GVR.Npc
{
    public class AiHandler : MonoBehaviour, IAnimationCallbacks
    {
        public NpcProperties properties;
        
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
                        PxVm.CallFunction(GameData.VmGothicPtr, properties.stateLoop, properties.npcPtr);
                        break;
                    case NpcProperties.LoopState.Loop:
                        PxVm.CallFunction(GameData.VmGothicPtr, properties.stateLoop, properties.npcPtr);
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

        public void StartRoutine(uint action, string wayPointName)
        {
            // We need to set WayPoint within Daedalus instance as it calls _self.wp_ during routine loops.
            PxVm.pxVmInstanceNpcSetWP(properties.npc.instancePtr, wayPointName);
            properties.npc.wp = wayPointName;

            properties.stateStart = action;

            var routineSymbol = PxDaedalusScript.GetSymbol(GameData.VmGothicPtr, action);
            
            var symbolLoop = PxDaedalusScript.GetSymbol(GameData.VmGothicPtr, $"{routineSymbol.name}_Loop");
            if (symbolLoop != null)
                properties.stateLoop = symbolLoop.id;
            
            var symbolEnd = PxDaedalusScript.GetSymbol(GameData.VmGothicPtr, $"{routineSymbol.name}_End");
            if (symbolEnd != null)
                properties.stateEnd = symbolEnd.id;
            
            properties.currentLoopState = NpcProperties.LoopState.Start;
            PxVm.CallFunction(GameData.VmGothicPtr, action, GetComponent<NpcProperties>().npcPtr);
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
                    PxVm.CallFunction(GameData.VmGothicPtr, properties.stateEnd, GetComponent<NpcProperties>().npcPtr);
            }
        }
        
        private void OnCollisionEnter(Collision coll)
        {
            properties.currentAction?.OnCollisionEnter(coll);
        }

        private void OnTriggerEnter(Collider coll)
        {
            properties.currentAction?.OnTriggerEnter(coll);
        }

        private void OnCollisionExit(Collision coll)
        {
            properties.currentAction?.OnCollisionExit(coll);

            // If NPC walks out of a FreePoint, it gets freed.
            if (!coll.gameObject.name.StartsWithIgnoreCase("FP_"))
                return;

            coll.gameObject.GetComponent<VobSpotProperties>().fp.IsLocked = false;
        }

        /// <summary>
        /// Sometimes a currentAnimation needs this information. Sometimes it's just for a FreePoint to clear up.
        /// </summary>
        private void OnTriggerExit(Collider coll)
        {
            properties.currentAction?.OnTriggerExit(coll);

            // If NPC walks out of a FreePoint, it gets freed.
            if (!coll.gameObject.name.StartsWithIgnoreCase("FP_"))
                return;

            coll.gameObject.GetComponent<VobSpotProperties>().fp.IsLocked = false;
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
