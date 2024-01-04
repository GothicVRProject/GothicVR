using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    /// <summary>
    /// Close dialog menu immediately.
    /// </summary>
    public class StopProcessInfos : AbstractAnimationAction
    {
        public StopProcessInfos(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            ControllerManager.I.HideDialog();
        }

        public override bool IsFinished()
        {
            return true;
        }
    }
}
