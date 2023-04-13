using System;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UnityBuildTools {

    public class UnityBuilderAction
    {
        static string[] SCENES = FindEnabledEditorScenes();
    
        static readonly string APP_NAME = "unZENity-VR";
        static readonly string TARGET_DIR = "build";


        [MenuItem("unZENity/CI/Build Quest2")]
        static void PerformQuestBuild()
        {
            string target_path = TARGET_DIR + "/Quest/" + APP_NAME + ".apk";
            SetAndroidSettings();

			GenericBuild(SCENES, target_path, BuildTargetGroup.Android, BuildTarget.Android, BuildOptions.None);
        }

		[MenuItem("unZENity/CI/Build Pico4")]
		static void PerformPicoBuild()
		{
			string target_path = TARGET_DIR + "/Pico/" + APP_NAME + ".apk";
            SetAndroidSettings();
			GenericBuild(SCENES, target_path, BuildTargetGroup.Android, BuildTarget.Android, BuildOptions.None);
		}

		[MenuItem("unZENity/CI/Build PCVR")]
		static void PerformPCVRBuild()
		{
			string target_path = TARGET_DIR + "/Windows64/" + APP_NAME + ".exe";
			GenericBuild(SCENES, target_path, BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, BuildOptions.None);
		}

		private static void GenericBuild(string[] scenes, string target_path, BuildTargetGroup build_target_group, BuildTarget build_target, BuildOptions build_options)
        {

            // Set the target platform for the build
            EditorUserBuildSettings.SwitchActiveBuildTarget(build_target_group, build_target);
    
            
            
    
            // Set BuildPlayerOptions
            BuildPlayerOptions options = new BuildPlayerOptions();
            options.scenes = scenes;
            options.locationPathName = target_path;
            options.target = build_target;
            options.targetGroup = build_target_group;
            options.options = build_options;
    
            // Build the project
            BuildReport report = BuildPipeline.BuildPlayer(options);
        }

        private static void SetAndroidSettings()
        {
			// Set the target device for the build
			PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
		}
    
        private static string[] FindEnabledEditorScenes()
        {
            List<string> EditorScenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;
                EditorScenes.Add(scene.path);
            }
            
	    return EditorScenes.ToArray();
        }
    }
}


