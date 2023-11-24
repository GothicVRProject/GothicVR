using System;
using System.Collections.Generic;
using UnityEngine;

namespace GVR
{
    public class DijkstraWaypoint : IComparable<DijkstraWaypoint>
    {
        public string Name = "";  // Used as index to find other data like position, underwater and probably isFree

        public double CoveredDistance = 0;  // Initial distance covered from the source node is 0

        public double SummedDistance = 99999;  // Initialized to a large number to represent infinity at the start of the algorithm. 
                                               // This is used for the priority queue to determine which node to visit next (smaller distance = higher priority)

        public Dictionary<string, float> DistanceToNeighbors = new Dictionary<string, float>();  // Stores the distances to neighboring nodes

        public List<string> Neighbors = new List<string>();  // Stores the neighboring nodes

        public Vector3 Position = new Vector3();  // Stores the position of the node

        public string FatherWP = "";  // Stores the previous node in the shortest path

        public DijkstraWaypoint(string name)
        {
            Name = name;
        }

        public int CompareTo(DijkstraWaypoint other)
        {
            throw new NotImplementedException();
        }
    }
}
