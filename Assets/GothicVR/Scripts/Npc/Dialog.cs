using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Globals;
using UnityEngine;
using ZenKit.Daedalus;

namespace GVR.Npc
{
    public class Dialog: BasePlayerBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            Debug.Log("Player collission");

            // TODO Call dialog
            var selectableDialogs = new List<InfoInstance>();

            foreach (var dialog in properties.Dialogs)
            {
                if (dialog.Permanent == 1)
                {
                    selectableDialogs.Add(dialog);
                    continue;
                }

                var result = GameData.GothicVm.Call<int>(dialog.Condition);
                if (result == 1)
                {
                    selectableDialogs.Add(dialog);
                }
            }
            
            selectableDialogs = selectableDialogs.OrderBy(d => d.Nr).ToList();
            
            // Next:
            // 1. Print entries
            // 2. Execute entry with audio and subtitles
        }
    }
}
