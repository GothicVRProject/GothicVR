using System;
using System.Collections.Generic;
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
        private CullingGroup vobCullingGroupSmall;
        private CullingGroup vobCullingGroupMedium;
        private CullingGroup vobCullingGroupLarge;

        private List<GameObject> vobObjectsSmall = new();
        private List<GameObject> vobObjectsMedium = new();
        private List<GameObject> vobObjectsLarge = new();

        private void Start()
        {
            // Unity demands it to be here.
            vobCullingGroupSmall = new();
            vobCullingGroupMedium = new();
            vobCullingGroupLarge = new();
            
            GvrSceneManager.I.sceneGeneralUnloaded.AddListener(PreWorldCreate);
            GvrSceneManager.I.sceneGeneralLoaded.AddListener(PostWorldCreate);
        }

        private void PreWorldCreate()
        {
            vobCullingGroupSmall.Dispose();
            vobCullingGroupMedium.Dispose();
            vobCullingGroupLarge.Dispose();

            vobCullingGroupSmall = new();
            vobCullingGroupMedium = new();
            vobCullingGroupLarge = new();
            
            vobObjectsSmall.Clear();
            vobObjectsMedium.Clear();
            vobObjectsLarge.Clear();
        }
        
        private void VobSmallChanged(CullingGroupEvent evt)
        {
            vobObjectsSmall[evt.index].SetActive(evt.hasBecomeVisible);
        }
        
        private void VobMediumChanged(CullingGroupEvent evt)
        {
            vobObjectsMedium[evt.index].SetActive(evt.hasBecomeVisible);
        }
        
        private void VobLargeChanged(CullingGroupEvent evt)
        {
            vobObjectsLarge[evt.index].SetActive(evt.hasBecomeVisible);
        }

        public void PrepareVobCulling(GameObject[] objects)
        {
            if (!FeatureFlags.I.vobCulling)
                return;

            var smallDim = FeatureFlags.I.vobCullingSmall.maxObjectSize;
            var mediumDim = FeatureFlags.I.vobCullingMedium.maxObjectSize;
            var spheresSmall = new List<BoundingSphere>();
            var spheresMedium = new List<BoundingSphere>();
            var spheresLarge = new List<BoundingSphere>();

            foreach (var obj in objects)
            {
                var mesh = GetMesh(obj);
                if (mesh == null)
                {
                    if (!obj.name.Equals("WASH_SLOT.ASC", StringComparison.OrdinalIgnoreCase)) // Wash slot is placed wrong in G1. Therefore skip.
                        Debug.LogError($"Couldn't find mesh for >{obj}< to be used for CullingGroup. Skipping...");
    
                    continue;
                }
                var bboxSize = mesh.bounds.size;
                
                var maxDimension = Mathf.Max(bboxSize.x, bboxSize.y, bboxSize.z); // Get biggest dim for calculation of object size group.
                var sphere = new BoundingSphere(obj.transform.position, maxDimension / 2); // Radius is half the size.
                
                if (maxDimension <= smallDim)
                {
                    vobObjectsSmall.Add(obj);
                    spheresSmall.Add(sphere);
                }
                else if (maxDimension <= mediumDim)
                {
                    vobObjectsMedium.Add(obj);
                    spheresMedium.Add(sphere);
                }
                else
                {
                    vobObjectsLarge.Add(obj);
                    spheresLarge.Add(sphere);
                }
            }

            vobCullingGroupSmall.onStateChanged = VobSmallChanged;
            vobCullingGroupMedium.onStateChanged = VobMediumChanged;
            vobCullingGroupLarge.onStateChanged = VobLargeChanged;
            
            vobCullingGroupSmall.SetBoundingDistances(new[]{FeatureFlags.I.vobCullingSmall.cullingDistance});
            vobCullingGroupMedium.SetBoundingDistances(new[]{FeatureFlags.I.vobCullingMedium.cullingDistance});
            vobCullingGroupLarge.SetBoundingDistances(new[]{FeatureFlags.I.vobCullingLarge.cullingDistance});

            vobCullingGroupSmall.SetBoundingSpheres(spheresSmall.ToArray());
            vobCullingGroupMedium.SetBoundingSpheres(spheresMedium.ToArray());
            vobCullingGroupLarge.SetBoundingSpheres(spheresLarge.ToArray());
        }

        /// <summary>
        /// TODO If performance allows it, we could also look dynamically for all the existing meshes inside GO
        /// TODO and look for maximum value for largest mesh. For now it should be fine.
        /// </summary>
        private Mesh GetMesh(GameObject go)
        {
            var transf = go.transform;
            
            if (transf.TryGetComponent<MeshFilter>(out var mesh0)) // Lookup: /
                return mesh0.mesh;
            else if (transf.GetChild(0).TryGetComponent<MeshFilter>(out var mesh1)) // Lookup: /BIP 01
                return mesh1.mesh;
            else if (transf.GetChild(0).GetChild(0).TryGetComponent<MeshFilter>(out var mesh2)) // Lookup: /BIP 01/...
                return mesh2.mesh;
            else if (transf.childCount > 1 && transf.GetChild(1).TryGetComponent<MeshFilter>(out var mesh3)) // Lookup: /ZM_0
                return mesh3.mesh;
            else
                return null;
        }
        
        /// <summary>
        /// Set main camera once world is loaded fully.
        /// Doesn't work at loading time as we change scenes etc.
        /// </summary>
        private void PostWorldCreate()
        {
            foreach (var group in new[] {vobCullingGroupSmall, vobCullingGroupMedium, vobCullingGroupLarge})
            {
                var mainCamera = Camera.main!;

                group.targetCamera = mainCamera; // Needed for FrustumCulling and OcclusionCulling to work.
                group.SetDistanceReferencePoint(mainCamera.transform); // Needed for BoundingDistances to work.
            }
        }
        
        private void OnDestroy()
        {
            vobCullingGroupSmall.Dispose();
            vobCullingGroupMedium.Dispose();
            vobCullingGroupLarge.Dispose();
        }
    }
}