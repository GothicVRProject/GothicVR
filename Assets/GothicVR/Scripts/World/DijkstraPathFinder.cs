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
using GVR.Creator;
using Unity.VisualScripting;

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

        public void SetDijkstraWaypointsOriginal(Dictionary<string, DijkstraWaypoint> dijkstraWapoints)
        {
            DijkstraWaypoints = dijkstraWapoints;
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
                FindFastestPath();
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

        public DijkstraWaypoint[] FindFastestPath(string startWaypoint = null, string endWaypoint = null)
        {
            // Start the timer
            var watch = System.Diagnostics.Stopwatch.StartNew(); 

            // If start or end waypoints are not provided, use the default Start and End waypoints
            if (startWaypoint == null || endWaypoint == null)
            {
                startWaypoint = Start;
                endWaypoint = End;
            }

            // Get the start and end waypoints from the DijkstraWaypoints dictionary
            var startDijkstraWaypoint = DijkstraWaypoints[startWaypoint];
            var endDijkstraWaypoint = DijkstraWaypoints[endWaypoint];

            // Initialize the previousNodes dictionary to keep track of the path
            var previousNodes = new Dictionary<string, DijkstraWaypoint>();
            // Initialize the unvisited priority queue to keep track of waypoints to be visited
            var unvisited = new PriorityQueue();

            // For each waypoint in DijkstraWaypoints 
            foreach (var waypointx in DijkstraWaypoints.Values)
            {
                // If the waypoint is the start waypoint, set its SummedDistance to 0
                if (waypointx.Name == startWaypoint)
                {
                    waypointx.SummedDistance = 0;
                }
                // Otherwise, set its SummedDistance to infinity
                else
                {
                    waypointx.SummedDistance = double.MaxValue;
                }

                // Add the waypoint to the unvisited set and set its previous node to null
                unvisited.Enqueue(waypointx, waypointx.SummedDistance);
                // Add the waypoint to the previousNodes dictionary and set its previous node to null
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

                    // If a shorter path to the neighbor is found, update its distance and previous node.
                    if (alt < neighbor.SummedDistance)
                    {
                        neighbor.SummedDistance = alt;
                        previousNodes[neighbor.Name] = currentWaypoint;

                        // If the neighbor is in the unvisited set, update its priority.
                        if (unvisited.Contains(neighbor))
                        {
                            unvisited.UpdatePriority(neighbor, alt);
                        }
                        // Otherwise, add it to the unvisited set with the new priority.
                        else
                        {
                            unvisited.Enqueue(neighbor, alt);
                        }
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

            if (Path.Length == 1)
            {
                Path.Append(Path[0]);
            }

            path.Reverse();

            // Stop the timer
            watch.Stop();
            // Print the time elapsed
            Debug.Log($"Time spent for finding the path: {watch.Elapsed}");
            // debug amount of nodes checked
            Debug.Log("Nodes checked: " + testing.Count);
            // debug amount of nodes in path
            Debug.Log("Nodes in path: " + path.Count);

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

        public bool Contains(DijkstraWaypoint item)
        {
            // Check if the queue contains the item by comparing the names of the waypoints
            return data.Select(x => x.Key.Name == item.Name).Count() > 0;
        }

        public void UpdatePriority(DijkstraWaypoint item, double priority)
        {
            // Find the index of the item
            int index = data.FindIndex(pair => pair.Key.Name == item.Name);
            if (index == -1)
            {
                // If the item is not in the queue, throw an exception (it should never do that so this is a good way to catch bugs)
                throw new ArgumentException("Item does not exist in the queue.");
            }

            // Get the old priority of the item
            double oldPriority = data[index].Value;
            // Update the priority of the item in the queue
            data[index] = new KeyValuePair<DijkstraWaypoint, double>(item, priority);

            // If the new priority is less than the old priority, sift up
            if (priority < oldPriority)
            {
                SiftUp(index);
            }
            // If the new priority is greater than the old priority, sift down
            else if (priority > oldPriority)
            {
                SiftDown(index);
            }
        }

        //  "Sift" in the context of a heap data structure refers to the process of adjusting 
        //  the position of an element to maintain the heap property. This is done by moving
        //  the element up or down in the heap until the heap property is satisfied.

        private void SiftUp(int index)
        {
            int parentIndex = (index - 1) / 2;
            while (index > 0 && data[index].Value < data[parentIndex].Value)
            {
                Swap(index, parentIndex);
                index = parentIndex;
                parentIndex = (index - 1) / 2;
            }
        }

        private void SiftDown(int index)
        {
            int leftChildIndex = index * 2 + 1;
            int rightChildIndex = index * 2 + 2;
            int smallestChildIndex = leftChildIndex;

            if (rightChildIndex < data.Count && data[rightChildIndex].Value < data[leftChildIndex].Value)
            {
                smallestChildIndex = rightChildIndex;
            }

            while (smallestChildIndex < data.Count && data[smallestChildIndex].Value < data[index].Value)
            {
                Swap(index, smallestChildIndex);
                index = smallestChildIndex;
                leftChildIndex = index * 2 + 1;
                rightChildIndex = index * 2 + 2;
                smallestChildIndex = leftChildIndex;

                if (rightChildIndex < data.Count && data[rightChildIndex].Value < data[leftChildIndex].Value)
                {
                    smallestChildIndex = rightChildIndex;
                }
            }
        }

        private void Swap(int index1, int index2)
        {
            var temp = data[index1];
            data[index1] = data[index2];
            data[index2] = temp;
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