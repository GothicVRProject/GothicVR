using System;
using GVR.Caches;
using GVR.Creator;
using GVR.Vm;
using GVR.Vob.WayNet;
using UnityEngine;
using ZenKit;

namespace GVR.Npc.Actions.AnimationActions
{
    public abstract class AbstractRotateAnimationAction : AbstractAnimationAction
    {
        private const float RotationSpeed = 0.5f;

        private float finalDirectionY;

        protected AbstractRotateAnimationAction(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            finalDirectionY = Props.CurrentWayNetPoint.Direction.y;
        }

        public override void Tick(Transform transform)
        {
            base.Tick(transform);

            HandleRotation(transform);
        }

        private void HandleRotation(Transform npcTransform)
        {
            npcTransform.eulerAngles += new Vector3(0, finalDirectionY * Time.deltaTime * RotationSpeed, 0); // We ignore y-axis.

            // Check if rotation is done.
            if (Math.Abs(npcTransform.eulerAngles.y - finalDirectionY) < 1f)
                IsFinishedFlag = true;
        }
    }
}
