using System;
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
        private IntPtr _npcsContainer = IntPtr.Zero;

        // Basic
        [DllImport(DLLNAME)] private static extern IntPtr vmGothicNpcInitInstancesContainer();
        [DllImport(DLLNAME)] private static extern void vmGothicNpcDisposeInstancesContainer(IntPtr npcsContainer);

        [DllImport(DLLNAME)] private static extern IntPtr vmGothicNpcInitInstance(IntPtr vm, IntPtr npcsContainer, int instanceId);
        [DllImport(DLLNAME)] private static extern IntPtr vmGothicNpcInitInstanceByName(IntPtr vm, IntPtr npcsContainer, string instanceName);
        [DllImport(DLLNAME)] private static extern uint vmGothicNpcGetSymbolIndex(IntPtr npcPtr);
        [DllImport(DLLNAME)] private static extern void vmGothicNpcCallFunctionByIndex(IntPtr vm, IntPtr npcsContainer, int index, IntPtr npcPtr);

        // Specific
        [DllImport(DLLNAME)] private static extern int vmGothicNpcGetRoutine(IntPtr npcPtr);
        [DllImport(DLLNAME)] private static extern int vmGothicNpcGetNameSize(IntPtr npcPtr);
        [DllImport(DLLNAME)] private static extern void vmGothicNpcGetName(IntPtr npc, StringBuilder name);

        [DllImport(DLLNAME)] private static extern void vmGothicNpcDispose(IntPtr npcsContainer, IntPtr npcPtr);


        public NpcBridge(VmBridge vmBridge)
        {
            _vmBridge = vmBridge;
            _npcsContainer = vmGothicNpcInitInstancesContainer();

            RegisterNpcCallbacks();
        }

        ~NpcBridge()
        {
            vmGothicNpcDisposeInstancesContainer(_npcsContainer);
        }

        // FIXME needs to be called when an NPC is destroyed from Unity scene to free memory on C++ side.
        public void DisposeNpc(IntPtr npcPtr)
        {
            vmGothicNpcDispose(_npcsContainer, npcPtr);
        }

        public void CallFunction(int index, IntPtr npcInstance)
        {
            vmGothicNpcCallFunctionByIndex(_vmBridge.VmPtr, _npcsContainer, index, npcInstance);
        }

        public IntPtr InitNpcInstance(int instanceId)
        {
            return vmGothicNpcInitInstance(_vmBridge.VmPtr, _npcsContainer, instanceId);
        }

        public IntPtr InitNpcInstance(string instanceName)
        {
            return vmGothicNpcInitInstanceByName(_vmBridge.VmPtr, _npcsContainer, instanceName);
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

        delegate void WLD_INSERT_NPC_CALLBACK(int npcinstance, string spawnpoint);
        delegate void TA_MIN_CALLBACK(IntPtr npcRef, int start_h, int start_m, int stop_h, int stop_m, int action, string waypoint);
        delegate void AI_OUTPUT_CALLBACK(IntPtr self, IntPtr target, string outputname);

        [DllImport(DLLNAME)] private static extern void vmGothicRegisterWld_InsertNpc(IntPtr vm, string functionName, WLD_INSERT_NPC_CALLBACK callback);
        [DllImport(DLLNAME)] private static extern void vmGothicNpcRegisterTA_MIN(IntPtr vm, TA_MIN_CALLBACK callback);
        [DllImport(DLLNAME)] private static extern void vmGothicNpcRegisterAI_Output(IntPtr vm, AI_OUTPUT_CALLBACK callback);

        private void RegisterNpcCallbacks()
        {
            vmGothicRegisterWld_InsertNpc(_vmBridge.VmPtr, "Wld_InsertNpc", NpcExternals.Wld_InsertNpc);
            vmGothicNpcRegisterTA_MIN(_vmBridge.VmPtr, NpcExternals.TA_MIN);
            vmGothicNpcRegisterAI_Output(_vmBridge.VmPtr, NpcExternals.AI_Output);
        }

#endregion

    }
}
