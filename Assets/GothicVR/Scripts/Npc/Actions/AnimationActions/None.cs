using System;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class None : AnimationAction
    {
        public None(Ai.Action action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            // NOP
        }

        public override void AnimationEventEndCallback()
        {
            throw new NotImplementedException("This method is not needed and shouldn't be called.");
        }

        public override bool IsFinished()
        {
            return true;
        }
    }
}