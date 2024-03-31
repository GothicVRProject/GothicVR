using System;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class AlignToWp : AbstractRotateAnimationAction
    {
        public AlignToWp(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        protected override Quaternion GetRotationDirection()
        {
            try
            {
                var euler = Props.CurrentWayPoint.Direction;
                return Quaternion.Euler(euler);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return Quaternion.identity;;
            }
        }
    }
}
