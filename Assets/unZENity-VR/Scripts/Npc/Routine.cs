using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
        private static readonly int SPEED = 50;
        private GameTime gameTime;
        public List<BRoutine> routines;

        private BRoutine curRoutine;
        private System.DateTime curTime;
        private System.DateTime timeToStartNextWalk;
        private Vector3 curPos;
        private BWaypoint curWaypoint;

        void Start()
        {
            curPos = GetCurrentPosition();
            gameTime = SingletonBehaviour<GameTime>.GetOrCreate();
            timeToStartNextWalk = curRoutine.stop;
            curWaypoint = getNextWaypoint();
        }

        void Update()
        {
            //Are NPCs Routines activated?
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().EnableNpcRoutines)
                return;

            curTime = GetCurrentTime();
            curRoutine = GetCurrentRoutine();

            //Does the NPC have a Routine?
            if (curRoutine == null)
                return;
            
            if (IsTimeToGetNextWaypoint())
                curWaypoint = getNextWaypoint();
            
            if (IsNotAtCurWaypoint())
            {
                curPos = GetCurrentPosition();
                moveToWaypoint();
            }
        }
        
        private System.DateTime GetCurrentTime()
        {
            return gameTime.getCurrentDateTime();
        }
        private BRoutine GetCurrentRoutine()
        {
            return routines.FirstOrDefault(item => (item.start <= curTime && curTime < item.stop));
        }
        private bool IsTimeToGetNextWaypoint()
        {
            timeToStartNextWalk = curRoutine.stop;
            return curTime < timeToStartNextWalk;
        }
        private BWaypoint getNextWaypoint()
        {
            //return PhoenixBridge.World.waypointsList.FirstOrDefault(item => item.name.ToLower() == curRoutine.waypoint.ToLower());
            return PhoenixBridge.World.waypointsDict[curRoutine.waypoint.ToLower()];
        }
        private bool IsNotAtCurWaypoint()
        {
            return curPos != curWaypoint.position;
        }
        private Vector3 GetCurrentPosition()
        {
            return gameObject.transform.position;
        }
        private void moveToWaypoint()
        {
            gameObject.transform.position = Vector3.MoveTowards(curPos, curWaypoint.position, SPEED * Time.deltaTime);
        }
    }
}