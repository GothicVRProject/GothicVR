using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZVR.Demo;
using UZVR.Phoenix.Bridge;
using UZVR.Phoenix.Vm.Gothic;
using UZVR.Util;

namespace UZVR.Npc
{
    public class Routine : MonoBehaviour
    {
        private static readonly int SPEED = 10;
        private GameTime gameTime;
        public List<BRoutine> routines;

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
            gameObject.transform.position = Vector3.MoveTowards(startPosition, waypoint.position, SPEED * Time.deltaTime);
        }

        private BRoutine GetCurrentRoutine()
        {
            var curTime = gameTime.getCurrentDateTime();

            var routine = routines.FirstOrDefault(item => (item.start <= curTime && curTime < item.stop));

            return routine;
        }
    }
}