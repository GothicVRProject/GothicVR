#if AR_FOUNDATION
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.OpenXR.Features.PICOSupport
{
    public class PICOCameraSubsystem: XRCameraSubsystem
    {
        internal const string k_SubsystemId = "PICO-Camera";
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            var cameraSubsystemCinfo = new XRCameraSubsystemCinfo
            {
                id = k_SubsystemId,
                providerType = typeof(PICOOpenXRProvider),
                subsystemTypeOverride = typeof(PICOCameraSubsystem),
                supportsAverageBrightness = false,
                supportsAverageColorTemperature = false,
                supportsColorCorrection = false,
                supportsDisplayMatrix = false,
                supportsProjectionMatrix = false,
                supportsTimestamp = false,
                supportsCameraConfigurations = false,
                supportsCameraImage = false,
                supportsAverageIntensityInLumens = false,
                supportsFocusModes = false,
                supportsFaceTrackingAmbientIntensityLightEstimation = false,
                supportsFaceTrackingHDRLightEstimation = false,
                supportsWorldTrackingAmbientIntensityLightEstimation = false,
                supportsWorldTrackingHDRLightEstimation = false,
                supportsCameraGrain = false,
            };

            if (!XRCameraSubsystem.Register(cameraSubsystemCinfo))
            {
                PLog.e($"Failed to register the {k_SubsystemId} subsystem.");
            }
        }

        class PICOOpenXRProvider : Provider
        {
            /// <summary>
            /// Construct the camera functionality provider for Meta.
            /// </summary>
            public PICOOpenXRProvider()
            {
            }

            /// <summary>
            /// Start the camera functionality.
            /// </summary>
            public override void Start() => PassthroughFeature.EnableSeeThroughManual(true);

            /// <summary>
            /// Stop the camera functionality.
            /// </summary>
            public override void Stop() => PassthroughFeature.EnableSeeThroughManual(false);

            /// <summary>
            /// Destroy any resources required for the camera functionality.
            /// </summary>
            public override void Destroy() => PassthroughFeature.Destroy();

           
        }
        
    }
}
#endif