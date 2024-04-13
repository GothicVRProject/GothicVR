using GVR.Caches;
using GVR.Creator;
using GVR.GothicVR.Scripts.Manager;
using UnityEngine;
using ZenKit.Daedalus;

namespace GVR.Npc.Actions.AnimationActions
{
    public class UseItemToState : AbstractAnimationAction
    {
        
        private const string animationScheme = "T_{0}_{1}_2_{2}";
        private const string LoopAnimationScheme = "S_{0}_S{1}";

        private int itemToUse => Action.Int0;
        private int desiredState => Action.Int1;


        public UseItemToState(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            PhysicsHelper.DisablePhysicsForNpc(Props);
            PlayTransitionAnimation();
        }

        private void PlayTransitionAnimation()
        {
            int oldItemAnimationState = Props.itemAnimationState;
            int newItemAnimationState;
            if (desiredState > Props.itemAnimationState)
            {
                Props.hasItemEquipped = true;
                Props.currentItem = itemToUse;
                newItemAnimationState = ++Props.itemAnimationState;
            }
            else
            {
                Props.hasItemEquipped = false;
                Props.currentItem = -1;
                // e.g. Babe brush doesn't call it automatically. We therefore need to force remove the brush item from hand.
                // AnimationEventCallback(new() { Type = ZenKit.EventType.ItemDestroy });
                newItemAnimationState = --Props.itemAnimationState;
            }

            ItemInstance item = AssetCache.TryGetItemData(itemToUse);
            string oldState = oldItemAnimationState == -1 ? "STAND" : $"S{oldItemAnimationState}";
            string newState = newItemAnimationState == -1 ? "STAND" : $"S{newItemAnimationState}";

            // e.g. T_POTION_STAND_2_S0
            var animationName = string.Format(animationScheme, item.SchemeName, oldState, newState);

            bool animationFound = AnimationCreator.PlayAnimation(Props.mdsNames, animationName, NpcGo);

            // e.g. BABE-T_BRUSH_S1_2_S0.man doesn't exist, but we can skip and use next one (S0_2_Stand)
            if (!animationFound)
            {
                // Go on with next animation.
                PlayTransitionAnimation();
            }
        }

        private void PlayLoopAnimation()
        {
            ItemInstance item = AssetCache.TryGetItemData(itemToUse);
            string animName = string.Format(LoopAnimationScheme, item.SchemeName, desiredState);
            AnimationCreator.PlayAnimation(Props.mdsNames, animName, NpcGo, true);
        }

        public override void AnimationEndEventCallback()
        {
            if (Props.itemAnimationState == desiredState)
            {
                PhysicsHelper.EnablePhysicsForNpc(Props);

                PlayLoopAnimation();

                IsFinishedFlag = true;
                return;
            }

            PlayTransitionAnimation();

            IsFinishedFlag = false;
        }
    }
}
