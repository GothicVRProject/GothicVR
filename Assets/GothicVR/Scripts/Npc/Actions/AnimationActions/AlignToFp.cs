using GVR.Manager;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class AlignToFp : AbstractRotateAnimationAction
    {
        public AlignToFp(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        protected override Vector3 GetRotationDirection()
        {
            return Props.CurrentFreePoint.Direction;
        }
    }
}