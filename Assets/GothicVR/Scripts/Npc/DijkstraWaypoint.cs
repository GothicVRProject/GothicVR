using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GVR
{
    public class DijkstraWaypoint
    {
        public string name = "";                                //as index to find other data like position, underwater and probably isFree

        public double distanceToDestination = Double.MaxValue;  //probably isnt needed

        public double coveredDistance;

        public List<string> neighbors = new List<string>();

        public string fatherWP = "";
    }
}
