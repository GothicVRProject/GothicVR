using System;
using System.Collections.Generic;
using UZVR.Phoenix.Vm.Gothic;
using UZVR.Phoenix.World;

namespace UZVR.Phoenix.Bridge
{
    public static class PhoenixBridge
    {
        public const string DLLNAME = "libphoenix-csharp-bridge";

        public static IntPtr VdfsPtr;
        public static IntPtr VmGothicPtr;

        public static BWorld World;

        // FIXME Find a better place for the NPC routines. E.g. on the NPCs itself? But we e.g. need to have a static NPCObject List to do so.
        public static Dictionary<IntPtr, List<BRoutine>> npcRoutines = new();
    }
}