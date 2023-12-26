using System;
using System.Collections.Generic;
using GVR.Debugging;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Util;
using GVR.World;
using UnityEngine;

namespace GVR.Npc
{
    /// <summary>
    /// Manages the Routines in a central spot. Routines Subscribe here. Calls the Routines when they are due.
    /// </summary>
    public class RoutineManager : SingletonBehaviour<RoutineManager>
    {
        GameTime gameTime;
        Dictionary<DateTime, List<Routine>> npcStartTimeDict = new();

        private void OnEnable()
        {
            gameTime = GameTime.I;
            gameTime.minuteChangeCallback.AddListener(Invoke);
        }

        private void OnDisable()
        {
            gameTime.minuteChangeCallback.RemoveListener(Invoke);
        }

        private void Start()
        {
            //Init starting position
            if (!FeatureFlags.I.enableNpcRoutines)
                return;
            DateTime StartTime = new(1, 1, 1, 15, 0, 0);
            Invoke(StartTime);
        }

        public void Subscribe(Routine npcID, List<RoutineData> routines)
        {
            if (!FeatureFlags.I.enableNpcRoutines)
                return;

            // We need to fill in routines backwards as e.g. Mud and Scorpio have duplicate routines. Last one needs to win.
            routines.Reverse();
            foreach (var routine in routines)
            {
                npcStartTimeDict.TryAdd(routine.start, new());
                npcStartTimeDict[routine.start].Add(npcID);
            }
        }

        public void Unsubscribe(Routine routineInstance, List<RoutineData> routines)
        {
            foreach (RoutineData routine in routines)
            {
                if (!npcStartTimeDict.TryGetValue(routine.start, out List<Routine> routinesForStartPoint))
                    return;

                routinesForStartPoint.Remove(routineInstance);

                // Remove element if empty
                if (npcStartTimeDict[routine.start].Count == 0)
                    npcStartTimeDict.Remove(routine.start);
            }
        }

        /// <summary>
        /// Calls the routineInstances that are due.
        /// Triggers Routine Change
        /// </summary>
        private void Invoke(DateTime now)
        {
            Debug.Log($"RoutineManager.timeChanged={now}");
            if (!npcStartTimeDict.TryGetValue(now, out var routineItems))
                return;
            
            foreach (var routineItem in routineItems)
            {
                routineItem.ChangeRoutine(now);
            }
        }
    }
}
