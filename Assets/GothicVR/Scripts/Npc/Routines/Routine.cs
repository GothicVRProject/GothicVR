using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Manager;
using UnityEngine;

namespace GVR.Npc.Routines
{
    public class Routine : MonoBehaviour
    {
        RoutineData currentDestination;

        public List<RoutineData> routines = new();

        private void Start()
        {
            RoutineManager.I.Subscribe(this, routines);
        }
        
        private void OnDisable()
        {
            RoutineManager.I.Unsubscribe(this, routines);
        }

        public void ChangeRoutine(DateTime time)
        {
            var npcRoutine = routines.First(item => item.start <= time && time < item.stop);

            GetComponent<AiHandler>().StartRoutine(npcRoutine.action, npcRoutine.waypoint);
        }
    }
}
