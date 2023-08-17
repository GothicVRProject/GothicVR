using System.Collections.Generic;
using TMPro;
using UnityEngine;
using GVR.Phoenix.Interface;
using GVR.Manager;
using GVR.Util;

namespace GVR.Player.Menu
{
    public class WorldSelectorDropdownController : SingletonBehaviour<WorldSelectorDropdownController>
    {
        private Dictionary<string, string> worlds = new();
        [SerializeField] private TMP_Dropdown worldDropdown;

        private Dictionary<string, List<string>> waypoints = new();
        [SerializeField] private TMP_Dropdown waypointDropdown;

        private void Start()
        {
            SetWorldDropdown();
            SetWaypointDropdown();

            worldDropdown.value = 0;
        }

        private void SetWorldDropdown()
        {
            worldDropdown.options.Clear();

            worlds.Add("World", "world.zen");
            worlds.Add("Old Mine", "oldmine.zen");
            worlds.Add("Orc Graveyard", "orcgraveyard.zen");
            worlds.Add("Orc Temple", "orctempel.zen");
            worlds.Add("Free Mine", "freemine.zen");

            worldDropdown.itemText.font = GameData.I.EmptyFont;
            worldDropdown.onValueChanged.AddListener(WorldDropdownItemSelected);
            foreach (var item in worlds)
            {
                worldDropdown.options.Add(new TMP_Dropdown.OptionData() { text = item.Key });
            }

        }

        void SetWaypointDropdown()
        {

            List<string> worldWaypoints = new()
        {
            "START",
            "ENTRANCE_SURFACE_OLDMINE",
            "ENTRANCE_SURFACE_ORCGRAVEYARD",
            "ENTRANCE_SURFACE_ORCTEMPLE",
            "ENTRANCE_FREEMINECAMP_FREEMINE"
        };

            List<string> oldMineWaypoints = new()
        {
            "START",
            "FP_ROAM_CRAWLER06_01",
            "FP_ROAM_CRAWLER13_02",
            "FP_GUARD_CAVE_02",
            "FP_ROAM_OM_CRAWLER01_01"
        };

            List<string> orcGraveyardWaypoints = new()
        {
            "START",
        };

            List<string> orcTempleWaypoints = new()
        {
            "START",
            "TPL_PRIEST_01",
            "TPL_PRIEST_03",
            "EVT_TPL_13_SPAWN_FP_ROAM_ROOMRIGHT_02",
            "TPL_NOVIZE_29"
        };

            List<string> freeMineWaypoints = new()
        {
            "START",
            "FP_GUARD_03",
            "FP_GUARD_04",
            "FP_ROAM_CRAWLER_05",
        };

            waypoints.Add("world.zen", worldWaypoints);
            waypoints.Add("oldmine.zen", oldMineWaypoints);
            waypoints.Add("orcgraveyard.zen", orcGraveyardWaypoints);
            waypoints.Add("orctempel.zen", orcTempleWaypoints);
            waypoints.Add("freemine.zen", freeMineWaypoints);

            WaypointSetDropdownValues();

            waypointDropdown.itemText.font = GameData.I.EmptyFont;
            waypointDropdown.onValueChanged.AddListener(WaypointDropdownItemSelected);

        }

        void WorldDropdownItemSelected(int value)
        {
            var item = worldDropdown.options[value].text;
            worldDropdown.itemText.font = GameData.I.EmptyFont;
            ConstantsManager.I.selectedWorld = worlds[item];
            WaypointSetDropdownValues();
        }

        public void WaypointSetDropdownValues()
        {
            waypointDropdown.options.Clear();

            foreach (var item in waypoints[ConstantsManager.I.selectedWorld])
            {
                waypointDropdown.options.Add(new TMP_Dropdown.OptionData() { text = item });
            }
        }

        void WaypointDropdownItemSelected(int value)
        {
            var item = waypointDropdown.options[value].text;

            waypointDropdown.itemText.font = GameData.I.EmptyFont;


            Debug.Log($"Waypoint DropdownItemSelected: {item} with value: {value}");

            ConstantsManager.I.selectedWaypoint = waypoints[ConstantsManager.I.selectedWorld][value];
        }
    }

}