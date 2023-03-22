using System;
using System.IO;
using System.Runtime.InteropServices;

namespace UZVR.Phoenix
{
    public class VmBridge
    {
        private const string DLLNAME = "phoenix-csharp-bridge";
        private const string G1DatDir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\_work\\DATA\\scripts\\_compiled\\";

        private IntPtr _vm;


        [DllImport(DLLNAME)] private static extern IntPtr createVM(string datFilePath);
        [DllImport(DLLNAME)] private static extern void disposeVM(IntPtr vm);
        [DllImport(DLLNAME)] private static extern void vmCallFunction(IntPtr vm, string functionName);



        delegate void NO_RETURN_STRING_PARAM_CALLBACK(string value);
        delegate void NO_RETURN_INT_STRING_PARAM_CALLBACK(int npcinstance, string spawnpoint);

        [DllImport(DLLNAME)] private static extern void registerDefaultExternal(IntPtr vm, NO_RETURN_STRING_PARAM_CALLBACK callbackPointer);
        [DllImport(DLLNAME)] private static extern void registerExternal(IntPtr vm, string functionName, NO_RETURN_INT_STRING_PARAM_CALLBACK callback);


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
        }

        public void CallFunction(string functionName)
        {
            vmCallFunction(_vm, functionName);
        }


        ~VmBridge()
        {
            disposeVM(_vm);
        }
    }

}