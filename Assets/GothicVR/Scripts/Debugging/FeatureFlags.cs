using System;
using System.Collections.Generic;
using GVR.Util;
using UnityEngine;
using ZenKit;
using ZenKit.Vobs;

namespace GVR.Debugging
{
    public class FeatureFlags: SingletonBehaviour<FeatureFlags>
    {
        public enum SunMovementPerformance
        {
            EveryIngameSecond,
            EveryIngameMinute,
            EveryIngameHour
        };

        [Header("__________Developer__________")]
        public bool skipMainMenu;
        public bool useXRDeviceSimulator;

        [Header("__________World__________")]
        public bool createWorldMesh;
        public bool createWaypoints;
        [Tooltip("True will render all pickables with dynamic attach points")]
        public bool vobItemsDynamicAttach;
        public bool showBarrier;

        [Header("__________World - Developer__________")]
        public bool createWayPointMeshes;
        public bool createWaypointEdgeMeshes;

        [Header("__________DayTime__________")]
        public bool enableDayTime;
        [Tooltip("Modifies how fast the in-game time passes")]
        [Range(0.5f, 300f)] public float TimeMultiplier;
        public SunMovementPerformance sunMovementPerformanceValue;
        [Range(0, 23)] public int startHour;
        [Range(0, 59)] public int startMinute;
        
        [Header("__________VOB__________")]
        public bool createVobs;
        public bool enableDecals;
        public bool vobCulling;
        public VobCullingGroupSetting vobCullingSmall;
        public VobCullingGroupSetting vobCullingMedium;
        public VobCullingGroupSetting vobCullingLarge;

        [Header("__________VOB - Developer__________")]
        [Tooltip("Leave blank if you want to spawn normal.")]
        public string spawnAtSpecificFreePoint;
        public bool drawFreePointMeshes;
        public bool drawVobCullingGizmos;
        [Tooltip("Set the VirtualObjectTypes to spawn only. (Ignored if empty)")]
        public List<VirtualObjectType> vobTypeToSpawn;
        
        [Header("__________NPCs__________")]
        public bool createOcNpcs;
        public bool enableNpcRoutines;

        [Header("__________NPCs - Developer__________")]
        [Tooltip("Add the Daedalus ids for NPCs to spawn. Take them from C_NPC instances. (Ignored if empty)")]
        public List<int> npcToSpawn;

        [Header("__________Audio__________")]
        public bool enableSounds;
        public bool enableMusic;
        public bool enableSoundCulling;

        [Header("__________Experimental / Do not use in Production__________")]
        [Tooltip("Looks already quite good for leaves.pfy in the forest, but fire is awkward.")]
        public bool enableVobParticles;

        [Header("__________Debug messages__________")]
        public LogLevel zenKitLogLevel;
        public bool showZspyLogs;
        public bool showZenKitVfsFileNotFoundErrors;
        public bool showMusicLogs;
        public bool showBarrierLogs;


        [Serializable]
        public class VobCullingGroupSetting
        {
            [Range(1f, 100f)] public float maxObjectSize;
            [Range(1f, 1000f)] public float cullingDistance;
        }
    }
}
