using System.Linq;
using GVR.Caches;
using GVR.Creator;
using GVR.Extensions;
using GVR.GothicVR.Scripts.Manager;
using GVR.Phoenix.Interface.Vm;
using GVR.Properties;
using JetBrains.Annotations;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class UseMob : AbstractWalkAnimationAction
    {
        private const string mobUseString = "T_{0}_{1}_{2}_2_{3}";
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

            walkState = WalkState.Done;
            Props.bodyState = VmGothicEnums.BodyState.BS_MOBINTERACT;
            
            NpcGo.transform.position = mobGo.transform.position;
            NpcGo.transform.rotation = mobGo.transform.rotation;

            var mobVisualName = mobGo.GetComponent<VobProperties>().visualScheme;
            var slotPositionName = GetSlotPositionTag(slotGo.name);
            
            // FIXME - Somewhat hardcoded. Needs more love in the future!
            var animName = string.Format(mobUseString, mobVisualName, slotPositionName, "S0", $"S{Action.Int0}");
            
            var mdh = AssetCache.TryGetMdh(Props.overlayMdhName);
            AnimationCreator.PlayAnimation(Props.baseMdsName, animName, mdh, NpcGo);
        }

        private string GetSlotPositionTag(string name)
        {
            if (name.EndsWithIgnoreCase("_FRONT"))
                return "FRONT";
            else if (name.EndsWithIgnoreCase("_BACK"))
                return "BACK";
            else
                return "";
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
