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
        
        public UseMob(Ai.Action action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            var mob = GetNearestMob();
            var slotPos = GetNearestMobSlot(mob);

            if (slotPos == null)
                return;

            mobGo = mob;
            slotGo = slotPos;
            movingLocation = slotPos.transform.position;
        }

        [CanBeNull]
        private GameObject GetNearestMob()
        {
            var pos = npcGo.transform.position;
            return VobManager.I.GetFreeInteractableWithin10M(pos, action.str0)?.gameObject;
        }
        
        [CanBeNull]
        private GameObject GetNearestMobSlot(GameObject mob)
        {
            if (mob == null)
                return null;
            
            var pos = npcGo.transform.position;
            var slotPos = VobManager.I.GetNearestSlot(mob.gameObject, pos);

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
            aiProps.bodyState = VmGothicEnums.BodyState.BS_MOBINTERACT;
            
            npcGo.transform.position = mobGo.transform.position;
            npcGo.transform.rotation = mobGo.transform.rotation;

            var mobVisualName = mobGo.GetComponent<VobProperties>().visualScheme;
            var slotPositionName = GetSlotPositionTag(slotGo.name);
            
            // FIXME - Somewhat hardcoded. Needs more love in the future!
            var animName = string.Format(mobUseString, mobVisualName, slotPositionName, "S0", $"S{action.i0}");
            
            var mdh = AssetCache.I.TryGetMdh(props.overlayMdhName);
            AnimationCreator.I.PlayAnimation(props.baseMdsName, animName, mdh, npcGo);
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
        
        /// <summary>
        /// Only after the Mob is reached and final animation is done, we will close the loop.
        /// </summary>
        public override void AnimationEventEndCallback()
        {
            if (walkState == WalkState.Done)
                animationEndCallbackDone = true;
        }
    }
}