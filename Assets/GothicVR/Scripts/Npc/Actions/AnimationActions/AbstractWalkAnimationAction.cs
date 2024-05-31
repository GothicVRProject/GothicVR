using System;
using GVR.Creator;
using GVR.Data.ZkEvents;
using GVR.Globals;
using GVR.Manager;
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
            WalkAndRotate, // If we're already walking and a new WP is the destination, we walk and rotate together.
            Done
        }

        protected WalkState walkState = WalkState.Initial;

        protected AbstractWalkAnimationAction(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }
        
        /// <summary>
        /// We need to define the final destination spot within overriding class.
        /// </summary>
        protected abstract Vector3 GetWalkDestination();

        protected abstract void OnDestinationReached();

        public override void Start()
        {
            base.Start();

            PhysicsHelper.EnablePhysicsForNpc(Props);
        }

        public override void Tick()
        {
            base.Tick();

            if (IsFinishedFlag)
                return;

            switch (walkState)
            {
                case WalkState.Initial:
                    walkState = WalkState.Rotate;
                    HandleRotation(NpcGo.transform, GetWalkDestination(), false);
                    return;
                case WalkState.Rotate:
                    HandleRotation(NpcGo.transform, GetWalkDestination(), false);
                    return;
                case WalkState.Walk:
                    HandleWalk(Props.colliderRootMotion.transform);
                    return;
                case WalkState.WalkAndRotate:
                    HandleRotation(NpcGo.transform, GetWalkDestination(), true);
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
                case VmGothicEnums.WalkMode.Run:
                    return "S_RUNL";
                default:
                    Debug.LogWarning($"Animation of type {Props.walkMode} not yet implemented.");
                    return "";
            }
        }

        private void StartWalk()
        {
            var animName = GetWalkModeAnimationString();
            AnimationCreator.PlayAnimation(Props.mdsNames, animName, NpcGo, true);

            walkState = WalkState.Walk;
        }


        private void HandleWalk(Transform transform)
        {
            var npcPos = transform.position;
            var walkPos = GetWalkDestination();
            var npcDistPos = new Vector3(npcPos.x, walkPos.y, npcPos.z);

            var distance = Vector3.Distance(npcDistPos, walkPos);

            // FIXME - Scorpio is above FP, but values don't represent it.
            if (distance < Constants.CloseToThreshold)
                OnDestinationReached();
        }


        private void HandleRotation(Transform transform, Vector3 destination, bool includesWalking)
        {
            var pos = transform.position;
            var sameHeightDirection = new Vector3(destination.x, pos.y, destination.z);
            var direction = (sameHeightDirection - pos).normalized;
            var dot = Vector3.Dot(direction, transform.forward);
            var lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, Time.deltaTime * 100);

            // Stop the rotation and start walking.
            if (Math.Abs(dot - 1f) < 0.0001f)
            {
                walkState = WalkState.Walk;

                // If we didn't walk so far, we do it now.
                if (!includesWalking)
                    StartWalk();
            }
        }

        /// <summary>
        /// We need to alter rootNode's position once walk animation is done.
        /// </summary>
        public override void AnimationEndEventCallback(SerializableEventEndSignal eventData)
        {
            base.AnimationEndEventCallback(eventData);

            // We need to ensure, that physics still apply when an animation is looped.
            if (walkState != WalkState.Done)
                PhysicsHelper.EnablePhysicsForNpc(Props);

            NpcGo.transform.localPosition = Props.bip01.position;
            Props.bip01.localPosition = Vector3.zero;
            Props.colliderRootMotion.localPosition = Vector3.zero;

            // TODO - Needed?
            // root.SetLocalPositionAndRotation(
            //     root.localPosition + bip01Transform.localPosition,
            //     root.localRotation * bip01Transform.localRotation);
        }
    }
}
