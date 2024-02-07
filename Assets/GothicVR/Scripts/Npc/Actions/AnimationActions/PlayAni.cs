using GVR.Creator;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class PlayAni : AbstractAnimationAction
    {
        public PlayAni(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        {
        }

        public override void Start()
        {
            AnimationCreator.PlayAnimation(Props.mdsNames, Action.String0, NpcGo);
        }
    }
}