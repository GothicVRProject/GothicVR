using System;
using GVR.Caches;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class GoToNpc : AbstractWalkAnimationAction
    {
        private Transform destinationTransform;
        private int otherId => Action.Int0;
        private int otherIndex => Action.Int1;

        public GoToNpc(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            // Hero
            if (otherId == 0)
                destinationTransform = Camera.main!.transform;
            else
                destinationTransform = LookupCache.NpcCache[otherIndex].transform;
        }

        protected override Vector3 GetWalkDestination()
        {
            return destinationTransform.position;
        }


        public override void AnimationEndEventCallback()
        {
            base.AnimationEndEventCallback();

            IsFinishedFlag = false;
        }
    }
}
