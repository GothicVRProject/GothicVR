using System;
using System.Runtime.InteropServices;
using UnityEngine;


namespace Unity.XR.OpenXR.Features.PICOSupport
{
    public enum XrResult
    {
        Success = 0,
        TimeoutExpored = 1,
        LossPending = 3,
        EventUnavailable = 4,
        SpaceBoundsUnavailable = 7,
        SessionNotFocused = 8,
        FrameDiscarded = 9,
        ValidationFailure = -1,
        RuntimeFailure = -2,
        OutOfMemory = -3,
        ApiVersionUnsupported = -4,
        InitializationFailed = -6,
        FunctionUnsupported = -7,
        FeatureUnsupported = -8,
        ExtensionNotPresent = -9,
        LimitReached = -10,
        SizeInsufficient = -11,
        HandleInvalid = -12,
        InstanceLOst = -13,
        SessionRunning = -14,
        SessionNotRunning = -16,
        SessionLost = -17,
        SystemInvalid = -18,
        PathInvalid = -19,
        PathCountExceeded = -20,
        PathFormatInvalid = -21,
        PathUnsupported = -22,
        LayerInvalid = -23,
        LayerLimitExceeded = -24,
        SpwachainRectInvalid = -25,
        SwapchainFormatUnsupported = -26,
        ActionTypeMismatch = -27,
        SessionNotReady = -28,
        SessionNotStopping = -29,
        TimeInvalid = -30,
        ReferenceSpaceUnsupported = -31,
        FileAccessError = -32,
        FileContentsInvalid = -33,
        FormFactorUnsupported = -34,
        FormFactorUnavailable = -35,
        ApiLayerNotPresent = -36,
        CallOrderInvalid = -37,
        GraphicsDeviceInvalid = -38,
        PoseInvalid = -39,
        IndexOutOfRange = -40,
        ViewConfigurationTypeUnsupported = -41,
        EnvironmentBlendModeUnsupported = -42,
        NameDuplicated = -44,
        NameInvalid = -45,
        ActionsetNotAttached = -46,
        ActionsetsAlreadyAttached = -47,
        LocalizedNameDuplicated = -48,
        LocalizedNameInvalid = -49,
        AndroidThreadSettingsIdInvalidKHR = -1000003000,
        AndroidThreadSettingsdFailureKHR = -1000003001,
        CreateSpatialAnchorFailedMSFT = -1000039001,
        SecondaryViewConfigurationTypeNotEnabledMSFT = -1000053000,
        MaxResult = 0x7FFFFFFF
    }

    public struct XrExtent2Df
    {
        public float width;
        public float height;

        public XrExtent2Df(float x, float y)
        {
            this.width = x;
            this.height = y;
        }

        public XrExtent2Df(Vector2 value)
        {
            width = value.x;
            height = value.y;
        }

        public override string ToString()
        {
            return $"{nameof(width)}: {width}, {nameof(height)}: {height}";
        }
    };

    // [Flags]
    public enum XrReferenceSpaceType
    {
        View = 1,
        Local = 2,
        Stage = 3,
        UnboundedMsft = 1000038000,
        CombinedEyeVarjo = 1000121000
    }

    public enum XrBodyJointSetBD
    {
        XR_BODY_JOINT_SET_DEFAULT_BD = 0, //default joint set XR_BODY_JOINT_SET_BODY_STAR_WITHOUT_ARM_BD
        XR_BODY_JOINT_SET_BODY_STAR_WITHOUT_ARM_BD = 1,
        XR_BODY_JOINT_SET_BODY_FULL_STAR_BD = 2,
        XR_BODY_JOINT_SET_MAX_ENUM_BD = 0x7FFFFFFF
    }

    public struct xrPose
    {
        public double PosX; // position of x
        public double PosY; // position of y
        public double PosZ; // position of z
        public double RotQx; // x components of Quaternion
        public double RotQy; // y components of Quaternion
        public double RotQz; // z components of Quaternion
        public double RotQw; // w components of Quaternion
    }

    public struct BodyTrackerResult
    {
        public bool IsActive;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public FPICOBodyState[] trackingdata;
    }

    public struct FPICOBodyState
    {
        public bool bIsValid;
        public xrPose pose;
    }

    public enum SecureContentFlag
    {
        SECURE_CONTENT_OFF = 0,
        SECURE_CONTENT_REPLACE_LAYER = 2
    }

