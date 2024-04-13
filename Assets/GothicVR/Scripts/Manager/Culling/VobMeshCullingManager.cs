using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GVR.Debugging;
using GVR.Globals;
using GVR.Util;
using UnityEngine;

namespace GVR.Manager.Culling
{
    /// <summary>
    /// CullingGroups are a way for objects inside a scene to be handled by frustum culling and occlusion culling.
    /// With this set up, VOBs are handled by camera's view and culling behaviour, so that the StateChanged() event disables/enables VOB GameObjects.
    /// @see https://docs.unity3d.com/Manual/CullingGroupAPI.html
    /// </summary>
    public class VobMeshCullingManager : SingletonBehaviour<VobMeshCullingManager>
    {
        // Stored for resetting after world switch
        private CullingGroup vobCullingGroupSmall;
        private CullingGroup vobCullingGroupMedium;
        private CullingGroup vobCullingGroupLarge;

        // Stored for later index mapping SphereIndex => GOIndex
        private readonly List<GameObject> vobObjectsSmall = new();
        private readonly List<GameObject> vobObjectsMedium = new();
        private readonly List<GameObject> vobObjectsLarge = new();

        // Stored for later position updates for moved Vobs
        private BoundingSphere[] vobSpheresSmall;
        private BoundingSphere[] vobSpheresMedium;
        private BoundingSphere[] vobSpheresLarge;

        private enum VobList
        {
            Small,
            Medium,
            Large
        }

        // Grabbed Vobs will be ignored from Culling until Grabbing stopped and velocity = 0
        private readonly Dictionary<GameObject, Tuple<VobList, int>> pausedVobs = new();
        private readonly Dictionary<GameObject, Rigidbody> pausedVobsToReenable = new();
        private readonly Dictionary<GameObject, Coroutine> pausedVobsToReenableCoroutine = new();

        private void Start()
        {
            GvrEvents.GeneralSceneUnloaded.AddListener(PreWorldCreate);
            GvrEvents.GeneralSceneLoaded.AddListener(PostWorldCreate);

            // Unity demands CullingGroups to be created in Awake() or Start() earliest.
            vobCullingGroupSmall = new();
            vobCullingGroupMedium = new();
            vobCullingGroupLarge = new();

            StartCoroutine(StopVobTrackingBasedOnVelocity());
        }

        /// <summary>
        /// This method will only be called within EditorMode. It's tested to not being executed within Standalone mode.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !FeatureFlags.I.drawVobCullingGizmos)
            {
                return;
            }

            Gizmos.color = new Color(.5f, 0, 0);
            if (vobSpheresSmall != null)
            {
                foreach (BoundingSphere sphere in vobSpheresSmall)
                {
                    Gizmos.DrawWireSphere(sphere.position, sphere.radius);
                }
            }
            Gizmos.color = new Color(.4f, 0, 0);
            if (vobSpheresMedium != null)
            {
                foreach (BoundingSphere sphere in vobSpheresMedium)
                {
                    Gizmos.DrawWireSphere(sphere.position, sphere.radius);
                }
            }
            Gizmos.color = new Color(.3f, 0, 0);
            if (vobSpheresLarge != null)
            {
                foreach (BoundingSphere sphere in vobSpheresLarge)
                {
                    Gizmos.DrawWireSphere(sphere.position, sphere.radius);
                }
            }
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

            vobSpheresSmall = null;
            vobSpheresMedium = null;
            vobSpheresLarge = null;

            pausedVobs.Clear();
            pausedVobsToReenable.Clear();
            pausedVobsToReenableCoroutine.Clear();
        }

        private void VobSmallChanged(CullingGroupEvent evt)
        {
            var smallPaused = pausedVobs
                .Where(i => i.Value.Item1 == VobList.Small)
                .Select(i => i.Value.Item2);

            if (smallPaused.Contains(evt.index))
                return;

            vobObjectsSmall[evt.index].SetActive(evt.hasBecomeVisible);
        }

