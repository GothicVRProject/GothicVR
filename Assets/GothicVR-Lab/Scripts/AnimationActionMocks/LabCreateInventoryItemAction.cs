using GVR.Globals;
using GVR.Npc.Actions;
using GVR.Vm;
using UnityEngine;
using ZenKit.Daedalus;

namespace GVR.GothicVR_Lab.Scripts.AnimationActionMocks
{
    public class LabCreateInventoryItemAction : LabImmediateAnimationAction
    {
        public LabCreateInventoryItemAction(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        {
        }

        public override void Start()
        {
            var itemSymbol = GameData.GothicVm.GetSymbolByName(Action.String0);
            // GameData.GothicVm.AllocInstance<ItemInstance>(itemSymbol!);
            // GameData.GothicVm.InitInstance<ItemInstance>(itemSymbol!);

            VmGothicExternals.CreateInvItem(Props.npcInstance, itemSymbol!.Index);


            base.Start();
        }
    }
}
