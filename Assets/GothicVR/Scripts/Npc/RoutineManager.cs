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
    GameTime gameTime;
    static Dictionary<DateTime, List<GVR.Npc.Routine>> npcStartTimeDict = new();

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
        //Es sollte der Start-Timestamp überprüft werden. Aber gibt es für jede Routine einen bei 15:00?
        //Wenn ich jetzt noch drauf käme, wie vorher sichergestellt wurde, dass das geht...
        //Init starting position
        if (!SingletonBehaviour<DebugSettings>.GetOrCreate().EnableNpcRoutines)
            return;
        DateTime StartTime = new(1, 1, 1, 15, 0, 0);
        Invoke(StartTime);
        
    }
    public void Subscribe(GVR.Npc.Routine npcID, List<RoutineData> routines)
    {
        if (!SingletonBehaviour<DebugSettings>.GetOrCreate().EnableNpcRoutines)
            return;
        foreach (RoutineData routine in routines)
        {
            try
            {
                if (npcStartTimeDict.ContainsKey(routine.start))
                {
                    //npcStartTimeDict.Add(routine.start, new List<GVR.Npc.Routine>()); //Die gehört hier nicht hin. Nur zu testzwecken da.
                    npcStartTimeDict[routine.start].Add(npcID);
                }
                else
                {
                    npcStartTimeDict.Add(routine.start, new List<GVR.Npc.Routine>());
                    npcStartTimeDict[routine.start].Add(npcID);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Console.WriteLine("Fehler beim Hinzufügen zur npcStartTimeDict: " + ex.Message);
            }
        }
    }
    public void Unsubscribe(GVR.Npc.Routine npcID, List<RoutineData> routines)
    {
        foreach (RoutineData routine in routines)
        {
            npcStartTimeDict[routine.start].Remove(npcID);  //Delete value from List
            if (!npcStartTimeDict[routine.start].Any())     //If List is empty afterwards
                npcStartTimeDict.Remove(routine.start);     //Delete key aswell
        }
    }
    private void Invoke(DateTime currTime)
    {
        
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
