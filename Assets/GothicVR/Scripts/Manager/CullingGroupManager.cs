using System.Linq;
using GVR.Debugging;
using GVR.Util;
using UnityEngine;

namespace GVR.Manager
{
    public class CullingGroupManager : SingletonBehaviour<CullingGroupManager>
    {
        private const float vobBoundingSphereRadius = 1f;
        
        private CullingGroup vobCullingGroup;
        private GameObject[] vobObjects;

        private void Start()
        {
            GvrSceneManager.I.sceneGeneralLoaded.AddListener(PostWorldCreate);
        }
        
        private void OnVobCullingGroupStateChanged(CullingGroupEvent evt)
        {
            // Debug.LogWarning($"CullingGroup StateChange: i={evt.index}, go={vobObjects[evt.index]}, isVisible={evt.isVisible}, hasBecomeVisible={evt.hasBecomeVisible}");
            vobObjects[evt.index].SetActive(evt.hasBecomeVisible);
        }

        public void SetVobObjects(GameObject[] objects)
        {
            if (!FeatureFlags.I.VobCulling)
                return;
            
            vobObjects = objects;

            if (vobCullingGroup != null)
                vobCullingGroup.Dispose();
            
            vobCullingGroup = new();
            vobCullingGroup.onStateChanged += OnVobCullingGroupStateChanged;
            
            var spheres = objects
                .Select(obj => new BoundingSphere(obj.transform.position, vobBoundingSphereRadius))
                .ToArray();
            vobCullingGroup.SetBoundingSpheres(spheres);
        }

        /// <summary>
        /// Set main camera once world is loaded fully.
        /// Doesn't work at loading time as we change scenes etc.
        /// </summary>
        public void PostWorldCreate()
        {
            if (vobCullingGroup != null)
                vobCullingGroup.targetCamera = Camera.main;
        }
        
        private void OnDestroy()
        {
            if (vobCullingGroup != null)
                vobCullingGroup.Dispose();
            vobCullingGroup = null;
        }
    }
}