using System;
using System.Diagnostics;
using GVR.Caches;
using GVR.Creator;
using GVR.Extensions;
using GVR.Manager;
using GVR.Properties;
using PxCs.Data.Animation;
using PxCs.Data.Event;
using PxCs.Interface;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GVR.Npc.Actions.AnimationActions
{
    public abstract class AbstractAnimationAction
    {
        protected readonly Ai.Action action;
        protected readonly GameObject npcGo;
        protected readonly NpcProperties props;
        protected readonly Ai aiProps;

        protected bool isFinished;

        public AbstractAnimationAction(Ai.Action action, GameObject npcGo)
        {
            this.action = action;
            this.npcGo = npcGo;
            this.props = npcGo.GetComponent<NpcProperties>();
            this.aiProps = npcGo.GetComponent<Ai>();
        }
            
        public abstract void Start();

        /// <summary>
        /// We just set the audio by default.
        /// </summary>
        public virtual void AnimationSfxEventCallback(PxEventSfxData sfxData)
        {
            var clip = SoundCreator.I.CreateAudioClip(sfxData.name);
            props.npcSound.clip = clip;
            props.npcSound.maxDistance = sfxData.range.ToMeter();
            props.npcSound.Play();

            if (sfxData.emptySlot)
                Debug.LogWarning($"PxEventSfxData.emptySlot not yet implemented: {sfxData.name}");
        }
        
        public virtual void AnimationEventCallback(PxEventTagData data)
        {
            // FIXME - I have no clue about this inventory_torch event, but it gets called quite often.
            if (data.type == PxModelScript.PxEventTagType.inventory_torch)
                return;
            
            Debug.LogError($"Animation for {action.ActionType} is not yet implemented.");
        }
        
        /// <summary>
        /// Most of our animations are fine if we just set this flag and return it via IsFinished()
        /// </summary>
        public virtual void AnimationEventEndCallback()
        {
            isFinished = true;
        }

        /// <summary>
        /// Only needed for walking or other movement related animations.
        /// </summary>
        public virtual void OnCollisionEnter(Collision collision)
        { }
        
        /// <summary>
        /// Called every update cycle.
        /// Can be used to handle frequent things internally.
        /// </summary>
        public virtual void Tick(Transform transform)
        { }
        
        /// <summary>
        /// Most of our animations are fine if we just set this flag and return it via IsFinished()
        /// </summary>
        public virtual bool IsFinished()
        {
            return isFinished;
        }
    }
}