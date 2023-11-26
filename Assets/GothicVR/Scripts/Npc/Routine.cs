using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Extensions;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Phoenix.Interface;
using GVR.Properties;
using PxCs.Data.WayNet;
using PxCs.Interface;
using UnityEngine;

namespace GVR.Npc
{
    public class Routine : MonoBehaviour
    {
        PxWayPointData waypoint;
        RoutineData currentDestination;

        public List<RoutineData> routines = new();

        private void Start()
        {
            RoutineManager.I.Subscribe(this, routines);
        }
        
        private void OnDisable()
        {
            RoutineManager.I.Unsubscribe(this, routines);
        }

        public void ChangeRoutine(DateTime time)
        {
            var instancePtr = GetComponent<NpcProperties>().npc.instancePtr;
            var npcRoutine = routines.FirstOrDefault(item => item.start <= time && time < item.stop);
            PxVm.CallFunction(GameData.VmGothicPtr, (uint)npcRoutine.action, instancePtr);
        }
    }
}
