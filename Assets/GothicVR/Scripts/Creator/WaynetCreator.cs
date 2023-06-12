using UnityEngine;
using UnityEngine.UI;
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
                //Display Text start
                var textObj = new GameObject("Text");
                textObj.transform.parent = wpobject.transform;
                var text = textObj.AddComponent<TextMesh>();

                text.text = world.waypoints[i].name;
                text.fontSize = 12; 
                text.anchor = TextAnchor.MiddleCenter; 
                text.alignment = TextAlignment.Center; 
                //Display Text end
                wpobject.name = world.waypoints[i].name;
                wpobject.transform.position = world.waypoints[i].position.ToUnityVector();

                wpobject.transform.parent = waypointsObj.transform;
                waypoints.Add(new(wpobject.name, wpobject.transform.position));
                WriteWaypointDataToDict(i);
            }
        }
        private void WriteWaypointDataToDict(int i)
        {
            waypointsDict.Add(waypoints[i].name.ToUpper(), waypoints[i]);
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
                
                //if (SingletonBehaviour<DebugSettings>.GetOrCreate().CreateWaypointEdges)
                    DrawEdgeLine(ref parent, edges[i]);
            }
        }
        private void CopyWorldDataToEdgesList(WorldData world, int i)
        {
            edges[i].startPointID = world.waypointEdges[i].a;
            edges[i].endPointID = world.waypointEdges[i].b;
        }
        private void CalculateEdgeMagnitude(WorldData world, int i)
        {
            var startPos = world.waypoints[(int)edges[i].startPointID].position.ToUnityVector();
            var endPos = world.waypoints[(int)edges[i].endPointID].position.ToUnityVector();
            var edgeVector = startPos - endPos;
            edges[i].edgeLength = edgeVector.magnitude;
        }
        private void CalculateEdgeName(WorldData world, int i)
        {
            edges[i].edgeName = world.waypoints[(int)edges[i].startPointID].name + world.waypoints[(int)edges[i].endPointID].name;
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

            lineObj.name = string.Format("{0}->{1}", edge.startPointID, edge.endPointID);
            lineObj.transform.position = edge.startPos;
            lineObj.transform.parent = waypointEdgesObj.transform;
            lr.enabled = true;
        }
        #endregion

        #region AddNeighbors
        private void AddWaypointNeighbors()
        {
            for (int i = 0; i < edges.Count; i++)
            {
                var startID = (int)edges[i].startPointID;
                var endID = (int)edges[i].endPointID;
                var bla = waypoints;
                var neighborA = waypoints[endID];
                var neighborB = waypoints[startID];
                waypoints[startID].AddNeighbor(neighborA);
                waypoints[endID].AddNeighbor(neighborB);
                
            }
        }

        #endregion


    }
}
