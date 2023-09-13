using System.Collections.Generic;
using TMPro;
using UnityEngine;
using GVR.Manager;
using GVR.Util;

namespace GVR.Player.Menu
{
    public class WorldSelectorDropdownController : SingletonBehaviour<WorldSelectorDropdownController>
    {
        private List<string> waypoints = new();
        [SerializeField] private TMP_Dropdown waypointDropdown;

        private void Start()
        {
            SetWaypointDropdown();
        }

        void SetWaypointDropdown()
        {

            waypoints = new()
            {
                "START",
                "ENTRANCE_SURFACE_OLDMINE",
                "ENTRANCE_SURFACE_ORCGRAVEYARD",
                "ENTRANCE_SURFACE_ORCTEMPLE",
                "ENTRANCE_FREEMINECAMP_FREEMINE",
                "OCC_CHAPEL_UPSTAIRS",
                "NC_KDW_CAVE_CENTER",
                "PSI_TEMPLE_COURT_GURU",
                "DT_E2_06",
            };

            foreach (var item in waypoints)
            {
                waypointDropdown.options.Add(new TMP_Dropdown.OptionData() { text = item });
            }

            WaypointSetDropdownValues();
            waypointDropdown.onValueChanged.AddListener(WaypointDropdownItemSelected);
            waypointDropdown.value = waypoints.IndexOf(ConstantsManager.I.selectedWaypoint);
        }

        public void WaypointSetDropdownValues()
        {


        }

        void WaypointDropdownItemSelected(int value)
        {
            var item = waypointDropdown.options[value].text;

            Debug.Log($"Waypoint DropdownItemSelected: {item} with value: {value}");

            ConstantsManager.I.selectedWaypoint = waypoints[value];
        }
    }

}