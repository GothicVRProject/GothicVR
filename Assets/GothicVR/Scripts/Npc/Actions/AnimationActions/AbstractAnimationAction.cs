using System.Linq;
using GVR.Extensions;
using GVR.GothicVR.Scripts.Manager;
using GVR.Npc.Data;
using GVR.Properties;
using PxCs.Data.Event;
using PxCs.Interface;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GVR.Npc.Actions.AnimationActions
{
    public abstract class AbstractAnimationAction
    {
        protected readonly AnimationAction Action;
        protected readonly GameObject NpcGo;
        protected readonly NpcProperties Props;

        // Root motion handling
        protected Vector3 AnimationStartPos;
        protected AnimationData AnimationData;

        protected bool isFinished;

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
        public virtual void AnimationSfxEventCallback(PxEventSfxData sfxData)
        {
            var clip = VobHelper.GetSoundClip(sfxData.name);
            Props.npcSound.clip = clip;
            Props.npcSound.maxDistance = sfxData.range.ToMeter();
            Props.npcSound.Play();

            if (sfxData.emptySlot)
                Debug.LogWarning($"PxEventSfxData.emptySlot not yet implemented: {sfxData.name}");
        }
        
        public virtual void AnimationEventCallback(PxEventTagData data)
        {
            // FIXME - I have no clue about this inventory_torch event, but it gets called quite often.
            if (data.type == PxModelScript.PxEventTagType.inventory_torch)
                return;
            
            Debug.LogError($"Animation for {Action.ActionType} is not yet implemented.");
        }
        
        /// <summary>
        /// Most of our animations are fine if we just set this flag and return it via IsFinished()
        /// </summary>
        public virtual void AnimationEndEventCallback()
        {
            isFinished = true;
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
        /// As we use legacy animations, we can't use RootMotion. We therefore need to rebuild it.
        /// </summary>
        protected void HandleRootMotion(Transform transform)
        {
            var currentTime = NpcGo.GetComponent<Animation>()[AnimationData.clip.name].time;

            // We seek the item, which is the exact animation at that time or the next with only a few milliseconds more time.
            // It's more performant to search for than doing a _between_ check ;-)
            var indexObj = AnimationData.rootMotions.FirstOrDefault(i => i.time >= currentTime);

            // location, when animation started + (rootMotion's location change rotated into direction of current localRot)
            transform.localPosition = AnimationStartPos + transform.localRotation * indexObj.position;

            // transform.localRotation = newRot * walkingStartRot;
        }
        
        /// <summary>
        /// Most of our animations are fine if we just set this flag and return it via IsFinished()
        /// </summary>
        public virtual bool IsFinished()
        {
            return isFinished;
        }
    }
}
