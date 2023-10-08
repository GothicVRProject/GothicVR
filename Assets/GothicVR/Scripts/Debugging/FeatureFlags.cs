using GVR.Util;
using UnityEngine;

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
        public bool useXRDeviceSimulator;
        [Tooltip("It's not yet clear if the new submesh-merging feature is better or worse for performance. We need to test with all the other improvements like VobCulling etc.")]
        public bool enableLegacyBigWorldMeshCreation;
        
        [Header("__________World__________")]
        public bool CreateVobs;
        public bool CreateWaypoints;
        public bool CreateWaypointEdges;
        public bool SkipMainMenu;

        [Header("__________DayTime__________")]
        public bool EnableDayTime;
        public SunMovementPerformance SunMovementPerformanceValue;
        [Range(0, 23)] public int StartHour;
        [Range(0, 59)] public int StartMinute;
        
        [Header("__________VOBs__________")]
        [Tooltip("Only for Debug purposes. It'll not change functionality itself.")]
        public bool EnableVobFPMesh;
        [Tooltip("For Debug purposes within Scene view in Editor only. Might imply some performance issues in Editor mode.")]
        public bool EnableVobFPMeshEditorLabel;
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

        [Header("__________Audio__________")]
        public bool EnableSounds;
        public bool EnableMusic;

    }
}
