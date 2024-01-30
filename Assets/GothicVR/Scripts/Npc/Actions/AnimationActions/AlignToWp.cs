using System;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class AlignToWp : AbstractRotateAnimationAction
    {
        public AlignToWp(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        protected override Vector3 GetRotationDirection()
        {
            try
            {
                return Props.CurrentWayPoint.Direction;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return Vector3.zero;
            }
        }
    }
}
