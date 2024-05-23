using System;
using System.Linq;
using GVR.Creator;
using GVR.Data.ZkEvents;
using GVR.Extensions;
using GVR.GothicVR.Scripts.Manager;
using GVR.Manager;
using GVR.Properties;
using GVR.Vm;
using UnityEngine;
using EventType = ZenKit.EventType;
using Object = UnityEngine.Object;

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

        public virtual void Start()
        {
            // By default every Daedalus aninmation starts without using physics. But they can always overwrite it (e.g.) for walking.
            PhysicsHelper.DisablePhysicsForNpc(Props);
        }

        /// <summary>
        /// We just set the audio by default.
        /// </summary>
        public virtual void AnimationSfxEventCallback(SerializableEventSoundEffect sfxData)
        {
            var clip = VobHelper.GetSoundClip(sfxData.Name);
            Props.npcSound.clip = clip;
            Props.npcSound.maxDistance = sfxData.Range.ToMeter();
            Props.npcSound.Play();

            if (sfxData.EmptySlot)
                Debug.LogWarning($"PxEventSfxData.emptySlot not yet implemented: {sfxData.Name}");
        }
        
        public virtual void AnimationEventCallback(SerializableEventTag data)
        {
            switch (data.Type)
            {
                case EventType.ItemInsert:
                    InsertItem(data.Slot1, data.Slot2);
                    break;
                case EventType.ItemDestroy:
                case EventType.ItemRemove:
                    RemoveItem();
                    break;
                case EventType.TorchInventory:
                    Debug.Log("EventType.inventory_torch: I assume this means: if torch is in inventory, then put it out. But not really sure. Need a NPC with real usage of it to predict right.");
                    break;
                default:
                    Debug.LogWarning($"EventType.type {data.Type} not yet supported.");
                    break;
            }
        }

        public virtual void AnimationMorphEventCallback(SerializableEventMorphAnimation data)
        {
            var type = Props.headMorph.GetAnimationTypeByName(data.Animation);

            Props.headMorph.StartAnimation(Props.BodyData.Head, type);
        }
        
        protected virtual void InsertItem(string slot1, string slot2)
        {
            if (slot2.Any())
                throw new Exception("Slot 2 is set but not yet handled by InsertItem as AnimationEvent.");

            var slotGo = NpcGo.FindChildRecursively(slot1);
            
            VobCreator.CreateItem(Props.currentItem, slotGo);

            Props.usedItemSlot = slot1;
        }

        private void RemoveItem()
        {
            // Some animations need to force remove items, some not.
            if (Props.usedItemSlot == "")
                return;

            var slotGo = NpcGo.FindChildRecursively(Props.usedItemSlot);
            var item = slotGo!.transform.GetChild(0);

            Object.Destroy(item.gameObject);
        }
        
        /// <summary>
        /// Most of our animations are fine if we just set this flag and return it via IsFinished()
        /// If an animation has also a next animation set, we will call it automatically.
        /// If this is not intended, the overwriting class can always reset the animation being played at the same frame.
        /// </summary>
        public virtual void AnimationEndEventCallback(SerializableEventEndSignal eventData)
        {
            // e.g. T_STAND_2_WASH -> S_WASH -> S_WASH ... -> T_WASH_2_STAND
            // Inside daedalus there is no information about S_WASH, but we need this animation automatically being played.
            if (eventData.NextAnimation.Any())
            {
                PhysicsHelper.DisablePhysicsForNpc(Props);
                AnimationCreator.PlayAnimation(Props.mdsNames, eventData.NextAnimation, Props.go);
            }
            // Play Idle animation
            // But only if NPC isn't using an item right now. Otherwise breathing will spawn hand to hips which looks wrong when (e.g.) drinking beer.
            else if (Props.currentItem < 0)
            {
                var animName = Props.walkMode switch
                {
                    VmGothicEnums.WalkMode.Walk => "S_WALK",
                    VmGothicEnums.WalkMode.Sneak => "S_SNEAK",
                    VmGothicEnums.WalkMode.Swim => "S_SWIM",
                    VmGothicEnums.WalkMode.Dive => "S_DIVE",
                    _ => "S_RUN"
                };
                var idleAnimPlaying = AnimationCreator.PlayAnimation(Props.mdsNames, animName, Props.go, true);
                if (!idleAnimPlaying)
                    Debug.LogError($"Animation {animName} not found for {NpcGo.name} on {this}.");
            }

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
