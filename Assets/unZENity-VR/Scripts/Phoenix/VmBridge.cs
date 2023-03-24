using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace UZVR.Phoenix
{
    public class VmBridge
    {
        private const string DLLNAME = "phoenix-csharp-bridge";
        private const string G1DatDir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\_work\\DATA\\scripts\\_compiled\\";

        private IntPtr _vm;


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



        public VmBridge(string datFilename)
        {
            CreateVm(datFilename);
            RegisterCallbacks();
        }

        private void CreateVm(string datFilename)
        {
            var filePath = G1DatDir + datFilename;

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath + " not found.");

            _vm = createVM(G1DatDir + datFilename);
        }

        private void RegisterCallbacks()
        {
            registerDefaultExternal(_vm, DaedalusExternals.NotImplementedCallback);
            registerExternal(_vm, "Wld_InsertNpc", DaedalusExternals.Wld_InsertNpc);
            registerTA_MIN(_vm, DaedalusExternals.TA_MIN);
        }

        public void CallFunction(string functionName)
        {
            vmCallFunctionByName(_vm, functionName);
        }

        public void CallFunction(int index, IntPtr npcInstance)
        {
            vmCallFunctionByIndex(_vm, index, npcInstance);
        }

        public IntPtr InitNpcInstance(int instanceId)
        {
            return initNpcInstance(_vm, instanceId);
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
            disposeVM(_vm);
        }
    }

}