using System;
using System.Linq;
using GVR.Caches;
using GVR.Creator;
using GVR.Manager;
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

        protected Vector3 movingLocation;
        protected WalkState walkState = WalkState.Initial;

        protected AbstractWalkAnimationAction(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }
        
        public override void Tick(Transform transform)
        {
            switch (walkState)
            {
                case WalkState.Initial:
                    walkState = WalkState.Rotate;
                    HandleRotation(transform, GetDestination());
                    return;
                case WalkState.Rotate:
                    HandleRotation(transform, GetDestination());
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

        public override void AnimationEventEndCallback()
        {
            base.AnimationEventEndCallback();

            walkingStartPos = npcGo.transform.localPosition;
            npcGo.GetComponent<Animation>()[rootMotions.Item2.name].time = 0f;
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

        private Tuple<RootMotionData, AnimationClip> rootMotions;
        private Vector3 walkingStartPos;

        private void StartWalk()
        {
            var animName = GetWalkModeAnimationString();
            var mdh = AssetCache.TryGetMdh(props.overlayMdhName);
            rootMotions = AnimationCreator.PlayAnimation(props.baseMdsName, animName, mdh, npcGo, true);

            walkingStartPos = npcGo.transform.localPosition;

            walkState = WalkState.Walk;
        }

        private void HandleWalk(Transform transform)
        {
            // RotateIfNeeded
            {
                var destination = GetDestination();
                if (GetDestination() != default)
                    HandleRotation(transform, destination);
            }
            HandleRootMotion(transform);
        }

        protected virtual Vector3 GetDestination()
        {
            return default;
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
            var currentTime = npcGo.GetComponent<Animation>()[rootMotions.Item2.name].time;

            // We seek the item, which is the exact animation at that time or the next with only a few milliseconds more time.
            // It's more performant to search for than doing a _between_ check ;-)
            var indexObj = rootMotions.Item1.PosX.keys.FirstOrDefault(i => i.time >= currentTime);
            var index = Array.IndexOf(rootMotions.Item1.PosX.keys, indexObj);

            var itemPosX = indexObj.value;
            var itemPosY = rootMotions.Item1.PosY.keys[index].value;
            var itemPosZ = rootMotions.Item1.PosZ.keys[index].value;

            var itemRotW = rootMotions.Item1.RotW.keys[index];
            var itemRotX = rootMotions.Item1.RotX.keys[index];
            var itemRotY = rootMotions.Item1.RotY.keys[index];
            var itemRotZ = rootMotions.Item1.RotZ.keys[index];

            var newPos = new Vector3(itemPosX, itemPosY, itemPosZ);
            var newRot = new Quaternion(itemRotX.value, itemRotY.value, itemRotZ.value, -itemRotW.value);

            // location, when animation started + (rootMotion's location change rotated into direction of current localRot)
            transform.localPosition = walkingStartPos + transform.localRotation * newPos;

            // transform.localRotation = newRot * walkingStartRot;

            Debug.Log(currentTime);
        }
    }
}
