using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GVR.Extensions;

namespace GVR {

    public class DijkstraPathFinder : MonoBehaviour
    {
        public static DijkstraPathFinder Instance;
        public string start;
        public string end;
        private List<Vector3> dijkstraWaypoints = new();
        private List<Vector3> myPath = new();

    public Dictionary<string, DijkstraWaypoint> DijkstraWaypoints;

    private void OnValidate()
    {
        Debug.Log("OnValidate");

            myPath.Add(gameObject.FindChildRecursively("OCR_CAMPFIRE_A_MOVEMENT2").transform.position);
            myPath.Add(gameObject.FindChildRecursively("OCR_CAMPFIRE_A_MOVEMENT3").transform.position);
            myPath.Add(gameObject.FindChildRecursively("OCR_MAINGATE_SQUARE").transform.position);
            myPath.Add(gameObject.FindChildRecursively("OCR_TO_MAINGATE").transform.position);
            myPath.Add(gameObject.FindChildRecursively("OCR_TO_MAINGATE").transform.position);
            myPath.Add(gameObject.FindChildRecursively("OCR_CAMPFIRE_A_MOVEMENT3").transform.position);

            if (DijkstraWaypoints.TryGetValue(start, out var startWaypoint) && 
            DijkstraWaypoints.TryGetValue(end, out var endWaypoint))
            {
                dijkstraWaypoints.Clear();
                dijkstraWaypoints.Add(gameObject.FindChildRecursively(start).transform.position);
                dijkstraWaypoints.Add(gameObject.FindChildRecursively(end).transform.position);
            Debug.Log("start: " + dijkstraWaypoints[0]);
            Debug.Log("end: " + dijkstraWaypoints[1]);
        }

            
    }

        void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.yellow;
            //I want to cnvert dijkstraWaypoints to a readonlyspan<Vector3> to use it in DrawLineList

            Gizmos.DrawLineList(myPath.ToArray());
            
        }

        private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetDijkstraWaypoints(Dictionary<string, DijkstraWaypoint> waypoints)
    {
        DijkstraWaypoints = waypoints;
        Debug.Log("DijkstraWaypoints set");
    }

    public string FindFastestNextWaypoint(string start, string end)
    {
        // Implement your Dijkstra logic here to find the fastest path
        // from the start waypoint to the end waypoint, and return the next waypoint in the path

        return null; // Placeholder return
    }

    public string FindPath(string start, string end)
    {
        // Implement your Dijkstra logic here to find the fastest path
        // from the start waypoint to the end waypoint, and return the next waypoint in the path

        /*FindFastestNextWaypoint(current, end);*/
        return null; // Placeholder return
    }
    }
}