using System;
using System.IO;
using GVR.Creator;
using GVR.Globals;
using GVR.Manager;
using GVR.Manager.Settings;
using GVR.Phoenix.Interface;
using PxCs.Interface;
using UnityEditor;
using UnityEngine;

namespace GVR.Editor.Tools
{
    /// HOW TO USE:
    /// 1. LOAD THE SCENE FOR WHICH YOU WANT OCCULUSION CULLING
    /// 2. RUN GothicVR/Tools/Load world meshes in editor
    /// 3. Window/Rendering/Occlusion Culling
    /// 4. BAKE THE OCCULUSION CULLING
    /// 5. SAVE THE SCENE
    public class OcclusionCullingTool : EditorWindow
    {
        private static IntPtr _vfsPtr = IntPtr.Zero;

        [MenuItem("GothicVR/Tools/Load world meshes in editor", true)]
        private static bool ValidateMyMenuItem()
        {
            // If game is in playmode, disable button.
            return !EditorApplication.isPlaying;
        }

        [MenuItem("GothicVR/Tools/Load world meshes in editor")]
        public static void ShowWindow()
        {
            // Do not show Window when game is started.
            if (Application.isPlaying)
                return;

            if (SettingsManager.GameSettings == null)
                SettingsManager.LoadGameSettings();

            GVRBootstrapper.SetLanguage();
            GVRBootstrapper.MountVfs(SettingsManager.GameSettings.GothicIPath);

            WorldCreator.LoadEditorWorld();
        }

        private void OnGUI()
        {
            // Do not show Window when game is started.
            if (Application.isPlaying)
                Close();
        }

        private void OnDestroy()
        {
            if (_vfsPtr == IntPtr.Zero)
                return;

            PxVfs.DestroyVfs(_vfsPtr);
            _vfsPtr = IntPtr.Zero;

            // Hint: If window closes as the game is started, we must not! clear GameData.I.VdfPtr as it would crash the game.
            // Therefore just leave it as it is...
        }
    }
}
