using GVR.Manager;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class LookAt : AbstractRotateAnimationAction
    {
        private Transform destinationTransform;
        private string waypointName => Action.String0;

        public LookAt(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        protected override Vector3 GetRotationDirection()
        {
            return WayNetHelper.GetWayNetPoint(waypointName).Direction;
        }
    }
}