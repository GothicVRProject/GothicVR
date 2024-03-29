using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Unity.XR.OpenXR.Features.PICOSupport
{
    public enum SystemDisplayFrequency
    {
        Default,
        RefreshRate72=72,
        RefreshRate90=90,
        RefreshRate120=120,
    }
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "OpenXR Display Refresh Rate",
        Hidden = false,
        BuildTargetGroups = new[] { UnityEditor.BuildTargetGroup.Android },
        Company = "PICO",
        OpenxrExtensionStrings = extensionString,
        Version = "1.0.0",
        FeatureId = featureId)]
#endif
    public class DisplayRefreshRateFeature : OpenXRFeatureBase
    {
        public const string featureId = "com.pico.openxr.feature.refreshrate";
        public const string extensionString = "XR_FB_display_refresh_rate";
        public static bool isExtensionEnable =false;
        public override void Initialize(IntPtr intPtr)
        {
            isExtensionEnable=_isExtensionEnable;
            initialize(intPtr, xrInstance);
        }
        public override string GetExtensionString()
        {
            return extensionString;
        }

        public override void SessionCreate()
        {
            PICOProjectSetting projectConfig = PICOProjectSetting.GetProjectConfig();
            if (projectConfig.displayFrequency != SystemDisplayFrequency.Default)
            {
                float displayRefreshRate = 0;
                GetDisplayRefreshRate(ref displayRefreshRate);
                PLog.i($"GetDisplayRefreshRate:{displayRefreshRate}");
                SetDisplayRefreshRate(projectConfig.displayFrequency);
            }
        }
        public static bool SetDisplayRefreshRate(SystemDisplayFrequency DisplayFrequency)
        {
            if (!isExtensionEnable)
            {
                return false;
            }

          
            PLog.i($"SetDisplayRefreshRate:{DisplayFrequency}");
            float rate = 0;
            switch (DisplayFrequency)
            {
                case SystemDisplayFrequency.Default:
                    return true;
                case SystemDisplayFrequency.RefreshRate72:
                    rate = 72;
                    break;
                case SystemDisplayFrequency.RefreshRate90:
                    rate = 90;
                    break;
                case SystemDisplayFrequency.RefreshRate120:
                    rate = 120;
                    break;
            }

            return SetDisplayRefreshRate(rate);
        }

        public static bool GetDisplayRefreshRate(ref float displayRefreshRate)
        {
            if (!isExtensionEnable)
            {
                return false;
            }
            return xrGetDisplayRefreshRateFB(
                xrSession, ref displayRefreshRate);
        }

        private static bool SetDisplayRefreshRate(float displayRefreshRate)
        {
            if (!isExtensionEnable)
            {
                return false;
            }
            return xrRequestDisplayRefreshRateFB(
                xrSession, displayRefreshRate);
        }

        private static bool EnumerateDisplayRefreshRates(uint displayRefreshRateCapacityInput,
            ref uint displayRefreshRateCountOutput, ref float displayRefreshRates)
        {
            if (!isExtensionEnable)
            {
                return false;
            }
            return xrEnumerateDisplayRefreshRatesFB(
                xrSession, displayRefreshRateCapacityInput, ref displayRefreshRateCountOutput, ref displayRefreshRates);
        }
        
        public static int GetDisplayRefreshRateCount()
        {
            if (!isExtensionEnable)
            {
                return 0;
            }
            return xrGetDisplayRefreshRateCount(xrSession);
        }
        
        public static bool TryGetSupportedDisplayRefreshRates(
             Allocator allocator, out NativeArray<float> refreshRates)
        {
            refreshRates = default;

            if (!isExtensionEnable)
            {
                return false;
            }

            var numDisplayRefreshRates = xrGetDisplayRefreshRateCount(xrSession);
            if (numDisplayRefreshRates == 0)
            {
                Debug.LogError($"{nameof(TryGetSupportedDisplayRefreshRates)} failed due to an unknown error.");
                return false;
            }

            unsafe
            {
                refreshRates = new NativeArray<float>(numDisplayRefreshRates, allocator);
                if (!refreshRates.IsCreated)
                    return false;

                return TryGetDisplayRefreshRates(xrSession,
                    NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(refreshRates),
                    (uint)numDisplayRefreshRates);
            }
        }

        private const string ExtLib = "openxr_pico";

        [DllImport(ExtLib, EntryPoint = "PICO_initialize_DisplayRefreshRates", CallingConvention = CallingConvention.Cdecl)]
        private static extern void initialize(IntPtr xrGetInstanceProcAddr, ulong xrInstance);

        [DllImport(ExtLib, EntryPoint = "PICO_xrEnumerateDisplayRefreshRatesFB", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool xrEnumerateDisplayRefreshRatesFB(ulong xrSession,
            uint displayRefreshRateCapacityInput, ref uint displayRefreshRateCountOutput,
            ref float displayRefreshRates);

        [DllImport(ExtLib, EntryPoint = "PICO_xrGetDisplayRefreshRateFB", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool xrGetDisplayRefreshRateFB(ulong xrSession, ref float displayRefreshRate);

        [DllImport(ExtLib, EntryPoint = "PICO_xrRequestDisplayRefreshRateFB", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool xrRequestDisplayRefreshRateFB(ulong xrSession, float displayRefreshRate);
        [DllImport(ExtLib, EntryPoint = "PICO_xrGetDisplayRefreshRateCount", CallingConvention = CallingConvention.Cdecl)]
        public static extern int xrGetDisplayRefreshRateCount(ulong xrSession);
        
        [DllImport(ExtLib, EntryPoint = "PICO_xrTryGetDisplayRefreshRates", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe bool TryGetDisplayRefreshRates(ulong xrSession,void* refreshRates, uint capacity);
    }
}