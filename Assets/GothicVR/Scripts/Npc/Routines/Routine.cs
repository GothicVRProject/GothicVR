using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Debugging;
using GVR.Manager;
using UnityEngine;

namespace GVR.Npc.Routines
{
    public class Routine : MonoBehaviour
    {
        public readonly List<RoutineData> Routines = new();

        public RoutineData CurrentRoutine;

        private void Start()
        {
            RoutineManager.I.Subscribe(this, Routines);
        }
        
        private void OnDisable()
        {
            RoutineManager.I.Unsubscribe(this, Routines);
        }

        public void ChangeRoutine(DateTime time)
        {
            var newRoutine = Routines.First(item => item.start <= time && time < item.stop);

            // Already set and started.
            if (newRoutine == CurrentRoutine)
            {
                Debug.LogWarning($"ChangeRoutine got called but the resulting routine was the same: NPC: >{gameObject.name}< WP: >{newRoutine.waypoint}<");
                return;
            }

            CurrentRoutine = newRoutine;

            GetComponent<AiHandler>().StartRoutine(CurrentRoutine.action, CurrentRoutine.waypoint);
        }


        public RoutineData GetCurrentRoutine()
        {
            // TODO - Will be changed to either 8am (new game) or value from loaded save game.
            // TODO - Later: Use only this value if we set a debug value inside feature flags.
            if (CurrentRoutine == null)
            {
                var now = DateTime.MinValue
                    .AddHours(FeatureFlags.I.startHour)
                    .AddMinutes(FeatureFlags.I.startMinute);
                ChangeRoutine(now);
            }

            return CurrentRoutine;
        }
    }
}
