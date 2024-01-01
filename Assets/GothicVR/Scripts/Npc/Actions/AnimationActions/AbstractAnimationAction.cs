using GVR.Extensions;
using GVR.GothicVR.Scripts.Manager;
using GVR.Properties;
using UnityEngine;
using ZenKit;
using EventType = ZenKit.EventType;

namespace GVR.Npc.Actions.AnimationActions
{
    public abstract class AbstractAnimationAction
    {
        protected readonly AnimationAction Action;
        protected readonly GameObject NpcGo;
        protected readonly NpcProperties Props;

        protected bool IsFinishedFlag;

        public AbstractAnimationAction(AnimationAction action, GameObject npcGo)
        {
            Action = action;
            NpcGo = npcGo;
            Props = npcGo.GetComponent<NpcProperties>();
        }
            
        public abstract void Start();

        /// <summary>
        /// We just set the audio by default.
        /// </summary>
        public virtual void AnimationSfxEventCallback(IEventSoundEffect sfxData)
        {
            var clip = VobHelper.GetSoundClip(sfxData.Name);
            Props.npcSound.clip = clip;
            Props.npcSound.maxDistance = sfxData.Range.ToMeter();
            Props.npcSound.Play();

            if (sfxData.EmptySlot)
                Debug.LogWarning($"PxEventSfxData.emptySlot not yet implemented: {sfxData.Name}");
        }
        
        public virtual void AnimationEventCallback(IEventTag data)
        {
            // FIXME - I have no clue about this inventory_torch event, but it gets called quite often.
            if (data.Type == EventType.TorchInventory)
                return;
            
            Debug.LogError($"Animation for {Action.ActionType} is not yet implemented.");
        }
        
        /// <summary>
        /// Most of our animations are fine if we just set this flag and return it via IsFinished()
        /// </summary>
        public virtual void AnimationEndEventCallback()
        {
            IsFinishedFlag = true;
        }

        public virtual void OnCollisionEnter(Collision coll)
        { }
        public virtual void OnTriggerEnter(Collider coll)
        { }

        public virtual void OnCollisionExit(Collision coll)
        { }
        public virtual void OnTriggerExit(Collider coll)
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
            return IsFinishedFlag;
        }
    }
}
