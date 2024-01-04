using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Creator.Sounds;
using GVR.Extensions;
using GVR.Globals;
using GVR.Lab.Handler;
using GVR.Npc.Actions;
using GVR.Npc.Actions.AnimationActions;
using GVR.Properties;
using JetBrains.Annotations;
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
            var dialogCanvas = GameObject.Find("DialogCanvas");
            var options = Enumerable.Range(0, dialogCanvas.transform.childCount)
                .Select(i => dialogCanvas.transform.GetChild(i).gameObject).ToList();

            for (var i = 0; i < dialogs.Count; i++)
            {
                var dialog = dialogs[i];
                var option = options[i];

                var text = option.FindChildRecursively("Text").GetComponent<TMP_Text>();
                text.text = dialog.Description;

                var dialogInformation = dialog.Information;
                option.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => GameObject.Find("NpcTool").GetComponent<NpcHandler>().DialogClick(dialogInformation));
            }
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
