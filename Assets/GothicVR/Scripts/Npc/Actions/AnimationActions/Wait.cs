using System;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class Wait : AbstractAnimationAction

    {
        private float waitSeconds;

        public Wait(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            waitSeconds = Action.Float0;
        }

        public override bool IsFinished()
        {
            waitSeconds -= Time.deltaTime;

            return waitSeconds <= 0f;
        }

        public override void AnimationEndEventCallback()
        {
            throw new NotImplementedException("This method is not needed and shouldn't be called.");
        }
    }
}