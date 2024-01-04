using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Data;
using GVR.Globals;
using GVR.Npc.Actions;
using GVR.Npc.Actions.AnimationActions;
using GVR.Properties;
using UnityEngine;
using ZenKit.Daedalus;

namespace GVR.GothicVR.Scripts.Manager
{
    public static class DialogHelper
    {
        public static void StartDialog(NpcProperties properties)
        {
            GameData.Dialogs.IsInDialog = true;

            // We are already inside a sub-dialog
            if (GameData.Dialogs.CurrentDialog.Options.Any())
            {
                ControllerManager.I.FillDialog(GameData.Dialogs.CurrentDialog.Options);
                ControllerManager.I.ShowDialog();
            }
            // There is at least one important entry, the NPC wants to talk to the hero about.
            else if (TryGetImportant(properties.Dialogs, out var infoInstance))
            {
                GameData.Dialogs.CurrentDialog.Instance = infoInstance;
                ControllerManager.I.HideDialog();
                GameData.GothicVm.Call(infoInstance.Information);
            }
            else
            {
                var selectableDialogs = new List<InfoInstance>();

                foreach (var dialog in properties.Dialogs)
                {
                    if (dialog.Condition == 0)
                        continue;

                    if (GameData.GothicVm.Call<int>(dialog.Condition) == 1)
                        selectableDialogs.Add(dialog);
                }

                selectableDialogs = selectableDialogs.OrderBy(d => d.Nr).ToList();
                ControllerManager.I.FillDialog(selectableDialogs);
                ControllerManager.I.ShowDialog();
            }

            // We always want to have a method to get the dialog menu back once all dialog lines are talked.
            properties.AnimationQueue.Enqueue(new StartProcessInfos(
                new(AnimationAction.Type.UnityStartProcessInfos),
                properties.gameObject));
        }

        /// <summary>
        /// If something is important, then call it automatically.
        /// </summary>
        private static bool TryGetImportant(List<InfoInstance> dialogs, out InfoInstance item)
        {
            item = dialogs.FirstOrDefault(
                dialog =>
                    dialog.Important == 1
                    && (dialog.Condition == 0 || GameData.GothicVm.Call<int>(dialog.Condition) == 1));

            return item != null;
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

        /// <summary>
        /// We update the Unity cached/created elements only.
        /// </summary>
        public static void ExtInfoClearChoices(int info)
        {
            GameData.Dialogs.CurrentDialog.Instance = null;
            GameData.Dialogs.CurrentDialog.Options.Clear();
        }

        public static void ExtInfoAddChoice(int info, string text, int function)
        {
            // Check if we need to change current instance as it wasn't cleared before.
            var oldInstance = GameData.Dialogs.CurrentDialog.Instance;

            // First entry of current dialog to add
            if (oldInstance == null)
            {
                GameData.Dialogs.CurrentDialog.Instance = GameData.Dialogs.Instances.First(i => i.Index == info);
                GameData.Dialogs.CurrentDialog.Options.Clear();
            }
            else if (oldInstance.Index != info)
            {
                throw new Exception($"Previous Dialog wasn't cleared. Gothic bug? " +
                               $"Desc={oldInstance.Description}, Npc={oldInstance.Npc}, Info= {oldInstance.Information}");
            }

            // Add new entry
            GameData.Dialogs.CurrentDialog.Options.Add(new DialogOption
            {
                Text = text,
                Function = function
            });
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
            ControllerManager.I.HideDialog();
            GameData.GothicVm.Call(dialogId);
        }

        public static void StopDialog()
        {
            GameData.Dialogs.CurrentDialog.Instance = null;
            GameData.Dialogs.CurrentDialog.Options.Clear();
            GameData.Dialogs.IsInDialog = false;

            ControllerManager.I.HideDialog();
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
