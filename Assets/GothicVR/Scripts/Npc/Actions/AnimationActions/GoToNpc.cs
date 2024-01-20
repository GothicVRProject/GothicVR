using System.Collections.Generic;
using System.Linq;
using GVR.Manager;
using GVR.World;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class GoToNpc : AbstractWalkAnimationAction
    {
        private Vector3 destination => Action.V30;

        public GoToNpc(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        protected override Vector3 GetWalkDestination()
        {
            return destination;
        }

        public override void AnimationEndEventCallback()
        {
            base.AnimationEndEventCallback();

            IsFinishedFlag = false;
        }
    }
}
