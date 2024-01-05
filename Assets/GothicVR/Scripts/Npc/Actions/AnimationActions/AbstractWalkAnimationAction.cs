using System;
using GVR.Caches;
using GVR.Creator;
using GVR.Extensions;
using GVR.Vm;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public abstract class AbstractWalkAnimationAction : AbstractAnimationAction
    {
        protected enum WalkState
        {
            Initial,
            Rotate,
            Walk,
            Done
        }

        protected WalkState walkState = WalkState.Initial;

        private float prevWalkVelocityUpAddition;


        protected AbstractWalkAnimationAction(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }
        
        /// <summary>
        /// We need to define the final destination spot within overriding class.
        /// </summary>
        protected abstract Vector3 GetWalkDestination();
        
        public override void Tick(Transform transform)
        {
            base.Tick(transform);

            if (IsFinishedFlag)
                return;

            switch (walkState)
            {
                case WalkState.Initial:
                    walkState = WalkState.Rotate;
                    HandleRotation(transform, GetWalkDestination());
                    return;
                case WalkState.Rotate:
                    HandleRotation(transform, GetWalkDestination());
                    return;
                case WalkState.Walk:
                    HandleWalk(transform);
                    return;
                case WalkState.Done:
                    return; // NOP
                default:
                    Debug.Log($"MovementState {walkState} not yet implemented.");
                    return;
            }
        }

        private string GetWalkModeAnimationString()
        {
            switch (Props.walkMode)
            {
                case VmGothicEnums.WalkMode.Walk:
                    return "S_WALKL";
                default:
                    Debug.LogWarning($"Animation of type {Props.walkMode} not yet implemented.");
                    return "";
            }
        }

        private void StartWalk()
        {
            var animName = GetWalkModeAnimationString();
            var mdh = AssetCache.TryGetMdh(Props.overlayMdhName);
            AnimationCreator.PlayAnimation(Props.baseMdsName, animName, mdh, NpcGo, true);

            walkState = WalkState.Walk;
        }

        private void HandleWalk(Transform transform)
        {
            HandleRootMotion(transform);
        }
        
        private void HandleRotation(Transform transform, Vector3 destination)
        {
            var sameHeightDirection = new Vector3(destination.x, transform.position.y, destination.z);
            var direction = (sameHeightDirection - transform.position).normalized;
            var dot = Vector3.Dot(direction, transform.forward);

            if (Math.Abs(dot - 1f) < 0.0001f)
            {
                StartWalk();
                walkState = WalkState.Walk;
                return;
            }

            var lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, Time.deltaTime * 100);
        }

        /// <summary>
        /// As we use legacy animations, we can't use RootMotion. We therefore need to rebuild it.
        /// </summary>
        private void HandleRootMotion(Transform transform)
        {
            /*
             * root
             *  /BIP01/ <- animation root
             *    /ColliderRootMotion <- Moved with animation as inside BIP01, but physics are applied and merged to root
             *    /... <- animation bones
             */

            // Apply physics based position change to root.
            NpcGo.transform.localPosition += Props.colliderRootMotion.localPosition;
            // Empty physics based diff. Next frame physics will be recalculated.
            Props.colliderRootMotion.localPosition = Vector3.zero;
        }

        /// <summary>
        /// We need to alter rootNode's position once walk animation is done.
        /// </summary>
        public override void AnimationEndEventCallback()
        {
            base.AnimationEndEventCallback();

            NpcGo.transform.localPosition = Props.bip01.position;
            Props.bip01.localPosition = Vector3.zero;
            Props.colliderRootMotion.localPosition = Vector3.zero;


            // root.SetLocalPositionAndRotation(
            //     root.localPosition + bip01Transform.localPosition,
            //     root.localRotation * bip01Transform.localRotation);
        }
    }
}
