using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
                var text = options[i].FindChildRecursively("Text").GetComponent<TMP_Text>();
                text.text = dialog.Description;
            }
        }
    }
}
