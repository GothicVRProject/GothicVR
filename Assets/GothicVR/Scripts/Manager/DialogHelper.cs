using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Globals;
using GVR.Lab.Handler;
using GVR.Npc.Actions;
using GVR.Npc.Actions.AnimationActions;
using GVR.Properties;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZenKit.Daedalus;

namespace GVR.GothicVR.Scripts.Manager
{
    public static class DialogHelper
    {
        public static void DrawDialogs(List<InfoInstance> dialogs)
        {
            ControllerManager.I.FillDialog(dialogs);
            ControllerManager.I.ShowDialog();
        }

        public static void ExtAiOutput(NpcInstance self, NpcInstance target, string outputName)
        {
            var isHero = self.Id == 0;
            var props = GetProperties(isHero ? target : self);
            var npcId = isHero ? target.Id : self.Id;

            props.AnimationQueue.Enqueue(new Output(
                new(AnimationAction.Type.AIOutput, int0: npcId, string0: outputName),
                props.gameObject));
        }

        public static void ExtAiStopProcessInfos(NpcInstance npc)
        {
            var props = GetProperties(npc);

            props.AnimationQueue.Enqueue(new StopProcessInfos(
                new(AnimationAction.Type.AIStopProcessInfo),
                props.gameObject));
        }

        public static void SelectionClicked(int dialogId)
        {
            GameData.GothicVm.Call(dialogId);
        }

        private static GameObject GetNpc(NpcInstance npc)
        {
            return GetProperties(npc).gameObject;
        }

        private static NpcProperties GetProperties(NpcInstance npc)
        {
            return LookupCache.NpcCache[npc.Index];
        }
    }
}
