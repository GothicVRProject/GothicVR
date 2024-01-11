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
            if (newRoutine == GetCurrentRoutine())
            {
                Debug.LogWarning($"ChangeRoutine got called but the resulting routine was the same: NPC: >{gameObject.name}< WP: >{newRoutine.waypoint}<");
                return;
            }

            CurrentRoutine = newRoutine;

            GetComponent<AiHandler>().StartRoutine(CurrentRoutine.action, CurrentRoutine.waypoint);
        }


        /// <summary>
        /// When loading and spawning NPC, we need to fetch the spawnpoint (WP) but not executing it.
        /// </summary>
        public RoutineData GetCurrentRoutine()
        {
            // If we don't have a routine set as current right now, we do it now.
            if (CurrentRoutine != null)
                return CurrentRoutine;

            // TODO - Will be changed to either 8am (new game) or value from loaded save game.
            // TODO - Later: Use only this value if we set a debug value inside feature flags.
            var now = DateTime.MinValue
                .AddHours(FeatureFlags.I.startHour)
                .AddMinutes(FeatureFlags.I.startMinute);

            var newRoutine = Routines.First(item => item.start <= now && now < item.stop);
            CurrentRoutine = newRoutine;

            return CurrentRoutine;
        }
    }
}
