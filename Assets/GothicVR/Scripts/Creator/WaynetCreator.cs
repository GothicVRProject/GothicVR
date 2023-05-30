using UnityEngine;
using GVR.Demo;
using GVR.Phoenix.Data;
using GVR.Phoenix.Util;
using GVR.Util;
using GVR.Phoenix.Data.Vm.Gothic;
using System.Collections.Generic;

namespace GVR.Creator
{
    public class WaynetCreator: SingletonBehaviour<WaynetCreator>
    {
        static Dictionary<string, EdgeData> edgesDict = new();
        static List<EdgeData> edges = new();
        public static Dictionary<string, WaypointRelationData> waypointsDict = new();
        static List<WaypointRelationData> waypoints = new();

        public void Create(GameObject root, WorldData world)
        {
            var waynetObj = new GameObject(string.Format("Waynet"));
            waynetObj.transform.parent = root.transform;


            CreateWaypoints(waynetObj, world);
            CreateWaypointEdges(waynetObj, world);
            AddWaypointNeighbors();
        }

        #region CreateWayPoints
        private void CreateWaypoints(GameObject parent, WorldData world)
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().CreateWaypoints)
                return;

            var waypointsObj = new GameObject(string.Format("Waypoints"));
            waypointsObj.transform.parent = parent.transform;

            for (int i = 0; i < world.waypoints.Length; i++)
            {
                
                var wpobject = GameObject.CreatePrimitive(PrimitiveType.Cube);

                wpobject.name = world.waypoints[i].name;
                wpobject.transform.position = world.waypoints[i].position.ToUnityVector();

                wpobject.transform.parent = waypointsObj.transform;
                //the index of waypoints list has to be the same as world.waypoints
                waypoints.Add(new(wpobject.name, wpobject.transform.position));
                WriteWaypointDataToDict(i);
            }
        }
        #endregion

        #region CreateWayPointEdges
        private void CreateWaypointEdges(GameObject parent, WorldData world)
        {
            for (int i = 0; i < world.waypointEdges.Length; i++)
            {
                edges.Add(new());
                CopyWorldDataToEdgesList(world, i);
                CalculateEdgeMagnitude(world, i);
                CalculateEdgeName(world, i);
                WriteEdgeDataToDict(i);
                
                if (SingletonBehaviour<DebugSettings>.GetOrCreate().CreateWaypointEdges)
                    DrawEdgeLine(ref parent, edges[i]);
            }
        }
        private void CopyWorldDataToEdgesList(WorldData world, int i)
        {
            edges[i].startID = world.waypointEdges[i].a;
            edges[i].endID = world.waypointEdges[i].b;
        }
        private void CalculateEdgeMagnitude(WorldData world, int i)
        {
            var startPos = world.waypoints[(int)edges[i].startID].position.ToUnityVector();
            var endPos = world.waypoints[(int)edges[i].endID].position.ToUnityVector();
            var edgeVector = startPos - endPos;
            edges[i].edgeLength = edgeVector.magnitude;
        }
        private void CalculateEdgeName(WorldData world, int i)
        {
            edges[i].edgeName = world.waypoints[(int)edges[i].startID].name + world.waypoints[(int)edges[i].endID].name;
        }
        private void WriteEdgeDataToDict(int i)
        {
            edgesDict.TryAdd(edges[i].edgeName, edges[i]);
        }
        private void DrawEdgeLine(ref GameObject parent, EdgeData edge)
        {
            var waypointEdgesObj = new GameObject(string.Format("Edges"));
            waypointEdgesObj.transform.parent = parent.transform;
            var lineObj = new GameObject();

            lineObj.AddComponent<LineRenderer>();
            LineRenderer lr = lineObj.GetComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Standard"));
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;
            lr.SetPosition(0, edge.startPos);
            lr.SetPosition(1, edge.endPos);

            lineObj.name = string.Format("{0}->{1}", edge.startID, edge.endID);
            lineObj.transform.position = edge.startPos;
            lineObj.transform.parent = waypointEdgesObj.transform;
        }
        #endregion

        #region AddNeighbors
        private void AddWaypointNeighbors()
        {
            for (int i = 0; i < edges.Count; i++)
            {
                var startID = (int)edges[i].startID;
                var endID = (int)edges[i].endID;
                var bla = waypoints;
                var neighborA = waypoints[endID];
                var neighborB = waypoints[startID];
                waypoints[startID].AddNeighbor(neighborA);
                waypoints[endID].AddNeighbor(neighborB);
                
            }
        }
        private void WriteWaypointDataToDict(int i)
        {
            waypointsDict.Add(waypoints[i].name, waypoints[i]);
        }
        #endregion


    }
}
