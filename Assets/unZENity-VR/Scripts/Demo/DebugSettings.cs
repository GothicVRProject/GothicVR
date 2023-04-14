﻿using UZVR.Util;

namespace UZVR.Demo
{
    public class DebugSettings: SingletonBehaviour<DebugSettings>
    {
        public bool CreateWaypoints;
        public bool CreateWaypointEdges;

        public bool EnableDayTime;
        public SunMovementPerformance SunMovementPerformanceValue;

        public bool EnableNpcRoutines;

        public enum SunMovementPerformance
        {
            EveryIngameSecond,
            EveryIngameMinute,
            Every10IngameMinutes
        };
    }
}
