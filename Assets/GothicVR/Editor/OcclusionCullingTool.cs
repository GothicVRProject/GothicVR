using System;
using System.IO;
using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes;
using GVR.Phoenix.Interface;
using GVR.Util;
using PxCs.Interface;
using UnityEditor;
using UnityEngine;

namespace GothicVR.Editor
{
    /// HOW TO USE:
    /// 1. LOAD THE SCENE FOR WHICH YOU WANT OCCULUSION CULLING
    /// 2. SET THE SCENE AS ACTIVE SCENE
    /// 3. RUN GothicVR/Tools/OcclusionCulling
    /// 4. MAKE SURE THAT THE CORRECT SCENE IS SET AS ACTIVE
    /// 5. SELECT ALL MESHES IN THE SCENE
    /// 6. Window/Rendering/Occlusion Culling
    /// 7. BAKE THE OCCULUSION CULLING
    /// 8. SAVE THE SCENE
    /// 9. DONE
    public class OcclusionCullingTool : EditorWindow
    {
        private const string _G1DIR = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic";
        private static IntPtr _vfsPtr = IntPtr.Zero;
        private static MeshCreator _meshCreator;
        
        [MenuItem("GothicVR/Tools/Occlusion Culling", true)]
        private static bool ValidateMyMenuItem()
        {
            // If game is in playmode, disable button.
            return !EditorApplication.isPlaying;
        }
        
        [MenuItem("GothicVR/Tools/Occlusion Culling")]
        public static void ShowWindow()
        {
            // Do not show Window when game is started.
            if (Application.isPlaying)
                return;
            
            _meshCreator = MeshCreator.I;
            var assetCache = AssetCache.I;

            // FIXME: We need to re-implement it as we refactured the whole MeshCreator logic.
            // _meshCreator.EditorInject(assetCache);
            
            var fullPath = Path.GetFullPath(Path.Join(_G1DIR, "Data"));
            _vfsPtr = VfsBridge.LoadVfsInDirectory(fullPath);
            
            // Hint: When we load meshes/textures via MeshCreator.cs, there's hard coded GameData.I.VdfPtr used.
            // As we ensured our Window is only active when not playing, we can safely reuse it for now.

            // FIXME - won't work as we have no instance set up before. Need to test it.
            GameData.I.VfsPtr = _vfsPtr;
            
            // use PhoenixImporter to handle loading the world and setting it to the correct scene.
            WorldCreator.I.LoadEditorWorld(_vfsPtr, "world");
        }

        void OnGUI()
        {
            // Do not show Window when game is started.
            if (Application.isPlaying)
            {
                Close();
                return;
            }
        }

        void OnDestroy()
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