using System;
using System.Collections.Generic;
using UZVR.Phoenix;

namespace UZVR
{

    /// <summary>
    /// This class acts as a temporary singleton to make phoenix-csharp-bridge entries globally accessible.
    /// Until we find a proper architecture to do so.
    /// </summary>
    public static class TestSingleton
    {
        public struct Routine
        {
            public int start_h;
            public int start_m;
            public DateTime start;
            public int stop_h;
            public int stop_m;
            public DateTime stop;
            public int action;
            public string waypoint;
        }


        public static PCBridge_World world;

        public static VmBridge vm;


        public static Dictionary<uint, List<Routine>> npcRoutines = new();
    }
}
