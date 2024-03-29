using System.Collections.Generic;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using Object = System.Object;
using UnityEngine.XR.OpenXR.Features.Interactions;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;


#if UNITY_EDITOR
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

#if AR_FOUNDATION
using UnityEngine.XR.ARSubsystems;
#endif

namespace Unity.XR.OpenXR.Features.PICOSupport
{
    /// <summary>
    /// Enables the PICO mobile OpenXR Loader for Android, and modifies the AndroidManifest to be compatible with Neo3.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "PICO Support",
        Desc = "Necessary to deploy an PICO compatible app.",
        Company = "PICO",
        Version = "1.0.0",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        CustomRuntimeLoaderBuildTargets = new[] { BuildTarget.Android },
        OpenxrExtensionStrings = PicoExtensionList,
        FeatureId = featureId
    )]
#endif

    [System.Serializable]
    public class PICOFeature : OpenXRFeature
    {
        public const string PicoExtensionList = "";
        public static string SDKVersion = "Unity_OpenXR_1.2.0";
        public static Action<bool> onAppFocusedAction;
        public bool isCameraSubsystem;
#if AR_FOUNDATION
        static List<XRHumanBodySubsystemDescriptor> s_HumanBodyDescriptors = new List<XRHumanBodySubsystemDescriptor>();
        static List<XRFaceSubsystemDescriptor> s_FaceDescriptors = new List<XRFaceSubsystemDescriptor>();
        static List<XRCameraSubsystemDescriptor> s_CameraDescriptors = new List<XRCameraSubsystemDescriptor>();
#endif

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.pico";
        private static ulong xrSession = 0ul;

#if UNITY_EDITOR
        static AddRequest request;
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            OpenXRSettings settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            

            var AdditionalRules = new ValidationRule[]
            {
                new ValidationRule(this)
                {
                    message = "Only the PICO Touch Interaction Profile is supported right now.",
                    checkPredicate = () =>
                    {
                        if (null == settings)
                            return false;

                        bool touchFeatureEnabled = false;
                        bool otherInteractionFeatureEnabled = false;

                        foreach (var feature in settings.GetFeatures<OpenXRInteractionFeature>())
                        {
                            if (feature.enabled)
                            {
                                if ((feature is PICONeo3ControllerProfile) || (feature is PICO4ControllerProfile) || (feature is EyeGazeInteraction)|| (feature is PICOG3ControllerProfile))
                                    touchFeatureEnabled = true;
                                else
                                    otherInteractionFeatureEnabled = true;
                            }
                        }

                        return touchFeatureEnabled && !otherInteractionFeatureEnabled;
                    },
                    fixIt = () =>
                    {
                        if (null == settings)
                            return;

                        foreach (var feature in settings.GetFeatures<OpenXRInteractionFeature>())
                        {
                            feature.enabled = ((feature is PICONeo3ControllerProfile) || (feature is PICO4ControllerProfile));
                        }
                    },
                    error = true,
                }, 
                new ValidationRule(this)
                {
                    message = "Only Unity OpenXR Plugin prior to version 1.9.1 is supported right now.",
                    checkPredicate = () =>
                    {
#if OPENXR_1_9_1
                        return false;
#else
                        return true;
#endif
                    },
                    fixIt = () =>
                    {
                        if (request == null)
                        {
                            request =  Client.Add("com.unity.xr.openxr@1.8.2");
                        }
                        EditorApplication.update += Progress;
                    },
                    error = true,
                    fixItMessage = "Unity OpenXR plugin will be downgraded to 1.8.2."
                }
            };

            rules.AddRange(AdditionalRules);
        }
        
        static void Progress()
        {
            if (request != null && request.IsCompleted)
            {
                if (request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + request.Result.packageId);
                else if (request.Status >= StatusCode.Failure)
                    Debug.Log(request.Error.message);
                EditorApplication.update -= Progress;
                request = null;
            }
        }
        
        internal class PICOFeatureEditorWindow : EditorWindow
        {
            private Object feature;
            private Editor featureEditor;

            public static EditorWindow Create(Object feature)
            {
                var window = EditorWindow.GetWindow<PICOFeatureEditorWindow>(true, "PICO Feature Configuration", true);
                window.feature = feature;
                window.featureEditor = Editor.CreateEditor((UnityEngine.Object)feature);
                return window;
            }

            private void OnGUI()
            {
                featureEditor.OnInspectorGUI();
            }
        }

