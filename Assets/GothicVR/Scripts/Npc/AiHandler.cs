﻿using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Creator;
using GVR.Extensions;
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
    public class AiHandler : MonoBehaviour, IAnimationCallbacks
    {
        public NpcProperties properties;
        
        private void Start()
        {
            properties.currentAction = new None(new(AnimationAction.Type.AINone), gameObject);
        }
        
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
                        PxVm.CallFunction(GameData.I.VmGothicPtr, properties.stateLoop, properties.npcPtr);
                        break;
                    case NpcProperties.LoopState.Loop:
                        PxVm.CallFunction(GameData.I.VmGothicPtr, properties.stateLoop, properties.npcPtr);
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

        public void StartRoutine(uint action)
        {
            properties.stateStart = action;

            var routineSymbol = PxDaedalusScript.GetSymbol(GameData.I.VmGothicPtr, action);
            
            var symbolLoop = PxDaedalusScript.GetSymbol(GameData.I.VmGothicPtr, $"{routineSymbol.name}_Loop");
            if (symbolLoop != null)
                properties.stateLoop = symbolLoop.id;
            
            var symbolEnd = PxDaedalusScript.GetSymbol(GameData.I.VmGothicPtr, $"{routineSymbol.name}_End");
            if (symbolEnd != null)
                properties.stateEnd = symbolEnd.id;
            
            properties.currentLoopState = NpcProperties.LoopState.Start;
            PxVm.CallFunction(GameData.I.VmGothicPtr, action, GetComponent<NpcProperties>().npcPtr);
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
                    PxVm.CallFunction(GameData.I.VmGothicPtr, properties.stateEnd, GetComponent<NpcProperties>().npcPtr);
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            properties.currentAction?.OnCollisionEnter(collision);
        }

        /// <summary>
        /// Sometimes a currentAnimation needs this information. Sometimes it's just for a FreePoint to clear up.
        /// </summary>
        private void OnCollisionExit(Collision collision)
        {
            properties.currentAction?.OnCollisionExit(collision);

            // If NPC walks out of a FreePoint, it gets freed.
            collision.contacts
                .Where(i => i.otherCollider.name.StartsWithIgnoreCase("FP_"))
                .Select(i => i.otherCollider.gameObject.GetComponent<VobSpotProperties>())
                .ToList()
                .ForEach(i => i.fp.IsLocked = false);
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
            properties.currentAction.AnimationEventEndCallback();
        }
    }
}