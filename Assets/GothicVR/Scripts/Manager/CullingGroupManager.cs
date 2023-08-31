using System.Linq;
using GVR.Debugging;
using GVR.Util;
using UnityEngine;

namespace GVR.Manager
{
    /// <summary>
    /// CullingGroups are a way for objects inside a scene to be handled by frustum culling and occlusion culling.
    /// With this set up, VOBs are handled by camera's view and culling behaviour, so that the StateChanged() event disables/enables VOB GameObjects.
    /// @see https://docs.unity3d.com/Manual/CullingGroupAPI.html
    /// </summary>
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

            // If the Vob is now within first range 0...x meter, then activate GameObject
            // vobObjects[evt.index].SetActive(evt.currentDistance == 0);
            // Debug.Log(evt.currentDistance);
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
            vobCullingGroup.SetBoundingDistances(new[]{10f});
            
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
            {
                vobCullingGroup.targetCamera = Camera.main;
                vobCullingGroup.SetDistanceReferencePoint(Camera.main.transform);
            }
        }
        
        private void OnDestroy()
        {
            if (vobCullingGroup != null)
                vobCullingGroup.Dispose();
            vobCullingGroup = null;
        }
    }
}