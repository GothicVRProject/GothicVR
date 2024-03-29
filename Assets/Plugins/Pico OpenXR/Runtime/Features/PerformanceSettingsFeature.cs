using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Unity.XR.OpenXR.Features.PICOSupport
{
    public enum XrPerfSettingsDomainEXT
    {
        XR_PERF_SETTINGS_DOMAIN_CPU_EXT = 1,
        XR_PERF_SETTINGS_DOMAIN_GPU_EXT = 2,
    }

    public enum XrPerfSettingsLevelEXT
    {
        XR_PERF_SETTINGS_LEVEL_POWER_SAVINGS_EXT = 0,
        XR_PERF_SETTINGS_LEVEL_SUSTAINED_LOW_EXT = 25,
        XR_PERF_SETTINGS_LEVEL_SUSTAINED_HIGH_EXT = 50,
        XR_PERF_SETTINGS_LEVEL_BOOST_EXT = 75,
    }

    public enum XrPerfSettingsSubDomainEXT
    {
        XR_PERF_SETTINGS_SUB_DOMAIN_COMPOSITING_EXT = 1,
        XR_PERF_SETTINGS_SUB_DOMAIN_RENDERING_EXT = 2,
        XR_PERF_SETTINGS_SUB_DOMAIN_THERMAL_EXT = 3,
    }

    public enum XrPerfSettingsNotificationLevelEXT
    {
        XR_PERF_SETTINGS_NOTIF_LEVEL_NORMAL_EXT = 0,
        XR_PERF_SETTINGS_NOTIF_LEVEL_WARNING_EXT = 25,
        XR_PERF_SETTINGS_NOTIF_LEVEL_IMPAIRED_EXT = 75,
    }
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "OpenXR Performance Settings",
        Hidden = false,
        BuildTargetGroups = new[] { UnityEditor.BuildTargetGroup.Android },
        Company = "PICO",
        OpenxrExtensionStrings = extensionString,
        Version = "1.0.0",
        FeatureId = featureId)]
#endif

    public class PerformanceSettingsFeature : OpenXRFeatureBase
    {
        public const string featureId = "com.pico.openxr.feature.performancesettings";
        public const string extensionString = "XR_EXT_performance_settings";
        public static bool isExtensionEnable = false;

        private static Action<XrPerfSettingsDomainEXT, XrPerfSettingsSubDomainEXT, XrPerfSettingsNotificationLevelEXT,
            XrPerfSettingsNotificationLevelEXT> mCallback;

        public delegate void EventDelegate(XrPerfSettingsDomainEXT domain, XrPerfSettingsSubDomainEXT subDomain,
            XrPerfSettingsNotificationLevelEXT fromLevel,
            XrPerfSettingsNotificationLevelEXT toLevel);

        public override void Initialize(IntPtr intPtr)
        {
            isExtensionEnable = _isExtensionEnable;
            initialize(intPtr, xrInstance, EventFromCpp);
        }


        [MonoPInvokeCallback(typeof(EventDelegate))]
        public static void EventFromCpp(XrPerfSettingsDomainEXT domain, XrPerfSettingsSubDomainEXT subDomain,
            XrPerfSettingsNotificationLevelEXT fromLevel,
            XrPerfSettingsNotificationLevelEXT toLevel)
        {
            if (mCallback != null)
            {
                mCallback(domain, subDomain, fromLevel, toLevel);
            }
        }

        public override string GetExtensionString()
        {
            return extensionString;
        }

        public static XrResult XrPerfSettingsSetPerformanceLevelEXT(XrPerfSettingsDomainEXT domain,
            XrPerfSettingsLevelEXT level)
        {
            return xrPerfSettingsSetPerformanceLevelEXT(xrSession, domain, level);
        }


        public static void AddPerfSettingsSetPerformanceEvent(
            Action<XrPerfSettingsDomainEXT, XrPerfSettingsSubDomainEXT, XrPerfSettingsNotificationLevelEXT,
                XrPerfSettingsNotificationLevelEXT> callback)
        {
            mCallback = callback;
        }

        private const string ExtLib = "openxr_pico";

        [DllImport(ExtLib, EntryPoint = "PICO_initialize_PerformanceSettings",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void initialize(IntPtr xrGetInstanceProcAddr, ulong xrInstance,
            EventDelegate eventDelegate);

        [DllImport(ExtLib, EntryPoint = "PICO_xrPerfSettingsSetPerformanceLevelEXT",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern XrResult xrPerfSettingsSetPerformanceLevelEXT(ulong xrSession,
            XrPerfSettingsDomainEXT domain, XrPerfSettingsLevelEXT level);
    }
}