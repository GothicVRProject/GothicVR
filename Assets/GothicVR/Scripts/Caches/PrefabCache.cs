using System;
using System.Collections.Generic;
using GVR.Phoenix.Util;
using GVR.Util;
using UnityEngine;

namespace GVR.Caches
{
    public class PrefabCache : SingletonBehaviour<PrefabCache>
    {
        public GameObject prefabRootGo;
        
        private Dictionary<PrefabType, GameObject> prefabCache = new();

        public enum PrefabType
        {
            VobItem,
            XRDeviceSimulator
        }

        private string GetPath(PrefabType type)
        {
            switch (type)
            {
                case PrefabType.VobItem:
                    return "Prefabs/Vobs/oCItem";
                case PrefabType.XRDeviceSimulator:
                    return "Prefabs/XR Device Simulator";
                default:
                    throw new Exception($"Enum value {type} not yet defined.");
            }
        }

        public GameObject TryGetObject(PrefabType type)
        {
            if (prefabCache.TryGetValue(type, out GameObject prefab))
                return Instantiate(prefab);
            
            var path = GetPath(type);
            var newPrefab = Resources.Load<GameObject>(path);

            prefabCache[type] = newPrefab;
            return Instantiate(newPrefab);
        }
    }
}
