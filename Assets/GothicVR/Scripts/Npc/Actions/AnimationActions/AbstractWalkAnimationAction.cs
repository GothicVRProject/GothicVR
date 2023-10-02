using System.Linq;
using GVR.Caches;
using GVR.Creator;
using GVR.Extensions;
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

        protected Vector3 movingLocation;
        protected WalkState walkState = WalkState.Initial;
        
        protected AbstractWalkAnimationAction(Ai.Action action, GameObject npcGo) : base(action, npcGo)
        { }
        
        
        /// <summary>
        /// As we use legacy animations, we can't use RootMotion. We therefore need to rebuild it.
        /// </summary>
        public override void Tick(Transform transform)
        {
            switch (walkState)
            {
                case WalkState.Initial:
                    StartRotation(transform);
                    return;
                case WalkState.Rotate:
                    HandleRotation(transform);
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

        private void StartRotation(Transform transform)
        {
            // FIXME - we need to evaluate if we turn right or left. Then choose TurnR/TurnL properly.
            var mdh = AssetCache.I.TryGetMdh(props.overlayMdhName);
            AnimationCreator.I.PlayAnimation(props.baseMdsName, "t_WalkWTurnR", mdh, npcGo, true);
            
            walkState = WalkState.Rotate;
        }
        
        private void HandleRotation(Transform transform)
        {
            var singleStep = 1.0f * Time.deltaTime;
            var targetDirection = movingLocation - transform.position;

            // If we set TargetDirection of >y< to 0, then we rotate left/right only.
            targetDirection.y = 0;
            var newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
            var newRotation = Quaternion.LookRotation(newDirection);

            // Rotation is done.
            if (transform.rotation == newRotation)
            {
                StartWalkAnimation();
                return;
            }

            transform.rotation = newRotation;
        }
        
        private string GetWalkModeAnimationString()
        {
            switch (aiProps.walkMode)
            {
                case VmGothicEnums.WalkMode.Walk:
                    return "S_WALKL";
                default:
                    Debug.LogWarning($"Animation of type {aiProps.walkMode} not yet implemented.");
                    return "";
            }
        }
        
        private void StartWalkAnimation()
        {
            // 1. Turn around (optional)
            // 2. Walk towards Mob
            // 3. if Collider hit, then start animation
            
            var animName = GetWalkModeAnimationString();
            var mdh = AssetCache.I.TryGetMdh(props.overlayMdhName);
            AnimationCreator.I.PlayAnimation(props.baseMdsName, animName, mdh, npcGo, true);

            walkState = WalkState.Walk;
        }
        
        private void HandleWalk(Transform transform)
        {
            var step =  1f * Time.deltaTime; // calculate distance to move
            var newPos = Vector3.MoveTowards(transform.position, movingLocation, step);
            
            transform.position = newPos;
        }
        
        /// <summary>
        /// Only after the Mob is reached and final animation is done, we will close the loop.
        /// </summary>
        public override void AnimationEventEndCallback()
        {
            if (walkState == WalkState.Done)
                animationEndCallbackDone = true;
        }
    }
}