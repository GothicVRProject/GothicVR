using GVR.Caches;
using GVR.Creator;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class UseItemToState : AbstractAnimationAction
    {
        
        private const string animationStartScheme = "T_{0}_STAND_2_S{1}" ;
        private const string animationEndScheme = "T_{0}_S{1}_2_STAND" ;
        
        public UseItemToState(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            // Nothing in hand && new item shall be put into hand
            if (Props.itemAnimationState < 0 && Action.Int1 >= 0)
            {
                StartItemStateAnimation(Action.Int1);
                Props.hasItemEquipped = true;
            }
            // Something in hand && item shall be removed
            else if (Props.itemAnimationState >= 0 && Action.Int1 < 0)
            {
                EndItemStateAnimation(Props.itemAnimationState);
                Props.hasItemEquipped = false;
            }

            Props.itemAnimationState = Action.Int1;
            Props.currentItem = Action.Int0;
        }

        private void StartItemStateAnimation(int itemAnimationState)
        {
            var item = AssetCache.TryGetItemData(Action.Int0);

            // e.g. T_POTION_STAND_2_S0
            var animationName = string.Format(animationStartScheme, item.SchemeName, itemAnimationState);
            
            AnimationCreator.PlayAnimation(Props.baseMdsName, animationName, Props.overlayMdhName, NpcGo);
        }

        private void EndItemStateAnimation(int itemAnimationState)
        {
            var item = AssetCache.TryGetItemData(Action.Int0);

            // e.g. T_POTION_S0_2_STAND
            var animationName = string.Format(animationEndScheme, item.SchemeName, itemAnimationState);
            
            AnimationCreator.PlayAnimation(Props.baseMdsName, animationName, Props.overlayMdhName, NpcGo);
        }
    }
}
