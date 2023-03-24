using System;
using UnityEngine;
using System.Linq;
using UZVR.Phoenix;
using Unity.VisualScripting;
using System.Collections.Generic;

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
            var npcRoutine = TestSingleton.vm.GetNpcRoutine(npc);

            TestSingleton.vm.CallFunction(npcRoutine, npc);

            var symbolId = TestSingleton.vm.GetNpcSymbolId(npc);

            if (TestSingleton.npcRoutines.TryGetValue(symbolId, out List<TestSingleton.Routine> routines))
            {
                initialSpawnpoint = TestSingleton.world.waypoints
                .FirstOrDefault(item => item.name.ToLower() == routines.First().waypoint.ToLower());
            }

            string name = TestSingleton.vm.GetNpcName(npc);

            var newNpc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            newNpc.name = string.Format("{0}-{1}", name, spawnpoint);

            newNpc.transform.position = initialSpawnpoint.position;
            newNpc.transform.parent = npcContainer.transform;
        }

        public static void TA_MIN(IntPtr npc, int start_h, int start_m, int stop_h, int stop_m, int action, string waypoint)
        {
            TestSingleton.Routine routine = new()
            {
                start_h = start_h,
                start_m = start_m,
                stop_h = stop_h,
                stop_m = stop_m,
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
