using GVR.Data.ZkEvents;
using GVR.GothicVR.Scripts.Manager;
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

            var pos = NpcGo.transform.position;
            fp = WayNetHelper.FindNearestFreePoint(pos, destination);
        }

        public override void OnTriggerEnter(Collider coll)
        {
            if (walkState != WalkState.Walk)
                return;

            if (coll.gameObject.name != fp.Name)
                return;

            Props.CurrentFreePoint = fp;
            fp.IsLocked = true;

            AnimationEndEventCallback(new SerializableEventEndSignal(nextAnimation: ""));

            walkState = WalkState.Done;
            IsFinishedFlag = true;
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
    }
}
