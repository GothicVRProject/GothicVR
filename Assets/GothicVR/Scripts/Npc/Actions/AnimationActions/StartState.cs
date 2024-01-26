using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class StartState : AbstractAnimationAction
    {
        public StartState(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            var ai = Props.GetComponent<AiHandler>();

            ai.ClearState(Action.Bool0);

            Props.isStateTimeActive = true;
            Props.stateTime = 0;

            ai.StartRoutine(Action.Int0, Action.String0);
        }

        /// <summary>
        /// This one is actually no animation, but we need to call Start() only.
        /// FIXME - We need to create an additional inheritance below AbstractAnimationAction if we have more like this class.
        /// </summary>
        /// <returns></returns>
        public override bool IsFinished()
        {
            return true;
        }
    }
}
