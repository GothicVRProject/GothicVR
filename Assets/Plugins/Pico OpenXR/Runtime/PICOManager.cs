using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Unity.XR.CoreUtils;
using UnityEngine.XR;

namespace Unity.XR.OpenXR.Features.PICOSupport
{
    public class PICOManager : MonoBehaviour
    {
        private const string TAG = "[PICOManager]";
        private static PICOManager instance = null;
        private Camera[] eyeCamera;
        private XROrigin _xrOrigin;
        private XROrigin _xrOriginT;
        static List<XRInputSubsystem> s_InputSubsystems = new List<XRInputSubsystem>();
        private float cameraYOffset;
        private float cameraY;
        private bool isTrackingOriginMode = false;
        private TrackingOriginModeFlags currentTrackingOriginMode = TrackingOriginModeFlags.Unknown;
        private Vector3 _xrOriginPos = Vector3.zero;
        private Vector3 _xrOriginTPos = Vector3.zero;
        private Quaternion _xrOriginRot = Quaternion.identity;
        private Quaternion _xrOriginTRot = Quaternion.identity;
        private static GameObject local = null;


        private Vector3 lastOriginPos = Vector3.zero;
        private Quaternion lastOriginRot = Quaternion.identity;

        private Vector3 lastOriginTPos = Vector3.zero;
        private Quaternion lastOriginTRot = Quaternion.identity;


        public static PICOManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<PICOManager>();
                    if (instance == null)
                    {
                        XROrigin origin = Camera.main.transform.GetComponentInParent<XROrigin>();
                        if (origin != null)
                        {
                            instance = origin.gameObject.AddComponent<PICOManager>();
                        }
                        else
                        {
                            GameObject go = new GameObject("[PICOManager]");
                            DontDestroyOnLoad(go);
                            instance = go.AddComponent<PICOManager>();
                        }

                    }
                }

                return instance;
            }
        }

        void Awake()
        {
            eyeCamera = new Camera[3];
            Camera[] cam = gameObject.GetComponentsInChildren<Camera>();

            for (int i = 0; i < cam.Length; i++)
            {
                if (cam[i].stereoTargetEye == StereoTargetEyeMask.Both && cam[i] == Camera.main)
                {
                    eyeCamera[0] = cam[i];
                }
                else if (cam[i].stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    eyeCamera[1] = cam[i];
                }
                else if (cam[i].stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    eyeCamera[2] = cam[i];
                }
            }

            _xrOrigin = gameObject.GetComponent<XROrigin>();

            if (_xrOrigin != null)
            {
                _xrOriginPos = new Vector3(Camera.main.transform.position.x, _xrOrigin.transform.position.y, Camera.main.transform.position.z);
                cameraYOffset = _xrOrigin.CameraYOffset;
            }
            _xrOriginRot = Camera.main.transform.parent.rotation;
            cameraY = this.transform.position.y;

            if (local == null)
            {
                local = new GameObject();
            }
        }

        public float getCameraYOffset()
        {
            if (currentTrackingOriginMode == TrackingOriginModeFlags.Floor)
            {
                return cameraY;
            }

            return cameraY + cameraYOffset;
        }

        private void Update()
        {
            if (!isTrackingOriginMode)
            {
                XRInputSubsystem subsystem = null;
                SubsystemManager.GetInstances(s_InputSubsystems);
                if (s_InputSubsystems.Count > 0)
                {
                    subsystem = s_InputSubsystems[0];
                }

                var mCurrentTrackingOriginMode = subsystem?.GetTrackingOriginMode();
                if (mCurrentTrackingOriginMode != null)
                {
                    isTrackingOriginMode = true;
                    currentTrackingOriginMode = (TrackingOriginModeFlags)mCurrentTrackingOriginMode;
                }
            }
        }

        private void OnEnable()
        {
            if (Camera.main.gameObject.GetComponent<CompositeLayerManager>() == null)
            {
                Camera.main.gameObject.AddComponent<CompositeLayerManager>();
            }

            foreach (var layer in CompositeLayerFeature.Instances)
            {
                if (eyeCamera[0] != null && eyeCamera[0].enabled)
                {
                    layer.RefreshCamera(eyeCamera[0], eyeCamera[0]);
                }
                else if (eyeCamera[1] != null && eyeCamera[1].enabled)
                {
                    layer.RefreshCamera(eyeCamera[1], eyeCamera[2]);
                }
            }
        }

        public Camera[] GetEyeCamera()
        {
            return eyeCamera;
        }

        public float GetOriginY()
        {
            return _xrOrigin.transform.position.y;
        }

        public bool GetOrigin(ref Vector3 pos, ref Quaternion rotation, ref Transform origin)
        {
            Transform transform = local.GetComponent<Transform>();
            transform.rotation = Quaternion.identity;
            origin = transform;
            XROrigin xrOrigin = FindObjectOfType<XROrigin>();

            if (!xrOrigin)
            {
                PLog.e(TAG + $" xrOrigin is false!");
                pos = Vector3.zero;
                rotation = Quaternion.identity;
                return false;
            }

            if (xrOrigin == _xrOrigin)
            {
                if (xrOrigin.transform.position != lastOriginPos || xrOrigin.transform.rotation != lastOriginRot)
                {
                    _xrOriginPos.x = Camera.main.transform.position.x;
                    _xrOriginPos.y = xrOrigin.transform.position.y;
                    _xrOriginPos.z = Camera.main.transform.position.z;
                    _xrOriginRot = Camera.main.transform.parent.rotation;
                    lastOriginPos = xrOrigin.transform.position;
                    lastOriginRot = xrOrigin.transform.rotation;
                }

                pos = _xrOriginPos;
                rotation = _xrOriginRot;
                return true;
            }
            else if (xrOrigin == _xrOriginT)
            {
                if (xrOrigin.transform.position != lastOriginTPos || xrOrigin.transform.rotation != lastOriginTRot)
                {
                    _xrOriginTPos.x = Camera.main.transform.position.x;
                    _xrOriginTPos.y = xrOrigin.transform.position.y;
                    _xrOriginTPos.z = Camera.main.transform.position.z;
                    _xrOriginTRot = Camera.main.transform.parent.rotation;
                    lastOriginTPos = xrOrigin.transform.position;
                    lastOriginTRot = xrOrigin.transform.rotation;
                }
                pos = _xrOriginTPos;
                rotation = _xrOriginTRot;
                return true;
            }

            _xrOriginT = xrOrigin;
            _xrOriginTPos = new Vector3(Camera.main.transform.parent.position.x, xrOrigin.transform.position.y, Camera.main.transform.parent.position.z);
            _xrOriginTRot = Camera.main.transform.parent.rotation;
            pos = _xrOriginTPos;
            rotation = _xrOriginTRot;
            return true;
        }
        public float GetRefreshRate()
        {
            float i = -1;
            DisplayRefreshRateFeature.GetDisplayRefreshRate(ref i);
            return i;
        }

        public XrExtent2Df GetReferenceSpaceBoundsRect()
        {
            XrExtent2Df extent2D = new XrExtent2Df();
            OpenXRExtensions.GetReferenceSpaceBoundsRect(XrReferenceSpaceType.Stage, ref extent2D);
            return extent2D;
        }
    }
}