#endif

        protected override void OnSubsystemCreate()
        {
            base.OnSubsystemCreate();
#if AR_FOUNDATION
            // PICOProjectSetting projectConfig = PICOProjectSetting.GetProjectConfig();
        
            isCameraSubsystem = isCameraSubsystem && OpenXRRuntime.IsExtensionEnabled("XR_FB_passthrough");
            if (isCameraSubsystem)
            {
                CreateSubsystem<XRCameraSubsystemDescriptor, XRCameraSubsystem>(
                    s_CameraDescriptors,
                    PICOCameraSubsystem.k_SubsystemId);
            }
#endif
        }

        protected override void OnSubsystemStart()
        {
#if AR_FOUNDATION
            if (isCameraSubsystem)
            {
                StartSubsystem<XRCameraSubsystem>();
            }
#endif
        }

        protected override void OnSubsystemStop()
        {
#if AR_FOUNDATION
            if (isCameraSubsystem)
            {
                StopSubsystem<XRCameraSubsystem>();
            }
#endif
        }

        protected override void OnSubsystemDestroy()
        {
#if AR_FOUNDATION
            if (isCameraSubsystem)
            {
                DestroySubsystem<XRCameraSubsystem>();
            }
#endif
        }
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            OpenXRExtensions.isPicoSupport = true;
            return base.OnInstanceCreate(xrInstance);
        }

        protected override void OnSessionStateChange(int oldState, int newState)
        {
            base.OnSessionStateChange(oldState, newState);
            if (onAppFocusedAction != null)
            {
                onAppFocusedAction(newState == 5);
            }
            if (newState == 1)
            {
#if AR_FOUNDATION
                if (isCameraSubsystem)
                {
                    StopSubsystem<XRCameraSubsystem>();
                }
#endif
            }
            else if (newState == 5)
            {
#if AR_FOUNDATION
                if (isCameraSubsystem)
                {
                    StartSubsystem<XRCameraSubsystem>();
                }
#endif
            }
        }
        protected override void OnSessionCreate(ulong xrSessionId)
        {
            xrSession = xrSessionId;
            base.OnSessionCreate(xrSessionId);
            //log level
            float logLevel = 0;
            if (GetLogLevel(xrSession, ref logLevel))
            {
                PLog.logLevel = (PLog.LogLevel)logLevel;
            }
            PLog.i($"OpenXR SDK Version:{SDKVersion}, logLevel:{(int)PLog.logLevel}");
            LogLevelCallback(OnMessage);
        }

        private delegate void OnChangeDelegate(int level);
        [MonoPInvokeCallback(typeof(OnChangeDelegate))]
        private static void OnMessage(int level)
        {
            if (level > 0)
            {
                PLog.logLevel = (PLog.LogLevel)level;
            }

            PLog.i($"OpenXR level:{level}, logLevel:{(int)PLog.logLevel}");
        }

        /// <inheritdoc/>
        protected override void OnSessionDestroy(ulong xrSessionId)
        {
            base.OnSessionDestroy(xrSessionId);
            xrSession = 0ul;
        }
        

        internal delegate void ReceiveLogLevelChangeDelegate(int level);

        const string extLib = "openxr_pico";
        [DllImport(extLib, EntryPoint = "PICO_GetLogLevel", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool GetLogLevel(ulong xrSpace, ref float ret);
        
        [DllImport(extLib, EntryPoint = "PICO_LogLevelCallback")]
        private static extern void LogLevelCallback(ReceiveLogLevelChangeDelegate callback);

    }
}