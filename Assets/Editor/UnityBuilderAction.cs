using System;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;
using UnityEngine.XR.OpenXR.Features.PICOSupport;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.Management;

using UnityEditor.XR.OpenXR.Features;


namespace UnityBuildTools {

    public class UnityBuilderAction
    {
        static string[] SCENES = FindEnabledEditorScenes();
    
        static readonly string APP_NAME = "GothicVR";
        static readonly string TARGET_DIR = "build";


        [MenuItem("GothicVR/CI/Build Quest2")]
        static void PerformQuestBuild()
        {
            string target_path = TARGET_DIR + "/Quest/" + APP_NAME + ".apk";
            SetAndroidSettings();
            SetQuestSettings();
			//GenericBuild(SCENES, target_path, BuildTargetGroup.Android, BuildTarget.Android, BuildOptions.None);
        }

		[MenuItem("GothicVR/CI/Build Pico4")]
		static void PerformPicoBuild()
		{
			string target_path = TARGET_DIR + "/Pico/" + APP_NAME + ".apk";
            SetAndroidSettings();
            SetPicoSettings();
			//GenericBuild(SCENES, target_path, BuildTargetGroup.Android, BuildTarget.Android, BuildOptions.None);
		}

		[MenuItem("GothicVR/CI/Build PCVR")]
		static void PerformWindows64Build()
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

        private static void SetPicoSettings(){

            OpenXRSettings.ActiveBuildTargetInstance.GetFeature<PICOTouchControllerProfile>().enabled = true;
            OpenXRSettings.ActiveBuildTargetInstance.GetFeature<PICOFeature>().enabled = true;

            //deactivate Meta
			OpenXRSettings.ActiveBuildTargetInstance.GetFeature<MetaQuestFeature>().enabled = false;
			OpenXRSettings.ActiveBuildTargetInstance.GetFeature<MetaQuestTouchProControllerProfile>().enabled = false;

			foreach (var item in OpenXRSettings.ActiveBuildTargetInstance.GetFeatures()) 
            {
				Debug.Log(item.name);
			}

            Debug.Log("OpenXR settings set for: Pico");

        
        }


        private static void SetQuestSettings(){
            foreach(var fet in OpenXRSettings.Instance.GetFeatures()) { Debug.Log(fet); }

            OpenXRSettings.ActiveBuildTargetInstance.GetFeature<MetaQuestTouchProControllerProfile>().enabled = true;
            OpenXRSettings.ActiveBuildTargetInstance.GetFeature<MetaQuestFeature>().enabled = true;

			//deactivate Pico
			OpenXRSettings.ActiveBuildTargetInstance.GetFeature<PICOFeature>().enabled = false;
			OpenXRSettings.ActiveBuildTargetInstance.GetFeature<PICOTouchControllerProfile>().enabled = false;

			Debug.Log("OpenXR settings set for: Quest");
            XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
        
        }

        
    }
}


