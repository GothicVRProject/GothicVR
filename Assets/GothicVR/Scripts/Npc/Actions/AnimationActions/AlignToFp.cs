using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class AlignToFp : AbstractRotateAnimationAction
    {
        public AlignToFp(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        protected override Quaternion GetRotationDirection()
        {
            var euler = Props.CurrentFreePoint.Direction;

            return Quaternion.Euler(euler);
        }
    }
}
