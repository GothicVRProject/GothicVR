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
            if (!CalculateCurrentRoutine(time.Hour, time.Minute))
            {
                Debug.LogWarning("ChangeRoutine got called but the resulting routine was the same: " +
                                 $"NPC: >{gameObject.name}< WP: >{CurrentRoutine.waypoint}<");
                return;
            }

            GetComponent<AiHandler>().StartRoutine(CurrentRoutine.action, CurrentRoutine.waypoint);
        }

        /// <summary>
        /// Calculate new routine based on given timestamp.
        /// Hints about normalization:
        ///   1. Daedalus handles routines with a 00:00 as midnight (24:00)
        ///   -> For the midnight topic, we normalize via %24
        ///   2. Routines can span multiple days (e.g. 22:00 - 09:00)
        ///   -> For the overnight topic, we leverage the second if when start > end
        /// </summary>
        /// <returns>Whether the routine changed or not.</returns>
        public bool CalculateCurrentRoutine(int currentHour, int currentMinute)
        {
            var normalizedNow = currentHour % 24 * 60 + currentMinute;

            RoutineData newRoutine = null;
            
            // There are routines where stop is lower than start. (e.g. now:8:00, routine:22:00-9:00), therefore the second check.
            foreach (var routine in Routines)
            {
                if (routine.normalizedStart <= normalizedNow && normalizedNow < routine.normalizedEnd)
                {
                    newRoutine = routine;
                    break;
                }
                // Handling the case where the time range spans across midnight
                else if (routine.normalizedStart > routine.normalizedEnd)
                {
                    if (routine.normalizedStart <= normalizedNow || normalizedNow < routine.normalizedEnd)
                    {
                        newRoutine = routine;
                        break;
                    }
                }
            }

            var changed = CurrentRoutine != newRoutine;
            CurrentRoutine = newRoutine;

            return changed;
        }
    }
}
