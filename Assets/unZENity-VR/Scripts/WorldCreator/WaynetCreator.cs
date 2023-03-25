﻿using UnityEngine;
using UZVR.Phoenix.World;
using UZVR.Util;

namespace UZVR.WorldCreator
{
    public class WaynetCreator: SingletonBehaviour<WaynetCreator>
    {
        public void Create(GameObject root, BWorld world)
        {
            var waynetObj = new GameObject(string.Format("Waynet"));
            waynetObj.transform.parent = root.transform;

            var waypointsObj = new GameObject(string.Format("Waypoints"));
            waypointsObj.transform.parent = waynetObj.transform;

            foreach (var waypoint in world.waypoints)
            {
                var wpobject = GameObject.CreatePrimitive(PrimitiveType.Cube);

                wpobject.name = waypoint.name;
                wpobject.transform.position= waypoint.position;

                wpobject.transform.parent = waypointsObj.transform;
            }


            var waypointEdgesObj = new GameObject(string.Format("Edges"));
            waypointEdgesObj.transform.parent = waynetObj.transform;

            for (int i=0; i<world.waypointEdges.Count; i++)
            {
                var edge = world.waypointEdges[i];
                var startPos = world.waypoints[(int)edge.a].position;
                var endPos = world.waypoints[(int)edge.b].position;
                var lineObj = new GameObject();

                lineObj.AddComponent<LineRenderer>();
                LineRenderer lr = lineObj.GetComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Standard"));
//                lr.SetColors(color, color);
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
                lr.SetPosition(0, startPos);
                lr.SetPosition(1, endPos);

                lineObj.name = string.Format("{0}->{1}", edge.a, edge.b);
                lineObj.transform.position = startPos;
                lineObj.transform.parent = waypointEdgesObj.transform;
            }
        }
    }
}
