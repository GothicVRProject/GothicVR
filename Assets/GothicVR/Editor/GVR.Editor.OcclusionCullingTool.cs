using System;
using System.IO;
using GVR.Caches;
using GVR.Creator;
using GVR.Phoenix.Interface;
using GVR.Util;
using PxCs.Interface;
using UnityEditor;
using UnityEngine;

namespace GothicVR.Editor
{
    public class OcclusionCullingTool : EditorWindow
    {
        private const string _G1DIR = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic";
        private static IntPtr _vdfsPtr = IntPtr.Zero;
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
            
            _meshCreator = SingletonBehaviour<MeshCreator>.GetOrCreate();
            var assetCache = SingletonBehaviour<AssetCache>.GetOrCreate();

            _meshCreator.EditorInject(assetCache);
            
            var fullPath = Path.GetFullPath(Path.Join(_G1DIR, "Data"));
            _vdfsPtr = VdfsBridge.LoadVdfsInDirectory(fullPath);
            
            // Hint: When we load meshes/textures via MeshCreator.cs, there's hard coded PhoenixBridge.VdfPtr used.
            // As we ensured our Window is only active when not playing, we can safely reuse it for now.
            PhoenixBridge.VdfsPtr = _vdfsPtr;
            
            var world = WorldBridge.LoadWorld(_vdfsPtr, "world.zen");

            _meshCreator.Create(world);
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
            if (_vdfsPtr == IntPtr.Zero)
                return;

            PxVdf.pxVdfDestroy(_vdfsPtr);
            _vdfsPtr = IntPtr.Zero;

            // Hint: If window closes as the game is started, we must not! clear PhoenixBridge.VdfPtr as it would crash the game.
            // Therefore just leave it as it is...
        }
    }
}