    public enum XrFaceTrackingModeBD
    {
        DEFAULT_BD = 0,                  // face
        COMBINED_AUDIO_BD = 1,           // combined bs
        COMBINED_AUDIO_WITH_LIP_BD = 2,  // combined vis
        ONLY_AUDIO_WITH_LIP_BD = 3,      // lip sync
    }

    /// <summary>
    /// Enum values that identify the face action units affecting the expression on the face.
    /// </summary>
    /// <remarks>Each action unit corresponds to a facial feature that can move. A coefficient of zero for the
    /// feature represents the neutral position, while a coefficient of one represents the fully articulated
    /// position.
    /// </remarks>
    public enum BlendShapeLocation
    {
        /// <summary>
        /// The coefficient describing closure of the eyelids over the left eye.
        /// </summary>
        EyeBlinkLeft = 0,

        /// <summary>
        /// The coefficient describing movement of the left eyelids consistent with a downward gaze.
        /// </summary>
        EyeLookDownLeft = 1,

        /// <summary>
        /// The coefficient describing movement of the left eyelids consistent with a rightward gaze.
        /// </summary>
        EyeLookInLeft = 2,

        /// <summary>
        /// The coefficient describing movement of the left eyelids consistent with a leftward gaze.
        /// </summary>
        EyeLookOutLeft = 3,

        /// <summary>
        /// The coefficient describing movement of the left eyelids consistent with an upward gaze.
        /// </summary>
        EyeLookUpLeft = 4,

        /// <summary>
        /// The coefficient describing contraction of the face around the left eye.
        /// </summary>
        EyeSquintLeft = 5,

        /// <summary>
        /// The coefficient describing a widening of the eyelids around the left eye.
        /// </summary>
        EyeWideLeft = 6,

        /// <summary>
        /// The coefficient describing closure of the eyelids over the right eye.
        /// </summary>
        EyeBlinkRight = 7,

        /// <summary>
        /// The coefficient describing movement of the right eyelids consistent with a downward gaze.
        /// </summary>
        EyeLookDownRight = 8,

        /// <summary>
        /// The coefficient describing movement of the right eyelids consistent with a leftward gaze.
        /// </summary>
        EyeLookInRight = 9,

        /// <summary>
        /// The coefficient describing movement of the right eyelids consistent with a rightward gaze.
        /// </summary>
        EyeLookOutRight = 10,

        /// <summary>
        /// The coefficient describing movement of the right eyelids consistent with an upward gaze.
        /// </summary>
        EyeLookUpRight = 11,

        /// <summary>
        /// The coefficient describing contraction of the face around the right eye.
        /// </summary>
        EyeSquintRight = 12,

        /// <summary>
        /// The coefficient describing a widening of the eyelids around the right eye.
        /// </summary>
        EyeWideRight = 13,

        /// <summary>
        /// The coefficient describing forward movement of the lower jaw.
        /// </summary>
        JawForward = 14,

        /// <summary>
        /// The coefficient describing leftward movement of the lower jaw.
        /// </summary>
        JawLeft = 15,

        /// <summary>
        /// The coefficient describing rightward movement of the lower jaw.
        /// </summary>
        JawRight = 16,

        /// <summary>
        /// The coefficient describing an opening of the lower jaw.
        /// </summary>
        JawOpen = 17,

        /// <summary>
        /// The coefficient describing closure of the lips independent of jaw position.
        /// </summary>
        MouthClose = 18,

        /// <summary>
        /// The coefficient describing contraction of both lips into an open shape.
        /// </summary>
        MouthFunnel = 19,

        /// <summary>
        /// The coefficient describing contraction and compression of both closed lips.
        /// </summary>
        MouthPucker = 20,

        /// <summary>
        /// The coefficient describing leftward movement of both lips together.
        /// </summary>
        MouthLeft = 21,

        /// <summary>
        /// The coefficient describing rightward movement of both lips together.
        /// </summary>
        MouthRight = 22,

        /// <summary>
        /// The coefficient describing upward movement of the left corner of the mouth.
        /// </summary>
        MouthSmileLeft = 23,

        /// <summary>
        /// The coefficient describing upward movement of the right corner of the mouth.
        /// </summary>
        MouthSmileRight = 24,

