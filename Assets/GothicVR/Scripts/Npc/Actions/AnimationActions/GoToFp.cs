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

            AnimationEndEventCallback();

            walkState = WalkState.Done;
            IsFinishedFlag = true;
        }

        public override void AnimationEndEventCallback()
        {
            base.AnimationEndEventCallback();

            IsFinishedFlag = false;
        }

        protected override Vector3 GetWalkDestination()
        {
            return fp.Position;
        }
    }
}
