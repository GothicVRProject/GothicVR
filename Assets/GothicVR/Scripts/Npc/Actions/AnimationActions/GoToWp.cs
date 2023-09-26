using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class GoToWp : AbstractAnimationAction
    {
        public GoToWp(Ai.Action action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            // FIXME - TODO
        }

        public override void AnimationEventEndCallback()
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