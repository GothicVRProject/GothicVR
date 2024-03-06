using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Debugging;
using GVR.Globals;
using GVR.Manager;
using TMPro;
using UnityEngine;
using ZenKit.Daedalus;

namespace GVR.Lab.Handler
{
    public class LabMusicHandler : MonoBehaviour, ILabHandler
    {
        public TMP_Dropdown fileSelector;
        public TMP_Dropdown themeSelector;

        private List<MusicThemeInstance> _musicInstances;

        public void Bootstrap()
        {
            var prototype = GameData.MusicVm.GetSymbolByName("C_MUSICTHEME_DEF");

            _musicInstances = GameData.MusicVm.Symbols
                .Where(s => s.Parent == prototype.Index)
                .Select(s => AssetCache.TryGetMusic(s.Name))
                .GroupBy(instance => instance.File, StringComparer.InvariantCultureIgnoreCase)
                .Select(group => group.First())
                .OrderBy(instance => instance.File)
                .ToList();

            fileSelector.options = _musicInstances.Select(i => new TMP_Dropdown.OptionData(i.File)).ToList();
            // themeSelector.options = musicMapping.First().Value.Select(i => new TMP_Dropdown.OptionData(i)).ToList();
        }

        public void MusicPlayClick()
        {
            MusicManager.I.SetEnabled(true);
            FeatureFlags.I.enableMusic = true;
            FeatureFlags.I.showMusicLogs = true;

            var item = _musicInstances[fileSelector.value];

            MusicManager.I.SetMusic(item);
        }
    }
}
