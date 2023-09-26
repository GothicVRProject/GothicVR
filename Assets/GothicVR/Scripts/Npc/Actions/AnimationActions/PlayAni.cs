using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class PlayAni : AbstractAnimationAction
    {
        public PlayAni(Ai.Action action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            AddItemIfAny();
            
            var mdh = AssetCache.I.TryGetMdh(props.overlayMdhName);
            // FIXME - We need to handle both mds and mdh options! (base vs overlay)
            AnimationCreator.I.PlayAnimation(props.baseMdsName, action.str0, mdh, npcGo);
        }

        private void AddItemIfAny()
        {
            var currentItem = aiProps.currentItem;
            
            // FIXME - Currently not handled if NPC has multiple items in inventory and all should be consumed one-after-another.
            var currentItemExpectedInventoryCount = aiProps.currentItemExpectedInventoryCount;

            if (currentItem == 0)
                return;

            var slotGo = NpcMeshCreator.I.GetSlot(npcGo, NpcMeshCreator.ItemSlot.LeftHand);
            VobCreator.I.CreateItem(currentItem, slotGo);
        }

        private void RemoveItemIfAny()
        {
            if (aiProps.currentItem == 0)
                return;

            var slotGo = NpcMeshCreator.I.GetSlot(npcGo, NpcMeshCreator.ItemSlot.LeftHand);
            var item = slotGo!.transform.GetChild(0);
            Object.Destroy(item.gameObject);

            aiProps.currentItem = 0;
        }

        public override bool IsFinished()
        {
            if (base.IsFinished())
            {
                RemoveItemIfAny();   

                return true;
            }

            return false;
        }
    }
}