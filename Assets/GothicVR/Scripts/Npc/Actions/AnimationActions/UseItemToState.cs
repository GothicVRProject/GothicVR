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
        
        public UseItemToState(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            // Nothing in hand && new item shall be put into hand
            if (Props.itemAnimationState < 0 && Action.Int0 >= 0)
            {
                StartItemStateAnimation(Action.Int0);
                Props.hasItemEquipped = true;
            }
            // Something in hand && item shall be removed
            else if (Props.itemAnimationState >= 0 && Action.Int0 < 0)
            {
                EndItemStateAnimation(Props.itemAnimationState);
                Props.hasItemEquipped = false;
            }

            Props.itemAnimationState = Action.Int0;
            Props.currentItem = Action.Uint0;
        }

        private void StartItemStateAnimation(int itemAnimationState)
        {
            var mdh = AssetCache.TryGetMdh(Props.overlayMdhName);
            var item = AssetCache.TryGetItemData(Action.Uint0);

            // e.g. T_POTION_STAND_2_S0
            var animationName = string.Format(animationStartScheme, item.SchemeName, itemAnimationState);
            
            AnimationCreator.PlayAnimation(Props.baseMdsName, animationName, mdh, NpcGo);
        }

        private void EndItemStateAnimation(int itemAnimationState)
        {
            var mdh = AssetCache.TryGetMdh(Props.overlayMdhName);
            var item = AssetCache.TryGetItemData(Action.Uint0);

            // e.g. T_POTION_S0_2_STAND
            var animationName = string.Format(animationEndScheme, item.SchemeName, itemAnimationState);
            
            AnimationCreator.PlayAnimation(Props.baseMdsName, animationName, mdh, NpcGo);
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
            var slotGo = NpcGo.FindChildRecursively(slot);
            VobCreator.CreateItem(Props.currentItem, slotGo);

            Props.usedItemSlot = slot;
        }

        private void DestroyItem()
        {
            // FIXME - This is called to late. Feels like the animation for T_*_S0_2_Stand is glued with another.
            // FIXME - So that frame y is more like frame x+y, where y is the frames count from previous call.
            var slotGo = NpcGo.FindChildRecursively(Props.usedItemSlot);
            var item = slotGo!.transform.GetChild(0);
            Object.Destroy(item.gameObject);
        }
    }
}
