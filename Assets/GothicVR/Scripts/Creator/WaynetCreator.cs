using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Debugging;
using GVR.Extensions;
using GVR.Globals;
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
            var wayEdges = world.WayNet.Edges;
            var wayPoints = world.WayNet.Points;

            // Using LINQ to transform wayEdges into DijkstraWaypoints.
            dijkstraWaypoints = wayEdges.SelectMany(edge => new[]
                {
                    // For each edge, create two entries: one for each direction of the edge.
                    // 'a' is the source waypoint, 'b' is the destination waypoint.
                    new { a = wayPoints[(int)edge.A], b = wayPoints[(int)edge.B] },
                    new { a = wayPoints[(int)edge.B], b = wayPoints[(int)edge.A] }
                })
                .GroupBy(x => x.a.Name) // Group the entries by the name of the source waypoint.
                .ToDictionary(g => g.Key, g => new DijkstraWaypoint(g.Key) // Transform each group into a DijkstraWaypoint.
                {
                    // The neighbors of the DijkstraWaypoint are the names of the destination waypoints in the group.
                    Neighbors = g.Select(x => x.b.Name).ToList()
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
            if (!FeatureFlags.I.createWaypoints)
                return;

            var waypointsObj = new GameObject(string.Format("Waypoints"));
            waypointsObj.SetParent(parent);

            foreach (var waypoint in world.WayNet.Points)
            {
                var wpObject = PrefabCache.TryGetObject(PrefabCache.PrefabType.WayPoint);

                // We remove the Renderer only if not wanted.
                // TODO - Can be outsourced to a different Prefab-variant without Renderer for a fractal of additional performance. ;-)
                if (!FeatureFlags.I.createWayPointMeshes)
                    Object.Destroy(wpObject.GetComponent<MeshRenderer>());

                wpObject.tag = Constants.SpotTag;
                wpObject.name = waypoint.Name;
                wpObject.transform.position = waypoint.Position.ToUnityVector();

                wpObject.SetParent(waypointsObj);
            }
        }

        private static void CreateWaypointEdges(GameObject parent, WorldData world)
        {
            if (!FeatureFlags.I.createWaypointEdgeMeshes)
                return;

            var waypointEdgesObj = new GameObject(string.Format("Edges"));
            waypointEdgesObj.SetParent(parent);

            for (var i = 0; i < world.WayNet.Edges.Count; i++)
            {
                var edge = world.WayNet.Edges[i];
                var startPos = world.WayNet.Points[(int)edge.A].Position.ToUnityVector();
                var endPos = world.WayNet.Points[(int)edge.B].Position.ToUnityVector();
                var lineObj = new GameObject();

                lineObj.AddComponent<LineRenderer>();
                var lr = lineObj.GetComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Standard"));
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
                lr.SetPosition(0, startPos);
                lr.SetPosition(1, endPos);

                lineObj.name = $"{edge.A}->{edge.B}";
                lineObj.transform.position = startPos;
                lineObj.transform.parent = waypointEdgesObj.transform;
            }

        }
    }
}
