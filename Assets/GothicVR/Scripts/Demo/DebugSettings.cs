using GVR.Util;

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
        public SunMovementPerformance SunMovementPerformanceValue;

        public bool EnableNpc;
        public bool EnableNpcRoutines;
        public bool CreateExampleAnimation;

        public bool ShowVdfsFileNotFoundErrors;
    }
}
