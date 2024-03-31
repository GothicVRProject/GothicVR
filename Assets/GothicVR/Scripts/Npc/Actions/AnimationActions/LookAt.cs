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

        protected override Quaternion GetRotationDirection()
        {
            var euler = WayNetHelper.GetWayNetPoint(waypointName).Direction;
            return Quaternion.Euler(euler);
        }
    }
}
