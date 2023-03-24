﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZVR.Phoenix;
using static UnityEditor.Progress;

namespace UZVR
{
    public static class DaedalusExternals
    {
        public static void NotImplementedCallback(string value)
        {
            //throw new NotImplementedException("External >" + value + "< not registered but required by DaedalusVM.");
            // DEBUG
            Debug.LogError("External >" + value + "< not registered but required by DaedalusVM.");

        }

        /// <summary>
        /// Original gothic uses this function to spawn an NPC instance into the world.
        /// 
        /// The startpoint to walk isn't neccessarily the spawnpoint mentioned here.
        /// It can also be the currently active routine point to walk to.
        /// We therefore briefly execute the daily routine once to collect current location and use this as spawn location.
        /// </summary>
        public static void Wld_InsertNpc(int npcinstance, string spawnpoint)
        {
            var npcContainer = GameObject.Find("NPCs");

            var initialSpawnpoint = TestSingleton.world.waypoints
                .FirstOrDefault(item => item.name.ToLower() == spawnpoint.ToLower());

            // Not very fast. Please consider using an index search via a Dictionary(string name, Waypoint waypoint);
            if (initialSpawnpoint.Equals(default(PCBridge_Waypoint)))
            {
                Debug.LogWarning(string.Format("spawnpoint={0} couldn't be found.", spawnpoint));
                return;
            }

            var npc = TestSingleton.vm.InitNpcInstance(npcinstance);

            string name = TestSingleton.vm.GetNpcName(npc);

            var newNpc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            newNpc.name = string.Format("{0}-{1}", name, spawnpoint);

            var npcRoutine = TestSingleton.vm.GetNpcRoutine(npc);

            TestSingleton.vm.CallFunction(npcRoutine, npc);

            var symbolId = TestSingleton.vm.GetNpcSymbolId(npc);

            if (TestSingleton.npcRoutines.TryGetValue(symbolId, out List<TestSingleton.Routine> routines))
            {
                initialSpawnpoint = TestSingleton.world.waypoints
                    .FirstOrDefault(item => item.name.ToLower() == routines.First().waypoint.ToLower());
                var routineComp = newNpc.AddComponent<NpcRoutine>();
                routineComp.routines = routines;
            }

            newNpc.transform.position = initialSpawnpoint.position;
            newNpc.transform.parent = npcContainer.transform;
        }

        public static void TA_MIN(IntPtr npc, int start_h, int start_m, int stop_h, int stop_m, int action, string waypoint)
        {
            Debug.Log(DateTime.MinValue);
            Debug.Log(new DateTime(1, 1, 1, 0, 0, 0));

            var stop_hFormatted = stop_h == 24 ? 0 : stop_h;

            TestSingleton.Routine routine = new()
            {
                start_h = start_h,
                start_m = start_m,
                start = new(1,1,1, start_h, start_m, 0),
                stop_h = stop_h,
                stop_m = stop_m,
                stop = new(1, 1, 1, stop_hFormatted, stop_m, 0),
                action = action,
                waypoint = waypoint
            };

            var symbolId = TestSingleton.vm.GetNpcSymbolId(npc);

            // Add element if key not yet exists.
            TestSingleton.npcRoutines.TryAdd(symbolId, new());

            TestSingleton.npcRoutines[symbolId].Add(routine);
        }
    }
}