        /// <summary>
        /// The coefficient describing downward movement of the left corner of the mouth.
        /// </summary>
        MouthFrownLeft = 25,

        /// <summary>
        /// The coefficient describing downward movement of the right corner of the mouth.
        /// </summary>
        MouthFrownRight = 26,

        /// <summary>
        /// The coefficient describing backward movement of the left corner of the mouth.
        /// </summary>
        MouthDimpleLeft = 27,

        /// <summary>
        /// The coefficient describing backward movement of the right corner of the mouth.
        /// </summary>
        MouthDimpleRight = 28,

        /// <summary>
        /// The coefficient describing leftward movement of the left corner of the mouth.
        /// </summary>
        MouthStretchLeft = 29,

        /// <summary>
        /// The coefficient describing rightward movement of the left corner of the mouth.
        /// </summary>
        MouthStretchRight = 30,

        /// <summary>
        /// The coefficient describing movement of the lower lip toward the inside of the mouth.
        /// </summary>
        MouthRollLower = 31,

        /// <summary>
        /// The coefficient describing movement of the upper lip toward the inside of the mouth.
        /// </summary>
        MouthRollUpper = 32,

        /// <summary>
        /// The coefficient describing outward movement of the lower lip.
        /// </summary>
        MouthShrugLower = 33,

        /// <summary>
        /// The coefficient describing outward movement of the upper lip.
        /// </summary>
        MouthShrugUpper = 34,

        /// <summary>
        /// The coefficient describing upward compression of the lower lip on the left side.
        /// </summary>
        MouthPressLeft = 35,

        /// <summary>
        /// The coefficient describing upward compression of the lower lip on the right side.
        /// </summary>
        MouthPressRight = 36,

        /// <summary>
        /// The coefficient describing downward movement of the lower lip on the left side.
        /// </summary>
        MouthLowerDownLeft = 37,

        /// <summary>
        /// The coefficient describing downward movement of the lower lip on the right side.
        /// </summary>
        MouthLowerDownRight = 38,

        /// <summary>
        /// The coefficient describing upward movement of the upper lip on the left side.
        /// </summary>
        MouthUpperUpLeft = 39,

        /// <summary>
        /// The coefficient describing upward movement of the upper lip on the right side.
        /// </summary>
        MouthUpperUpRight = 40,

        /// <summary>
        /// The coefficient describing downward movement of the outer portion of the left eyebrow.
        /// </summary>
        BrowDownLeft = 41,

        /// <summary>
        /// The coefficient describing downward movement of the outer portion of the right eyebrow.
        /// </summary>
        BrowDownRight = 42,

        /// <summary>
        /// The coefficient describing upward movement of the inner portion of both eyebrows.
        /// </summary>
        BrowInnerUp = 43,

        /// <summary>
        /// The coefficient describing upward movement of the outer portion of the left eyebrow.
        /// </summary>
        BrowOuterUpLeft = 44,

        /// <summary>
        /// The coefficient describing upward movement of the outer portion of the right eyebrow.
        /// </summary>
        BrowOuterUpRight = 45,

        /// <summary>
        /// The coefficient describing outward movement of both cheeks.
        /// </summary>
        CheekPuff = 46,

        /// <summary>
        /// The coefficient describing upward movement of the cheek around and below the left eye.
        /// </summary>
        CheekSquintLeft = 47,

        /// <summary>
        /// The coefficient describing upward movement of the cheek around and below the right eye.
        /// </summary>
        CheekSquintRight = 48,

        /// <summary>
        /// The coefficient describing a raising of the left side of the nose around the nostril.
        /// </summary>
        NoseSneerLeft = 49,

        /// <summary>
        /// The coefficient describing a raising of the right side of the nose around the nostril.
        /// </summary>
        NoseSneerRight = 50,

        /// <summary>
        /// The coefficient describing extension of the tongue.
        /// </summary>
        TongueOut = 51
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PxrFaceTrackingInfo
    {
        public Int64 timestamp;                             //
        public fixed float faceExpressionWeights[52];       //  
        public fixed float lipsyncExpressionWeights[20];    // 
        public bool isUpperFaceDataValid;                   // 
        public bool isLowerFaceDataValid;                   // 
    };

