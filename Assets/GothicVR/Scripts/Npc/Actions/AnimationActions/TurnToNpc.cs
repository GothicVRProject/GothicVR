using GVR.Caches;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class TurnToNpc : AbstractRotateAnimationAction
    {
        private int otherId => Action.Int0;
        private int otherIndex => Action.Int1;

        public TurnToNpc(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        {
        }

        protected override Quaternion GetRotationDirection()
        {
            var destinationTransform = LookupCache.NpcCache[otherIndex].transform;
            var temp = destinationTransform.position - NpcGo.transform.position;
            return Quaternion.LookRotation(temp, Vector3.up);
        }

        public override void AnimationEndEventCallback()
        {
            base.AnimationEndEventCallback();

            IsFinishedFlag = false;
        }
    }
}
