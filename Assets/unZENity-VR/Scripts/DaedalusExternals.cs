using System;
using UnityEngine;
using System.Linq;
using UZVR.Phoenix;
using Unity.VisualScripting;

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

            var npcInstance = TestSingleton.vm.InitNpcInstance(npcinstance);
            var npcRoutine = TestSingleton.vm.GetNpcRoutine(npcInstance);

            TestSingleton.vm.CallFunction(npcRoutine, npcInstance);            

            var newNpc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            newNpc.name = string.Format("{0}-{1}", npcinstance, spawnpoint);

            newNpc.transform.position = initialSpawnpoint.position;
            newNpc.transform.parent = npcContainer.transform;
        }

        public static void TA_MIN(IntPtr npc, int start_h, int start_m, int stop_h, int stop_m, int action, string waypoint)
        {
            // TODO Store routine as array onto the actual NPC (e.g. dictionary inside Unity)
            // TODO Use the current routine end point as Wld_InsertNpc's "real" spawn point.
            // TODO the npcRef is always 0x0. Also on main.cc. Are we missing sym.set_instance(h); ? Why? Is it even used during call?

            int a = 2;
        }
    }
}
