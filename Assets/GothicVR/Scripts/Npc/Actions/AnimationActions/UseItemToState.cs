using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes;
using GVR.Extensions;
using PxCs.Data.Event;
using PxCs.Interface;
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
            {
                StartItemStateAnimation(action.i0);
                aiProps.hasItemEquipped = true;
            }
            // Something in hand && item shall be removed
            else if (aiProps.itemAnimationState >= 0 && action.i0 < 0)
            {
                EndItemStateAnimation(aiProps.itemAnimationState);
                aiProps.hasItemEquipped = false;
            }

            aiProps.itemAnimationState = action.i0;
            aiProps.currentItem = action.ui0;
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

        public override void AnimationEventCallback(PxEventTagData data)
        {
            switch (data.type)
            {
                case PxModelScript.PxEventTagType.insert_item:
                    InsertItem(data.slot);
                    break;
                case PxModelScript.PxEventTagType.destroy_item:
                    DestroyItem();
                    break;
                case PxModelScript.PxEventTagType.inventory_torch:
                    Debug.Log("PxEventTagType.inventory_torch: I assume this means: if torch is in inventory, then put it out. But not really sure. Need a NPC with real usage of it to predict right.");
                    break;
                default:
                    Debug.LogWarning($"PxEventTagData.type {data.type} not yet supported.");
                    break;
            }
        }
        
        private void InsertItem(string slot)
        {
            var slotGo = npcGo.FindChildRecursively(slot);
            VobCreator.I.CreateItem(aiProps.currentItem, slotGo);

            aiProps.usedItemSlot = slot;
        }

        private void DestroyItem()
        {
            // FIXME - This is called to late. Feels like the animation for T_*_S0_2_Stand is glued with another.
            // FIXME - So that frame y is more like frame x+y, where y is the frames count from previous call.
            var slotGo = npcGo.FindChildRecursively(aiProps.usedItemSlot);
            var item = slotGo!.transform.GetChild(0);
            Object.Destroy(item.gameObject);
        }
    }
}