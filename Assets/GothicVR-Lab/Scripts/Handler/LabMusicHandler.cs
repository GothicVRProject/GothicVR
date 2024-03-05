using System.Collections.Generic;
using System.Linq;
using GVR.Manager;
using TMPro;
using UnityEngine;

namespace GVR.Lab.Handler
{
    public class LabMusicHandler : MonoBehaviour, ILabHandler
    {
        public TMP_Dropdown fileSelector;
        public TMP_Dropdown themeSelector;

        private Dictionary<string, List<string>> musicMapping = new()
        {
            { "ow_day_std.sgt", new(){ "DEF_DAY_STD" } },
            { "ban_day_std.sgt", new(){ "STA_DAY_STD" } },
            { "oc_day_std.sgt", new(){ "" } },
        };

        public void Bootstrap()
        {
            fileSelector.options = musicMapping.Select(dict => new TMP_Dropdown.OptionData(dict.Key)).ToList();
            themeSelector.options = musicMapping.First().Value.Select(i => new TMP_Dropdown.OptionData(i)).ToList();
        }

        public void MusicPlayClick()
        {
            MusicManager.I.SetMusic(fileSelector.itemText.text, themeSelector.itemText.text);
        }
    }
}
