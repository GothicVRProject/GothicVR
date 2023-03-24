using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZVR.Demo;
using UZVR.Phoenix;
using UZVR.Phoenix.Vm;
using UZVR.Phoenix.Vm.Externals;
using UZVR.Util;

namespace UZVR.WorldCreator
{
    public class NpcCreator : SingletonBehaviour<NpcCreator>
    {
        void Start()
        {
            NpcExternals.PhoenixWld_InsertNpc.AddListener(Wld_InsertNpc);
            NpcExternals.PhoenixTA_MIN.AddListener(TA_MIN);
        }

        public static void Wld_InsertNpc(int npcInstance, string spawnpoint)
        {
            var npcContainer = GameObject.Find("NPCs");

            var initialSpawnpoint = PhoenixBridge.World.waypoints
                .FirstOrDefault(item => item.name.ToLower() == spawnpoint.ToLower());

            // Not very fast. Please consider using an index search via a Dictionary(string name, Waypoint waypoint);
            if (initialSpawnpoint == null)
            {
                Debug.LogWarning(string.Format("spawnpoint={0} couldn't be found.", spawnpoint));
                return;
            }

            var npc = PhoenixBridge.VMBridge.InitNpcInstance(npcInstance);

            string name = PhoenixBridge.VMBridge.GetNpcName(npc);

            var newNpc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            newNpc.name = string.Format("{0}-{1}", name, spawnpoint);

            var npcRoutine = PhoenixBridge.VMBridge.GetNpcRoutine(npc);

            PhoenixBridge.VMBridge.CallFunction(npcRoutine, npc);

            var symbolId = PhoenixBridge.VMBridge.GetNpcSymbolId(npc);

            if (PhoenixBridge.npcRoutines.TryGetValue(symbolId, out List<PBRoutine> routines))
            {
                initialSpawnpoint = PhoenixBridge.World.waypoints
                    .FirstOrDefault(item => item.name.ToLower() == routines.First().waypoint.ToLower());
                var routineComp = newNpc.AddComponent<NpcRoutine>();
                routineComp.routines = routines;
            }

            newNpc.transform.position = initialSpawnpoint.position;
            newNpc.transform.parent = npcContainer.transform;
        }

        private static void TA_MIN(NpcExternals.TA_MINData data)
        {
            Debug.Log(DateTime.MinValue);
            Debug.Log(new DateTime(1, 1, 1, 0, 0, 0));

            var stop_hFormatted = data.stop_h == 24 ? 0 : data.stop_h;

            PBRoutine routine = new()
            {
                start_h = data.start_h,
                start_m = data.start_m,
                start = new(1, 1, 1, data.start_h, data.start_m, 0),
                stop_h = data.stop_h,
                stop_m = data.stop_m,
                stop = new(1, 1, 1, stop_hFormatted, data.stop_m, 0),
                action = data.action,
                waypoint = data.waypoint
            };

            var symbolId = PhoenixBridge.VMBridge.GetNpcSymbolId(data.npc);

            // Add element if key not yet exists.
            PhoenixBridge.npcRoutines.TryAdd(symbolId, new());

            PhoenixBridge.npcRoutines[symbolId].Add(routine);

        }
    }
}
