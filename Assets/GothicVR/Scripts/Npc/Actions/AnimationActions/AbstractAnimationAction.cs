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
            var bip01Transform = NpcGo.FindChildRecursively("BIP01").transform;
            var root = NpcGo.transform;
            root.position = bip01Transform.position;
            bip01Transform.localPosition = Vector3.zero;

            // root.SetLocalPositionAndRotation(
            //     root.localPosition + bip01Transform.localPosition,
            //     root.localRotation * bip01Transform.localRotation);

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

        private float prevVelocityUpAddition;

        /// <summary>
        /// As we use legacy animations, we can't use RootMotion. We therefore need to rebuild it.
        /// </summary>
        protected void HandleRootMotion(Transform transform)
        {
            /*
             * root
             *  /BIP01/ <- animation root
             *    /BIP01/... <- animation bones
             *  /RootCollider/ <- gets transform.pos+rot from /BIP01
             */

            // The whole RootMotion needs to be copied over to the NPCs Collider to ensure we have proper collision detection during animation time.
            var bip01Transform = NpcGo.FindChildRecursively("BIP01").transform;

            Props.rootMotionGo.transform.SetLocalPositionAndRotation(bip01Transform.localPosition, bip01Transform.localRotation);


            /*
             * On top of collision, we also need to handle physics. This is done by changing root's position with dynamic rigidbody's velocity.
             * Hint: If an NPC walks up, the +y velocity isn't enough. Therefore we add up some force to help the NPC to not fall through the ground.
             * FIXME - There will be better solutions like setting it static to a value of ~+2f etc. Need to check later!
             */
            var velocity = Props.rootMotionGo.GetComponent<Rigidbody>().velocity;
            if (velocity.y > 0.0f)
            {
                velocity.y += prevVelocityUpAddition;
                prevVelocityUpAddition += 0.1f;
            }
            else
            {
                prevVelocityUpAddition = 0f;
            }

            NpcGo.transform.localPosition += velocity * Time.deltaTime;
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
