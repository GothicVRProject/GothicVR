using System;
using System.Collections.Generic;
using TMPro;
using GVR.Phoenix.Data;
using GVR.Phoenix.Data.Vm.Gothic;

namespace GVR.Phoenix.Interface
{
    public static class PhoenixBridge
    {
        public static IntPtr VdfsPtr;
        public static IntPtr VmGothicPtr;

        public static WorldData World;

        public static TMP_FontAsset GothicMenuFont;
        public static TMP_FontAsset GothicSubtitleFont;

        // FIXME Find a better place for the NPC routines. E.g. on the NPCs itself? But we e.g. need to have a static NPCObject List to do so.
        public static Dictionary<IntPtr, List<RoutineData>> npcRoutines = new();
    }
}