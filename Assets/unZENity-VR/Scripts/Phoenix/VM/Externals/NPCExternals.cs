﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UZVR.Demo;

namespace UZVR.Phoenix.Vm.Externals
{
    public static class NpcExternals
    {
        public struct TA_MINData
        {
            public IntPtr npc;
            public int start_h;
            public int start_m;
            public int stop_h;
            public int stop_m;
            public int action;
            public string waypoint;
        }

        public static UnityEvent<int, string> PhoenixWld_InsertNpc = new();
        public static UnityEvent<TA_MINData> PhoenixTA_MIN = new();

        /// <summary>
        /// Original Gothic uses this function to spawn an NPC instance into the world.
        /// 
        /// The startpoint to walk isn't neccessarily the spawnpoint mentioned here.
        /// It can also be the currently active routine point to walk to.
        /// We therefore execute the daily routines to collect current location and use this as spawn location.
        /// </summary>
        public static void Wld_InsertNpc(int npcinstance, string spawnpoint)
        {
            PhoenixWld_InsertNpc.Invoke(npcinstance, spawnpoint);
        }

        public static void TA_MIN(IntPtr npc, int start_h, int start_m, int stop_h, int stop_m, int action, string waypoint)
        {
            PhoenixTA_MIN.Invoke(
                new TA_MINData
                {
                    npc = npc,
                    start_h = start_h,
                    start_m = start_m,
                    stop_h = stop_h,
                    stop_m = stop_m,
                    action = action,
                    waypoint = waypoint
                }
            );
        }
    }
}
