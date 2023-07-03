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

        [Header("__________World__________")]
        public bool CreateVobs;
        public bool CreateWaypoints;
        public bool CreateWaypointEdges;

        [Header("__________DayTime__________")]
        public bool EnableDayTime;
        public SunMovementPerformance SunMovementPerformanceValue;
        [Range(0, 23)] public int StartHour;
        [Range(0, 59)] public int StartMinute;
        
        [Header("__________NPCs__________")]
        public bool EnableNpc;
        public bool EnableNpcRoutines;
        public bool CreateExampleAnimation;

        [Header("__________SPAMmy debug messages__________")]
        public bool ShowVdfsFileNotFoundErrors;

        [Header("__________Audio__________")]
        public bool EnableSounds;
        public bool EnableMusic;

    }
}
