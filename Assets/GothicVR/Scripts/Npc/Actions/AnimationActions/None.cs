using System;
using GVR.Data.ZkEvents;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class None : AbstractAnimationAction
    {
        public None(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            // NOP
        }

        public override void AnimationEndEventCallback(SerializableEventEndSignal eventData)
        {
            throw new NotImplementedException("This method is not needed and shouldn't be called.");
        }

        public override bool IsFinished()
        {
            return true;
        }
    }
}
