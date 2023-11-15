using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace GVR
{
    public class DijkstraWaypoint : IComparable<DijkstraWaypoint>
    {
        public string _name = "";                                //as index to find other data like position, underwater and probably isFree

        public double _coveredDistance = 0;

        public double _summedDistance = 99999;
        
        public Dictionary<string, float> _distanceToNeighbors = new Dictionary<string, float>();

        public List<string> _neighbors = new List<string>();

        public Vector3 _position = new Vector3();

        public string _fatherWP = "";

        public DijkstraWaypoint(string name)
        {
            _name = name;
        }

        public int CompareTo(DijkstraWaypoint other)
        {
            throw new NotImplementedException();
        }
    }
}
