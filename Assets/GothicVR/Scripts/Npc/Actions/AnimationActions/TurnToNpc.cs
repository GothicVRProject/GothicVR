using GVR.Caches;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class TurnToNpc : AbstractRotateAnimationAction
    {
        private Transform destinationTransform;
        private int otherId => Action.Int0;
        private int otherIndex => Action.Int1;

        public TurnToNpc(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        {
        }

        public override void Start()
        {
            destinationTransform = LookupCache.NpcCache[otherIndex].transform;
        }

        protected override Vector3 GetRotationDirection()
        {
            return (destinationTransform.position - NpcGo.transform.position).normalized;
        }

        public override void Tick(Transform transform)
        {
            // As we turn to a movable NPC, we need to recalculate the finalDirection dynamically with each tick.
            finalDirection = GetRotationDirection();
            
            base.Tick(transform);
        }

        public override void AnimationEndEventCallback()
        {
            base.AnimationEndEventCallback();

            IsFinishedFlag = false;
        }
    }
}
