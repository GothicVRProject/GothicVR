using GVR.GothicVR.Scripts.Manager;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class UseMob : AbstractWalkAnimationAction
    {
        public UseMob(Ai.Action action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            var slotPos = GetNearestMobSlot();

            if (slotPos == null)
                return;
            
            movingLocation = slotPos.transform.position;

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
        
        public override bool IsFinished()
        {
            // FIXME - DEBUG
            return false;
        }

    }
}