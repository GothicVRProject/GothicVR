using System;
using System.Linq;
using GVR.Caches;
using GVR.Creator;
using GVR.Npc.Data;
using GVR.Phoenix.Interface.Vm;
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

        protected AbstractWalkAnimationAction(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }
        
        /// <summary>
        /// We need to define the final destination spot within overriding class.
        /// </summary>
        protected abstract Vector3 GetWalkDestination();
        
        public override void Tick(Transform transform)
        {
            base.Tick(transform);

            if (isFinished)
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

        public override void AnimationEndEventCallback()
        {
            base.AnimationEndEventCallback();

            AnimationStartPos = NpcGo.transform.localPosition;
            NpcGo.GetComponent<Animation>()[AnimationData.clip.name].time = 0f;
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
            AnimationData = AnimationCreator.PlayAnimation(Props.baseMdsName, animName, mdh, NpcGo, true);

            AnimationStartPos = NpcGo.transform.localPosition;

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
    }
}
