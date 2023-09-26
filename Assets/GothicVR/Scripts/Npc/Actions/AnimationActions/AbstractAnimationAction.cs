using System;
using PxCs.Data.Animation;
using PxCs.Data.Event;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public abstract class AbstractAnimationAction
    {
        protected readonly Ai.Action action;
        protected readonly GameObject npcGo;
        protected readonly Properties props;
        protected readonly Ai aiProps;

        protected bool animationEndCallbackDone;

        public AbstractAnimationAction(Ai.Action action, GameObject npcGo)
        {
            this.action = action;
            this.npcGo = npcGo;
            this.props = npcGo.GetComponent<Properties>();
            this.aiProps = npcGo.GetComponent<Ai>();
        }
            
        public abstract void Start();

        /// <summary>
        /// Most of our animations are fine if we just set this flag and return it via IsFinished()
        /// </summary>
        public virtual bool IsFinished()
        {
            return animationEndCallbackDone;
        }

        public virtual void AnimationEventCallback(PxEventTagData data)
        {
            Debug.LogError($"Animation for {action.ActionType} is not yet implemented.");
        }
        
        /// <summary>
        /// Most of our animations are fine if we just set this flag and return it via IsFinished()
        /// </summary>
        public virtual void AnimationEventEndCallback()
        {
            animationEndCallbackDone = true;
        }
    }
}