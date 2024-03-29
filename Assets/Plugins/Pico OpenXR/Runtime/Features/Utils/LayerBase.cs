using System;
using UnityEngine;

namespace Unity.XR.OpenXR.Features.PICOSupport
{
    public class LayerBase : MonoBehaviour
    {
        public static int ID = 0;
        private Transform overlayTransform;
        private Camera[] overlayEyeCamera = new Camera[2];
        private Camera xrRig;
        private Matrix4x4[] mvMatrixs = new Matrix4x4[2];
        private Vector3[] modelScales = new Vector3[2];
        private Vector3[] modelTranslations = new Vector3[2];
        private Quaternion[] modelRotations = new Quaternion[2];
        private Quaternion[] cameraRotations = new Quaternion[2];
        private Vector3[] cameraTranslations = new Vector3[2];

        public void Awake()
        {
            ID++;
            xrRig = Camera.main;
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
        }
        private void OnDestroy()
        {
            ID--;
        }
        public void UpdateCoords()
        {
            if (null == overlayTransform || !overlayTransform.gameObject.activeSelf || null == overlayEyeCamera[0] ||
                null == overlayEyeCamera[1])
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

        public void GetCurrentTransform(ref GeometryInstanceTransform geometryInstanceTransform)
        {
            Quaternion quaternion = new Quaternion(modelRotations[0].x,
                modelRotations[0].y, modelRotations[0].z,
                modelRotations[0].w);
            Vector3 cameraPos = Vector3.zero;
            Quaternion cameraRot = Quaternion.identity;
            Transform origin = null;
            bool ret = PICOManager.Instance.GetOrigin(ref cameraPos, ref cameraRot, ref origin);
            if (!ret)
            {
                return;
            }

            Quaternion lQuaternion = new Quaternion(-cameraRot.x, -cameraRot.y, -cameraRot.z, cameraRot.w);
            Vector3 pos = new Vector3(modelTranslations[0].x - cameraPos.x,
                modelTranslations[0].y - PICOManager.Instance.getCameraYOffset() +
                PICOManager.Instance.GetOriginY() - cameraPos.y, modelTranslations[0].z - cameraPos.z);

            quaternion *= lQuaternion;
            origin.rotation *= lQuaternion;
            pos = origin.TransformPoint(pos);

            geometryInstanceTransform.pose.position.x = pos.x;
            geometryInstanceTransform.pose.position.y = pos.y;
            geometryInstanceTransform.pose.position.z = -pos.z;
            geometryInstanceTransform.pose.orientation.x = -quaternion.x;
            geometryInstanceTransform.pose.orientation.y = -quaternion.y;
            geometryInstanceTransform.pose.orientation.z = quaternion.z;
            geometryInstanceTransform.pose.orientation.w = quaternion.w;

            geometryInstanceTransform.scale.x = modelScales[0].x;
            geometryInstanceTransform.scale.y = modelScales[0].y;
            geometryInstanceTransform.scale.z = 1;
        }
    }
}