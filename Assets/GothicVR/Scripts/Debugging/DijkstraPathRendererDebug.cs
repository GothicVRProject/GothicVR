using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GVR.Extensions;
using GVR.Globals;
using GVR.Manager;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GVR.Debugging
{
    public class DijkstraPathRendererDebug : MonoBehaviour
    {
        public string debugStart;
        public string debugEnd;

        public List<string> pathDistanceCalculation;

        private Vector3[] gizmoWayPoints;
        private GameObject wayPointsGo;

        /// <summary>
        /// Debug method to draw Gizmo line for selected debugStart -> debugEnd
        /// </summary>
        private void OnValidate()
        {
            // Load rootGo for the first time. Start() would be too early, as world loads later.
            // And we want to have this load only during Editor time, therefore inside OnValidate()
            if (wayPointsGo == null)
                wayPointsGo = GameObject.Find("World/Waynet/Waypoints");

            if (GameData.DijkstraWaypoints.TryGetValue(debugStart, out var startWaypoint) &&
                GameData.DijkstraWaypoints.TryGetValue(debugEnd, out var endWaypoint))
            {

                LightUpWaypoint(debugStart, Color.green);
                LightUpWaypoint(debugEnd, Color.green);

                var watch = Stopwatch.StartNew();
                var path = WayNetHelper.FindFastestPath(debugStart, debugEnd);
                watch.Stop();
                Debug.Log($"Path found in {watch.Elapsed.Seconds} seconds.");

                var tempGizmoWayPoints = new List<Vector3>();
                for (var i = 0; i < path.Length - 1; i++)
                {
                    tempGizmoWayPoints.Add(path[i].Position);
                    tempGizmoWayPoints.Add(path[i+1].Position);
                }
                gizmoWayPoints = tempGizmoWayPoints.ToArray();

                Debug.Log("Start: " + gizmoWayPoints.First());
                Debug.Log("End: " + gizmoWayPoints.Last());
            }

            if (pathDistanceCalculation.Count > 0)
            {
                var summarizedDistance = 0.0f;
                for (var i = 0; i < pathDistanceCalculation.Count - 1; i++)
                {
                    var wayPointName1 = pathDistanceCalculation[i];
                    var wayPointName2 = pathDistanceCalculation[i+1];
                    var wp1 = FindWaypointGo(wayPointName1);
                    var wp2 = FindWaypointGo(wayPointName2);

                    if (wp1 == null || wp2 == null)
                    {
                        summarizedDistance = 0.0f;
                        break;
                    }
                    summarizedDistance += Vector3.Distance(wp1.transform.position, wp2.transform.position);
                }

                if (summarizedDistance > 0.0f)
                    Debug.Log($"Summarized distance: {summarizedDistance}");
            }
        }

        private void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.green;
            if (gizmoWayPoints == null)
                return;

            Gizmos.DrawLineList(gizmoWayPoints);
        }

        private void LightUpWaypoint(string wayPointName, Color color)
        {
            var waypoint = FindWaypointGo(wayPointName);
            if (waypoint == null)
                return;
            var wpRenderer = waypoint.GetComponent<Renderer>();
            if (wpRenderer == null)
                return;
            wpRenderer.material.color = color;
        }

        [CanBeNull]
        private GameObject FindWaypointGo(string wayPointName)
        {
            return wayPointsGo.FindChildRecursively(wayPointName);
        }
    }
}
