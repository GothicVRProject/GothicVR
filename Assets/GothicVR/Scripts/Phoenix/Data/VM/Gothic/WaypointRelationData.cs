using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GVR.Phoenix.Data.Vm.Gothic
{
    public class WaypointRelationData : MonoBehaviour
    {
        public new string name = "";
        public Vector3 position;
        public List<WaypointRelationData> neighbors = new();
        public float cost = 9999999999999999999999f; //Current Costs
        public float distanceToGoal; //Heuristic
        public float sum; //cost + heuristic
        public WaypointRelationData predecessor;

        public WaypointRelationData(string name, Vector3 position)
        {
            this.name = name;
            this.position = position;
        }

        public void AddNeighbor(WaypointRelationData neighbor)
        {
            neighbors.Add(neighbor);
        }
    }

}

