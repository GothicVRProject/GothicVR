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
                    HandleWalk(transform, GetWalkDestination());
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

            walkingStartPos = npcGo.transform.localPosition;
            npcGo.GetComponent<Animation>()[animationData.clip.name].time = 0f;
        }

        private string GetWalkModeAnimationString()
        {
            switch (props.walkMode)
            {
                case VmGothicEnums.WalkMode.Walk:
                    return "S_WALKL";
                default:
                    Debug.LogWarning($"Animation of type {props.walkMode} not yet implemented.");
                    return "";
            }
        }

        private AnimationData animationData;
        private Vector3 walkingStartPos;

        private void StartWalk()
        {
            var animName = GetWalkModeAnimationString();
            var mdh = AssetCache.TryGetMdh(props.overlayMdhName);
            animationData = AnimationCreator.PlayAnimation(props.baseMdsName, animName, mdh, npcGo, true);

            walkingStartPos = npcGo.transform.localPosition;

            walkState = WalkState.Walk;
        }

        private void HandleWalk(Transform transform, Vector3 destination)
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
            var currentTime = npcGo.GetComponent<Animation>()[animationData.clip.name].time;

            // We seek the item, which is the exact animation at that time or the next with only a few milliseconds more time.
            // It's more performant to search for than doing a _between_ check ;-)
            var indexObj = animationData.rootMotions.FirstOrDefault(i => i.time >= currentTime);
            
            // location, when animation started + (rootMotion's location change rotated into direction of current localRot)
            transform.localPosition = walkingStartPos + transform.localRotation * indexObj.position;

            // transform.localRotation = newRot * walkingStartRot;

            Debug.Log(currentTime);
        }
    }
}
