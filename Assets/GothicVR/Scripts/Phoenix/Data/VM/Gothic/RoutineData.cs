/*using System;

namespace GVR.Phoenix.Data.Vm.Gothic
{
    public class RoutineData
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
}*/
using System;

public class RoutineData
{
    public int start_h;
    public int start_m;
    public DateTime start;
    public int stop_h;
    public int stop_m;
    public DateTime stop;
    public int action;
    public string waypoint;

    public RoutineData()
    {
        start_h = 0;
        start_m = 0;
        start = new DateTime(1, 1, 1, 0, 0, 0);
        stop_h = 0;
        stop_m = 0;
        stop = new DateTime(1, 1, 1, 0, 0, 0);
    }
}
