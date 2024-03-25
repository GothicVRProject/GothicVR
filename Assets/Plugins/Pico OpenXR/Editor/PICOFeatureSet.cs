
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;


namespace Unity.XR.OpenXR.Features.PICOSupport
{

    [OpenXRFeatureSet(
        FeatureIds = new string[] {
            PICOFeature.featureId,
            OpenXRExtensions.featureId,
            DisplayRefreshRateFeature.featureId,
            LayerSecureContentFeature.featureId,
            FoveationFeature.featureId,
            PassthroughFeature.featureId,
        },
        UiName = "PICO XR",
        Description = "Feature set for using PICO XR Features",
        FeatureSetId = featureSetId,
        SupportedBuildTargets = new BuildTargetGroup[] { BuildTargetGroup.Android},
        RequiredFeatureIds = new string[]
        {
            PICOFeature.featureId,
            OpenXRExtensions.featureId,
        }
    )]
    class PICOFeatureSet
    {
        public const string featureSetId = "com.picoxr.openxr.features";
    }
}


