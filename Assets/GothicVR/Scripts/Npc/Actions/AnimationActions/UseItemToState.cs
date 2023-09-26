using System;
using GVR.Creator;
using GVR.Creator.Meshes;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class UseItemToState : AnimationAction
    {
        public UseItemToState(Ai.Action action, GameObject go) : base(action, go)
        { }
    
        public override void Start()
        {
            // FIXME - Still unclear if we need to create something here or just set properties until animation kicks in
            // var slotGo = NpcMeshCreator.I.GetSlot(npcGo, NpcMeshCreator.ItemSlot.LeftHand);
            // VobCreator.I.CreateItem(action.ui0, slotGo);
        }

        public override void AnimationEventEndCallback()
        {
            // FIXME - remove item?
        }
        
        public override bool IsFinished()
        {
            // FIXME - DEBUG
            return true;
        }

    }
}