using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class StandUp : AbstractAnimationAction
    {
        public StandUp(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            // FIXME - TODO
        }

        public override void AnimationEndEventCallback()
        {
            // FIXME - TODO
        }
        
        public override bool IsFinished()
        {
            // FIXME - DEBUG
            return true;
        }
    }
}