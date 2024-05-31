using GVR.Data.ZkEvents;
using GVR.Manager;
using GVR.Vob.WayNet;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class GoToFp : AbstractWalkAnimationAction
    {
        private FreePoint fp;

        private string destination => Action.String0;

        private FreePoint freePoint;

        public GoToFp(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            base.Start();

            var npcPos = NpcGo.transform.position;
            fp = WayNetHelper.FindNearestFreePoint(npcPos, destination);

            // Fix - If NPC is spawned directly in front of the FP, we start transition immediately (otherwise trigger/collider won't be called).
            if (Vector3.Distance(npcPos, fp!.Position) < 1f)
                OnDestinationReached();
        }

        public override void AnimationEndEventCallback(SerializableEventEndSignal eventData)
        {
            base.AnimationEndEventCallback(eventData);

            IsFinishedFlag = false;
        }

        protected override Vector3 GetWalkDestination()
        {
            return fp.Position;
        }

        protected override void OnDestinationReached()
        {
            Props.CurrentFreePoint = fp;
            fp.IsLocked = true;

            AnimationEndEventCallback(new SerializableEventEndSignal(nextAnimation: ""));

            walkState = WalkState.Done;
            IsFinishedFlag = true;
        }
    }
}
