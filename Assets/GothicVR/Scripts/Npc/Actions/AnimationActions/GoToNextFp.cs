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
            var pos = npcGo.transform.position;

            fp = WayNetHelper.FindNearestFreePoint(pos, action.str0);

            movingLocation = fp.Position;
        }
        
        public override void OnTriggerEnter(Collider coll)
        {
            if (walkState != WalkState.Walk)
                return;

            if (coll.gameObject.name != fp.Name)
                return;

            props.CurrentFreePoint = fp;
            fp.IsLocked = true;

            var animationComp = npcGo.GetComponent<Animation>();
            animationComp.Stop();

            walkState = WalkState.Done;
            isFinished = true;
        }

        public override void AnimationEventEndCallback()
        {
            base.AnimationEventEndCallback();

            isFinished = false;
        }

        protected override Vector3 GetDestination()
        {
            return fp.Position;
        }

    }
}
