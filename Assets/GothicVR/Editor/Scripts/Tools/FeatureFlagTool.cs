using System;
using System.Collections.Generic;
using System.Reflection;
using GVR.Debugging;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GVR.Editor.Tools
{
    public class FeatureFlagTool : EditorWindow
    {
        /// <summary>
        /// FieldName, FieldType, Value
        /// </summary>
        private static readonly List<Tuple<string, Type, object>> ProductionFlags = new()
        {
            // Booleans
            new(nameof(FeatureFlags.createVobs), typeof(bool), true),
            new (nameof(FeatureFlags.createWaypoints), typeof(bool), true),
            new (nameof(FeatureFlags.enableDayTime), typeof(bool), true),
            new (nameof(FeatureFlags.enableSounds), typeof(bool), true),
            new (nameof(FeatureFlags.enableMusic), typeof(bool), true),
            new (nameof(FeatureFlags.vobCulling), typeof(bool), true),
            new (nameof(FeatureFlags.enableSoundCulling), typeof(bool), true),
            new (nameof(FeatureFlags.vobItemsDynamicAttach), typeof(bool), true),

            // Ints
            new (nameof(FeatureFlags.startHour), typeof(int), 8), // Official start time of G1 - new game
            new (nameof(FeatureFlags.startMinute), typeof(int), 0), // Official start time of G1 - new game

            // Enums (Handled as Int internally)
            new (nameof(FeatureFlags.sunMovementPerformanceValue), typeof(int), FeatureFlags.SunMovementPerformance.EveryIngameMinute),

            // Special types
            new (nameof(FeatureFlags.vobCullingSmall), typeof(FeatureFlags.VobCullingGroupSetting),
                new FeatureFlags.VobCullingGroupSetting{ maxObjectSize = 1.2f, cullingDistance = 50f}),
            new (nameof(FeatureFlags.vobCullingMedium), typeof(FeatureFlags.VobCullingGroupSetting),
                new FeatureFlags.VobCullingGroupSetting{ maxObjectSize = 5f, cullingDistance = 100f}),
            new (nameof(FeatureFlags.vobCullingLarge), typeof(FeatureFlags.VobCullingGroupSetting),
                new FeatureFlags.VobCullingGroupSetting{ maxObjectSize = 100f, cullingDistance = 200f})
        };


        [MenuItem("GothicVR/Tools/FeatureFlags - Set Production ready state")]
        public static void SetFeatureFlags()
        {
            var scene = SceneManager.GetSceneByName("Bootstrap");

            if (scene == default)
            {
                Debug.LogError(">Bootstrap< scene needs to be loaded.");
                return;
            }

            var featureFlags = GameObject.Find("FeatureFlags").GetComponent<FeatureFlags>();
            var fields = featureFlags.GetType().GetFields();

            ResetFlags(featureFlags, fields);
            SetProductionFlags(featureFlags);

            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("FeatureFlags successfully set to production values.");
        }

        /// <summary>
        /// We reset all flags to a default value. It's expected, that e.g. a bool=false is default.
        /// </summary>
        private static void ResetFlags(FeatureFlags featureFlags, FieldInfo[] fields)
        {
            foreach (var field in fields)
            {
                switch (field.FieldType.Name)
                {
                    case "Boolean":
                        field.SetValue(featureFlags, false);
                        break;
                    case "Int32":
                    case "SunMovementPerformance":
                        field.SetValue(featureFlags, 0);
                        break;
                    case "String":
                            field.SetValue(featureFlags, "");
                            break;
                    case "VobCullingGroupSetting":
                            field.SetValue(featureFlags, new FeatureFlags.VobCullingGroupSetting());
                            break;
                    case "List`1":
                        switch (field.FieldType.GenericTypeArguments[0].Name)
                        {
                            case "Int32":
                                ((List<int>)field.GetValue(featureFlags)).Clear();
                                break;
                            default:
                                Debug.LogError($"Unsupported field type {field.FieldType.Name}");
                                break;
                        }
                        break;
                    default:
                        Debug.LogError($"Unsupported field type {field.FieldType.Name}");
                        break;
                }
            }
        }

        /// <summary>
        /// Pick the ProductionFlags and set values to demanded values.
        /// </summary>
        private static void SetProductionFlags(FeatureFlags featureFlags)
        {
            foreach (var flag in ProductionFlags)
            {
                switch (flag.Item2.Name)
                {
                    case "Boolean":
                    case "Int32":
                    case "VobCullingGroupSetting":
                        var field = featureFlags.GetType().GetField(flag.Item1);
                        field.SetValue(featureFlags, flag.Item3);
                        break;
                    default:
                        Debug.LogError($"Unsupported/Untested field type {flag.Item2.Name}");
                        break;
                }
            }
        }
    }
}
