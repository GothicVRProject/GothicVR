using GVR.Globals;
using GVR.Npc.Actions;
using GVR.Npc.Actions.AnimationActions;
using UnityEngine;

namespace GVR.Lab.AnimationActionMocks
{
    public class LabUseItemToState : UseItemToState
    {
        public LabUseItemToState(AnimationAction action, GameObject npcGo): base(CalculateItemIndex(action), npcGo)
        {
        }

        private static AnimationAction CalculateItemIndex(AnimationAction action)
        {
            var item = GameData.GothicVm.GetSymbolByName(action.String0);

            return new(
                int0: item!.Index,
                int1: action.Int1
            );

        }
    }
}
