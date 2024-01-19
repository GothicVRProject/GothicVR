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
        private bool isRotateLeft;

        protected AbstractRotateAnimationAction(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            finalDirectionY = Props.CurrentWayNetPoint.Direction.y;

            // Already aligned.
            if (Math.Abs(NpcGo.transform.eulerAngles.y - finalDirectionY) < 1f)
            {
                IsFinishedFlag = true;
                return;
            }

            AnimationCreator.PlayAnimation(Props.mdsNames, GetRotateModeAnimationString(), NpcGo, true);

            // https://discussions.unity.com/t/determining-whether-to-rotate-left-or-right/44021
            var cross = Vector3.Cross(Props.CurrentWayNetPoint.Direction, NpcGo.transform.forward);
            isRotateLeft = (cross.y >= 0);
        }

        private string GetRotateModeAnimationString()
        {
            switch (Props.walkMode)
            {
                case VmGothicEnums.WalkMode.Walk:
                    return (isRotateLeft ? "T_WALKWTURNL" : "T_WALKWTURNR");
                default:
                    Debug.LogWarning($"Animation of type {Props.walkMode} not yet implemented.");
                    return "";
            }
        }

        public override void Tick(Transform transform)
        {
            base.Tick(transform);

            HandleRotation(transform);
        }

        /// <summary>
        /// Unfortunately it seems that G1 rotation animations have no root motions for the rotation (unlike walking).
        /// We therefore need to set it manually here.
        /// </summary>
        private void HandleRotation(Transform npcTransform)
        {
            // We use y-axis only.
            if (isRotateLeft)
                npcTransform.eulerAngles -= new Vector3(0, finalDirectionY * Time.deltaTime * RotationSpeed, 0);
            else
                npcTransform.eulerAngles += new Vector3(0, finalDirectionY * Time.deltaTime * RotationSpeed, 0);

            // Check if rotation is done.
            if (Math.Abs(npcTransform.eulerAngles.y - finalDirectionY) < 1f)
            {
                AnimationCreator.StopAnimation(NpcGo);
                IsFinishedFlag = true;
            }
        }
    }
}
