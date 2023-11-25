using System;
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

        [Header("__________Developer__________")]
        [Tooltip("This will be used within Editor mode only. No effect for Standalone.")]
        public bool SkipMainMenu;
        public bool useXRDeviceSimulator;
        public bool createWayPointMeshes;
        public bool createWaypointEdgeMeshes;
        public bool drawVobCullingGizmos;

        [Header("__________World__________")]
        public bool CreateVobs;
        public bool CreateWaypoints;
        [Tooltip("Leave blank if you want to spawn normal.")]
        public string spawnAtSpecificFreePoint;
        [Tooltip("True will render all pickables with dynamic attach points")]
        public bool vobItemsDynamicAttach;

        [Header("__________DayTime__________")]
        public bool EnableDayTime;
        public SunMovementPerformance SunMovementPerformanceValue;
        [Range(0, 23)] public int StartHour;
        [Range(0, 59)] public int StartMinute;
        
        [Header("__________VOBs__________")]
        [Tooltip("Only for Debug purposes. It'll not change functionality itself.")]
        public bool EnableVobFPMesh;
        public bool EnableDecals;
        
        [Header("__________NPCs__________")]
        public bool CreateOcNpcs;
        public bool EnableNpcRoutines;
        public bool CreateNpcArmor;
        public bool CreateExampleAnimation;
        public bool CreateDebugIdleAnimations;

        [Header("__________SPAMmy debug messages__________")]
        public bool ShowPhoenixDebugMessages;
        public bool ShowZspyLogs;
        public bool ShowPhoenixVfsFileNotFoundErrors;
        public bool ShowMusicLogs;

        [Header("__________Audio__________")]
        public bool EnableSounds;
        public bool EnableMusic;
        public bool enableSoundCulling;
        
        [Serializable]
        public class VobCullingGroupSetting
        {
            [Range(1f, 100f)] public float maxObjectSize;
            [Range(1f, 1000f)] public float cullingDistance;
        }

        [Header("__________Performance__________")]
        public bool vobCulling;
        public VobCullingGroupSetting vobCullingSmall;
        public VobCullingGroupSetting vobCullingMedium;
        public VobCullingGroupSetting vobCullingLarge;

        [Header("__________Experimental / Do not use in Production__________")]
        [Tooltip("Looks already quite good for leaves.pfy in the forest, but fire is awkward.")]
        public bool enableVobParticles;
        [Tooltip("The current implementation costs more frames than it saves. But it's a potential starting point for further enhancements like gluing small related objects together. Stored here for future use.")]
        public bool enableFineGrainedWorldMeshCreation;
        [Tooltip("Experimental. Looks weird without proper distance shadow. Could save some frames if combined with well looking distance shadow.")]
        public bool enableWorldCulling;
        
        // Not yet implemented.
        // [Header("__________Performance: NPC Culling__________")]
        // public bool npcCulling;
        // public VobCullingGroupSetting npcVobCullingSetting;
    }
}
