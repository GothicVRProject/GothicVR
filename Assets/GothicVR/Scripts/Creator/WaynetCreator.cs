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

        public void Create(GameObject root, WorldData world)
        {
            var waynetObj = new GameObject(string.Format("Waynet"));
            waynetObj.transform.parent = root.transform;


            CreateWaypoints(waynetObj, world);
            CreateWaypointEdges(waynetObj, world);
        }

        private void CreateWaypoints(GameObject parent, WorldData world)
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().CreateWaypoints)
                return;

            var waypointsObj = new GameObject(string.Format("Waypoints"));
            waypointsObj.transform.parent = parent.transform;

            foreach (var waypoint in world.waypoints)
            {
                var wpobject = GameObject.CreatePrimitive(PrimitiveType.Cube);

                wpobject.name = waypoint.name;
                wpobject.transform.position = waypoint.position.ToUnityVector();

                wpobject.transform.parent = waypointsObj.transform;
            }
        }

        private void CreateWaypointEdges(GameObject parent, WorldData world)
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().CreateWaypointEdges) //will be deleted sooner or later, since the lines below are needed for routines
                return;

            for (int i = 0; i < world.waypointEdges.Length; i++)
            {
                edges[i].a = world.waypointEdges[i].a;
                edges[i].b = world.waypointEdges[i].b;
                var startPos = world.waypoints[(int)edges[i].a].position.ToUnityVector();
                var endPos = world.waypoints[(int)edges[i].b].position.ToUnityVector();
                var edgeVector = startPos-endPos;
                edges[i].edgeLength = edgeVector.magnitude;
                edges[i].edgeName = world.waypoints[(int)edges[i].a].name + world.waypoints[(int)edges[i].b].name;
                edgesDict.TryAdd(edges[i].edgeName, edges[i]);

                DrawLine(ref parent, edges[i]);
            }
        }

        void DrawLine(ref GameObject parent, EdgeData edge)
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().CreateWaypointEdges) //again, because the part above will later be always in the game
                return;

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

            lineObj.name = string.Format("{0}->{1}", edge.a, edge.b);
            lineObj.transform.position = edge.startPos;
            lineObj.transform.parent = waypointEdgesObj.transform;
        }
    }
}