    public enum ConfigsEXT
    {
        RENDER_TEXTURE_WIDTH = 0,
        RENDER_TEXTURE_HEIGHT,
        SHOW_FPS,
        RUNTIME_LOG_LEVEL,
        PXRPLUGIN_LOG_LEVEL,
        UNITY_LOG_LEVEL,
        UNREAL_LOG_LEVEL,
        NATIVE_LOG_LEVEL,
        TARGET_FRAME_RATE,
        NECK_MODEL_X,
        NECK_MODEL_Y,
        NECK_MODEL_Z,
        DISPLAY_REFRESH_RATE,
        ENABLE_6DOF,
        CONTROLLER_TYPE,
        PHYSICAL_IPD,
        TO_DELTA_SENSOR_Y,
        GET_DISPLAY_RATE,
        FOVEATION_SUBSAMPLED_ENABLED = 18,
        TRACKING_ORIGIN_HEIGHT = 19,
        RENDER_FPS = 20,
        MRC_POSITION_Y_OFFSET,
        GET_SINGLEPASS = 22,
        GET_FOVLEVEL,
        SDK_TRACE_ENABLE,
        SDK_SEETHROUGH_DELAY_LOG_ENABLE = 25,
        GET_SEETHROUGH_STATE = 26,
        EYEORIENTATAION_LEFT_X = 27,
        EYEORIENTATAION_LEFT_Y = 28,
        EYEORIENTATAION_LEFT_Z = 29,
        EYEORIENTATAION_LEFT_W = 30,
        EYEORIENTATAION_RIGHT_X = 31,
        EYEORIENTATAION_RIGHT_Y = 32,
        EYEORIENTATAION_RIGHT_Z = 33,
        EYEORIENTATAION_RIGHT_W = 34,
        SDK_SEETHROUGH_DELAY_DATA_REPORT = 35,
    };

    public struct PxrRecti
    {
        public int x;
        public int y;
        public int width;
        public int height;
    };

