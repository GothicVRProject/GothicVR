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
                LightUpWaypoint(start, Color.green);
                LightUpWaypoint(end, Color.green);
                Debug.Log("start: " + waypointsPosition[0]);
                Debug.Log("end: " + waypointsPosition[1]);
                StartCoroutine(FindFastestPath());
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
                    waypoint.Value._position = result;
                }
            }
            Debug.Log("DijkstraWaypoints set");
            CalculateNeighbourDistances();

        }

        public IEnumerator FindFastestPath(string startWaypoint = null, string endWaypoint = null)
        {
            if (startWaypoint == null || endWaypoint == null)
            {
                startWaypoint = start;
                endWaypoint = end;
            }
            var startDijkstraWaypoint = DijkstraWaypoints[startWaypoint];
            var endDijkstraWaypoint = DijkstraWaypoints[endWaypoint];

            var previousNodes = new Dictionary<string, DijkstraWaypoint>();
            var unvisited = new PriorityQueue();

            foreach (var waypointx in DijkstraWaypoints.Values)
            {
                if (waypointx._name == startWaypoint)
                {
                    waypointx._summedDistance = 0;
                }
                else
                {
                    waypointx._summedDistance = double.MaxValue;
                }

                unvisited.Enqueue(waypointx, waypointx._summedDistance);
                previousNodes[waypointx._name] = null;
            }

            while (unvisited.Count > 0)
            {
                var currentWaypoint = unvisited.Dequeue();
                Debug.Log(currentWaypoint._name + " " + currentWaypoint._summedDistance);
                LightUpWaypoint(currentWaypoint._name, Color.yellow);

                foreach (var neighborName in currentWaypoint._distanceToNeighbors.Keys)
                {
                    var neighbor = DijkstraWaypoints[neighborName];
                    var alt = currentWaypoint._summedDistance + currentWaypoint._distanceToNeighbors[neighborName];
                    if (alt < neighbor._summedDistance || neighbor._name == endDijkstraWaypoint._name)
                    {
                        neighbor._summedDistance = alt;
                        previousNodes[neighbor._name] = currentWaypoint;
                        unvisited.Remove(neighbor);
                        unvisited.Enqueue(neighbor, alt + Heuristic(neighbor, endDijkstraWaypoint));
                    }
                }
                
                // Check if a valid path from start to end has been found
                var lastChecked = endWaypoint;
                while (lastChecked != null && previousNodes[lastChecked] != null)
                {
                    lastChecked = previousNodes[lastChecked]._name;
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
                LightUpWaypoint(waypoint._name, Color.green);
                waypoint = previousNodes[waypoint._name];
            }

            for (int i = 0; i < path.Count; i++)
            {
                Debug.Log("[" + i + "]" + path[i]._name);
            }

            var testing = previousNodes.Where(x => x.Value != null).Select(x => x).ToList();

            _path = path.ToArray();

            if(_path.Length == 1)
            {
                _path.Append(_path[0]);
            }

            yield return path.ToArray();
        }

        private double Heuristic(DijkstraWaypoint a, DijkstraWaypoint b)
        {
            double euclidean = Vector3.Distance(a._position, b._position);

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
            int index = data.FindIndex(pair => pair.Key._name == waypoint._name);
            if (index == -1)
            {
                //throw new ArgumentException("The specified waypoint is not in the queue.");
                Debug.Log("The specified waypoint " + waypoint._name + " is not in the queue.");
                return;
            }
            data.RemoveAt(index);
        }
    }
}