using GVR.Debugging;
using GVR.Extensions;
using GVR.Manager;
using GVR.Phoenix.Data;
using System.Collections.Generic;
using System.Linq;
using GVR.Phoenix.Interface;
using GVR.World;
using UnityEngine;

namespace GVR.Creator
{
    public static class WaynetCreator
    {
        public static void Create(GameObject root, WorldData world)
        {
            var waynetObj = new GameObject(string.Format("Waynet"));

            waynetObj.transform.parent = root.transform;

            CreateWaypoints(waynetObj, world);
            CreateDijkstraWaypoints(world);
            CreateWaypointEdges(waynetObj, world);
        }

        private static void CreateDijkstraWaypoints(WorldData world)
        {
            CreateDijkstraWaypointEntries(world);
            AttachWaypointPositionToDijkstraEntries();
            CalculateDijkstraNeighbourDistances();
        }

        private static void CreateDijkstraWaypointEntries(WorldData world)
        {
            Dictionary<string, DijkstraWaypoint> dijkstraWaypoints = new();
            var wayEdges = world.waypointEdges;
            var wayPoints = world.waypoints;

            // Using LINQ to transform wayEdges into DijkstraWaypoints.
            dijkstraWaypoints = wayEdges.SelectMany(edge => new[]
                {
                    // For each edge, create two entries: one for each direction of the edge.
                    // 'a' is the source waypoint, 'b' is the destination waypoint.
                    new { a = wayPoints[(int)edge.a], b = wayPoints[(int)edge.b] },
                    new { a = wayPoints[(int)edge.b], b = wayPoints[(int)edge.a] }
                })
                .GroupBy(x => x.a.name) // Group the entries by the name of the source waypoint.
                .ToDictionary(g => g.Key, g => new DijkstraWaypoint(g.Key) // Transform each group into a DijkstraWaypoint.
                {
                    // The neighbors of the DijkstraWaypoint are the names of the destination waypoints in the group.
                    Neighbors = g.Select(x => x.b.name).ToList()
                });

            GameData.DijkstraWaypoints = dijkstraWaypoints;
        }

        private static void AttachWaypointPositionToDijkstraEntries()
        {
            foreach (var waypoint in GameData.DijkstraWaypoints)
            {
                var result = GameData.WayPoints.First(i => i.Key == waypoint.Key).Value.Position;
                waypoint.Value.Position = result;
            }
        }
        /// <summary>
        /// Needed for future calculations.
        /// </summary>
        private static void CalculateDijkstraNeighbourDistances()
        {
            foreach (var waypoint in GameData.DijkstraWaypoints.Values)
            {
                foreach (var neighbour in waypoint.Neighbors)
                {
                    if (waypoint.DistanceToNeighbors.ContainsKey(neighbour))
                        continue;
                    waypoint.DistanceToNeighbors.Add(neighbour, Vector3.Distance(waypoint.Position, GameData.DijkstraWaypoints[neighbour].Position));
                }
            }
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
            waypointEdgesObj.SetParent(parent);

            for (var i = 0; i < world.waypointEdges.Length; i++)
            {
                var edge = world.waypointEdges[i];
                var startPos = world.waypoints[(int)edge.a].position.ToUnityVector();
                var endPos = world.waypoints[(int)edge.b].position.ToUnityVector();
                var lineObj = new GameObject();

                lineObj.AddComponent<LineRenderer>();
                var lr = lineObj.GetComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Standard"));
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
                lr.SetPosition(0, startPos);
                lr.SetPosition(1, endPos);

                lineObj.name = $"{edge.a}->{edge.b}";
                lineObj.transform.position = startPos;
                lineObj.transform.parent = waypointEdgesObj.transform;
            }

        }
    }
}
