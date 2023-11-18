using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace GVR
{
    public class DijkstraWaypoint : IComparable<DijkstraWaypoint>
    {
        public string Name = "";                                //as index to find other data like position, underwater and probably isFree

        public double CoveredDistance = 0;

        public double SummedDistance = 99999;
        
        public Dictionary<string, float> DistanceToNeighbors = new Dictionary<string, float>();

        public List<string> Neighbors = new List<string>();

        public Vector3 Position = new Vector3();

        public string FatherWP = "";

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
