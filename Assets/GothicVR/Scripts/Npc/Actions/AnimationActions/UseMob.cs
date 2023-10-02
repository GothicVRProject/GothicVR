using System.Linq;
using GVR.Caches;
using GVR.Creator;
using GVR.Extensions;
using GVR.GothicVR.Scripts.Manager;
using GVR.Phoenix.Interface.Vm;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace GVR.Npc.Actions.AnimationActions
{
    public class UseMob : AbstractAnimationAction
    {
        public UseMob(Ai.Action action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            var slotPos = GetNearestMobSlot();

            if (slotPos == null)
                return;
            
            aiProps.isMoving = true;
            aiProps.movingLocation = slotPos.transform.position;

            StartWalkAnimation();
            // FIXME - Go on!
            // AnimationCreator.I.PlayAnimation(props.baseMdsName, animationName, mdh, npcGo);
        }

        private GameObject GetNearestMobSlot()
        {
            var pos = npcGo.transform.position;
            var obj = VobManager.I.GetFreeInteractableWithin10M(pos, action.str0);
            
            if (obj == null)
                return null;
            
            var slotPos = VobManager.I.GetNearestSlot(obj.gameObject, pos);

            if (slotPos == null)
                return null;

            return slotPos;
        }

        private void StartWalkAnimation()
        {
            // 1. Turn around (optional)
            // 2. Walk towards Mob
            // 3. if Collider hit, then start animation
            
            var animName = GetWalkModeAnimationString();
            var mdh = AssetCache.I.TryGetMdh(props.overlayMdhName);
            // AnimationCreator.I.PlayAnimation(props.baseMdsName, animName, mdh, npcGo, true);
        }

        private string GetWalkModeAnimationString()
        {
            switch (aiProps.walkMode)
            {
                case VmGothicEnums.WalkMode.Walk:
                    return "S_WALKL";
                default:
                    Debug.LogWarning($"Animation of type {aiProps.walkMode} not yet implemented.");
                    return "";
            }
        }
            
        
        public override bool IsFinished()
        {
            // FIXME - DEBUG
            return false;
        }

    }
}