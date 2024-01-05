using System.Collections.Generic;
using System.Linq;
using GVR.Globals;
using GVR.Util;
using TMPro;
using UnityEngine;

namespace GVR.Player.Menu
{
    public class WorldSelectorDropdownController : SingletonBehaviour<WorldSelectorDropdownController>
    {
        private Dictionary<string, string> waypoints = new Dictionary<string, string>();
        [SerializeField] private TMP_Dropdown waypointDropdown;

        private void Start()
        {
            SetWaypointDropdown();
        }

        void SetWaypointDropdown()
        {
            waypoints = new Dictionary<string, string>()
            {
                { "START", "Start" },
                { "ENTRANCE_SURFACE_OLDMINE", "Entrance Old Mine" },
                { "ENTRANCE_FREEMINECAMP_FREEMINE", "Entrance Free Mine" },
                { "ENTRANCE_SURFACE_ORCGRAVEYARD", "Entrance Orc Graveyard" },
                { "ENTRANCE_SURFACE_ORCTEMPLE", "Entrance Orc Temple" },
                { "OCC_CHAPEL_UPSTAIRS", "Old Camp" },
                { "NC_KDW_CAVE_CENTER", "New Camp" },
                { "PSI_TEMPLE_COURT_GURU", "Sect Camp" },
                { "DT_E2_06", "Xardas' Tower" }
            };

            WaypointSetDropdownValues();
            waypointDropdown.onValueChanged.AddListener(WaypointDropdownItemSelected);
            waypointDropdown.value = waypoints.Keys.ToList().IndexOf(Constants.selectedWaypoint);
        }

        public void WaypointSetDropdownValues()
        {
            waypointDropdown.options.Clear();

            foreach (var item in waypoints)
            {
                waypointDropdown.options.Add(new TMP_Dropdown.OptionData() { text = item.Value });
            }
        }

        void WaypointDropdownItemSelected(int value)
        {
            Constants.selectedWaypoint = waypoints.Keys.ElementAt(value);
        }
    }
}
