using System;
using GVR.Util;
using UnityEngine;

namespace GVR.Demo
{
    public class DebugSettings: SingletonBehaviour<DebugSettings>
    {
        public enum SunMovementPerformance
        {
            EveryIngameSecond,
            EveryIngameMinute,
            EveryIngameHour
        };


        public bool CreateVobs;
        public bool CreateWaypoints;
        public bool CreateWaypointEdges;

        public bool EnableDayTime;

        [Range(0, 23)] public int StartHour;
        [Range(0, 59)] public int StartMinute;

        public string foo;

        public SunMovementPerformance SunMovementPerformanceValue;

        public bool EnableNpc;
        public bool EnableNpcRoutines;
        public bool CreateExampleAnimation;

        public bool ShowVdfsFileNotFoundErrors;
    }
}
