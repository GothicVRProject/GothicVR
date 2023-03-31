using System;
using System.Collections.Generic;
using UZVR.Phoenix.Bridge.Vm;
using UZVR.Phoenix.Bridge.Vm.Gothic;
using UZVR.Phoenix.Vm.Gothic;
using UZVR.Phoenix.World;

namespace UZVR.Phoenix.Bridge
{
    public static class PhoenixBridge
    {
        public const string DLLNAME = "libphoenix-csharp-bridge";

        public static IntPtr VdfsPtr;

        /// <summary>
        /// No need to get WorldBridge as everything is loaded right from the start and stored inside World.
        /// Therefore we support this shortcut only.
        /// </summary>
        public static WorldBridge WorldBridge { private get; set; }
        public static BWorld World {
            get { return WorldBridge.World; }
            private set { }
        }


        /// Gothic VM consists of a lot of elements. We therefore split it into areas like NPC, Items, ... and store it for use.
        public static VmGothicBridge VmGothicBridge { get; set; }
        public static NpcBridge VmGothicNpcBridge { get; set; }

        // FIXME Find a better place for the NPC routines. E.g. on the NPCs itself? But we e.g. need to have a static NPCObject List to do so.
        public static Dictionary<uint, List<BRoutine>> npcRoutines = new();
    }
}