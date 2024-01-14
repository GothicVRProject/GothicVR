using System;
using System.Collections.Generic;
using GVR.Debugging;
using GVR.Globals;
using GVR.Npc.Routines;
using GVR.Util;
using UnityEngine;

namespace GVR.Manager
{
    /// <summary>
    /// Manages the Routines in a central spot. Routines Subscribe here. Calls the Routines when they are due.
    /// </summary>
    public class RoutineManager : SingletonBehaviour<RoutineManager>
    {
        Dictionary<int, List<Routine>> npcStartTimeDict = new();

        private void OnEnable()
        {
            GvrEvents.GameTimeMinuteChangeCallback.AddListener(Invoke);
        }

        private void OnDisable()
        {
            GvrEvents.GameTimeMinuteChangeCallback.RemoveListener(Invoke);
        }

        private void Start()
        {
            //Init starting position
            if (!FeatureFlags.I.enableNpcRoutines)
                return;
            
            GvrEvents.GeneralSceneLoaded.AddListener(WorldLoadedEvent);
        }

        private void WorldLoadedEvent()
        {
            var time = new DateTime(1, 1, 1,
                FeatureFlags.I.startHour, FeatureFlags.I.startMinute, 0);
            
            Invoke(time);
        }

        public void Subscribe(Routine npcID, List<RoutineData> routines)
        {
            if (!FeatureFlags.I.enableNpcRoutines)
                return;

            // We need to fill in routines backwards as e.g. Mud and Scorpio have duplicate routines. Last one needs to win.
            routines.Reverse();
            foreach (var routine in routines)
            {
                npcStartTimeDict.TryAdd(routine.normalizedStart, new());
                npcStartTimeDict[routine.normalizedStart].Add(npcID);
            }
        }

        public void Unsubscribe(Routine routineInstance, List<RoutineData> routines)
        {
            foreach (RoutineData routine in routines)
            {
                if (!npcStartTimeDict.TryGetValue(routine.normalizedStart, out List<Routine> routinesForStartPoint))
                    return;

                routinesForStartPoint.Remove(routineInstance);

                // Remove element if empty
                if (npcStartTimeDict[routine.normalizedStart].Count == 0)
                    npcStartTimeDict.Remove(routine.normalizedStart);
            }
        }

        /// <summary>
        /// Calls the routineInstances that are due.
        /// Triggers Routine Change
        /// </summary>
        private void Invoke(DateTime now)
        {
            var normalizedNow = now.Hour % 24 * 60 + now.Minute;
            
            Debug.Log($"RoutineManager.timeChanged={now}");
            if (!npcStartTimeDict.TryGetValue(normalizedNow, out var routineItems))
                return;
            
            foreach (var routineItem in routineItems)
            {
                routineItem.ChangeRoutine(now);
            }
        }
    }
}
