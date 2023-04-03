using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UZVR.Demo;
using UZVR.Npc;
using UZVR.Phoenix.Bridge;
using UZVR.Phoenix.Vm.Gothic;
using UZVR.Phoenix.Vm.Gothic.Externals;
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

            var initialSpawnpoint = 
                PhoenixBridge.World.waypointsList.FirstOrDefault(item => item.name.ToLower() == spawnpoint.ToLower());

            if (initialSpawnpoint == null)
            {
                Debug.LogWarning(string.Format("spawnpoint={0} couldn't be found.", spawnpoint));
                return;
            }

            var npc = PhoenixBridge.VmGothicNpcBridge.InitNpcInstance(npcInstance);

            string npcName = PhoenixBridge.VmGothicNpcBridge.GetNpcName(npc);

            var newNpc = Instantiate(Resources.Load<GameObject>("Prefabs/Npc"));
            newNpc.name = string.Format("{0}-{1}", npcName, spawnpoint);

            var npcRoutine = PhoenixBridge.VmGothicNpcBridge.GetNpcRoutine(npc);

            PhoenixBridge.VmGothicNpcBridge.CallFunction(npcRoutine, npc);

            var symbolId = PhoenixBridge.VmGothicNpcBridge.GetNpcSymbolId(npc);
            newNpc.GetComponent<Properties>().DaedalusSymbolId = symbolId;

            if (PhoenixBridge.npcRoutines.TryGetValue(symbolId, out List<BRoutine> routines))
            {
                initialSpawnpoint = 
                    PhoenixBridge.World.waypointsList.FirstOrDefault(routine => routine.name.ToLower() == routines.First().waypoint.ToLower());
                newNpc.GetComponent<Routine>().routines = routines;
            }

            newNpc.transform.position = initialSpawnpoint.position;
            newNpc.transform.parent = npcContainer.transform;
        }

        private static void TA_MIN(NpcExternals.TA_MINData data)
        {
            // If we put h=24, DateTime will throw an error instead of rolling.
            var stop_hFormatted = data.stop_h == 24 ? 0 : data.stop_h;

            BRoutine routine = new()
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

            var symbolId = PhoenixBridge.VmGothicNpcBridge.GetNpcSymbolId(data.npc);

            // Add element if key not yet exists.
            PhoenixBridge.npcRoutines.TryAdd(symbolId, new());

            PhoenixBridge.npcRoutines[symbolId].Add(routine);

        }
    }
}
