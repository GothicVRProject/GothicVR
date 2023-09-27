using System;
using System.Diagnostics;
using GVR.Caches;
using GVR.Creator;
using GVR.Extensions;
using GVR.Manager;
using PxCs.Data.Animation;
using PxCs.Data.Event;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GVR.Npc.Actions.AnimationActions
{
    public abstract class AbstractAnimationAction
    {
        protected readonly Ai.Action action;
        protected readonly GameObject npcGo;
        protected readonly Properties props;
        protected readonly Ai aiProps;

        protected bool animationEndCallbackDone;

        public AbstractAnimationAction(Ai.Action action, GameObject npcGo)
        {
            this.action = action;
            this.npcGo = npcGo;
            this.props = npcGo.GetComponent<Properties>();
            this.aiProps = npcGo.GetComponent<Ai>();
        }
            
        public abstract void Start();

        /// <summary>
        /// We just set the audio by default.
        /// </summary>
        public virtual void AnimationSfxEventCallback(PxEventSfxData sfxData)
        {
            SoundCreator.I.SetSound(props.npcSound, sfxData.name, sfxData.range.ToMeter());
            props.npcSound.Play();
            if (sfxData.emptySlot)
                Debug.LogWarning($"PxEventSfxData.emptySlot not yet implemented: {sfxData.name}");
        }
        
        public virtual void AnimationEventCallback(PxEventTagData data)
        {
            Debug.LogError($"Animation for {action.ActionType} is not yet implemented.");
        }
        
        /// <summary>
        /// Most of our animations are fine if we just set this flag and return it via IsFinished()
        /// </summary>
        public virtual void AnimationEventEndCallback()
        {
            animationEndCallbackDone = true;
        }
        
        /// <summary>
        /// Most of our animations are fine if we just set this flag and return it via IsFinished()
        /// </summary>
        public virtual bool IsFinished()
        {
            return animationEndCallbackDone;
        }
    }
}