using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GVR.Caches
{
    public static class PrefabCache
    {
        private static Dictionary<PrefabType, GameObject> prefabCache = new();

        public enum PrefabType
        {
            Npc,
            WayPoint,
            VobItem,
            VobInteractable,
            VobSpot,
            VobPfx,
            VobMusic,
            VobSound,
            VobSoundDaytime,
            XRDeviceSimulator
        }

        private static string GetPath(PrefabType type)
        {
            return type switch
            {
                PrefabType.Npc => "Prefabs/Npc",
                PrefabType.WayPoint => "Prefabs/WayPoint",
                PrefabType.VobItem => "Prefabs/Vobs/oCItem",
                PrefabType.VobInteractable => "Prefabs/Vobs/Interactable",
                PrefabType.VobSpot => "Prefabs/Vobs/zCVobSpot",
                PrefabType.VobPfx => "Prefabs/Vobs/vobPfx",
                PrefabType.VobMusic => "Prefabs/Vobs/oCZoneMusic",
                PrefabType.VobSound => "Prefabs/Vobs/zCVobSound",
                PrefabType.VobSoundDaytime => "Prefabs/Vobs/zCVobSoundDaytime",
                PrefabType.XRDeviceSimulator => "Prefabs/XR Device Simulator",
                _ => throw new Exception($"Enum value {type} not yet defined.")
            };
        }

        public static GameObject TryGetObject(PrefabType type)
        {
            if (prefabCache.TryGetValue(type, out var prefab))
                return Object.Instantiate(prefab);
            
            var path = GetPath(type);
            var newPrefab = Resources.Load<GameObject>(path);

            prefabCache[type] = newPrefab;
            return Object.Instantiate(newPrefab);
        }
    }
}