        private void VobMediumChanged(CullingGroupEvent evt)
        {
            var mediumPaused = pausedVobs
                .Where(i => i.Value.Item1 == VobList.Medium)
                .Select(i => i.Value.Item2);

            if (mediumPaused.Contains(evt.index))
                return;

            vobObjectsMedium[evt.index].SetActive(evt.hasBecomeVisible);
        }

        private void VobLargeChanged(CullingGroupEvent evt)
        {
            var largePaused = pausedVobs
                .Where(i => i.Value.Item1 == VobList.Large)
                .Select(i => i.Value.Item2);

            if (largePaused.Contains(evt.index))
                return;

            vobObjectsLarge[evt.index].SetActive(evt.hasBecomeVisible);
        }

        /// <summary>
        /// Fill CullingGroups with GOs based on size (radius), position, and object size (small/medium/large)
        /// </summary>
        public void PrepareVobCulling(List<GameObject> objects)
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
                if (!obj)
                {
                    continue;
                }

                // FIXME - Particles (like leaves in the forest) will be handled like big vobs, but could potentially
                // FIXME - be handled as small ones as leaves shouldn't be visible from 100 of meters away.
                var bounds = GetLocalBounds(obj);
                if (!bounds.HasValue)
                {
                    Debug.LogError($"Couldn't find mesh for >{obj}< to be used for CullingGroup. Skipping...");

                    continue;
                }

                var sphere = GetSphere(obj, bounds.Value);
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

            vobCullingGroupSmall.SetBoundingDistances(new[] { FeatureFlags.I.vobCullingSmall.cullingDistance });
            vobCullingGroupMedium.SetBoundingDistances(new[] { FeatureFlags.I.vobCullingMedium.cullingDistance });
            vobCullingGroupLarge.SetBoundingDistances(new[] { FeatureFlags.I.vobCullingLarge.cullingDistance });

            vobSpheresSmall = spheresSmall.ToArray();
            vobSpheresMedium = spheresMedium.ToArray();
            vobSpheresLarge = spheresLarge.ToArray();

