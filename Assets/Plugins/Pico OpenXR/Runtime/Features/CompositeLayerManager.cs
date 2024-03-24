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
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.XR.OpenXR.Features.PICOSupport
{
    public class CompositeLayerManager : MonoBehaviour
    {
        private void OnEnable()
        {
#if UNITY_2019_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                RenderPipelineManager.beginFrameRendering += BeginRendering;
                RenderPipelineManager.endFrameRendering += EndRendering;
            }
            else
            {
                Camera.onPreRender += OnPreRenderCallBack;
                Camera.onPostRender += OnPostRenderCallBack;
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_2019_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                RenderPipelineManager.beginFrameRendering -= BeginRendering;
                RenderPipelineManager.endFrameRendering -= EndRendering;
            }
            else
            {
                Camera.onPreRender -= OnPreRenderCallBack;
                Camera.onPostRender -= OnPostRenderCallBack;
            }
#endif
        }

        private void Start()
        {

        }

        private void BeginRendering(ScriptableRenderContext arg1, Camera[] arg2)
        {
            foreach (Camera cam in arg2)
            {
                OnPreRenderCallBack(cam);
            }
        }

        private void EndRendering(ScriptableRenderContext arg1, Camera[] arg2)
        {
            foreach (Camera cam in arg2)
            {
                OnPostRenderCallBack(cam);
            }
        }

        private void OnPreRenderCallBack(Camera cam)
        {
            // There is only one XR main camera in the scene.
            if (null == Camera.main) return;
            if (cam == null || cam != Camera.main || cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right) return;

            //CompositeLayers
            if (null == CompositeLayerFeature.Instances) return;
            if (CompositeLayerFeature.Instances.Count > 0)
            {
                foreach (var overlay in CompositeLayerFeature.Instances)
                {
                    if (!overlay.isActiveAndEnabled) continue;
                    if (null == overlay.layerTextures) continue;
                    if (!overlay.isClones && overlay.layerTextures[0] == null && overlay.layerTextures[1] == null) continue;
                    if (overlay.overlayTransform != null && !overlay.overlayTransform.gameObject.activeSelf) continue;
                    overlay.CreateTexture();
                }
            }
        }

        private void OnPostRenderCallBack(Camera cam)
        {
            // There is only one XR main camera in the scene.
            if (null == Camera.main) return;
            if (cam == null || cam != Camera.main || cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right) return;

            if (null == CompositeLayerFeature.Instances) return;
            if (CompositeLayerFeature.Instances.Count > 0)
            {
                CompositeLayerFeature.Instances.Sort();
                foreach (var compositeLayer in CompositeLayerFeature.Instances)
                {
                    if (null == compositeLayer) continue;
                    compositeLayer.UpdateCoords();
                    if (!compositeLayer.isActiveAndEnabled) continue;
                    if (null == compositeLayer.layerTextures) continue;
                    if (!compositeLayer.isClones && compositeLayer.layerTextures[0] == null && compositeLayer.layerTextures[1] == null) continue;
                    if (compositeLayer.overlayTransform != null && null == compositeLayer.overlayTransform.gameObject) continue;
                    if (compositeLayer.overlayTransform != null && !compositeLayer.overlayTransform.gameObject.activeSelf) continue;

                    Vector4 colorScale = compositeLayer.GetLayerColorScale();
                    Vector4 colorBias = compositeLayer.GetLayerColorOffset();
                    bool isHeadLocked = compositeLayer.overlayTransform != null && compositeLayer.overlayTransform.parent == transform;

                    if (!compositeLayer.CopyRT()) continue;
                    if (null == compositeLayer.cameraRotations || null == compositeLayer.modelScales || null == compositeLayer.modelTranslations) continue;

                    PxrLayerHeader2 header = new PxrLayerHeader2();
                    PxrPosef poseLeft = new PxrPosef();
                    PxrPosef poseRight = new PxrPosef();

                    header.layerId = compositeLayer.overlayIndex;
                    header.colorScaleX = colorScale.x;
                    header.colorScaleY = colorScale.y;
                    header.colorScaleZ = colorScale.z;
                    header.colorScaleW = colorScale.w;
                    header.colorBiasX = colorBias.x;
                    header.colorBiasY = colorBias.y;
                    header.colorBiasZ = colorBias.z;
                    header.colorBiasW = colorBias.w;
                    header.compositionDepth = compositeLayer.layerDepth;
                    header.headPose.orientation.x = compositeLayer.cameraRotations[0].x;
                    header.headPose.orientation.y = compositeLayer.cameraRotations[0].y;
                    header.headPose.orientation.z = -compositeLayer.cameraRotations[0].z;
                    header.headPose.orientation.w = -compositeLayer.cameraRotations[0].w;
                    header.headPose.position.x = (compositeLayer.cameraTranslations[0].x + compositeLayer.cameraTranslations[1].x) / 2;
                    header.headPose.position.y = (compositeLayer.cameraTranslations[0].y + compositeLayer.cameraTranslations[1].y) / 2;
                    header.headPose.position.z = -(compositeLayer.cameraTranslations[0].z + compositeLayer.cameraTranslations[1].z) / 2;
                    header.layerShape = compositeLayer.overlayShape;
                    header.useLayerBlend = (UInt32)(compositeLayer.useLayerBlend ? 1 : 0);
                    header.layerBlend.srcColor = compositeLayer.srcColor;
                    header.layerBlend.dstColor = compositeLayer.dstColor;
                    header.layerBlend.srcAlpha = compositeLayer.srcAlpha;
                    header.layerBlend.dstAlpha = compositeLayer.dstAlpha;
                    header.useImageRect = (UInt32)(compositeLayer.useImageRect ? 1 : 0);
                    header.imageRectLeft = compositeLayer.getPxrRectiLeft(true);
                    header.imageRectRight = compositeLayer.getPxrRectiLeft(false);

                    if (isHeadLocked)
                    {
                        poseLeft.orientation.x = compositeLayer.overlayTransform.localRotation.x;
                        poseLeft.orientation.y = compositeLayer.overlayTransform.localRotation.y;
                        poseLeft.orientation.z = -compositeLayer.overlayTransform.localRotation.z;
                        poseLeft.orientation.w = -compositeLayer.overlayTransform.localRotation.w;
                        poseLeft.position.x = compositeLayer.overlayTransform.localPosition.x;
                        poseLeft.position.y = compositeLayer.overlayTransform.localPosition.y;
                        poseLeft.position.z = -compositeLayer.overlayTransform.localPosition.z;

                        poseRight.orientation.x = compositeLayer.overlayTransform.localRotation.x;
                        poseRight.orientation.y = compositeLayer.overlayTransform.localRotation.y;
                        poseRight.orientation.z = -compositeLayer.overlayTransform.localRotation.z;
                        poseRight.orientation.w = -compositeLayer.overlayTransform.localRotation.w;
                        poseRight.position.x = compositeLayer.overlayTransform.localPosition.x;
                        poseRight.position.y = compositeLayer.overlayTransform.localPosition.y;
                        poseRight.position.z = -compositeLayer.overlayTransform.localPosition.z;

                        header.layerFlags = (UInt32)(
                            PxrLayerSubmitFlags.PxrLayerFlagLayerPoseNotInTrackingSpace |
                            PxrLayerSubmitFlags.PxrLayerFlagHeadLocked);
                    }
                    else
                    {
                        Quaternion quaternion = new Quaternion(compositeLayer.modelRotations[0].x,
                            compositeLayer.modelRotations[0].y, compositeLayer.modelRotations[0].z,
                            compositeLayer.modelRotations[0].w);

                        Vector3 cameraPos = Vector3.zero;
                        Quaternion cameraRot = Quaternion.identity;
                        Transform origin = null;
                        bool ret = PICOManager.Instance.GetOrigin(ref cameraPos, ref cameraRot, ref origin);
                        if (!ret)
                        {
                            PLog.e(" GetOrigin ret false!");
                            return;
                        }

                        Quaternion lQuaternion = new Quaternion(-cameraRot.x, -cameraRot.y, -cameraRot.z, cameraRot.w);
                        Vector3 pos = new Vector3(compositeLayer.modelTranslations[0].x - cameraPos.x,
                            compositeLayer.modelTranslations[0].y - PICOManager.Instance.getCameraYOffset() +
                            PICOManager.Instance.GetOriginY() - cameraPos.y, compositeLayer.modelTranslations[0].z - cameraPos.z);

                        quaternion *= lQuaternion;
                        origin.rotation *= lQuaternion;
                        pos = origin.TransformPoint(pos);

                        // Quaternion.l
                        poseLeft.position.x = pos.x;
                        poseLeft.position.y = pos.y;
                        poseLeft.position.z = -pos.z;
                        poseLeft.orientation.x = -quaternion.x;
                        poseLeft.orientation.y = -quaternion.y;
                        poseLeft.orientation.z = quaternion.z;
                        poseLeft.orientation.w = quaternion.w;

                        poseRight.position.x = pos.x;
                        poseRight.position.y = pos.y;
                        poseRight.position.z = -pos.z;
                        poseRight.orientation.x = -quaternion.x;
                        poseRight.orientation.y = -quaternion.y;
                        poseRight.orientation.z = quaternion.z;
                        poseRight.orientation.w = quaternion.w;

                        header.layerFlags = (UInt32)(
                            PxrLayerSubmitFlags.PxrLayerFlagUseExternalHeadPose |
                            PxrLayerSubmitFlags.PxrLayerFlagLayerPoseNotInTrackingSpace);
                    }

                    if (compositeLayer.overlayShape == CompositeLayerFeature.OverlayShape.Quad)
                    {
                        PxrLayerQuad layerSubmit2 = new PxrLayerQuad();
                        layerSubmit2.header = header;
                        layerSubmit2.poseLeft = poseLeft;
                        layerSubmit2.poseRight = poseRight;
                        layerSubmit2.sizeLeft.x = compositeLayer.modelScales[0].x;
                        layerSubmit2.sizeLeft.y = compositeLayer.modelScales[0].y;
                        layerSubmit2.sizeRight.x = compositeLayer.modelScales[0].x;
                        layerSubmit2.sizeRight.y = compositeLayer.modelScales[0].y;

                        if (compositeLayer.useImageRect)
                        {
                            Vector3 lPos = new Vector3();
                            Vector3 rPos = new Vector3();
                            Quaternion quaternion = new Quaternion(compositeLayer.modelRotations[0].x, compositeLayer.modelRotations[0].y, -compositeLayer.modelRotations[0].z, -compositeLayer.modelRotations[0].w);

                            lPos.x = compositeLayer.modelScales[0].x * (-0.5f + compositeLayer.dstRectLeft.x + 0.5f * Mathf.Min(compositeLayer.dstRectLeft.width, 1 - compositeLayer.dstRectLeft.x));
                            lPos.y = compositeLayer.modelScales[0].y * (-0.5f + compositeLayer.dstRectLeft.y + 0.5f * Mathf.Min(compositeLayer.dstRectLeft.height, 1 - compositeLayer.dstRectLeft.y));
                            lPos.z = 0;
                            lPos = quaternion * lPos;
                            layerSubmit2.poseLeft.position.x += lPos.x;
                            layerSubmit2.poseLeft.position.y += lPos.y;
                            layerSubmit2.poseLeft.position.z += lPos.z;

                            rPos.x = compositeLayer.modelScales[0].x * (-0.5f + compositeLayer.dstRectRight.x + 0.5f * Mathf.Min(compositeLayer.dstRectRight.width, 1 - compositeLayer.dstRectRight.x));
                            rPos.y = compositeLayer.modelScales[0].y * (-0.5f + compositeLayer.dstRectRight.y + 0.5f * Mathf.Min(compositeLayer.dstRectRight.height, 1 - compositeLayer.dstRectRight.y));
                            rPos.z = 0;
                            rPos = quaternion * rPos;
                            layerSubmit2.poseRight.position.x += rPos.x;
                            layerSubmit2.poseRight.position.y += rPos.y;
                            layerSubmit2.poseRight.position.z += rPos.z;

                            layerSubmit2.sizeLeft.x = compositeLayer.modelScales[0].x * Mathf.Min(compositeLayer.dstRectLeft.width, 1 - compositeLayer.dstRectLeft.x);
                            layerSubmit2.sizeLeft.y = compositeLayer.modelScales[0].y * Mathf.Min(compositeLayer.dstRectLeft.height, 1 - compositeLayer.dstRectLeft.y);
                            layerSubmit2.sizeRight.x = compositeLayer.modelScales[0].x * Mathf.Min(compositeLayer.dstRectRight.width, 1 - compositeLayer.dstRectRight.x);
                            layerSubmit2.sizeRight.y = compositeLayer.modelScales[0].y * Mathf.Min(compositeLayer.dstRectRight.height, 1 - compositeLayer.dstRectRight.y);
                        }
                        if (compositeLayer.layerSubmitPtr != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(compositeLayer.layerSubmitPtr);
                            compositeLayer.layerSubmitPtr = IntPtr.Zero;
                        }

                        compositeLayer.layerSubmitPtr = Marshal.AllocHGlobal(Marshal.SizeOf(layerSubmit2));
                        Marshal.StructureToPtr(layerSubmit2, compositeLayer.layerSubmitPtr, false);

                        OpenXRExtensions.SubmitLayerQuad(compositeLayer.layerSubmitPtr);
                    }
                    else if (compositeLayer.overlayShape == CompositeLayerFeature.OverlayShape.Cylinder)
                    {
                        PxrLayerCylinder layerSubmit2 = new PxrLayerCylinder();
                        layerSubmit2.header = header;
                        layerSubmit2.poseLeft = poseLeft;
                        layerSubmit2.poseRight = poseRight;

                        if (compositeLayer.modelScales[0].z != 0)
                        {
                            layerSubmit2.centralAngleLeft = compositeLayer.modelScales[0].x / compositeLayer.modelScales[0].z;
                            layerSubmit2.centralAngleRight = compositeLayer.modelScales[0].x / compositeLayer.modelScales[0].z;
                        }
                        else
                        {
                            PLog.e("Cylinder modelScales scale.z is 0!");
                        }
                        layerSubmit2.heightLeft = compositeLayer.modelScales[0].y;
                        layerSubmit2.heightRight = compositeLayer.modelScales[0].y;
                        layerSubmit2.radiusLeft = compositeLayer.modelScales[0].z;
                        layerSubmit2.radiusRight = compositeLayer.modelScales[0].z;

                        if (compositeLayer.layerSubmitPtr != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(compositeLayer.layerSubmitPtr);
                            compositeLayer.layerSubmitPtr = IntPtr.Zero;
                        }

                        compositeLayer.layerSubmitPtr = Marshal.AllocHGlobal(Marshal.SizeOf(layerSubmit2));
                        Marshal.StructureToPtr(layerSubmit2, compositeLayer.layerSubmitPtr, false);

                        OpenXRExtensions.SubmitLayerCylinder(compositeLayer.layerSubmitPtr);
                    }
                    else if (compositeLayer.overlayShape == CompositeLayerFeature.OverlayShape.Equirect)
                    {
                        PxrLayerEquirect layerSubmit2 = new PxrLayerEquirect();
                        layerSubmit2.header = header;
                        layerSubmit2.poseLeft = poseLeft;
                        layerSubmit2.poseRight = poseRight;

                        layerSubmit2.radiusLeft = compositeLayer.radius;
                        layerSubmit2.radiusRight = compositeLayer.radius;
                        layerSubmit2.centralHorizontalAngleLeft = compositeLayer.dstRectLeft.width * 2 * Mathf.PI;
                        layerSubmit2.centralHorizontalAngleRight = compositeLayer.dstRectRight.width * 2 * Mathf.PI;
                        layerSubmit2.upperVerticalAngleLeft = (compositeLayer.dstRectLeft.height + compositeLayer.dstRectLeft.y - 0.5f) * Mathf.PI;
                        layerSubmit2.upperVerticalAngleRight = (compositeLayer.dstRectRight.height + compositeLayer.dstRectRight.y - 0.5f) * Mathf.PI;
                        layerSubmit2.lowerVerticalAngleLeft = (compositeLayer.dstRectLeft.y - 0.5f) * Mathf.PI;
                        layerSubmit2.lowerVerticalAngleRight = (compositeLayer.dstRectRight.y - 0.5f) * Mathf.PI;

                        if (compositeLayer.layerSubmitPtr != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(compositeLayer.layerSubmitPtr);
                            compositeLayer.layerSubmitPtr = IntPtr.Zero;
                        }

                        compositeLayer.layerSubmitPtr = Marshal.AllocHGlobal(Marshal.SizeOf(layerSubmit2));
                        Marshal.StructureToPtr(layerSubmit2, compositeLayer.layerSubmitPtr, false);

                        OpenXRExtensions.SubmitLayerEquirect(compositeLayer.layerSubmitPtr);
                    }
                    else if (compositeLayer.overlayShape == CompositeLayerFeature.OverlayShape.Cubemap)
                    {
                        PxrLayerCube layerSubmit2 = new PxrLayerCube();
                        layerSubmit2.header = header;
                        layerSubmit2.poseLeft = poseLeft;
                        layerSubmit2.poseRight = poseRight;

                        if (compositeLayer.layerSubmitPtr != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(compositeLayer.layerSubmitPtr);
                            compositeLayer.layerSubmitPtr = IntPtr.Zero;
                        }

                        compositeLayer.layerSubmitPtr = Marshal.AllocHGlobal(Marshal.SizeOf(layerSubmit2));
                        Marshal.StructureToPtr(layerSubmit2, compositeLayer.layerSubmitPtr, false);

                        OpenXRExtensions.SubmitLayerCube(compositeLayer.layerSubmitPtr);
                    }
                }
            }
        }
    }
}