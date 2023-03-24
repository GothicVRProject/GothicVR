using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace UZVR.Phoenix
{
    public class VmBridge
    {
        public IntPtr VmPtr { get; private set; } = IntPtr.Zero;

        private const string DLLNAME = PhoenixBridge.DLLNAME;
        // Generic functions
        [DllImport(DLLNAME)] private static extern IntPtr createVM(string datFilePath);
        [DllImport(DLLNAME)] private static extern void disposeVM(IntPtr vm);
        [DllImport(DLLNAME)] private static extern void vmCallFunctionByName(IntPtr vm, string functionName);
        [DllImport(DLLNAME)] private static extern void vmCallFunctionByIndex(IntPtr vm, int index, IntPtr npcInstance);


        delegate void NO_RETURN_STRING_PARAM_CALLBACK(string value);
        delegate void NO_RETURN_INT_STRING_PARAM_CALLBACK(int npcinstance, string spawnpoint);
        delegate void REGISTER_TA_MIN(IntPtr npcRef, int start_h, int start_m, int stop_h, int stop_m, int action, string waypoint);

        [DllImport(DLLNAME)] private static extern void registerDefaultExternal(IntPtr vm, NO_RETURN_STRING_PARAM_CALLBACK callbackPointer);
        [DllImport(DLLNAME)] private static extern void registerExternal(IntPtr vm, string functionName, NO_RETURN_INT_STRING_PARAM_CALLBACK callback);
        [DllImport(DLLNAME)] private static extern void registerTA_MIN(IntPtr vm, REGISTER_TA_MIN callback);

        // NPC functions
        [DllImport(DLLNAME)] private static extern IntPtr initNpcInstance(IntPtr vm, int instanceId);
        [DllImport(DLLNAME)] private static extern int getNpcRoutine(IntPtr npc);
        [DllImport(DLLNAME)] private static extern uint getNpcSymbolIndex(IntPtr npc);
        [DllImport(DLLNAME)] private static extern int getNpcNameSize(IntPtr npc);
        [DllImport(DLLNAME)] private static extern void getNpcName(IntPtr npc, StringBuilder name);



        public VmBridge(string datFilePath)
        {
            CreateVm(datFilePath);
            RegisterCallbacks();
        }

        private void CreateVm(string datFilePath)
        {
            if (!File.Exists(datFilePath))
                throw new FileNotFoundException(datFilePath + " not found.");

            VmPtr = createVM(datFilePath);
        }

        private void RegisterCallbacks()
        {
            registerDefaultExternal(VmPtr, NPCExternals.NotImplementedCallback);
            registerExternal(VmPtr, "Wld_InsertNpc", NPCExternals.Wld_InsertNpc);
            registerTA_MIN(VmPtr, NPCExternals.TA_MIN);
        }

        public void CallFunction(string functionName)
        {
            vmCallFunctionByName(VmPtr, functionName);
        }

        public void CallFunction(int index, IntPtr npcInstance)
        {
            vmCallFunctionByIndex(VmPtr, index, npcInstance);
        }

        public IntPtr InitNpcInstance(int instanceId)
        {
            return initNpcInstance(VmPtr, instanceId);
        }

        public uint GetNpcSymbolId(IntPtr npc)
        {
            return getNpcSymbolIndex(npc);
        }

        public int GetNpcRoutine(IntPtr npc)
        {
            return getNpcRoutine(npc);
        }

        public string GetNpcName(IntPtr npc)
        {
            var size = getNpcNameSize(npc);
            var name = new StringBuilder(size);
            getNpcName(npc, name);

            return name.ToString();
        }

        ~VmBridge()
        {
            disposeVM(VmPtr);
        }
    }

}