using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UZVR.Demo;
using UZVR.Phoenix.Bridge;
using UZVR.Phoenix.Vm.Gothic;
using UZVR.Phoenix.World;
using UZVR.Util;

namespace UZVR.Npc
{
    public class Routine : MonoBehaviour
    {
        private static readonly int SPEED = 10;
        private GameTime gameTime;
        public List<BRoutine> routines;

        private BRoutine routine;
        private System.DateTime curTime;
        private BWaypoint waypoint;

        void Start()
        {
            gameTime = SingletonBehaviour<GameTime>.GetOrCreate();
        }

        void Update()
        {
            //Are NPCs Routines activated?
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().EnableNpcRoutines)
                return;

            routine = GetCurrentRoutine();

            //Does the NPC have a Routine?
            if (routine == null)
                return;

            waypoint = getNextWaypoint();

            moveToWaypoint();
        }

        private void moveToWaypoint()
        {
            Vector3 startPosition = gameObject.transform.position;
            gameObject.transform.position = Vector3.MoveTowards(startPosition, waypoint.position, SPEED * Time.deltaTime);
        }


        private BRoutine GetCurrentRoutine()
        {
            curTime = gameTime.getCurrentDateTime();

            return routines.FirstOrDefault(item => (item.start <= curTime && curTime < item.stop));
        }
        private BWaypoint getNextWaypoint()
        {
            return PhoenixBridge.World.waypoints.FirstOrDefault(item => item.name.ToLower() == routine.waypoint.ToLower());
        }
    }
}