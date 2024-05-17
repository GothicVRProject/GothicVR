using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace GVR.Editor.Tools
{
    public class ContextTool
    {
        private const string HVR_COMPILER_FLAG = "GVR_HVR_INSTALLED";

        /// <summary>
        /// Activate compiler flag being used inside GvrContext classes. This decides whether to build or don't build HVR classes.
        /// </summary>
        [MenuItem("GothicVR/Context/Activate HVR Plugin")]
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
        [MenuItem("GothicVR/Context/De-activate HVR Plugin")]
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
