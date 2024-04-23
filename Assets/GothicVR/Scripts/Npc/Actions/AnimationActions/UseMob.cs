using System;
using System.Linq;
using GVR.Creator;
using GVR.Data.ZkEvents;
using GVR.Extensions;
using GVR.GothicVR.Scripts.Manager;
using GVR.Manager;
using GVR.Properties;
using GVR.Vm;
using JetBrains.Annotations;
using UnityEngine;
using ZenKit.Vobs;
using EventType = ZenKit.EventType;

namespace GVR.Npc.Actions.AnimationActions
{
    public class UseMob : AbstractWalkAnimationAction
    {
        private const string MobTransitionAnimationString = "T_{0}{1}{2}_2_{3}";
        private const string MobLoopAnimationString = "S_{0}_S{1}";
        private GameObject mobGo;
        private GameObject slotGo;
        private Vector3 destination;

        private bool isStopUsingMob => Action.Int0 <= -1;

        public UseMob(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            base.Start();

            // NPC is already interacting with a Mob, we therefore assume it's a change of state (e.g. -1 to stop Mob usage)
            if (Props.bodyState == VmGothicEnums.BodyState.BS_MOBINTERACT)
            {
                mobGo = Props.currentInteractable;
                slotGo = Props.currentInteractableSlot;

                StartMobUseAnimation();
                return;
            }

            // Else: We have a new animation where we seek the Mob before walking towards and executing action.
            var mob = GetNearestMob();
            var slot = GetNearestMobSlot(mob);

            if (slot == null)
            {
                IsFinishedFlag = true;
                return;
            }

            mobGo = mob;
            slotGo = slot;
            destination = slotGo.transform.position;

            Props.currentInteractable = mobGo;
            Props.currentInteractableSlot = slotGo;
            Props.bodyState = VmGothicEnums.BodyState.BS_MOBINTERACT;

            // Fix - If NPC is spawned directly in front of the Mob, we start transition immediately (otherwise trigger/collider won't be called).
            if (Vector3.Distance(NpcGo.transform.position, slotGo.transform.position) < 0.5f)
                StartMobUseAnimation();
        }

        [CanBeNull]
        private GameObject GetNearestMob()
        {
            var pos = NpcGo.transform.position;
            return VobHelper.GetFreeInteractableWithin10M(pos, Action.String0)?.gameObject;
        }
        
        [CanBeNull]
        private GameObject GetNearestMobSlot(GameObject mob)
        {
            if (mob == null)
                return null;
            
            var pos = NpcGo.transform.position;
            var slot = VobHelper.GetNearestSlot(mob.gameObject, pos);

            return slot;
        }

        public override void OnTriggerEnter(Collider coll)
        {
            if (walkState != WalkState.Walk)
                return;

            if (coll.gameObject != slotGo)
                return;

            StartMobUseAnimation();
        }

        private void StartMobUseAnimation()
        {
            walkState = WalkState.Done;
            PhysicsHelper.DisablePhysicsForNpc(Props);

            // AnimationCreator.StopAnimation(NpcGo);
            NpcGo.transform.SetPositionAndRotation(slotGo.transform.position, slotGo.transform.rotation);

            PlayTransitionAnimation();
        }

        private string GetSlotPositionTag(string name)
        {
            if (name.EndsWithIgnoreCase("_FRONT"))
                return "_FRONT_";
            else if (name.EndsWithIgnoreCase("_BACK"))
                return "_BACK_";
            else
                return "_";
        }

        protected override Vector3 GetWalkDestination()
        {
            return destination;
        }

        /// <summary>
        /// Only after the Mob is reached and final transition animation is done, we will finalize this Action.
        /// </summary>
        public override void AnimationEndEventCallback(SerializableEventEndSignal eventData)
        {
            base.AnimationEndEventCallback(eventData);
            IsFinishedFlag = false;

            if (walkState != WalkState.Done)
                return;

            UpdateState();

            // If we arrived at the Mobsi, we will further execute the transitions step-by-step until demanded state is reached.
            if (Props.currentInteractableStateId != Action.Int0)
            {
                PlayTransitionAnimation();
                return;
            }

            // Mobsi isn't in use any longer
            if (Props.currentInteractableStateId == -1)
            {
                Props.currentInteractable = null;
                Props.currentInteractableSlot = null;
                Props.bodyState = VmGothicEnums.BodyState.BS_STAND;

                PhysicsHelper.EnablePhysicsForNpc(Props);
            }
            // Loop Mobsi animation until the same UseMob with -1 is called.
            else
            {
                var mobVisualName = mobGo.GetComponent<VobProperties>().visualScheme;
                var animName = string.Format(MobLoopAnimationString, mobVisualName, Action.Int0);
                AnimationCreator.PlayAnimation(Props.mdsNames, animName, NpcGo, true);
            }
            IsFinishedFlag = true;
        }

        private void UpdateState()
        {
            // FIXME - We need to check. For Cauldron/Cook we have only t_s0_2_Stand, but not t_s1_2_s0 - But is it for all of them?
            if (isStopUsingMob)
            {
                Props.currentInteractableStateId = -1;
            }
            else
            {
                var newStateAddition = Props.currentInteractableStateId > Action.Int0 ? -1 : +1;
                Props.currentInteractableStateId += newStateAddition;
            }
        }

        private void PlayTransitionAnimation()
        {
            string from;
            string to;

            // FIXME - We need to check. For Cauldron/Cook we have only t_s0_2_Stand, but not t_s1_2_s0 - But is it for all of them?
            if (isStopUsingMob)
            {
                from = "S0";
                to = "Stand";
            }
            else
            {
                from = Props.currentInteractableStateId.ToString();
                to = $"S{Props.currentInteractableStateId + 1}";

                from = from switch
                {
                    "-1" => "Stand",
                    _ => $"S{from}"
                };
            }

            var mobVisualName = mobGo.GetComponent<VobProperties>().visualScheme;
            var slotPositionName = GetSlotPositionTag(slotGo.name);
            var animName = string.Format(MobTransitionAnimationString, mobVisualName, slotPositionName, from, to);

            AnimationCreator.PlayAnimation(Props.mdsNames, animName, NpcGo);
        }
        
        protected override void InsertItem(string slot1, string slot2)
        {
            if (slot2.Any())
                throw new Exception("Slot 2 is set but not yet handled by InsertItem as AnimationEvent.");

            var slotGo = NpcGo.FindChildRecursively(slot1);
            var item = ((InteractiveObject)mobGo.GetComponent<VobProperties>().Properties).Item;
            VobCreator.CreateItem(item, slotGo);

            Props.usedItemSlot = slot1;
        }
    }
}
