using System;
using System.Collections.Generic;
using UnityEngine;

namespace GVR.Data
{
    /// <summary>
    /// This class is serialized via JsonUtility.
    /// Knwon limitations:
    /// 1. It doesn't support using the List of points directly as JsonUtilities.FromJson(List<Point>).
    /// 2. Dictionaries aren't supported as well. We therefore fill them after loading as temp variables.
    /// </summary>
    public class VobItemAttachPoints
    {
        public  List<Point> points = new();
        public Dictionary<string, Point> dict = new();

        [Serializable]
        public class Point
        {
            public string name;
            public bool isDynamicAttach;
            public Vector3 position;
            public Vector3 rotation;
        }
    }
}
