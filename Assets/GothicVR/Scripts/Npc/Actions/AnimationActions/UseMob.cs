using System.Linq;
using GVR.Caches;
using GVR.Creator;
using GVR.Extensions;
using GVR.GothicVR.Scripts.Manager;
using GVR.Vm;
using GVR.Properties;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;

namespace GVR.Npc.Actions.AnimationActions
{
    public class UseMob : AbstractWalkAnimationAction
    {
        private const string MobTransitionAnimationString = "T_{0}{1}{2}_2_{3}";
        private const string MobLoopAnimationString = "S_{0}_S{1}";
        private GameObject mobGo;
        private GameObject slotGo;
        private Vector3 destination;


        public UseMob(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
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
            var slotPos = GetNearestMobSlot(mob);

            if (slotPos == null)
            {
                IsFinishedFlag = true;
                return;
            }

            mobGo = mob;
            slotGo = slotPos;
            destination = slotGo.transform.position;

            Props.currentInteractable = mobGo;
            Props.currentInteractableSlot = slotGo;
            Props.bodyState = VmGothicEnums.BodyState.BS_MOBINTERACT;
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
            var slotPos = VobHelper.GetNearestSlot(mob.gameObject, pos);

            return slotPos;
        }

        public override void OnCollisionEnter(Collision collision)
        {
            if (walkState != WalkState.Walk)
                return;

            var expectedGo = collision.contacts
                .Select(i => i.otherCollider.gameObject)
                .FirstOrDefault(i => i == slotGo);

            if (expectedGo == null)
                return;

            StartMobUseAnimation();
        }

        private void StartMobUseAnimation()
        {
            walkState = WalkState.Done;

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
        public override void AnimationEndEventCallback()
        {
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
            }
            // Loop Mobsi animation until the same UseMob with -1 is called.
            else
            {
                var mobVisualName = mobGo.GetComponent<VobProperties>().visualScheme;
                var animName = string.Format(MobLoopAnimationString, mobVisualName, Action.Int0);
                AnimationCreator.PlayAnimation(Props.baseMdsName, animName, Props.overlayMdhName, NpcGo, true);
            }
            IsFinishedFlag = true;
        }

        private void UpdateState()
        {
            var newStateAddition = Props.currentInteractableStateId > Action.Int0 ? -1 : +1;
            Props.currentInteractableStateId += newStateAddition;
        }

        private void PlayTransitionAnimation()
        {
            var nextStepChange = Props.currentInteractableStateId < Action.Int0 ? +1 : -1;

            var from = Props.currentInteractableStateId.ToString();
            var to = (Props.currentInteractableStateId + nextStepChange).ToString();

            from = from switch
            {
                "-1" => "Stand",
                _ => $"S{from}"
            };

            to = to switch
            {
                "-1" => "Stand",
                _ => $"S{to}"
            };

            var mobVisualName = mobGo.GetComponent<VobProperties>().visualScheme;
            var slotPositionName = GetSlotPositionTag(slotGo.name);
            var animName = string.Format(MobTransitionAnimationString, mobVisualName, slotPositionName, from, to);

            AnimationCreator.PlayAnimation(Props.baseMdsName, animName, Props.overlayMdhName, NpcGo);
        }
    }
}