            vobCullingGroupSmall.SetBoundingSpheres(vobSpheresSmall);
            vobCullingGroupMedium.SetBoundingSpheres(vobSpheresMedium);
            vobCullingGroupLarge.SetBoundingSpheres(vobSpheresLarge);
        }

        private BoundingSphere GetSphere(GameObject go, Bounds bounds)
        {
            var bboxSize = bounds.size;
            Vector3 worldCenter = go.transform.TransformPoint(bounds.center);

            var maxDimension = Mathf.Max(bboxSize.x, bboxSize.y, bboxSize.z); // Get biggest dim for calculation of object size group.
            var sphere = new BoundingSphere(worldCenter, maxDimension / 2); // Radius is half the size.

            return sphere;
        }

        /// <summary>
        /// TODO If performance allows it, we could also look dynamically for all the existing meshes inside GO
        /// TODO and look for maximum value for largest mesh. But it should be fine for now.
        /// </summary>
        private Bounds? GetLocalBounds(GameObject go)
        {
            try
            {
                if (go.TryGetComponent<ParticleSystemRenderer>(out var particleRenderer))
                {
                    return particleRenderer.bounds;
                }
                else if (go.TryGetComponent(out Light light))
                {
                    return new Bounds(Vector3.zero, Vector3.one * light.range * 2);
                }
                else if (go.TryGetComponent(out StationaryLight stationaryLight))
                {
                    return new Bounds(Vector3.zero, Vector3.one * stationaryLight.Range * 2);
                }
                else
                {
                    return go.GetComponentInChildren<MeshFilter>().mesh.bounds;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        /// <summary>
        /// Set main camera once world is loaded fully.
        /// Doesn't work at loading time as we change scenes etc.
        /// </summary>
        private void PostWorldCreate()
        {
            foreach (var group in new[] { vobCullingGroupSmall, vobCullingGroupMedium, vobCullingGroupLarge })
            {
                var mainCamera = Camera.main!;
                group.targetCamera = mainCamera; // Needed for FrustumCulling and OcclusionCulling to work.
                group.SetDistanceReferencePoint(mainCamera.transform); // Needed for BoundingDistances to work.
            }
        }

        public void StartTrackVobPositionUpdates(GameObject go)
        {
            CancelStopTrackVobPositionUpdates(go);

            // Entry is already in list
            if (pausedVobs.ContainsKey(go))
                return;

            // Check Small list
            int index = vobObjectsSmall.IndexOf(go);
            var vobType = VobList.Small;
            // Check Medium list
            if (index == -1)
            {
                index = vobObjectsMedium.IndexOf(go);
                vobType = VobList.Medium;
            }
            // Check Large list
            if (index == -1)
            {
                index = vobObjectsLarge.IndexOf(go);
                vobType = VobList.Large;
            }

            pausedVobs.Add(go, new(vobType, index));
        }

        public void StopTrackVobPositionUpdates(GameObject go)
        {
            if (pausedVobsToReenableCoroutine.ContainsKey(go))
                return;

            pausedVobsToReenableCoroutine.Add(go, StartCoroutine(StopTrackVobPositionUpdatesDelayed(go)));
        }

        /// <summary>
        /// If we execute Start() and Stop() during a short time frame, we need to cancel all the "stop" features.
        /// e.g. If we start grabbing it while it's still in release-stop mode, we cancel delay Coroutine and loop itself.
        /// </summary>
        private void CancelStopTrackVobPositionUpdates(GameObject go)
        {
            if (pausedVobsToReenableCoroutine.ContainsKey(go))
            {
                StopCoroutine(pausedVobsToReenableCoroutine[go]);
                pausedVobsToReenableCoroutine.Remove(go);
            }

            if (pausedVobsToReenable.ContainsKey(go))
            {
                pausedVobsToReenable.Remove(go);
            }

        }

        /// <summary>
        /// We need to wait a few frames before the velocity of object is != 0.
        /// Therefore we put the object into the list delayed.
        /// </summary>
        private IEnumerator StopTrackVobPositionUpdatesDelayed(GameObject go)
        {
            yield return new WaitForSeconds(1f);
            pausedVobsToReenableCoroutine.Remove(go);
            if(!pausedVobsToReenable.ContainsKey(go))
                pausedVobsToReenable.Add(go, go.GetComponent<Rigidbody>());
        }

        private IEnumerator StopVobTrackingBasedOnVelocity()
        {
            while (true)
            {
                for (int i = pausedVobsToReenable.Keys.Count - 1; i >= 0; i--)
                {
                    GameObject key = pausedVobsToReenable.Keys.ElementAt(i);
                    Rigidbody rigidBody = pausedVobsToReenable[key];
                    if (rigidBody.velocity != Vector3.zero)
                        continue;

                    UpdateSpherePosition(key);

                    pausedVobs.Remove(key);
                    pausedVobsToReenable.Remove(key);
                }

                yield return null;
            }
        }

        private void UpdateSpherePosition(GameObject go)
        {
            var grabbed = pausedVobs[go];
            var vobType = grabbed.Item1;
            var index = grabbed.Item2;

            // We need to find the GO's correlated Sphere in the right VobArray.
            BoundingSphere[] sphereList = vobType switch
            {
                VobList.Small => vobSpheresSmall,
                VobList.Medium => vobSpheresMedium,
                VobList.Large => vobSpheresLarge,
                _ => throw new ArgumentOutOfRangeException()
            };

            sphereList[index].position = go.transform.position;
        }

        private void OnDestroy()
        {
            vobCullingGroupSmall.Dispose();
            vobCullingGroupMedium.Dispose();
            vobCullingGroupLarge.Dispose();
        }
    }
}
