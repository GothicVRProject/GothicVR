using System;
using System.Collections.Generic;
using GVR.Editor.Tools;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;
using Pico;
using Unity.XR.OpenXR.Features.PICOSupport;

namespace GVR.Editor.Builds.UnityBuildTools
{
	public class UnityBuilderAction
    {
        static string[] SCENES = FindEnabledEditorScenes();
    
        static readonly string APP_NAME = "GothicVR";
        static readonly string TARGET_DIR = "build";


		/// <summary>
		/// Perform Quest Build
		/// </summary>

		[MenuItem("GothicVR/Build/Build Quest")]
		static void PerformLocalQuestBuild()
		{
			bool buildProductionReady = ShowConfirmationPopup("Should the feature flags be set to Production Ready?");
			PerformQuestBuild(buildProductionReady);
		}

		static void PerformQuestBuild()
		{
			string target_path = TARGET_DIR + "/Quest/" + APP_NAME + ".apk";
			SetQuestSettings();
				FeatureFlagTool.SetFeatureFlags();
				EditorSceneManager.SaveScene(SceneManager.GetSceneByName("Bootstrap"));
			GenericBuild(SCENES, target_path, BuildTargetGroup.Android, BuildTarget.Android, BuildOptions.None);
		}

		static void PerformQuestBuild(bool resetFeatureFlags = true)
        {
            string target_path = TARGET_DIR + "/Quest/" + APP_NAME + ".apk";
            SetQuestSettings();
            if (resetFeatureFlags)
            {
                FeatureFlagTool.SetFeatureFlags();
				EditorSceneManager.SaveScene(SceneManager.GetSceneByName("Bootstrap"));
			}
			GenericBuild(SCENES, target_path, BuildTargetGroup.Android, BuildTarget.Android, BuildOptions.None);
        }

		/// <summary>
		/// Perform Quest Build
		/// </summary>

		[MenuItem("GothicVR/Build/Build Pico4")]
		static void PerformLocalPicoBuild()
		{
			bool buildProductionReady = ShowConfirmationPopup("Should the feature flags be set to Production Ready?");
			PerformPicoBuild(buildProductionReady);
		}

		static void PerformPicoBuild()
		{
			string target_path = TARGET_DIR + "/Pico/" + APP_NAME + ".apk";
			SetPicoSettings();
				FeatureFlagTool.SetFeatureFlags();
				EditorSceneManager.SaveScene(SceneManager.GetSceneByName("Bootstrap"));
			GenericBuild(SCENES, target_path, BuildTargetGroup.Android, BuildTarget.Android, BuildOptions.None);
		}
		static void PerformPicoBuild(bool resetFeatureFlags = true)
		{
			string target_path = TARGET_DIR + "/Pico/" + APP_NAME + ".apk";
            SetPicoSettings();
			if (resetFeatureFlags)
			{
				FeatureFlagTool.SetFeatureFlags();
				EditorSceneManager.SaveScene(SceneManager.GetSceneByName("Bootstrap"));
			}
			GenericBuild(SCENES, target_path, BuildTargetGroup.Android, BuildTarget.Android, BuildOptions.None);
		}

		/// <summary>
		/// Perform Quest Build
		/// </summary>

		[MenuItem("GothicVR/Build/Build PCVR")]
		static void PerformLocalWindows64Build()
		{
			bool buildProductionReady = ShowConfirmationPopup("Should the feature flags be set to Production Ready?");
			PerformWindows64Build(buildProductionReady);
		}

		static void PerformWindows64Build()
		{
			string target_path = TARGET_DIR + "/Windows64/" + APP_NAME + ".exe";
				FeatureFlagTool.SetFeatureFlags();
				EditorSceneManager.SaveScene(SceneManager.GetSceneByName("Bootstrap"));	
			GenericBuild(SCENES, target_path, BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, BuildOptions.None);
		}
		static void PerformWindows64Build(bool resetFeatureFlags = true)
		{
			string target_path = TARGET_DIR + "/Windows64/" + APP_NAME + ".exe";
			if (resetFeatureFlags)
			{
				FeatureFlagTool.SetFeatureFlags();
				EditorSceneManager.SaveScene(SceneManager.GetSceneByName("Bootstrap"));
			}
			GenericBuild(SCENES, target_path, BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, BuildOptions.None);
		}

		static bool ShowConfirmationPopup(string message)
		{
			return EditorUtility.DisplayDialog("Confirmation", message, "Yes", "No");
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

            // TODO: Check GitHub Issue: https://github.com/game-ci/unity-builder/issues/563
            Debug.Log("Logging fake Build results so that the build via game-ci/unity-builder does not fail...");
            Debug.Log($"###########################{Environment.NewLine}#      Build results      #{Environment.NewLine}###########################{Environment.NewLine}" +
            $"{Environment.NewLine}Duration: 00:00:00.0000000{Environment.NewLine}Warnings: 0{Environment.NewLine}Errors: 0{Environment.NewLine}Size: 0 bytes{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}Build succeeded!");
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

			PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            //Enable Pico
			OpenXRSettings.ActiveBuildTargetInstance.GetFeature<PICO4ControllerProfile>().enabled = true;
            OpenXRSettings.ActiveBuildTargetInstance.GetFeature<PICOFeature>().enabled = true;

            //Disable Meta
			OpenXRSettings.ActiveBuildTargetInstance.GetFeature<MetaQuestFeature>().enabled = false;
			// OpenXRSettings.ActiveBuildTargetInstance.GetFeature<MetaQuestTouchProControllerProfile>().enabled = false;
			OpenXRSettings.ActiveBuildTargetInstance.GetFeature<OculusTouchControllerProfile>().enabled = false;

			foreach (var item in OpenXRSettings.ActiveBuildTargetInstance.GetFeatures()) 
            {
				Debug.Log(item.name);
			}
            Debug.Log("OpenXR settings set for: Pico");  
            
        }


        private static void SetQuestSettings(){

			PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

			//Enable Meta
			// OpenXRSettings.ActiveBuildTargetInstance.GetFeature<MetaQuestTouchProControllerProfile>().enabled = true;
			OpenXRSettings.ActiveBuildTargetInstance.GetFeature<OculusTouchControllerProfile>().enabled = true;
			OpenXRSettings.ActiveBuildTargetInstance.GetFeature<MetaQuestFeature>().enabled = true;

			//Disable Pico
			OpenXRSettings.ActiveBuildTargetInstance.GetFeature<PICOFeature>().enabled = false;
			OpenXRSettings.ActiveBuildTargetInstance.GetFeature<PICO4ControllerProfile>().enabled = false;

			Debug.Log("OpenXR settings set for: Quest");
        
        }

        
    }
}


