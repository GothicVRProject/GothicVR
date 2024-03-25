using UnityEditor;
using UnityEngine.XR.OpenXR.Features;
using System.Runtime.InteropServices;
using System;
using Unity.XR.OpenXR.Features.PICOSupport;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;

[OpenXRFeature(UiName = "OpenXR Foveation",
    BuildTargetGroups = new[] { BuildTargetGroup.Android },
    OpenxrExtensionStrings = extensionList,
    Company = "PICO",
    Version = "1.0.0",
    FeatureId = featureId)]
#endif


public class FoveationFeature : OpenXRFeatureBase
{
    public const string extensionList = "XR_FB_foveation " +
                                        "XR_FB_foveation_configuration " +
                                        "XR_FB_foveation_vulkan " +
                                        "XR_META_foveation_eye_tracked " +
                                        "XR_META_vulkan_swapchain_create_info " +
                                        "XR_FB_swapchain_update_state ";

    public const string featureId = "com.pico.openxr.feature.foveation";

    public enum FoveatedRenderingLevel
    {
        Off = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }
    public enum FoveatedRenderingMode
    {
        FixedFoveatedRendering = 0,
        EyeTrackedFoveatedRendering = 1
    }
    private static string TAG = "FoveationFeature";

    private static UInt32 _foveatedRenderingLevel = 0;
    private static UInt32 _useDynamicFoveation = 0;
    public static bool isExtensionEnable =false;

    public override string GetExtensionString()
    {
        return extensionList;
    }
    public override void Initialize(IntPtr intPtr)
    {
        isExtensionEnable=_isExtensionEnable;
    }
    public override void SessionCreate()
    {
        if (!isExtensionEnable)
        {
            return ;
        }
        PICOProjectSetting projectConfig = PICOProjectSetting.GetProjectConfig();
        if (projectConfig.foveationEnable)
        {
            setFoveationEyeTracked(projectConfig.foveatedRenderingMode ==
                                   FoveatedRenderingMode.EyeTrackedFoveatedRendering);
            foveatedRenderingLevel = projectConfig.foveatedRenderingLevel;
            setSubsampledEnabled(projectConfig.isSubsampledEnabled);
        }
    }
    public static FoveatedRenderingLevel foveatedRenderingLevel
    {
        get
        {
            if (!isExtensionEnable)
            {
                return FoveatedRenderingLevel.Off;
            }
            UInt32 level;
            FBGetFoveationLevel(out level);
            PLog.i($"  foveatedRenderingLevel get if level= {level}");
            return (FoveatedRenderingLevel)level;
        }
        set
        {
            if (!isExtensionEnable)
            {
                return;
            }
            PLog.i($"  foveatedRenderingLevel set if value= {value}");
            _foveatedRenderingLevel = (UInt32)value;
            FBSetFoveationLevel(xrSession, _foveatedRenderingLevel, 0.0f, _useDynamicFoveation);
        }
    }

    public static bool useDynamicFoveatedRendering
    {
        get
        {
            if (!isExtensionEnable)
            {
                return false;
            }
            UInt32 dynamic;
            FBGetFoveationLevel(out dynamic);
            return dynamic != 0;
        }
        set
        {
            if (!isExtensionEnable)
            {
                return ;
            }
            if (value)
                _useDynamicFoveation = 1;
            else
                _useDynamicFoveation = 0;
            FBSetFoveationLevel(xrSession, _foveatedRenderingLevel, 0.0f, _useDynamicFoveation);
        }
    }

    public static bool supportsFoveationEyeTracked
    {
        get
        {
            if (!isExtensionEnable)
            {
                return false;
            }
            return isSupportsFoveationEyeTracked(xrInstance);
        }
    }
    


    #region OpenXR Plugin DLL Imports

    [DllImport("UnityOpenXR", EntryPoint = "FBSetFoveationLevel")]
    private static extern void FBSetFoveationLevel(UInt64 session, UInt32 level, float verticalOffset, UInt32 dynamic);

    [DllImport("UnityOpenXR", EntryPoint = "FBGetFoveationLevel")]
    private static extern void FBGetFoveationLevel(out UInt32 level);

    [DllImport("UnityOpenXR", EntryPoint = "FBGetFoveationDynamic")]
    private static extern void FBGetFoveationDynamic(out UInt32 dynamic);

    #endregion

    const string extLib = "openxr_pico";

    [DllImport(extLib, EntryPoint = "PICO_isSupportsFoveationEyeTracked", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool isSupportsFoveationEyeTracked(ulong xrInstance);

    [DllImport(extLib, EntryPoint = "PICO_setFoveationEyeTracked", CallingConvention = CallingConvention.Cdecl)]
    private static extern void setFoveationEyeTracked(bool value);
    [DllImport(extLib, EntryPoint = "PICO_setSubsampledEnabled", CallingConvention = CallingConvention.Cdecl)]
    private static extern void setSubsampledEnabled(bool value);
}