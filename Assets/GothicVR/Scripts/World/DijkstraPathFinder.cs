using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GVR.Extensions;
using PxCs.Interface;
using GVR.Phoenix.Data;
using System.Collections;
using GVR.Vob.WayNet;
using System.Linq;
using System;
using System.IO;

namespace GVR
{

    public class DijkstraPathFinder : MonoBehaviour
    {
        public static DijkstraPathFinder Instance;
        public string Start;
        public string End;
        private List<Vector3> WaypointsPosition = new();
        private DijkstraWaypoint[] Path = null;
        public bool WaypointsRendered { get; set; } = false;

        public Dictionary<string, DijkstraWaypoint> DijkstraWaypoints; // The original waypoints, as read from the world data

        public void SetDijkstraWaypointsOriginal(WorldData world)
        {
            DijkstraWaypoints = DijkstraWayPointCreator.Create(world);      //Is there a better place for this call?
        }

        private void CalculateNeighbourDistances()
        {
            foreach (var waypoint in DijkstraWaypoints.Values)
            {
                foreach (var neighbour in waypoint.Neighbors)
                {
                    if (waypoint.DistanceToNeighbors.ContainsKey(neighbour))
                    {
                        continue;
                    }
                    waypoint.DistanceToNeighbors.Add(neighbour, Vector3.Distance(waypoint.Position, DijkstraWaypoints[neighbour].Position));
                }
            }
        }

        private void OnValidate()
        {

            Debug.Log("OnValidate");

            if (DijkstraWaypoints != null && DijkstraWaypoints.TryGetValue(Start, out var startWaypoint) &&
            DijkstraWaypoints.TryGetValue(End, out var endWaypoint))
            {
                WaypointsPosition.Clear();
                WaypointsPosition.Add(DijkstraWaypoints[Start].Position);
                WaypointsPosition.Add(DijkstraWaypoints[End].Position);
                LightUpWaypoint(Start, Color.green);
                LightUpWaypoint(End, Color.green);
                Debug.Log("Start: " + WaypointsPosition[0]);
                Debug.Log("End: " + WaypointsPosition[1]);
                StartCoroutine(FindFastestPath());
            }


        }

        void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.green;
            if (WaypointsPosition == null)
            {
                return;
            }
            Gizmos.DrawLineList(WaypointsPosition.ToArray());
            if (Path != null)
            {
                var path = Path.Select(waypoint => waypoint.Position).ToList();
                var finalPath = new List<Vector3>();

                for (int i = 0; i < path.Count; i++)
                {
                    finalPath.Add(path[i]);
                    if (i != 0 && i != path.Count - 1)
                    {
                        finalPath.Add(path[i]);
                    }
                }
                Gizmos.color = Color.red;
                Gizmos.DrawLineList(finalPath.ToArray());
            }

        }

        private void LightUpWaypoint(string wayPointName, Color color)
        {
            var waypoint = FindWaypointGO(wayPointName);
            if (waypoint == null)
            {
                return;
            }
            var newColor = color == null ? Color.green : color;
            waypoint.GetComponent<Renderer>().material.color = color;
        }

        private GameObject FindWaypointGO(string wayPointName)
        {
            var result = gameObject.FindChildRecursively(wayPointName);
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

            foreach (var waypoint in DijkstraWaypoints)
            {
                var result = gameObject.FindChildRecursively(waypoint.Key).transform.position;
                if (result != null)
                {
                    waypoint.Value.Position = result;
                }
            }
            Debug.Log("DijkstraWaypoints set");
            CalculateNeighbourDistances();

        }

