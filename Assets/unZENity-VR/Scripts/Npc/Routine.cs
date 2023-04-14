using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZVR.Demo;
using UZVR.Phoenix.Data.Vm.Gothic;
using UZVR.Phoenix.Interface;
using UZVR.Phoenix.Util;
using UZVR.Util;
using UZVR.World;

namespace UZVR.Npc
{
    public class Routine : MonoBehaviour
    {
        private static readonly int SPEED = 10;
        private GameTime gameTime;
        public List<RoutineData> routines = new();

        void Start()
        {
            gameTime = SingletonBehaviour<GameTime>.GetOrCreate();
        }

        void Update()
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().EnableNpcRoutines)
                return;

            var routine = GetCurrentRoutine();

            if (routine == null)
                return;

            var waypoint = PhoenixBridge.World.waypoints
                .FirstOrDefault(item => item.name.ToLower() == routine.waypoint.ToLower());

            var startPosition = gameObject.transform.position;
            gameObject.transform.position = Vector3.MoveTowards(startPosition, waypoint.position.ToUnityVector(), SPEED * Time.deltaTime);
        }

        private RoutineData GetCurrentRoutine()
        {
            var curTime = gameTime.GetCurrentDateTime();

            var routine = routines.FirstOrDefault(item => (item.start <= curTime && curTime < item.stop));

            return routine;
        }
    }
}