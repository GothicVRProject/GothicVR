﻿using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UZVR.Phoenix.Data;
using UZVR.Phoenix.Data.Vm.Gothic;

namespace UZVR.Phoenix.Interface
{
    public static class PhoenixBridge
    {
        public static IntPtr VdfsPtr;
        public static IntPtr VmGothicPtr;

        public static WorldData World;

        public static TMP_FontAsset DefaultFont;

        // FIXME Find a better place for the NPC routines. E.g. on the NPCs itself? But we e.g. need to have a static NPCObject List to do so.
        public static Dictionary<IntPtr, List<RoutineData>> npcRoutines = new();
    }
}