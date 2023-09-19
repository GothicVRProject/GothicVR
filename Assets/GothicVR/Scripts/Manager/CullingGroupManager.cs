using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GVR.Debugging;
using GVR.Util;
using UnityEngine;
using UnityEngine.XR;

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

        // Need to be stored for later update of values for Vobs which are moved.
        private BoundingSphere[] vobSpheresSmall;
        private BoundingSphere[] vobSpheresMedium;
        private BoundingSphere[] vobSpheresLarge;
        
        private enum VobList
        {
            Small,
            Medium,
            Large
        }

        private Dictionary<GameObject, Tuple<VobList, int>> grabbedObjects = new();
        private Coroutine grabbedVobsUpdate;
        
        private void Start()
        {
            GvrSceneManager.I.sceneGeneralUnloaded.AddListener(PreWorldCreate);
            GvrSceneManager.I.sceneGeneralLoaded.AddListener(PostWorldCreate);

            // Unity demands CullingGroups to be created in Awake() or Start() earliest.
            // Vobs
            vobCullingGroupSmall = new();
            vobCullingGroupMedium = new();
            vobCullingGroupLarge = new();
        }

        private void PreWorldCreate()
        {
            // Vobs
            vobCullingGroupSmall.Dispose();
            vobCullingGroupMedium.Dispose();
            vobCullingGroupLarge.Dispose();

            vobCullingGroupSmall = new();
            vobCullingGroupMedium = new();
            vobCullingGroupLarge = new();
            
            vobObjectsSmall.Clear();
            vobObjectsMedium.Clear();
            vobObjectsLarge.Clear();

            vobSpheresSmall = null;
            vobSpheresMedium = null;
            vobSpheresLarge = null;
            grabbedObjects.Clear();
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

                var sphere = GetSphere(obj, mesh);
                var size = sphere.radius * 2;
                
                if (size <= smallDim)
                {
                    vobObjectsSmall.Add(obj);
                    spheresSmall.Add(sphere);
                }
                else if (size <= mediumDim)
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

            vobSpheresSmall = spheresSmall.ToArray();
            vobSpheresMedium = spheresMedium.ToArray();
            vobSpheresLarge = spheresLarge.ToArray();
            
            vobCullingGroupSmall.SetBoundingSpheres(vobSpheresSmall);
            vobCullingGroupMedium.SetBoundingSpheres(vobSpheresMedium);
            vobCullingGroupLarge.SetBoundingSpheres(vobSpheresLarge);
        }

        private BoundingSphere GetSphere(GameObject go, Mesh mesh)
        {
            var bboxSize = mesh.bounds.size;
                
            var maxDimension = Mathf.Max(bboxSize.x, bboxSize.y, bboxSize.z); // Get biggest dim for calculation of object size group.
            var sphere = new BoundingSphere(go.transform.position, maxDimension / 2); // Radius is half the size.

            return sphere;
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

        public void StartTrackVobPositionUpdates(GameObject go)
        {
            // Check Small list
            var index = Array.IndexOf(vobObjectsSmall.ToArray(), go);
            var vobType = VobList.Small;
            // Check Medium list
            if (index == -1)
            {
                index = Array.IndexOf(vobObjectsMedium.ToArray(), go);
                vobType = VobList.Medium;
            }
            // Check Large list
            if (index == -1)
            {
                index = Array.IndexOf(vobObjectsLarge.ToArray(), go);
                vobType = VobList.Large;
            }
            
            grabbedObjects.Add(go, new(vobType, index));

            // If there is no Coroutine started so far, then do it now!
            grabbedVobsUpdate ??= StartCoroutine(GrabbedVobsUpdate());
        }

        private IEnumerator GrabbedVobsUpdate()
        {
            foreach (var grabbed in grabbedObjects)
            {
                var go = grabbed.Key;
                var vobType = grabbed.Value.Item1;
                var index = grabbed.Value.Item2;
                
                // We need to find the GO's correlated Sphere in the right VobArray.
                BoundingSphere sphere = vobType switch
                {
                    VobList.Small => vobSpheresSmall[index],
                    VobList.Medium => vobSpheresMedium[index],
                    VobList.Large => vobSpheresLarge[index],
                    _ => throw new ArgumentOutOfRangeException()
                };

                sphere.position = go.transform.position;
            }

            yield return new WaitForSeconds(0.25f); // Just a good guess for performance and efficiency.
        }
        
        public void StopTrackVobPositionUpdates(GameObject go)
        {
            grabbedObjects.Remove(go);

            if (!grabbedObjects.Any())
                StopCoroutine(grabbedVobsUpdate);
        }
        
        private void OnDestroy()
        {
            vobCullingGroupSmall.Dispose();
            vobCullingGroupMedium.Dispose();
            vobCullingGroupLarge.Dispose();
        }
    }
}