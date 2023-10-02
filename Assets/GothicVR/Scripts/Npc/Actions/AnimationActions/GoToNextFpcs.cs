using System.Linq;
using GVR.Manager;
using GVR.Vob.WayNet;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class GoToNextFp : AbstractWalkAnimationAction
    {
        private FreePoint fp;
        
        public GoToNextFp(Ai.Action action, GameObject npcGo) : base(action, npcGo)
        { }
        
        public override void Start()
        {
            var pos = npcGo.transform.position;

            fp = WayNetManager.I.FindNearestFreePoint(pos, action.str0);

            movingLocation = fp.Position;
        }
        
        public override void OnCollisionEnter(Collision collision)
        {
            if (walkState != WalkState.Walk)
                return;
            
            var expectedGo = collision.contacts
                .Select(i => i.otherCollider.gameObject)
                .FirstOrDefault(i => i.name == fp.Name);

            if (expectedGo == null)
                return;

            // FIXME - We also need to set a "blocked"/"locked" state on the FP itself, so that other NPCs ignore it for now.
            props.CurrentFreePoint = fp;
            
            var animationComp = npcGo.GetComponent<Animation>();
            animationComp.Stop();

            walkState = WalkState.Done;
            isFinished = true;
        }
    }
}