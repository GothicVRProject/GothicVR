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
        public string start;
        public string end;
        private List<Vector3> waypointsPosition = new();
        private DijkstraWaypoint[] _path = null;
        public bool waypointsRendered { get; set; } = false;

        public Dictionary<string, DijkstraWaypoint> DijkstraWaypoints; // The original waypoints, as read from the world data

        public void SetDijkstraWaypointsOriginal(WorldData world)
        {
            DijkstraWaypoints = DijkstraWayPointCreator.Create(world);      //Is there a better place for this call?
        }

        private void CalculateNeighbourDistances()
        {
            foreach (var waypoint in DijkstraWaypoints.Values)
            {
                foreach (var neighbour in waypoint._neighbors)
                {
                    if (waypoint._distanceToNeighbors.ContainsKey(neighbour))
                    {
                        continue;
                    }
                    waypoint._distanceToNeighbors.Add(neighbour, Vector3.Distance(waypoint._position, DijkstraWaypoints[neighbour]._position));
                }
            }
        }

        private void OnValidate()
        {

            Debug.Log("OnValidate");

            if (DijkstraWaypoints != null && DijkstraWaypoints.TryGetValue(start, out var startWaypoint) &&
            DijkstraWaypoints.TryGetValue(end, out var endWaypoint))
            {
                waypointsPosition.Clear();
                waypointsPosition.Add(DijkstraWaypoints[start]._position);
                waypointsPosition.Add(DijkstraWaypoints[end]._position);
                LightUpWaypoint(start);
                LightUpWaypoint(end);
                Debug.Log("start: " + waypointsPosition[0]);
                Debug.Log("end: " + waypointsPosition[1]);
                FindFastestPath();
            }


        }

        void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.green;
            if (waypointsPosition == null)
            {
                return;
            }
            Gizmos.DrawLineList(waypointsPosition.ToArray());
            if (_path != null)
            {
                var path = _path.Select(waypoint => waypoint._position).ToList();
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

        private void LightUpWaypoint(string wayPointName)
        {
            var waypoint = FindWaypointGO(wayPointName);
            if (waypoint == null)
            {
                return;
            }
            waypoint.GetComponent<Renderer>().material.color = Color.green;
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
                    waypoint.Value._position = result;
                }
            }
            Debug.Log("DijkstraWaypoints set");
            CalculateNeighbourDistances();

        }

        public DijkstraWaypoint[] FindFastestPath(DijkstraWaypoint start = null, DijkstraWaypoint end = null)
        {
            if (start == null)
            {
                start = DijkstraWaypoints[this.start] ?? throw new ArgumentException("The specified start waypoint is not in the queue.");
            }
            if (end == null)
            {
                end = DijkstraWaypoints[this.end] ?? throw new ArgumentException("The specified end waypoint is not in the queue.");
            }

            var startDijkstraWaypoint = DijkstraWaypoints[start];
            var endDijkstraWaypoint = DijkstraWaypoints[end];

            var distances = new Dictionary<string, double>();
            var previousNodes = new Dictionary<string, DijkstraWaypoint>();
            var unvisited = new PriorityQueue();

            foreach (var waypointx in DijkstraWaypoints.Values)
            {
                if (waypointx._name == start)
                {
                    distances[waypointx._name] = 0;
                }
                else
                {
                    distances[waypointx._name] = double.MaxValue;
                }

                unvisited.Enqueue(waypointx, distances[waypointx._name]);
                previousNodes[waypointx._name] = null;
            }

            while (unvisited.Count > 0)
            {
                var currentWaypoint = unvisited.Dequeue();

                if (currentWaypoint._name == end)
                {
                    break;
                }

                foreach (var neighborName in currentWaypoint._distanceToNeighbors.Keys)
                {
                    var neighbor = DijkstraWaypoints[neighborName];
                    var alt = distances[currentWaypoint._name] + currentWaypoint._distanceToNeighbors[neighborName];
                    if (alt < distances[neighbor._name])
                    {
                        distances[neighbor._name] = alt;
                        previousNodes[neighbor._name] = currentWaypoint;
                        unvisited.Remove(neighbor);
                        unvisited.Enqueue(neighbor, alt);
                    }
                }
            }

            // Construct the shortest path
            var path = new List<DijkstraWaypoint>();
            var waypoint = endDijkstraWaypoint;
            while (waypoint != null)
            {
                path.Insert(0, waypoint);
                LightUpWaypoint(waypoint._name);
                waypoint = previousNodes[waypoint._name];
            }

            for (int i = 0; i < path.Count; i++)
            {
                Debug.Log("[" + i + "]" + path[i]._name);
            }

            _path = path.ToArray();

            return path.ToArray();
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
            int index = data.FindIndex(pair => pair.Key == waypoint);
            if (index == -1)
            {
                throw new ArgumentException("The specified waypoint is not in the queue.");
            }
            data.RemoveAt(index);
        }
    }
}