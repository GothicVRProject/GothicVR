﻿using System;
using System.Collections.Generic;
using UnityEngine;

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
        public List<Vector3> route;
    }
}

