using System;
using System.Collections.Generic;
using GVR.Context;
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
                PrefabType.VobContainer => "HVR/Prefabs/Vobs/oCMobContainer", // FIXME - Need to implement a lookup logic for real adapter instead of hard coding it.
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

        /// <summary>
        /// Lookup is done in following places:
        /// 1. CONTEXT_NAME/Prefabs/... - overwrites lookup path below, used for specific prefabs, for current context (HVR, Flat, ...)
        /// 2. Prefabs/... - Located inside core module (GVR), if we don't need special handling.
        /// </summary>
        public static GameObject TryGetObject(PrefabType type)
        {
            if (Cache.TryGetValue(type, out var prefab))
                return Object.Instantiate(prefab);

            var defaultPath = GetPath(type);
            var contextPath = $"{GVRContext.InteractionAdapter.GetContextName()}/{defaultPath}";

            foreach (var path in new[]{contextPath, defaultPath})
            {
                var newPrefab = Resources.Load<GameObject>(path);

                if (newPrefab == null)
                    continue;

                Cache[type] = newPrefab;
                return Object.Instantiate(newPrefab);
            }

            throw new ArgumentException($"No suitable prefab found for >{type}<");
        }

        public static void Dispose()
        {
            Cache.Clear();
        }
    }
}
