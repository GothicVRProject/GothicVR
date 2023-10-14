using System;
using System.Collections.Generic;
using GothicVR.Vob;
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
            VobInteractable,
            VobSpot,
            VobMusic,
            VobSound,
            VobSoundDaytime,
            XRDeviceSimulator
        }

        private string GetPath(PrefabType type)
        {
            return type switch
            {
                PrefabType.VobItem => "Prefabs/Vobs/oCItem",
                PrefabType.VobInteractable => "Prefabs/Vobs/Interactable",
                PrefabType.VobSpot => "Prefabs/Vobs/zCVobSpot",
                PrefabType.VobMusic => "Prefabs/Vobs/oCZoneMusic",
                PrefabType.VobSound => "Prefabs/Vobs/zCVobSound",
                PrefabType.VobSoundDaytime => "Prefabs/Vobs/zCVobSoundDaytime",
                PrefabType.XRDeviceSimulator => "Prefabs/XR Device Simulator",
                _ => throw new Exception($"Enum value {type} not yet defined.")
            };
        }

        public GameObject TryGetObject(PrefabType type)
        {
            if (prefabCache.TryGetValue(type, out var prefab))
                return Instantiate(prefab);
            
            var path = GetPath(type);
            var newPrefab = Resources.Load<GameObject>(path);

            prefabCache[type] = newPrefab;
            return Instantiate(newPrefab);
        }
    }
}
