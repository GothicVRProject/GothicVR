using System.Collections.Generic;
using System;
using UZVR.Phoenix;
using UZVR.Phoenix.Vm;
using UZVR.Phoenix.World;

namespace UZVR.Phoenix
{
    public static class PhoenixBridge
    {
        public const string DLLNAME = "phoenix-csharp-bridge";

        public static WorldBridge WorldBridge { private get; set; }
        public static VmBridge VMBridge { get; set; }

        /// <summary>
        /// No need to get WorldBridge as everything is loaded right from the start and stored inside World.
        /// Therefore we support this shortcut only.
        /// </summary>
        public static PBWorld World {
            get { return WorldBridge.World; }
            private set { }
        }

        public static Dictionary<uint, List<PBRoutine>> npcRoutines = new();
    }
}