using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Unity.XR.OpenXR.Features.PICOSupport
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "OpenXR Composition Layer Secure Content",
        Hidden = false,
        BuildTargetGroups = new[] { UnityEditor.BuildTargetGroup.Android },
        Company = "PICO",
        OpenxrExtensionStrings = extensionString,
        Version = "1.0.0",
        FeatureId = featureId)]
#endif
    public class LayerSecureContentFeature : OpenXRFeatureBase
    {
        public const string featureId = "com.pico.openxr.feature.LayerSecureContent";
        public const string extensionString = "XR_FB_composition_layer_secure_content";

        public static bool isExtensionEnable =false;
        public override string GetExtensionString()
        {
            return extensionString;
        }

        public override void Initialize(IntPtr intPtr)
        {
            isExtensionEnable=_isExtensionEnable;
        }
        public override void SessionCreate()
        {
            PICOProjectSetting projectConfig = PICOProjectSetting.GetProjectConfig();
            if (projectConfig.useContentProtect)
            {
                SetSecureContentFlag(projectConfig.contentProtectFlags);
            }
        }
        

        public static void SetSecureContentFlag(SecureContentFlag flag)
        {
            if (!isExtensionEnable)
            {
                return;
            }

            setSecureContentFlag((int)flag);
        }

        const string extLib = "openxr_pico";

        [DllImport(extLib, EntryPoint = "PICO_setSecureContentFlag", CallingConvention = CallingConvention.Cdecl)]
        private static extern void setSecureContentFlag(Int32 flag);
    }
}