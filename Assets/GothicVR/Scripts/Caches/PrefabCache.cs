using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GVR.Caches
{
    public static class PrefabCache
    {
        private static readonly Dictionary<PrefabType, GameObject> Cache = new();

        public enum PrefabType
        {
            Npc,
            WayPoint,
            Vob,
            VobAnimate,
            VobItem,
            VobContainer,
            VobDoor,
            VobInteractable,
            VobMovable,
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
                PrefabType.Vob => "Prefabs/Vobs/Vob",
                PrefabType.VobAnimate => "Prefabs/Vobs/zCVobAnimate",
                PrefabType.VobItem => "Prefabs/Vobs/oCItem",
                PrefabType.VobContainer => "Prefabs/Vobs/oCMobContainer",
                PrefabType.VobDoor => "Prefabs/Vobs/oCMobDoor",
                PrefabType.VobInteractable => "Prefabs/Vobs/oCMobInter",
                PrefabType.VobMovable => "Prefabs/Vobs/oCMobMovable",
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
            if (Cache.TryGetValue(type, out var prefab))
                return Object.Instantiate(prefab);
            
            var path = GetPath(type);
            var newPrefab = Resources.Load<GameObject>(path);

            Cache[type] = newPrefab;
            return Object.Instantiate(newPrefab);
        }

        public static void Dispose()
        {
            Cache.Clear();
        }
    }
}
