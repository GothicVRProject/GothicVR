using GVR.Demo;
using GVR.Npc;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Util;
using GVR.World;
using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Debugging;
using UnityEngine;

/// <summary>
/// Manages the Routines in a central spot. Routines Subscribe here. Calls the Routines when they are due.
/// </summary>
public class RoutineManager : SingletonBehaviour<RoutineManager>
{
    GameTime gameTime;
    Dictionary<DateTime, List<Routine>> npcStartTimeDict = new();

    private void OnEnable()
    {
        gameTime = SingletonBehaviour<GameTime>.GetOrCreate();
        gameTime.minuteChangeCallback.AddListener(Invoke);
    }

    private void OnDisable()
    {
        gameTime.minuteChangeCallback.RemoveListener(Invoke);
    }

    private void Start()
    {
        //Init starting position
        if (!FeatureFlags.I.EnableNpcRoutines)
            return;
        DateTime StartTime = new(1, 1, 1, 15, 0, 0);
        Invoke(StartTime);
    }

    public void Subscribe(Routine npcID, List<RoutineData> routines)
    {
        if (!FeatureFlags.I.EnableNpcRoutines)
            return;
        
        foreach (RoutineData routine in routines)   //Todo: fill in routines backwards, for Mud and Scorpio have bugged Routines and will be picked the wrong way as is.
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
    private void Invoke(DateTime currTime)
    {
        if (npcStartTimeDict.ContainsKey(currTime))
        {
            List<Routine> routineItems = npcStartTimeDict[currTime];
            foreach (var routineItem in routineItems)
            {
                routineItem.ChangeRoutine(currTime);
            }
        }
    }
}
