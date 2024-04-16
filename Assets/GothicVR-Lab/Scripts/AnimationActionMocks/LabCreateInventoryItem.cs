using GVR.Globals;
using GVR.Npc.Actions;
using GVR.Vm;
using UnityEngine;

namespace GVR.Lab.AnimationActionMocks
{
    public class LabCreateInventoryItem : AbstractLabAnimationAction
    {
        public LabCreateInventoryItem(AnimationAction action, GameObject npcGo) : base(action, npcGo)
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