        public IEnumerator FindFastestPath(string startWaypoint = null, string endWaypoint = null)
        {
            if (startWaypoint == null || endWaypoint == null)
            {
                startWaypoint = Start;
                endWaypoint = End;
            }
            var startDijkstraWaypoint = DijkstraWaypoints[startWaypoint];
            var endDijkstraWaypoint = DijkstraWaypoints[endWaypoint];

            var previousNodes = new Dictionary<string, DijkstraWaypoint>();
            var unvisited = new PriorityQueue();

            foreach (var waypointx in DijkstraWaypoints.Values)
            {
                if (waypointx.Name == startWaypoint)
                {
                    waypointx.SummedDistance = 0;
                }
                else
                {
                    waypointx.SummedDistance = double.MaxValue;
                }

                unvisited.Enqueue(waypointx, waypointx.SummedDistance);
                previousNodes[waypointx.Name] = null;
            }

            while (unvisited.Count > 0)
            {
                var currentWaypoint = unvisited.Dequeue();
                Debug.Log(currentWaypoint.Name + " " + currentWaypoint.SummedDistance);
                LightUpWaypoint(currentWaypoint.Name, Color.yellow);

                foreach (var neighborName in currentWaypoint.DistanceToNeighbors.Keys)
                {
                    var neighbor = DijkstraWaypoints[neighborName];
                    var alt = currentWaypoint.SummedDistance + currentWaypoint.DistanceToNeighbors[neighborName];
                    if (alt < neighbor.SummedDistance || neighbor.Name == endDijkstraWaypoint.Name)
                    {
                        neighbor.SummedDistance = alt;
                        previousNodes[neighbor.Name] = currentWaypoint;
                        unvisited.Remove(neighbor);
                        unvisited.Enqueue(neighbor, alt + Heuristic(neighbor, endDijkstraWaypoint));
                    }
                }
                
                // Check if a valid path from Start to End has been found
                var lastChecked = endWaypoint;
                while (lastChecked != null && previousNodes[lastChecked] != null)
                {
                    lastChecked = previousNodes[lastChecked].Name;
                }
                if (lastChecked == startWaypoint)
                {
                    break;
                }
            }

            // Construct the shortest path
            var path = new List<DijkstraWaypoint>();
            var waypoint = endDijkstraWaypoint;
            while (waypoint != null)
            {
                path.Insert(0, waypoint);
                LightUpWaypoint(waypoint.Name, Color.green);
                waypoint = previousNodes[waypoint.Name];
            }

            for (int i = 0; i < path.Count; i++)
            {
                Debug.Log("[" + i + "]" + path[i].Name);
            }

            var testing = previousNodes.Where(x => x.Value != null).Select(x => x).ToList();

            Path = path.ToArray();

            if(Path.Length == 1)
            {
                Path.Append(Path[0]);
            }

            yield return path.ToArray();
        }

        private double Heuristic(DijkstraWaypoint a, DijkstraWaypoint b)
        {
            double euclidean = Vector3.Distance(a.Position, b.Position);

            return euclidean;
        }
    }

    public class PriorityQueue
    {
        private List<KeyValuePair<DijkstraWaypoint, double>> data;

        public PriorityQueue()
        {
            this.data = new List<KeyValuePair<DijkstraWaypoint, double>>();
        }

        public void Enqueue(DijkstraWaypoint waypoint, double priority)
        {
            data.Add(new KeyValuePair<DijkstraWaypoint, double>(waypoint, priority));
            int currentIndex = data.Count - 1;

            while (currentIndex > 0)
            {
                int parentIndex = (currentIndex - 1) / 2;

                if (data[currentIndex].Value >= data[parentIndex].Value)
                {
                    break;
                }

                var tmp = data[currentIndex];
                data[currentIndex] = data[parentIndex];
                data[parentIndex] = tmp;

                currentIndex = parentIndex;
            }
        }

        public DijkstraWaypoint Dequeue()
        {
            int lastIndex = data.Count - 1;
            DijkstraWaypoint frontItem = data[0].Key;
            data[0] = data[lastIndex];
            data.RemoveAt(lastIndex);

            --lastIndex;
            int parentIndex = 0;

            while (true)
            {
                int leftChildIndex = parentIndex * 2 + 1;
                if (leftChildIndex > lastIndex) break;

                int rightChildIndex = leftChildIndex + 1;
                if (rightChildIndex <= lastIndex && data[rightChildIndex].Value < data[leftChildIndex].Value)
                {
                    leftChildIndex = rightChildIndex;
                }

                if (data[parentIndex].Value <= data[leftChildIndex].Value) break;

                var tmp = data[parentIndex];
                data[parentIndex] = data[leftChildIndex];
                data[leftChildIndex] = tmp;

                parentIndex = leftChildIndex;
            }

            return frontItem;
        }

        public int Count
        {
            get
            {
                return data.Count;
            }
        }

        public void Remove(DijkstraWaypoint waypoint)
        {
            int index = data.FindIndex(pair => pair.Key.Name == waypoint.Name);
            if (index == -1)
            {
                //throw new ArgumentException("The specified waypoint is not in the queue.");
                Debug.Log("The specified waypoint " + waypoint.Name + " is not in the queue.");
                return;
            }
            data.RemoveAt(index);
        }
    }
}