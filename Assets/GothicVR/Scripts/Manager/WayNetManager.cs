using System.Collections.Generic;
using System.Linq;
using GVR.Util;
using GVR.Vob;
using UnityEngine;

namespace GVR.Manager
{
    public class WayNetManager : SingletonBehaviour<WayNetManager>
    {
        public Dictionary<string, FreePoint> FreePoints = new();
        
        public List<FreePoint> FindFreePointsWithName(Vector3 lookupPosition, string namePart, float maxDistance)
        {
            var matchingFreePoints = FreePoints
                .Where(pair => pair.Key.Contains(namePart))
                .Where(pair => Vector3.Distance(lookupPosition, pair.Value.Position) <= maxDistance) // PF is in range
                .OrderBy(pair => Vector3.Distance(lookupPosition, pair.Value.Position)) // order from nearest to farthest
                .Select(pair => pair.Value);
            
            return matchingFreePoints.ToList();
        }
    }
}