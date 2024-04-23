using GVR.Caches;
using GVR.Data.ZkEvents;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class LookAtNpc : AbstractRotateAnimationAction
    {
        private int otherId => Action.Int0;
        private int otherIndex => Action.Int1;

        public LookAtNpc(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        {
        }

        protected override Quaternion GetRotationDirection()
        {
            // FIXME - implement!
            return default;
            // var destinationTransform = LookupCache.NpcCache[otherIndex].transform;
            // var temp = destinationTransform.position - NpcGo.transform.position;
            // return new Vector3(0, temp.y, 0);
        }

        public override void AnimationEndEventCallback(SerializableEventEndSignal eventData)
        {
            base.AnimationEndEventCallback(eventData);

            IsFinishedFlag = false;
        }
    }
}
