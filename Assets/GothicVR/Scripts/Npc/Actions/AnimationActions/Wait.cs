using System;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class Wait : AnimationAction

    {
        private float waitSeconds;

        public Wait(Ai.Action action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            waitSeconds = action.f0;
        }

        public override bool IsFinished()
        {
            waitSeconds -= Time.deltaTime;

            return waitSeconds <= 0f;
        }

        public override void AnimationEventEndCallback()
        {
            throw new NotImplementedException("This method is not needed and shouldn't be called.");
        }
    }
}