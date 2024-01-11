using System.Linq;
using GVR.Caches;
using GVR.Creator;
using GVR.Extensions;
using GVR.GothicVR.Scripts.Manager;
using GVR.Vm;
using GVR.Properties;
using JetBrains.Annotations;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class UseMob : AbstractWalkAnimationAction
    {
        private const string mobUseString = "T_{0}{1}{2}_2_{3}";
        private GameObject mobGo;
        private GameObject slotGo;
        private Vector3 destination;


        public UseMob(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            var mob = GetNearestMob();
            var slotPos = GetNearestMobSlot(mob);

            if (slotPos == null)
                return;

            mobGo = mob;
            slotGo = slotPos;
            destination = slotGo.transform.position;
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


            AnimationCreator.StopAnimation(NpcGo);
            Props.bodyState = VmGothicEnums.BodyState.BS_MOBINTERACT;

            NpcGo.transform.SetPositionAndRotation(slotGo.transform.position, slotGo.transform.rotation);

            var mobVisualName = mobGo.GetComponent<VobProperties>().visualScheme;
            var slotPositionName = GetSlotPositionTag(slotGo.name);

            // FIXME - Somewhat hardcoded. Needs more love in the future!
            var animName = string.Format(mobUseString, mobVisualName, slotPositionName, "S0", $"S{Action.Int0}");

            AnimationCreator.PlayAnimation(Props.baseMdsName, animName, Props.overlayMdhName, NpcGo);
            walkState = WalkState.Done;
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
        /// Only after the Mob is reached and final animation is done, we will close the loop.
        /// </summary>
        public override void AnimationEndEventCallback()
        {
            if (walkState == WalkState.Done)
                IsFinishedFlag = true;
        }
    }
}
