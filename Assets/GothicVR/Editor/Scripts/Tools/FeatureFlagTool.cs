using System;
using System.Collections.Generic;
using System.Reflection;
using GVR.Debugging;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZenKit;
using ZenKit.Vobs;

namespace GVR.Editor.Tools
{
    public class FeatureFlagTool : EditorWindow
    {
        /// <summary>
        /// FieldName, FieldType, Value
        /// </summary>
        private static readonly List<(string name, Type type, object value)> ProductionFlags = new()
        {
            // Booleans
            new (nameof(FeatureFlags.createWorldMesh), typeof(bool), true),
            new (nameof(FeatureFlags.createVobs), typeof(bool), true),
            new (nameof(FeatureFlags.enableSounds), typeof(bool), true),
            new (nameof(FeatureFlags.enableMusic), typeof(bool), true),
            new (nameof(FeatureFlags.vobCulling), typeof(bool), true),
            new (nameof(FeatureFlags.enableSoundCulling), typeof(bool), true),
            new (nameof(FeatureFlags.vobItemsDynamicAttach), typeof(bool), true),
            new (nameof(FeatureFlags.showBarrier), typeof(bool), true),

            // Ints / Floats
            new (nameof(FeatureFlags.TimeMultiplier), typeof(float), 1),
            new (nameof(FeatureFlags.startHour), typeof(int), 8), // Official start time of G1 - new game
            new (nameof(FeatureFlags.startMinute), typeof(int), 0), // Official start time of G1 - new game
            new (nameof(FeatureFlags.fireLightRange), typeof(float), 10),

            // Enums (Handled as Int internally)
            new (nameof(FeatureFlags.sunMovementPerformanceValue), typeof(int),
                FeatureFlags.SunMovementPerformance.EveryIngameMinute),
            new (nameof(FeatureFlags.zenKitLogLevel), typeof(int), LogLevel.Error),
            
            // Special types
            new (nameof(FeatureFlags.vobCullingSmall), typeof(FeatureFlags.VobCullingGroupSetting),
                new FeatureFlags.VobCullingGroupSetting{ maxObjectSize = 1.2f, cullingDistance = 50f}),
            new (nameof(FeatureFlags.vobCullingMedium), typeof(FeatureFlags.VobCullingGroupSetting),
                new FeatureFlags.VobCullingGroupSetting{ maxObjectSize = 5f, cullingDistance = 100f}),
            new (nameof(FeatureFlags.vobCullingLarge), typeof(FeatureFlags.VobCullingGroupSetting),
                new FeatureFlags.VobCullingGroupSetting{ maxObjectSize = 100f, cullingDistance = 200f}),
            new (nameof(FeatureFlags.fireLightColor), typeof(Color),
                new Color(1, .87f, .44f, 1))
        };

        private static Scene _bootstrapScene
        {
            get
            {
                var scene = SceneManager.GetSceneByName("Bootstrap");
                if (scene == default)
                    Debug.LogError(">Bootstrap< scene needs to be loaded.");
                return scene;
            }
        }
        private static FeatureFlags _featureFlags => GameObject.Find("FeatureFlags").GetComponent<FeatureFlags>();


        [MenuItem("GothicVR/Tools/FeatureFlags - Production", priority = 1)]
        public static void SetFeatureFlagsProduction()
        {
            var fields = _featureFlags.GetType().GetFields();

            ResetFlags(fields);
            SetProductionFlags();

            EditorSceneManager.MarkSceneDirty(_bootstrapScene);

            Debug.Log("FeatureFlags successfully set to >production<.");
        }

        [MenuItem("GothicVR/Tools/FeatureFlags - NpcTest", priority = 2)]
        public static void SetFeatureFlagsNpcTest()
        {
            SetFeatureFlagsProduction();

            SetFlags(new()
            {
                new (nameof(FeatureFlags.createOcNpcs), typeof(bool), true),
                new (nameof(FeatureFlags.enableNpcRoutines), typeof(bool), true),
                // 1 - Diego, 100 - Gomez, 233 - Blodwyn
                new (nameof(FeatureFlags.npcToSpawn), typeof(List<int>), new List<int>{1, 100, 233})
            });

            Debug.Log("FeatureFlags successfully set to >NpcTest<.");
        }

        /// <summary>
        /// We reset all flags to a default value. It's expected, that e.g. a bool=false is default.
        /// </summary>
        private static void ResetFlags(FieldInfo[] fields)
        {
            foreach (var field in fields)
            {
                switch (field.FieldType.Name)
                {
                    case "Boolean":
                        field.SetValue(_featureFlags, false);
                        break;
                    case "Int32":
                    case "Single": // float
                    case "SunMovementPerformance":
                        field.SetValue(_featureFlags, 0);
                        break;
                    case "LogLevel":
                        field.SetValue(_featureFlags, LogLevel.Error);
                        break;
                    case "String":
                            field.SetValue(_featureFlags, "");
                            break;
                    case "Color":
                        field.SetValue(_featureFlags, Color.white);
                        break;
                    case "VobCullingGroupSetting":
                            field.SetValue(_featureFlags, new FeatureFlags.VobCullingGroupSetting());
                            break;
                    case "List`1":
                        switch (field.FieldType.GenericTypeArguments[0].Name)
                        {
                            case "Int32":
                                ((List<int>)field.GetValue(_featureFlags)).Clear();
                                break;
                            case nameof(VirtualObjectType):
                                ((List<VirtualObjectType>)field.GetValue(_featureFlags)).Clear();
                                break;
                            default:
                                Debug.LogError($"Unsupported field type >{field.FieldType.GenericTypeArguments[0].Name}<");
                                break;
                        }
                        break;
                    default:
                        Debug.LogError($"Unsupported field type >{field.FieldType.Name}< for >{field.Name}<");
                        break;
                }
            }
        }

        /// <summary>
        /// Pick the ProductionFlags and set values to demanded values.
        /// </summary>
        private static void SetProductionFlags()
        {
            SetFlags(ProductionFlags);
        }

        /// <summary>
        /// Set whatever flags we want.
        /// </summary>
        private static void SetFlags(List<(string name, Type type, object value)> flags)
        {
            foreach (var flag in flags)
            {
                var field = _featureFlags.GetType().GetField(flag.name);
                switch (flag.type.Name)
                {
                    case "Boolean":
                    case "Int32":
                    case "Single": // float
                    case "Color":
                    case "VobCullingGroupSetting":
                        field.SetValue(_featureFlags, flag.value);
                        break;
                    case "List`1":
                        switch (field.FieldType.GenericTypeArguments[0].Name)
                        {
                            case "Int32":
                                field.SetValue(_featureFlags, flag.value);
                                break;
                            default:
                                Debug.LogError($"Unsupported field type >{field.FieldType.GenericTypeArguments[0].Name}<");
                                break;
                        }
                        break;
                    default:
                        Debug.LogError($"Unsupported/Untested field type >{flag.type.Name}< for >{flag.name}<");
                        break;
                }
            }
        }
    }
}
