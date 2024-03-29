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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Unity.XR.OpenXR.Features.PICOSupport
{
    public class CompositeLayerFeature : MonoBehaviour, IComparable<CompositeLayerFeature>
    {
        private const string TAG = "[POXR_CompositeLayers]";
        public static List<CompositeLayerFeature> Instances = new List<CompositeLayerFeature>();

        private static int overlayID = 0;
        [NonSerialized]
        public int overlayIndex;
        public int layerDepth;
        public int imageIndex = 0;
        public OverlayType overlayType = OverlayType.Overlay;
        public OverlayShape overlayShape = OverlayShape.Quad;
        public TextureType textureType = TextureType.StaticTexture;
        public Transform overlayTransform;
        public Camera xrRig;

        public Texture[] layerTextures = new Texture[2] { null, null };
        public bool isDynamic = false;
        public int[] overlayTextureIds = new int[2];
        public Matrix4x4[] mvMatrixs = new Matrix4x4[2];
        public Vector3[] modelScales = new Vector3[2];
        public Quaternion[] modelRotations = new Quaternion[2];
        public Vector3[] modelTranslations = new Vector3[2];
        public Quaternion[] cameraRotations = new Quaternion[2];
        public Vector3[] cameraTranslations = new Vector3[2];
        public Camera[] overlayEyeCamera = new Camera[2];

        public bool overrideColorScaleAndOffset = false;
        public Vector4 colorScale = Vector4.one;
        public Vector4 colorOffset = Vector4.zero;

        // Eac
        public Vector3 offsetPosLeft = Vector3.one;
        public Vector3 offsetPosRight = Vector3.one;
        public Vector4 offsetRotLeft = Vector4.one;
        public Vector4 offsetRotRight = Vector4.one;
        public DegreeType degreeType = DegreeType.Eac360;
        public float overlapFactor = 0;

        private Vector4 overlayLayerColorScaleDefault = Vector4.one;
        private Vector4 overlayLayerColorOffsetDefault = Vector4.zero;

        // 360 
        public float radius = 0; // >0

        // ImageRect
        public bool useImageRect = false;
        public TextureRect textureRect = TextureRect.StereoScopic;
        public DestinationRect destinationRect = DestinationRect.Default;
        public Rect srcRectLeft = new Rect(0, 0, 1, 1);
        public Rect srcRectRight = new Rect(0, 0, 1, 1);
        public Rect dstRectLeft = new Rect(0, 0, 1, 1);
        public Rect dstRectRight = new Rect(0, 0, 1, 1);

        public PxrRecti imageRectLeft;
        public PxrRecti imageRectRight;


        // LayerBlend
        public bool useLayerBlend = false;
        public PxrBlendFactor srcColor = PxrBlendFactor.One;
        public PxrBlendFactor dstColor = PxrBlendFactor.One;
        public PxrBlendFactor srcAlpha = PxrBlendFactor.One;
        public PxrBlendFactor dstAlpha = PxrBlendFactor.One;
        public float[] colorMatrix = new float[18] {
            1,0,0, // left
            0,1,0,
            0,0,1,
            1,0,0, // right
            0,1,0,
            0,0,1,
        };

        public bool isClones = false;
        public bool isClonesToNew = false;
        public CompositeLayerFeature originalOverLay;


        public IntPtr layerSubmitPtr = IntPtr.Zero;

        private bool toCreateSwapChain = false;
        private bool toCopyRT = false;
        private bool copiedRT = false;
        private int eyeCount = 2;
        private UInt32 imageCounts = 0;
        private PxrLayerParam overlayParam = new PxrLayerParam();
        private struct NativeTexture
        {
            public Texture[] textures;
        };
        private NativeTexture[] nativeTextures;
        private static Material cubeM;
        private IntPtr leftPtr = IntPtr.Zero;
        private IntPtr rightPtr = IntPtr.Zero;


        public int CompareTo(CompositeLayerFeature other)
        {
            return layerDepth.CompareTo(other.layerDepth);
        }

        protected void Awake()
        {
            xrRig = Camera.main;
            Instances.Add(this);
            if (null == xrRig.gameObject.GetComponent<CompositeLayerManager>())
            {
                xrRig.gameObject.AddComponent<CompositeLayerManager>();
            }
            overlayEyeCamera[0] = xrRig;
            overlayEyeCamera[1] = xrRig;

            overlayTransform = GetComponent<Transform>();
#if UNITY_ANDROID && !UNITY_EDITOR
            if (overlayTransform != null)
            {
                MeshRenderer render = overlayTransform.GetComponent<MeshRenderer>();
                if (render != null)
                {
                    render.enabled = false;
                }
            }
#endif

            if (!isClones)
            {
                InitializeBuffer();
            }
        }

        private void Start()
        {
            if (isClones)
            {
                InitializeBuffer();
            }

            if (PICOManager.Instance == null)
            {
                PLog.e(TAG + "  PICOManager.Instance is null!");
                return;
            }

            Camera[] cam = PICOManager.Instance.GetEyeCamera();
            if (cam[0] != null && cam[0].enabled)
            {
                RefreshCamera(cam[0], cam[0]);
            }
            else if (cam[1] != null && cam[2] != null)
            {
                RefreshCamera(cam[1], cam[2]);
            }
        }

        public void RefreshCamera(Camera leftCamera, Camera rightCamera)
        {
            overlayEyeCamera[0] = leftCamera;
            overlayEyeCamera[1] = rightCamera;
        }

        private void InitializeBuffer()
        {
            if (null == layerTextures[0] && null == layerTextures[1])
            {
                PLog.e(TAG + "  The left and right images are all empty!");
                return;
            }
            else if (null == layerTextures[0] && null != layerTextures[1])
            {
                layerTextures[0] = layerTextures[1];
            }
            else if (null != layerTextures[0] && null == layerTextures[1])
            {
                layerTextures[1] = layerTextures[0];
            }

            overlayID++;
            overlayIndex = overlayID;
            overlayParam.layerId = overlayIndex;
            overlayParam.layerShape = overlayShape == 0 ? OverlayShape.Quad : overlayShape;
            overlayParam.layerType = overlayType;
            overlayParam.width = (uint)layerTextures[1].width;
            overlayParam.height = (uint)layerTextures[1].height;
            overlayParam.arraySize = 1;
            overlayParam.mipmapCount = 1;
            overlayParam.sampleCount = 1;
            overlayParam.layerFlags = 0;
            overlayParam.faceCount = 1;
            if (OverlayShape.Cubemap == overlayShape)
            {
                overlayParam.faceCount = 6;
                if (cubeM == null)
                    cubeM = new Material(Shader.Find("PXR_SDK/PXR_CubemapBlit"));
            }

            if (GraphicsDeviceType.Vulkan == SystemInfo.graphicsDeviceType)
            {
                overlayParam.format = QualitySettings.activeColorSpace == ColorSpace.Linear ? (UInt64)ColorForamt.VK_FORMAT_R8G8B8A8_SRGB : (UInt64)RenderTextureFormat.Default;
            }
            else
            {
                overlayParam.format = QualitySettings.activeColorSpace == ColorSpace.Linear ? (UInt64)ColorForamt.GL_SRGB8_ALPHA8 : (UInt64)RenderTextureFormat.Default;
            }

            if (isClones)
            {
                if (null != originalOverLay)
                {
                    overlayParam.layerFlags |= (UInt32)PxrLayerCreateFlags.PxrLayerFlagSharedImagesBetweenLayers;
                    leftPtr = Marshal.AllocHGlobal(Marshal.SizeOf(originalOverLay.overlayIndex));
                    rightPtr = Marshal.AllocHGlobal(Marshal.SizeOf(originalOverLay.overlayIndex));
                    Marshal.WriteInt64(leftPtr, originalOverLay.overlayIndex);
                    Marshal.WriteInt64(rightPtr, originalOverLay.overlayIndex);
                    overlayParam.leftExternalImages = leftPtr;
                    overlayParam.rightExternalImages = rightPtr;
                    isDynamic = originalOverLay.isDynamic;
                    overlayParam.width = (UInt32)Mathf.Min(overlayParam.width, originalOverLay.overlayParam.width);
                    overlayParam.height = (UInt32)Mathf.Min(overlayParam.height, originalOverLay.overlayParam.height);
                }
                else
                {
                    PLog.e(TAG + "  In clone state, originalOverLay cannot be empty!");
                }
            }

            if (!isDynamic)
            {
                overlayParam.layerFlags |= (UInt32)PxrLayerCreateFlags.PxrLayerFlagStaticImage;
            }

            if (layerTextures[0] == layerTextures[1])
            {
                eyeCount = 1;
                overlayParam.layerLayout = LayerLayout.Mono;
            }
            else
            {
                eyeCount = 2;
                overlayParam.layerLayout = LayerLayout.Stereo;
            }

            OpenXRExtensions.CreateLayerParam(overlayParam);
            toCreateSwapChain = true;
            CreateTexture();
        }

        public void UpdateCoords()
        {
            if (null == overlayTransform || !overlayTransform.gameObject.activeSelf || null == overlayEyeCamera[0] || null == overlayEyeCamera[1])
            {
                return;
            }

            for (int i = 0; i < mvMatrixs.Length; i++)
            {
                mvMatrixs[i] = overlayEyeCamera[i].worldToCameraMatrix * overlayTransform.localToWorldMatrix;
                if (overlayTransform is RectTransform uiTransform)
                {
                    var rect = uiTransform.rect;
                    var lossyScale = overlayTransform.lossyScale;
                    modelScales[i] = new Vector3(rect.width * lossyScale.x,
                        rect.height * lossyScale.y, 1);
                    modelTranslations[i] = uiTransform.TransformPoint(rect.center);
                }
                else
                {
                    modelScales[i] = overlayTransform.lossyScale;
                    modelTranslations[i] = overlayTransform.position;
                }
                modelRotations[i] = overlayTransform.rotation;
                cameraRotations[i] = overlayEyeCamera[i].transform.rotation;
                cameraTranslations[i] = overlayEyeCamera[i].transform.position;
            }
        }

        public bool CreateTexture()
        {
            if (!toCreateSwapChain)
            {
                return false;
            }

            if (null == nativeTextures)
                nativeTextures = new NativeTexture[eyeCount];

            for (int i = 0; i < eyeCount; i++)
            {
                int ret = OpenXRExtensions.GetLayerImageCount(overlayIndex, (EyeType)i, ref imageCounts);
                if (ret != 0 || imageCounts < 1)
                {
                    PLog.e(TAG + $" ret={ret}, imageCounts={imageCounts}");
                    return false;
                }

                if (null == nativeTextures[i].textures)
                {
                    nativeTextures[i].textures = new Texture[imageCounts];
                }

                for (int j = 0; j < imageCounts; j++)
                {
                    IntPtr ptr = IntPtr.Zero;
                    OpenXRExtensions.GetLayerImagePtr(overlayIndex, (EyeType)i, j, ref ptr);

                    if (IntPtr.Zero == ptr)
                    {
                        PLog.e(TAG + $" ptr is zero!");
                        return false;
                    }

                    Texture texture;
                    if (OverlayShape.Cubemap == overlayShape)
                    {
                        texture = Cubemap.CreateExternalTexture((int)overlayParam.width, TextureFormat.RGBA32, false, ptr);
                    }
                    else
                    {
                        texture = Texture2D.CreateExternalTexture((int)overlayParam.width, (int)overlayParam.height, TextureFormat.RGBA32, false, true, ptr);
                    }

                    if (null == texture)
                    {
                        PLog.e(TAG + $" texture is null!");
                        return false;
                    }
                    nativeTextures[i].textures[j] = texture;
                    PLog.i($"composition_layer 2. i={i}, j={j},imageCounts={imageCounts}, ptr={ptr}");
                }
                PLog.i("composition_layer 3");
            }

            toCreateSwapChain = false;
            toCopyRT = true;
            copiedRT = false;

            FreePtr();

            return true;
        }

        public bool CopyRT()
        {
            if (isClones)
            {
                return true;
            }

            if (!toCopyRT)
            {
                return copiedRT;
            }

            if (!isDynamic && copiedRT)
            {
                return copiedRT;
            }

            if (null == nativeTextures)
            {
                PLog.e(TAG + $" nativeTextures is null!");
                return false;
            }

            OpenXRExtensions.GetLayerNextImageIndex(overlayIndex, ref imageIndex);

            for (int i = 0; i < eyeCount; i++)
            {
                Texture nativeTexture = nativeTextures[i].textures[imageIndex];

                if (null == nativeTexture || null == layerTextures[i])
                    continue;

                RenderTexture texture = layerTextures[i] as RenderTexture;

                if (OverlayShape.Cubemap == overlayShape && null == layerTextures[i] as Cubemap)
                {
                    PLog.e(TAG + $" Cubemap. The type of layerTextures is not a Cubemap!");
                    return false;
                }
                                
                bool enable  = QualitySettings.activeColorSpace == ColorSpace.Gamma && texture != null && texture.format == RenderTextureFormat.ARGB32;

                for (int f = 0; f < (int)overlayParam.faceCount; f++)
                {
                    if (enable)
                    {
                        PLog.d(TAG + $" gamma CopyTexture. f={f}");
                        Graphics.CopyTexture(layerTextures[i], f, 0, nativeTexture, f, 0);
                    }
                    else
                    {
                        RenderTextureDescriptor rtDes = new RenderTextureDescriptor((int)overlayParam.width, (int)overlayParam.height, RenderTextureFormat.ARGB32, 0);
                        rtDes.msaaSamples = (int)overlayParam.sampleCount;
                        rtDes.useMipMap = true;
                        rtDes.autoGenerateMips = false;
                        rtDes.sRGB = true;

                        RenderTexture renderTexture = RenderTexture.GetTemporary(rtDes);

                        if (!renderTexture.IsCreated())
                        {
                            renderTexture.Create();
                        }
                        renderTexture.DiscardContents();

                        if (OverlayShape.Cubemap == overlayShape)
                        {
                            cubeM.SetInt("_d", f);
                            Graphics.Blit(layerTextures[i], renderTexture, cubeM);
                        }
                        else
                        {
                            Graphics.Blit(layerTextures[i], renderTexture);
                        }
                        PLog.d(TAG + $" linear CopyTexture. f={f}");
                        Graphics.CopyTexture(renderTexture, 0, 0, nativeTexture, f, 0);
                        RenderTexture.ReleaseTemporary(renderTexture);
                    }
                }
                copiedRT = true;
            }

            return copiedRT;
        }

        public void SetTexture(Texture texture, bool dynamic)
        {
            if (isClones)
            {
                return;
            }
            else
            {
                foreach (CompositeLayerFeature overlay in CompositeLayerFeature.Instances)
                {
                    if (overlay.isClones && null != overlay.originalOverLay && overlay.originalOverLay.overlayIndex == overlayIndex)
                    {
                        overlay.DestroyLayer();
                        overlay.isClonesToNew = true;
                    }
                }
            }

            toCopyRT = false;
            OpenXRExtensions.DestroyLayerByRender(overlayIndex);
            ClearTexture();
            for (int i = 0; i < layerTextures.Length; i++)
            {
                layerTextures[i] = texture;
            }

            isDynamic = dynamic;
            InitializeBuffer();

            if (!isClones)
            {
                foreach (CompositeLayerFeature overlay in CompositeLayerFeature.Instances)
                {
                    if (overlay.isClones && overlay.isClonesToNew)
                    {
                        overlay.originalOverLay = this;
                        overlay.InitializeBuffer();
                        overlay.isClonesToNew = false;
                    }
                }
            }
        }

        private void FreePtr()
        {
            if (leftPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(leftPtr);
                leftPtr = IntPtr.Zero;
            }

            if (rightPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(rightPtr);
                rightPtr = IntPtr.Zero;
            }

            if (layerSubmitPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(layerSubmitPtr);
                layerSubmitPtr = IntPtr.Zero;
            }
        }

        public void OnDestroy()
        {
            DestroyLayer();
            Instances.Remove(this);
        }

        public void DestroyLayer()
        {
            if (!isClones)
            {
                List<CompositeLayerFeature> toDestroyClones = new List<CompositeLayerFeature>();
                foreach (CompositeLayerFeature overlay in Instances)
                {
                    if (overlay.isClones && null != overlay.originalOverLay && overlay.originalOverLay.overlayIndex == overlayIndex)
                    {
                        toDestroyClones.Add(overlay);
                    }
                }

                foreach (CompositeLayerFeature overLay in toDestroyClones)
                {
                    OpenXRExtensions.DestroyLayerByRender(overLay.overlayIndex);
                    ClearTexture();
                }
            }

            OpenXRExtensions.DestroyLayerByRender(overlayIndex);
            ClearTexture();
        }

        private void ClearTexture()
        {
            FreePtr();

            if (null == nativeTextures || isClones)
            {
                return;
            }

            for (int i = 0; i < eyeCount; i++)
            {
                if (null == nativeTextures[i].textures)
                {
                    continue;
                }

                for (int j = 0; j < imageCounts; j++)
                    DestroyImmediate(nativeTextures[i].textures[j]);
            }

            nativeTextures = null;
        }

        public void SetLayerColorScaleAndOffset(Vector4 scale, Vector4 offset)
        {
            colorScale = scale;
            colorOffset = offset;
        }

        public void SetEACOffsetPosAndRot(Vector3 leftPos, Vector3 rightPos, Vector4 leftRot, Vector4 rightRot, float factor)
        {
            offsetPosLeft = leftPos;
            offsetPosRight = rightPos;
            offsetRotLeft = leftRot;
            offsetRotRight = rightRot;
            overlapFactor = factor;
        }

        public Vector4 GetLayerColorScale()
        {
            if (!overrideColorScaleAndOffset)
            {
                return overlayLayerColorScaleDefault;
            }
            return colorScale;
        }

        public Vector4 GetLayerColorOffset()
        {
            if (!overrideColorScaleAndOffset)
            {
                return overlayLayerColorOffsetDefault;
            }
            return colorOffset;
        }

        public PxrRecti getPxrRectiLeft(bool left)
        {
            if (left)
            {
                imageRectLeft.x = (int)(overlayParam.width * srcRectLeft.x);
                imageRectLeft.y = (int)(overlayParam.height * srcRectLeft.y);
                imageRectLeft.width = (int)(overlayParam.width * Mathf.Min(srcRectLeft.width, 1 - srcRectLeft.x));
                imageRectLeft.height = (int)(overlayParam.height * Mathf.Min(srcRectLeft.height, 1 - srcRectLeft.y));
                // Debug.LogFormat("imageRectLeft  width={0}, height={1}, x={2}, y={3}",imageRectLeft.width,imageRectLeft.height,imageRectLeft.x,imageRectLeft.y);
                return imageRectLeft;
            }
            else
            {
                imageRectRight.x = (int)(overlayParam.width * srcRectRight.x);
                imageRectRight.y = (int)(overlayParam.height * srcRectRight.y);
                imageRectRight.width = (int)(overlayParam.width * Mathf.Min(srcRectRight.width, 1 - srcRectRight.x));
                imageRectRight.height = (int)(overlayParam.height * Mathf.Min(srcRectRight.height, 1 - srcRectRight.y));
                // Debug.LogFormat("imageRectRight  width={0}, height={1}, x={2}, y={3}",imageRectRight.width,imageRectRight.height,imageRectRight.x,imageRectRight.y);
                return imageRectRight;
            }
        }

        public enum OverlayShape
        {
            Quad = 1,
            Cylinder = 2,
            Equirect = 4,
            Cubemap = 5,
        }

        public enum OverlayType
        {
            Overlay = 0,
            Underlay = 1
        }

        public enum TextureType
        {
            DynamicTexture,
            StaticTexture
        }

        public enum LayerLayout
        {
            Stereo = 0,
            DoubleWide = 1,
            Array = 2,
            Mono = 3
        }

        public enum Surface3DType
        {
            Single = 0,
            LeftRight,
            TopBottom
        }

        public enum TextureRect
        {
            MonoScopic,
            StereoScopic,
            Custom
        }

        public enum DestinationRect
        {
            Default,
            Custom
        }

        public enum DegreeType
        {
            Eac360 = 0,
            Eac180 = 4,
        }

        public enum ColorForamt
        {
            VK_FORMAT_R8G8B8A8_UNORM = 37,
            VK_FORMAT_R8G8B8A8_SRGB = 43,
            GL_SRGB8_ALPHA8 = 0x8c43,
            GL_RGBA8 = 0x8058
        }
    }
}