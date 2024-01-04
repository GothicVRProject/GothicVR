using GVR.Extensions;
using GVR.Globals;
using GVR.GothicVR.Scripts.Manager;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class StartProcessInfos: AbstractAnimationAction
    {
        public StartProcessInfos(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            // AI_STopProcessInfos was called before this Action
            if (!GameData.Dialogs.IsInDialog)
                return;

            if (GameData.Dialogs.CurrentDialog.Instance.Permanent == 0 && GameData.Dialogs.CurrentDialog.Options.IsEmpty())
            {
                // FIXME - Remove dialog option from NPC as it was called, is not permanent, and no further options are available.
            }

            DialogHelper.StartDialog(Props);

            IsFinishedFlag = true;
        }
    }
}
