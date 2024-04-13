using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Data;
using GVR.Globals;
using GVR.Npc;
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
                ControllerManager.I.FillDialog(properties.npcInstance.Index, GameData.Dialogs.CurrentDialog.Options);
                ControllerManager.I.ShowDialog();
            }
            // There is at least one important entry, the NPC wants to talk to the hero about.
            else if (TryGetImportant(properties.Dialogs, out var infoInstance))
            {
                properties.go.GetComponent<AiHandler>().ClearState(true);

                GameData.Dialogs.CurrentDialog.Instance = infoInstance;
                
                CallInformation(properties.npcInstance.Index, infoInstance.Information, true);
            }
            else
            {
                properties.go.GetComponent<AiHandler>().ClearState(false);
                var selectableDialogs = new List<InfoInstance>();

                foreach (var dialog in properties.Dialogs)
                {
                    if (dialog.Condition == 0)
                        continue;

                    if (GameData.GothicVm.Call<int>(dialog.Condition) == 1)
                        selectableDialogs.Add(dialog);
                }

                selectableDialogs = selectableDialogs.OrderBy(d => d.Nr).ToList();
                ControllerManager.I.FillDialog(properties.npcInstance.Index, selectableDialogs);
                ControllerManager.I.ShowDialog();
            }
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
            // Always the NPC we're talking to!
            var npcProps = GetProperties(isHero ? target : self);
            var speakerId = self.Id;

            npcProps.AnimationQueue.Enqueue(new Output(
                new(int0: speakerId, string0: outputName),
                npcProps.go));
        }
        
        /// <summary>
        /// SVM (Standard Voice Module) dialogs are only for NPCs between each other. Not related to Hero dialogs.
        /// </summary>
        public static void ExtAiOutputSvm(NpcInstance npc, NpcInstance target, string svmName)
        {
            var props = GetProperties(npc);
            
            if (target != null)
                Debug.LogError($"Ai_OutputSvm() - Handling with target not yet implemented!");

            props.AnimationQueue.Enqueue(new OutputSvm(
                new(int0: props.npcInstance.Id, string0: svmName),
                props.go));
        }

        /// <summary>
        /// We update the Unity cached/created elements only.
        /// </summary>
        public static void ExtInfoClearChoices(int info)
        {
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

            props.AnimationQueue.Enqueue(new StopProcessInfos(new(), props.go));
        }

        public static void SelectionClicked(int npcInstanceIndex, int dialogId, bool isMainDialog)
        {
            CallInformation(npcInstanceIndex, dialogId, isMainDialog);
        }

        public static void StopDialog()
        {
            GameData.Dialogs.CurrentDialog.Instance = null;
            GameData.Dialogs.CurrentDialog.Options.Clear();
            GameData.Dialogs.IsInDialog = false;

            ControllerManager.I.HideDialog();
        }

        private static void CallInformation(int npcInstanceIndex, int information, bool isMainDialog)
        {
            var npcProperties = LookupCache.NpcCache[npcInstanceIndex];

            // If a C_Info is clicked, then set a new CurrentInstance.
            if (isMainDialog)
                GameData.Dialogs.CurrentDialog.Instance = npcProperties.Dialogs
                    .First(d => d.Information == information);

            ControllerManager.I.HideDialog();

            // We always need to set "self" before executing any Daedalus function.
            GameData.GothicVm.GlobalSelf = npcProperties.npcInstance;
            GameData.GothicVm.GlobalOther = GameData.GothicVm.GlobalHero;
            GameData.GothicVm.Call(information);

            // We always want to have a method to get the dialog menu back once all dialog lines are talked.
            npcProperties.AnimationQueue.Enqueue(new StartProcessInfos(
                new(int0: information),
                npcProperties.go));
        }

        private static GameObject GetNpc(NpcInstance npc)
        {
            return GetProperties(npc).go;
        }

        private static NpcProperties GetProperties(NpcInstance npc)
        {
            return LookupCache.NpcCache[npc.Index];
        }
    }
}