    public enum PxrBlendFactor
    {
        Zero = 0,
        One = 1,
        SrcAlpha = 2,
        OneMinusSrcAlpha = 3,
        DstAlpha = 4,
        OneMinusDstAlpha = 5
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PxrLayerParam
    {
        public int layerId;
        public CompositeLayerFeature.OverlayShape layerShape;
        public CompositeLayerFeature.OverlayType layerType;
        public CompositeLayerFeature.LayerLayout layerLayout;
        public UInt64 format;
        public UInt32 width;
        public UInt32 height;
        public UInt32 sampleCount;
        public UInt32 faceCount;
        public UInt32 arraySize;
        public UInt32 mipmapCount;
        public UInt32 layerFlags;
        public UInt32 externalImageCount;
        public IntPtr leftExternalImages;
        public IntPtr rightExternalImages;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PxrVector4f
    {
        public float x;
        public float y;
        public float z;
        public float w;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PxrVector3f
    {
        public float x;
        public float y;
        public float z;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PxrPosef
    {
        public PxrVector4f orientation;
        public PxrVector3f position;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PxrLayerBlend
    {
        public PxrBlendFactor srcColor;
        public PxrBlendFactor dstColor;
        public PxrBlendFactor srcAlpha;
        public PxrBlendFactor dstAlpha;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PxrLayerHeader2
    {
        public int layerId;
        public UInt32 layerFlags;
        public float colorScaleX;
        public float colorScaleY;
        public float colorScaleZ;
        public float colorScaleW;
        public float colorBiasX;
        public float colorBiasY;
        public float colorBiasZ;
        public float colorBiasW;
        public int compositionDepth;
        public int sensorFrameIndex;
        public int imageIndex;
        public PxrPosef headPose;
        public CompositeLayerFeature.OverlayShape layerShape;
        public UInt32 useLayerBlend;
        public PxrLayerBlend layerBlend;
        public UInt32 useImageRect;
        public PxrRecti imageRectLeft;
        public PxrRecti imageRectRight;
        public UInt64 reserved0;
        public UInt64 reserved1;
        public UInt64 reserved2;
        public UInt64 reserved3;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PxrVector2f
    {
        public float x;
        public float y;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PxrLayerQuad
    {
        public PxrLayerHeader2 header;
        public PxrPosef poseLeft;
        public PxrPosef poseRight;
        public PxrVector2f sizeLeft;
        public PxrVector2f sizeRight;
    };

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PxrLayerCylinder
    {
        public PxrLayerHeader2 header;
        public PxrPosef poseLeft;
        public PxrPosef poseRight;
        public float radiusLeft;
        public float radiusRight;
        public float centralAngleLeft;
        public float centralAngleRight;
        public float heightLeft;
        public float heightRight;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PxrLayerEquirect
    {
        public PxrLayerHeader2 header;
        public PxrPosef poseLeft;
        public PxrPosef poseRight;
        public float radiusLeft;
        public float radiusRight;
        public float centralHorizontalAngleLeft;
        public float centralHorizontalAngleRight;
        public float upperVerticalAngleLeft;
        public float upperVerticalAngleRight;
        public float lowerVerticalAngleLeft;
        public float lowerVerticalAngleRight;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PxrLayerCube
    {
        public PxrLayerHeader2 header;
        public PxrPosef poseLeft;
        public PxrPosef poseRight;
    };

    public enum PxrLayerSubmitFlags
    {
        PxrLayerFlagNoCompositionDepthTesting = 1 << 3,
        PxrLayerFlagUseExternalHeadPose = 1 << 5,
        PxrLayerFlagLayerPoseNotInTrackingSpace = 1 << 6,
        PxrLayerFlagHeadLocked = 1 << 7,
        PxrLayerFlagUseExternalImageIndex = 1 << 8,
    }

    public enum PxrLayerCreateFlags
    {
        PxrLayerFlagAndroidSurface = 1 << 0,
        PxrLayerFlagProtectedContent = 1 << 1,
        PxrLayerFlagStaticImage = 1 << 2,
        PxrLayerFlagUseExternalImages = 1 << 4,
        PxrLayerFlag3DLeftRightSurface = 1 << 5,
        PxrLayerFlag3DTopBottomSurface = 1 << 6,
        PxrLayerFlagEnableFrameExtrapolation = 1 << 7,
        PxrLayerFlagEnableSubsampled = 1 << 8,
        PxrLayerFlagEnableFrameExtrapolationPTW = 1 << 9,
        PxrLayerFlagSharedImagesBetweenLayers = 1 << 10,
    }

    public enum EyeType
    {
        EyeLeft,
        EyeRight,
        EyeBoth
    };

    /// <summary>
    /// Information about PICO Motion Tracker's connection state.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PxrFitnessBandConnectState
    {
        /// <summary>
        /// 
        /// </summary>
        public Byte num;
        /// <summary>
        /// 
        /// </summary>
        public fixed Byte trackerID[12];
    }

    public enum PassthroughColorMapType
    {
        None = 0,
        MonoToRgba = 1,
        MonoToMono = 2,
        BrightnessContrastSaturation = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Colorf
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "R:{0:F3} G:{1:F3} B:{2:F3} A:{3:F3}", r, g, b, a);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _PassthroughStyle
    {
        public uint enableEdgeColor;
        public uint enableColorMap;
        public float TextureOpacityFactor;
        public Colorf EdgeColor;
        public PassthroughColorMapType TextureColorMapType;
        public uint TextureColorMapDataSize;
        public IntPtr TextureColorMapData;
    }
    public struct PassthroughStyle
    {
        public bool enableEdgeColor;
        public bool enableColorMap;
        public float TextureOpacityFactor;
        public Color EdgeColor;
        public PassthroughColorMapType TextureColorMapType;
        public uint TextureColorMapDataSize;
        public IntPtr TextureColorMapData;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct GeometryInstanceTransform
    {
        public PxrPosef pose;
        public PxrVector3f scale;

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Rotation:({0:F3},{1:F3},{2:F3},{3:F3})  Position:({4:F3},{5:F3},{6:F3})  scale:({7},{8},{9})", pose.orientation.x,
                pose.orientation.y, pose.orientation.z, pose.orientation.w, pose.position.x, pose.position.y,
                pose.position.z,scale.x,scale.y,scale.z);
        }
    };
    [StructLayout(LayoutKind.Sequential)]
    public struct PxrSensorState2
    {
        public int status;
        public PxrPosef pose;
        public PxrPosef globalPose;
        public PxrVector3f angularVelocity;
        public PxrVector3f linearVelocity;
        public PxrVector3f angularAcceleration;
        public PxrVector3f linearAcceleration;
        public UInt64 poseTimeStampNs;
    }
}