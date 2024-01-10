using System.Linq;
using GVR.Manager;
using GVR.Vob.WayNet;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class GoToNextFp : AbstractWalkAnimationAction
    {
        private FreePoint fp;
        
        public GoToNextFp(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }
        
        public override void Start()
        {
            var pos = NpcGo.transform.position;

            fp = WayNetHelper.FindNearestFreePoint(pos, Action.String0);
        }
        
        public override void OnTriggerEnter(Collider coll)
        {
            if (walkState != WalkState.Walk)
                return;

            if (coll.gameObject.name != fp.Name)
                return;

            Props.CurrentWayNetPoint = fp;
            fp.IsLocked = true;

            var animationComp = NpcGo.GetComponent<Animation>();
            animationComp.Stop();
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
