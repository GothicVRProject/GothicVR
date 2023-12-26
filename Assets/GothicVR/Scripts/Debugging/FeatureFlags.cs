using System;
using System.Collections.Generic;
using GVR.Util;
using UnityEngine;
using UnityEngine.Serialization;

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

        [FormerlySerializedAs("SkipMainMenu")]
        [Header("__________Developer__________")]
        [Tooltip("This will be used within Editor mode only. No effect for Standalone.")]
        public bool skipMainMenu;
        public bool useXRDeviceSimulator;

        [Header("__________World__________")]
        public bool createWaypoints;
        [Tooltip("True will render all pickables with dynamic attach points")]
        public bool vobItemsDynamicAttach;

        [Header("__________World - Developer__________")]
        public bool createWayPointMeshes;
        public bool createWaypointEdgeMeshes;

        [Header("__________DayTime__________")]
        public bool enableDayTime;
        public SunMovementPerformance sunMovementPerformanceValue;
        [Range(0, 23)] public int startHour;
        [Range(0, 59)] public int startMinute;
        
        [Header("__________VOB__________")]
        [Tooltip("Only for Debug purposes. It'll not change functionality itself.")]
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
        
        
        [Header("__________NPCs__________")]
        public bool createOcNpcs;
        public bool enableNpcRoutines;

        [Header("__________NPCs - Developer__________")]
        [Tooltip("Add the Daedalus ids for NPCs to spawn. Take them from C_NPC instances. (Ignored if empty)")]
        public List<int> npcToSpawn;

        [Header("__________SPAMmy debug messages__________")]
        public bool showPhoenixDebugMessages;
        public bool showZspyLogs;
        public bool showPhoenixVfsFileNotFoundErrors;
        public bool showMusicLogs;

        [Header("__________Audio__________")]
        public bool enableSounds;
        public bool enableMusic;
        public bool enableSoundCulling;

        [Header("__________Experimental / Do not use in Production__________")]
        [Tooltip("Looks already quite good for leaves.pfy in the forest, but fire is awkward.")]
        public bool enableVobParticles;
        [Tooltip("The current implementation costs more frames than it saves. But it's a potential starting point for further enhancements like gluing small related objects together. Stored here for future use.")]
        public bool enableFineGrainedWorldMeshCreation;
        [Tooltip("Experimental. Looks weird without proper distance shadow. Could save some frames if combined with well looking distance shadow.")]
        public bool enableWorldCulling;
        
        // Not yet implemented. Left here for future use.
        // [Header("__________Performance: NPC Culling__________")]
        // public bool npcCulling;
        // public VobCullingGroupSetting npcVobCullingSetting;
        
        [Serializable]
        public class VobCullingGroupSetting
        {
            [Range(1f, 100f)] public float maxObjectSize;
            [Range(1f, 1000f)] public float cullingDistance;
        }
    }
}
