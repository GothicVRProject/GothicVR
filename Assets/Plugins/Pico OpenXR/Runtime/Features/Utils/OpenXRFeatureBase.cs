using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace Unity.XR.OpenXR.Features.PICOSupport
{
    public abstract class OpenXRFeatureBase : OpenXRFeature
    {
        protected static ulong xrInstance = 0ul;
        protected static ulong xrSession = 0ul;
        protected string extensionUrl = "";
        public bool _isExtensionEnable = false;
        
        protected override bool OnInstanceCreate(ulong instance)
        {
            extensionUrl = GetExtensionString();
            _isExtensionEnable = isExtensionEnabled();
            if (!_isExtensionEnable)
            {
                return false;
            }

            xrInstance = instance;
            xrSession = 0ul;

            Initialize(xrGetInstanceProcAddr);
            return true;
        }

#if UNITY_EDITOR
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
            rules.Add(new ValidationRule(this)
            {
                message = "No PICO OpenXR Features selected.",
                checkPredicate = () =>
                {
                    if (null == settings)
                        return false;
                    
                    foreach (var feature in settings.GetFeatures<OpenXRFeature>())
                    {
                        if (feature is OpenXRExtensions)
                        {
                            return feature.enabled;
                        }
                    }

                    return false;
                },
                fixIt = () =>
                {
                    if (null == settings)
                        return ;
                    var openXRExtensions = settings.GetFeature<OpenXRExtensions>();
                    if (openXRExtensions != null)
                    {
                        openXRExtensions.enabled = true;
                    }
                },
                error = true
            });
        }
#endif

        public  bool isExtensionEnabled()
        {
            string[] exts = extensionUrl.Split(' ');
            if (exts.Length>0)
            {
                foreach (var _ext in exts)
                {
                    if (!string.IsNullOrEmpty(_ext) && !OpenXRRuntime.IsExtensionEnabled(_ext))
                    {
                        PLog.e(_ext + " is not enabled");
                        return false;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(extensionUrl) && !OpenXRRuntime.IsExtensionEnabled(extensionUrl))
                {
                    PLog.e(extensionUrl + " is not enabled");
                    return false;
                }

            }
            return true;
        }

        protected override void OnSessionCreate(ulong xrSessionId)
        {
            xrSession = xrSessionId;
            base.OnSessionCreate(xrSession);
            SessionCreate();
        }

        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            base.OnInstanceDestroy(xrInstance);
            xrInstance = 0ul;
        }

        protected override void OnSessionDestroy(ulong xrSessionId)
        {
            base.OnSessionDestroy(xrSessionId);
            xrSession = 0ul;
        }

        public virtual void Initialize(IntPtr intPtr)
        {
        }

        public abstract string GetExtensionString();
       
        public virtual void SessionCreate()
        {
        }
        public static bool IsSuccess(XrResult result) => result == 0;
    }
}