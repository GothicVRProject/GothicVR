﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using UZVR.Phoenix.Vm.Gothic.Externals;

namespace UZVR.Phoenix.Bridge.Vm.Gothic
{
    /// <summary>
    /// Contains special NPC methods only available in Gothic Daedalus module.
    /// </summary>
    public class NpcBridge
    {
        private const string DLLNAME = PhoenixBridge.DLLNAME;
        private readonly VmBridge _vmBridge;

        // Basic
        [DllImport(DLLNAME)] private static extern IntPtr vmGothicNpcinitInstance(IntPtr vm, int instanceId);
        [DllImport(DLLNAME)] private static extern uint vmGothicNpcGetSymbolIndex(IntPtr npcPtr);
        [DllImport(DLLNAME)] private static extern void vmGothicNpcCallFunctionByIndex(IntPtr vm, int index, IntPtr npcPtr);

        // Specific
        [DllImport(DLLNAME)] private static extern int vmGothicNpcGetRoutine(IntPtr npcPtr);
        [DllImport(DLLNAME)] private static extern int vmGothicNpcGetNameSize(IntPtr npcPtr);
        [DllImport(DLLNAME)] private static extern void vmGothicNpcGetName(IntPtr npc, StringBuilder name);

        [DllImport(DLLNAME)] private static extern void vmGothicNpcDispose(IntPtr npcPtr);


        public NpcBridge(VmBridge vmBridge)
        {
            _vmBridge = vmBridge;

            RegisterNpcCallbacks();
        }

        // FIXME needs to be called when an NPC is destroyed from Unity scene to free memory on C++ side.
        public void DisposeNpc(IntPtr npcPtr)
        {
            vmGothicNpcDispose(npcPtr);
        }

        public void CallFunction(int index, IntPtr npcInstance)
        {
            vmGothicNpcCallFunctionByIndex(_vmBridge.VmPtr, index, npcInstance);
        }

        public IntPtr InitNpcInstance(int instanceId)
        {
            return vmGothicNpcinitInstance(_vmBridge.VmPtr, instanceId);
        }

        public uint GetNpcSymbolId(IntPtr npc)
        {
            return vmGothicNpcGetSymbolIndex(npc);
        }

        public int GetNpcRoutine(IntPtr npc)
        {
            return vmGothicNpcGetRoutine(npc);
        }

        public string GetNpcName(IntPtr npc)
        {
            var size = vmGothicNpcGetNameSize(npc);
            var name = new StringBuilder(size);
            vmGothicNpcGetName(npc, name);

            return name.ToString();
        }


#region Externals

        delegate void REGISTER_TA_MIN(IntPtr npcRef, int start_h, int start_m, int stop_h, int stop_m, int action, string waypoint);
        delegate void NO_RETURN_INT_STRING_PARAM_CALLBACK(int npcinstance, string spawnpoint);

        [DllImport(DLLNAME)] private static extern void vmGothicRegisterWld_InsertNpc(IntPtr vm, string functionName, NO_RETURN_INT_STRING_PARAM_CALLBACK callback);
        [DllImport(DLLNAME)] private static extern void vmGothicNpcRegisterTA_MIN(IntPtr vm, REGISTER_TA_MIN callback);


        private void RegisterNpcCallbacks()
        {
            vmGothicRegisterWld_InsertNpc(_vmBridge.VmPtr, "Wld_InsertNpc", NpcExternals.Wld_InsertNpc);
            vmGothicNpcRegisterTA_MIN(_vmBridge.VmPtr, NpcExternals.TA_MIN);
        }

#endregion

    }
}
