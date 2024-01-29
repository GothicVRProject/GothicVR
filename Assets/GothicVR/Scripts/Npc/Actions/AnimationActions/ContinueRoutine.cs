using GVR.Npc.Routines;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class ContinueRoutine : AbstractAnimationAction
    {
        public ContinueRoutine(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        {
        }

        public override void Start()
        {
            var ai = Props.GetComponent<AiHandler>();

            ai.ClearState(false);
            
            var routine = Props.GetComponent<Routine>().CurrentRoutine;
            
            ai.StartRoutine(routine.action, routine.waypoint);
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