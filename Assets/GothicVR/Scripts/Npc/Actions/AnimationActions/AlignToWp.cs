using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class AlignToWp : AbstractRotateAnimationAction
    {
        public AlignToWp(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        protected override Vector3 GetRotationDirection()
        {
            if(Props.CurrentWayPoint == null)
                return Vector3.zero;
            return Props.CurrentWayPoint.Direction;
        }
    }
}
