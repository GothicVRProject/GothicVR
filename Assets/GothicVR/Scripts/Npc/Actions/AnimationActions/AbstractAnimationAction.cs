using System;
using System.Linq;
using GVR.Creator;
using GVR.Data.ZkEvents;
using GVR.Extensions;
using GVR.GothicVR.Scripts.Manager;
using GVR.Manager;
using GVR.Properties;
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
            var type = Props.headMorph.GetTypeByName(data.Animation);

            Props.headMorph.StartAnimation(Props.BodyData.Head, type, false);
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
            var slotGo = NpcGo.FindChildRecursively(Props.usedItemSlot);
            var item = slotGo!.transform.GetChild(0);
            Object.Destroy(item.gameObject);
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
