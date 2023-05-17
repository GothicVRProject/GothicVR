using GVR.Phoenix.Data.Vm.Gothic;
using GVR.World;
using System;
using System.Collections;
using GVR.Demo;
using System.Collections.Generic;
using UnityEngine;
using GVR.Util;
using System.Linq;

public class RoutineManager : MonoBehaviour
{
    GameTime gameTime = new();
    Dictionary<DateTime, List<GVR.Npc.Routine>> npcStartTimeDict;

    private void OnEnable()
    {
        gameTime.minuteChangeCallback.AddListener(Invoke);
    }
    private void Start()
    {
        //Es sollte der Start-Timestamp überprüft werden. Aber gibt es für jede Routine einen bei 15:00?
        //Wenn ich jetzt noch drauf käme, wie vorher sichergestellt wurde, dass das geht...
        //Init starting position
        DateTime StartTime = new(1, 1, 1, 15, 0, 0);
        Invoke(StartTime);
        
    }
    public void Subscribe(GVR.Npc.Routine npcID, List<RoutineData> routines)
    {
        foreach (RoutineData routine in routines)
        {
            if (!npcStartTimeDict.ContainsKey(routine.start))
            {
                npcStartTimeDict.Add(routine.start, new());
                npcStartTimeDict[routine.start].Add(npcID);
            }
            else
            {
                npcStartTimeDict[routine.start].Add(npcID);
            }
        }
    }
    public void Unsubscribe(GVR.Npc.Routine npcID, List<RoutineData> routines)
    {
        foreach (RoutineData routine in routines)
        {
            npcStartTimeDict[routine.start].Remove(npcID);
            if (!npcStartTimeDict[routine.start].Any())
            {
                npcStartTimeDict.Remove(routine.start);
            }
        }
    }
    private void Invoke(DateTime currTime)
    {
        if (!SingletonBehaviour<DebugSettings>.GetOrCreate().EnableNpcRoutines)
            return;
        if (npcStartTimeDict.ContainsKey(currTime))
        {
            List<GVR.Npc.Routine> routineItems = npcStartTimeDict[currTime];
            foreach (var routineItem in routineItems)
            {
                routineItem.lookUpRoutine(currTime);
            }
        }
    }
}
