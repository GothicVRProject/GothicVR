using GVR.Extensions;
using GVR.Manager;
using GVR.Phoenix.Interface;
using GVR.World;
using UnityEngine;

namespace GVR.Debugging
{
    public class DijkstraPathRendererDebug : MonoBehaviour
    {
        public string debugStart;
        public string debugEnd;
        private DijkstraWaypoint[] waypointsPosition = {};
        private GameObject wayPointsGo;

        /// <summary>
        /// Debug method to draw Gizmo line for selected debugStart -> debugEnd
        /// </summary>
        private void OnValidate()
        {
            Debug.Log("OnValidate");

            if (GameData.DijkstraWaypoints.TryGetValue(debugStart, out var startWaypoint) &&
                GameData.DijkstraWaypoints.TryGetValue(debugEnd, out var endWaypoint))
            {
                waypointsPosition = WayNetHelper.FindFastestPath(debugStart, debugEnd);

                // Load rootGo for the first time
                if (wayPointsGo == null)
                    wayPointsGo = GameObject.Find("World/Waynet/Waypoints");

                // waypointsPosition.Add(GameData.DijkstraWaypoints[debugStart].Position);
                // waypointsPosition.Add(GameData.DijkstraWaypoints[debugEnd].Position);
                LightUpWaypoint(debugStart, Color.green);
                LightUpWaypoint(debugEnd, Color.green);
                Debug.Log("Start: " + waypointsPosition[0]);
                Debug.Log("End: " + waypointsPosition[1]);
            }
        }

        // With this function, we want to draw lines for the routes. But I'm currently unsure how to use it with DijkstraObjects instead of Vector3.
        // Something to re-activate in the future.

        // void OnDrawGizmos()
        // {
        //     // Draw a yellow sphere at the transform's position
        //     Gizmos.color = Color.green;
        //     if (waypointsPosition == null)
        //     {
        //         return;
        //     }
        //     Gizmos.DrawLineList(waypointsPosition.ToArray());
        //     if (path != null)
        //     {
        //         var path = this.path.Select(waypoint => waypoint.Position).ToList();
        //         var finalPath = new List<Vector3>();
        //
        //         for (int i = 0; i < path.Count; i++)
        //         {
        //             finalPath.Add(path[i]);
        //             if (i != 0 && i != path.Count - 1)
        //             {
        //                 finalPath.Add(path[i]);
        //             }
        //         }
        //         Gizmos.color = Color.red;
        //         Gizmos.DrawLineList(finalPath.ToArray());
        //     }
        // }

        private void LightUpWaypoint(string wayPointName, Color color)
        {
            var waypoint = FindWaypointGo(wayPointName);
            if (waypoint == null)
                return;
            waypoint.GetComponent<Renderer>().material.color = color;
        }

        private GameObject FindWaypointGo(string wayPointName)
        {
            var result = wayPointsGo.FindChildRecursively(wayPointName);
            if (result != null)
            {
                return result;
            }
            else
            {
                Debug.Log("Waypoint not found");
                return null;
            }
        }

    }
}
