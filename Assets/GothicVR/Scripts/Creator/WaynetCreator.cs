using GVR.Debugging;
using GVR.Extensions;
using GVR.Manager;
using GVR.Phoenix.Data;
using UnityEngine;

namespace GVR.Creator
{
    public static class WaynetCreator
    {
        public static void Create(GameObject root, WorldData world)
        {
            var waynetObj = new GameObject(string.Format("Waynet"));

            DijkstraPathFinder pathFinder = waynetObj.AddComponent<DijkstraPathFinder>();
            waynetObj.transform.parent = root.transform;

            CreateWaypoints(waynetObj, world);
            pathFinder.SetDijkstraWaypointsOriginal(world); // Has to be here to get the waypoints position

            CreateWaypointEdges(waynetObj, world);
        }

        private static void CreateWaypoints(GameObject parent, WorldData world)
        {
            if (!FeatureFlags.I.CreateWaypoints)
                return;

            var waypointsObj = new GameObject(string.Format("Waypoints"));
            waypointsObj.SetParent(parent);

            foreach (var waypoint in world.waypoints)
            {
                GameObject wpObject;
                if (FeatureFlags.I.createWayPointMeshes)
                {
                    wpObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wpObject.transform.localScale = new(0.5f, 0.5f, 0.5f);
                    GameObject.Destroy(wpObject.GetComponent<Collider>());
                }
                else
                    wpObject = new GameObject();

                wpObject.tag = ConstantsManager.SpotTag;
                wpObject.name = waypoint.name;
                wpObject.transform.position = waypoint.position.ToUnityVector();

                wpObject.SetParent(waypointsObj);
            }
        }

        private static void CreateWaypointEdges(GameObject parent, WorldData world)
        {
            if (!FeatureFlags.I.createWaypointEdgeMeshes)
                return;

            var waypointEdgesObj = new GameObject(string.Format("Edges"));
            waypointEdgesObj.transform.parent = parent.transform;

            for (int i = 0; i < world.waypointEdges.Length; i++)
            {
                var edge = world.waypointEdges[i];
                var startPos = world.waypoints[(int)edge.a].position.ToUnityVector();
                var endPos = world.waypoints[(int)edge.b].position.ToUnityVector();
                var lineObj = new GameObject();

                lineObj.AddComponent<LineRenderer>();
                LineRenderer lr = lineObj.GetComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Standard"));
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
