using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Debugging;
using GVR.Manager;
using GVR.World;
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

        public void ChangeRoutine(DateTime now)
        {
            if (!CalculateCurrentRoutine())
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
        public bool CalculateCurrentRoutine()
        {
            var currentTime = GameTime.I.GetCurrentDateTime();
            
            var normalizedNow = currentTime.Hour % 24 * 60 + currentTime.Minute;

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

            // e.g. Mud has a bug as there is no routine covering 8am. We therefore pick the last one as seen in original G1. (sit)
            if (newRoutine == null)
                newRoutine = Routines.Last();

            var changed = CurrentRoutine != newRoutine;
            CurrentRoutine = newRoutine;

            return changed;
        }
    }
}
