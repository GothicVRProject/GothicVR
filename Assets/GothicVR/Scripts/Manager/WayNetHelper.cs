using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Extensions;
using GVR.Phoenix.Interface;
using GVR.Vob.WayNet;
using JetBrains.Annotations;
using UnityEngine;

namespace GVR.Manager
{
    public static class WayNetHelper
    {
        /// <summary>
        /// Check within WayPoints and FreePoints if an entry exists.
        /// </summary>
        /// <param name="pointName"></param>
        /// <returns></returns>
        [CanBeNull]
        public static WayNetPoint GetWayNetPoint(string pointName)
        {
            var wayPoint = GameData.WayPoints
                .FirstOrDefault(item => item.Key.Equals(pointName, StringComparison.OrdinalIgnoreCase))
                .Value;
            if (wayPoint != null)
                return wayPoint;
            
            var freePoint = GameData.FreePoints
                .FirstOrDefault(pair => pair.Key.Equals(pointName, StringComparison.OrdinalIgnoreCase))
                .Value;
            return freePoint;
        }
        
        public static List<FreePoint> FindFreePointsWithName(Vector3 lookupPosition, string namePart, float maxDistance)
        {
            var matchingFreePoints = GameData.FreePoints
                .Where(pair => pair.Key.Contains(namePart))
                .Where(pair => Vector3.Distance(lookupPosition, pair.Value.Position) <= maxDistance) // PF is in range
                .OrderBy(pair => Vector3.Distance(lookupPosition, pair.Value.Position)) // order from nearest to farthest
                .Select(pair => pair.Value);
            
            return matchingFreePoints.ToList();
        }

        public static WayPoint FindNearestWayPoint(Vector3 lookupPosition)
        {
            var nearestWayPoint = GameData.WayPoints
                .OrderBy(pair => Vector3.Distance(pair.Value.Position, lookupPosition))
                .First();

            return nearestWayPoint.Value;
        }

        [CanBeNull]
        public static FreePoint FindNearestFreePoint(Vector3 lookupPosition, string fpNamePart)
        {
            return GameData.FreePoints
                .Where(pair => pair.Value.Name.ContainsIgnoreCase(fpNamePart))
                .OrderBy(pair => Vector3.Distance(pair.Value.Position, lookupPosition))
                .Select(pair => pair.Value)
                .FirstOrDefault();
        }
    }
}
