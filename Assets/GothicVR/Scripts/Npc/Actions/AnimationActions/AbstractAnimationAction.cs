using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public abstract class AbstractAnimationAction
    {
        protected readonly Ai.Action action;
        protected readonly GameObject npcGo;
        protected readonly Properties props;

        protected bool animationEndCallbackDone;
        
        public AbstractAnimationAction(Ai.Action action, GameObject npcGo)
        {
            this.action = action;
            this.npcGo = npcGo;
            this.props = npcGo.GetComponent<Properties>();
        }
            
        public abstract void Start();

        /// <summary>
        /// Most of our animations are fine if we just set this flag and return it via IsFinished()
        /// </summary>
        public virtual bool IsFinished()
        {
            return animationEndCallbackDone;
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