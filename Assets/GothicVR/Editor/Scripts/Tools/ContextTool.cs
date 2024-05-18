using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace GVR.Editor.Tools
{
    public class ContextTool
    {
        private const string HVR_COMPILER_FLAG = "GVR_HVR_INSTALLED";


        [MenuItem("GothicVR/Context/Check HVR status", priority = 1)]
        private static void CheckHVRPluginStatus()
        {
            var hvrFolder = Application.dataPath + "/HurricaneVR";
            var hvrExists = Directory.Exists(hvrFolder) && Directory.EnumerateFiles(hvrFolder).Count() != 0;
            var hvrCompilerSettingExists = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone).Contains(HVR_COMPILER_FLAG);

            var message =
                $"Plugin installed: {hvrExists}\n" +
                $"Include in Build: {hvrCompilerSettingExists}\n";

            EditorUtility.DisplayDialog("Hurricane VR - Status", message, "Close");
        }

        /// <summary>
        /// Activate compiler flag being used inside GvrContext classes. This decides whether to build or don't build HVR classes.
        /// </summary>
        [MenuItem("GothicVR/Context/Activate HVR in Build", priority = 2)]
        private static void ActivatePlugin()
        {
            var settings = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone)
                .Split(";")
                .ToList();

            if (settings.Any(i => i.Equals(HVR_COMPILER_FLAG)))
                return;

            settings.Add(HVR_COMPILER_FLAG);
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, string.Join(";", settings));
        }

        /// <summary>
        /// Deactivate compiler flag being used inside GvrContext classes. This decides whether to build or don't build HVR classes.
        /// </summary>
        [MenuItem("GothicVR/Context/De-activate HVR", priority = 3)]
        private static void DeactivatePlugin()
        {
            var settings = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone)
                .Split(";")
                .ToList();

            if (!settings.Any(i => i.Equals(HVR_COMPILER_FLAG)))
                return;

            settings.Remove(HVR_COMPILER_FLAG);
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, string.Join(";", settings));
        }
    }
}
