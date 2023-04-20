using PxCs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZVR.Npc;
using UZVR.Phoenix.Data.Vm.Gothic;
using UZVR.Phoenix.Interface;
using UZVR.Phoenix.Interface.Vm;
using UZVR.Phoenix.Util;
using UZVR.Util;

namespace UZVR.Creator
{
    public class NpcCreator : SingletonBehaviour<NpcCreator>
    {
        void Start()
        {
            VmGothicBridge.PhoenixWld_InsertNpc.AddListener(Wld_InsertNpc);
            VmGothicBridge.PhoenixTA_MIN.AddListener(TA_MIN);
        }

        /// <summary>
        /// Original Gothic uses this function to spawn an NPC instance into the world.
        /// 
        /// The startpoint to walk isn't neccessarily the spawnpoint mentioned here.
        /// It can also be the currently active routine point to walk to.
        /// We therefore execute the daily routines to collect current location and use this as spawn location.
        /// </summary>
        public static void Wld_InsertNpc(int npcInstance, string spawnpoint)
        {
            var npcContainer = GameObject.Find("NPCs");

            var initialSpawnpoint = PhoenixBridge.World.waypoints
                .FirstOrDefault(item => item.name.ToLower() == spawnpoint.ToLower());

            if (initialSpawnpoint == null)
            {
                Debug.LogWarning(string.Format("spawnpoint={0} couldn't be found.", spawnpoint));
                return;
            }

            var pxNpc = PxVm.InitializeNpc(PhoenixBridge.VmGothicPtr, (uint)npcInstance);

            var newNpc = Instantiate(Resources.Load<GameObject>("Prefabs/Npc"));
            newNpc.name = string.Format("{0}-{1}", string.Concat(pxNpc.names), spawnpoint);
            var npcRoutine = pxNpc.routine;

            PxVm.CallFunction(PhoenixBridge.VmGothicPtr, (uint)npcRoutine, pxNpc.npcPtr);

            newNpc.GetComponent<Properties>().npc = pxNpc;

            if (PhoenixBridge.npcRoutines.TryGetValue(pxNpc.npcPtr, out List<RoutineData> routines))
            {
                initialSpawnpoint = PhoenixBridge.World.waypoints
                    .FirstOrDefault(item => item.name.ToLower() == routines.First().waypoint.ToLower());
                newNpc.GetComponent<Routine>().routines = routines;
            }

            newNpc.transform.position = initialSpawnpoint.position.ToUnityVector();
            newNpc.transform.parent = npcContainer.transform;
        }

        private static void TA_MIN(VmGothicBridge.TA_MINData data)
        {
            // If we put h=24, DateTime will throw an error instead of rolling.
            var stop_hFormatted = data.stop_h == 24 ? 0 : data.stop_h;

            RoutineData routine = new()
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

            // Add element if key not yet exists.
            PhoenixBridge.npcRoutines.TryAdd(data.npc, new());
            PhoenixBridge.npcRoutines[data.npc].Add(routine);
        }
    }
}
