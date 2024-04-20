using GVR.Npc.Actions;
using GVR.Npc.Actions.AnimationActions;
using UnityEngine;

namespace GVR.Lab.AnimationActionMocks
{
    /// <summary>
    /// Gothic has two types of External calls:
    /// 1. Execute immediately when parsed (e.g. CreateInvItem())
    /// 2. Execute as animation in order (e.g. AI_PlayAni())
    ///
    /// We need to create a mechanism to handle immediate actions as if they would've been called by Daedalus.
    /// These mock classes help implementing it where needed in the Lab.
    /// </summary>
    public abstract class AbstractLabAnimationAction : AbstractAnimationAction
    {
        protected AbstractLabAnimationAction(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        {
        }

        public override void Start()
        {
            IsFinishedFlag = true;
        }

    }
}
