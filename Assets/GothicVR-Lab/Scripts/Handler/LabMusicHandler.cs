using System;
using System.Linq;
using GVR.Caches;
using GVR.Debugging;
using GVR.Globals;
using GVR.Manager;
using TMPro;
using UnityEngine;

namespace GVR.Lab.Handler
{
    public class LabMusicHandler : MonoBehaviour, ILabHandler
    {
        public TMP_Dropdown fileSelector;


        public void Bootstrap()
        {
            var prototype = GameData.MusicVm.GetSymbolByName("C_MUSICTHEME_DEF");

            var musicInstances = GameData.MusicVm.Symbols
                .Where(s => s.Parent == prototype.Index)
                .Select(s => AssetCache.TryGetMusic(s.Name))
                .GroupBy(instance => instance.File, StringComparer.InvariantCultureIgnoreCase)
                .Select(group => group.First())
                .OrderBy(instance => instance.File)
                .ToList();

            fileSelector.options = musicInstances.Select(i => new TMP_Dropdown.OptionData(i.File)).ToList();
        }

        public void MusicPlayClick()
        {
            if (!FeatureFlags.I.enableMusic)
                Debug.LogError($"Music is deactivated inside ${nameof(FeatureFlags.enableMusic)}");

            MusicManager.Play(fileSelector.options[fileSelector.value].text);
        }
    }
}
