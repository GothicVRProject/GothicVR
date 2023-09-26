using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class UseItemToState : AbstractAnimationAction
    {
        
        private const string animationStartScheme = "T_{0}_STAND_2_S{1}" ;
        private const string animationEndScheme = "T_{0}_S{1}_2_STAND" ;
        
        public UseItemToState(Ai.Action action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            // Nothing in hand && new item shall be put into hand
            if (aiProps.itemAnimationState < 0 && action.i0 >= 0)
                StartItemStateAnimation(action.i0);
            // Something in hand && item shall be removed
            else if (aiProps.itemAnimationState >= 0 && action.i0 < 0)
                EndItemStateAnimation(aiProps.itemAnimationState);

            aiProps.itemAnimationState = action.i0;
        }

        private void StartItemStateAnimation(int itemAnimationState)
        {
            var mdh = AssetCache.I.TryGetMdh(props.overlayMdhName);
            var item = AssetCache.I.TryGetItemData(action.ui0);

            // e.g. T_POTION_STAND_2_S0
            var animationName = string.Format(animationStartScheme, item.schemeName, itemAnimationState);
            
            AnimationCreator.I.PlayAnimation(props.baseMdsName, animationName, mdh, npcGo);
        }

        private void EndItemStateAnimation(int itemAnimationState)
        {
            var mdh = AssetCache.I.TryGetMdh(props.overlayMdhName);
            var item = AssetCache.I.TryGetItemData(action.ui0);

            // e.g. T_POTION_S0_2_STAND
            var animationName = string.Format(animationEndScheme, item.schemeName, itemAnimationState);
            
            AnimationCreator.I.PlayAnimation(props.baseMdsName, animationName, mdh, npcGo);
        }

        private void SpawnItem()
        {
            var slotGo = NpcMeshCreator.I.GetSlot(npcGo, NpcMeshCreator.ItemSlot.LeftHand);
            VobCreator.I.CreateItem(aiProps.currentItem, slotGo);
        }

        private void DestroyItem()
        {
            if (aiProps.currentItem == 0)
                return;

            var slotGo = NpcMeshCreator.I.GetSlot(npcGo, NpcMeshCreator.ItemSlot.LeftHand);
            var item = slotGo!.transform.GetChild(0);
            Object.Destroy(item.gameObject);
        }
    }
}