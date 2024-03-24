/*******************************************************************************
Copyright © 2015-2022 PICO Technology Co., Ltd.All rights reserved.  

NOTICE：All information contained herein is, and remains the property of 
PICO Technology Co., Ltd. The intellectual and technical concepts 
contained herein are proprietary to PICO Technology Co., Ltd. and may be 
covered by patents, patents in process, and are protected by trade secret or 
copyright law. Dissemination of this information or reproduction of this 
material is strictly forbidden unless prior written permission is obtained from
PICO Technology Co., Ltd. 
*******************************************************************************/

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.OpenXR.Features.PICOSupport
{
    [System.Serializable]
    public class PICOProjectSetting: ScriptableObject
    {
        public bool useContentProtect;
        public bool isEyeTracking;
        public bool isHandTracking;
        public bool isCameraSubsystem;
        public bool isEyeTrackingCalibration;
        public SystemDisplayFrequency displayFrequency;
        public SecureContentFlag contentProtectFlags ;
        public  bool foveationEnable;
        public  FoveationFeature.FoveatedRenderingMode foveatedRenderingMode;
        public  FoveationFeature.FoveatedRenderingLevel foveatedRenderingLevel;
        public bool isSubsampledEnabled;

        [SerializeField, Tooltip("Set the system splash screen picture in PNG format.")]
        public Texture2D systemSplashScreen;
        private string splashPath = string.Empty;

        public static PICOProjectSetting GetProjectConfig()
        {
            PICOProjectSetting projectConfig = Resources.Load<PICOProjectSetting>("PICOProjectSetting");
#if UNITY_EDITOR
            if (projectConfig == null)
            {
                projectConfig = CreateInstance<PICOProjectSetting>();
                projectConfig.useContentProtect = false;
                projectConfig.contentProtectFlags = SecureContentFlag.SECURE_CONTENT_OFF;
                projectConfig.isEyeTracking = false;
                projectConfig.isCameraSubsystem = false;
                projectConfig.isEyeTrackingCalibration = false;
                projectConfig.isHandTracking = false;
                projectConfig.displayFrequency = SystemDisplayFrequency.Default;
                projectConfig.foveationEnable = false;
                projectConfig.foveatedRenderingMode = FoveationFeature.FoveatedRenderingMode.FixedFoveatedRendering;
                projectConfig.foveatedRenderingLevel = FoveationFeature.FoveatedRenderingLevel.Off;
                projectConfig.isSubsampledEnabled = false;
                string path = Application.dataPath + "/Resources";
                if (!Directory.Exists(path))
                {
                    UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                    UnityEditor.AssetDatabase.CreateAsset(projectConfig, "Assets/Resources/PICOProjectSetting.asset");
                }
                else
                {
                    UnityEditor.AssetDatabase.CreateAsset(projectConfig, "Assets/Resources/PICOProjectSetting.asset");
                }
            }
#endif
            return projectConfig;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (systemSplashScreen != null)
            {
                splashPath = AssetDatabase.GetAssetPath(systemSplashScreen);
                if (Path.GetExtension(splashPath).ToLower() != ".png")
                {
                    systemSplashScreen = null;
                    Debug.LogError("Invalid file format of System Splash Screen, only PNG format is supported. The asset path: " + splashPath);
                    splashPath = string.Empty;
                }
            }
        }

        public string GetSystemSplashScreen(string path)
        {
            if (systemSplashScreen == null || splashPath == string.Empty)
            {
                return "0";
            }

            string targetPath = Path.Combine(path, "src/main/assets/pico_splash.png");
            FileUtil.ReplaceFile(splashPath, targetPath);
            return "1";
        }
#endif
    }
}
