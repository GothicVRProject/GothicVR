using System.Collections.Generic;
using System.Linq;
using GVR.Debugging;
using GVR.Globals;
using GVR.Util;
using JetBrains.Annotations;
using UnityEngine;

namespace GVR.Manager.Culling
{
    public class VobSoundCullingManager : SingletonBehaviour<VobSoundCullingManager>
    {
        // Stored for resetting after world switch
        private CullingGroup soundCullingGroup;

        // Stored for later index mapping SphereIndex => GOIndex
        private readonly List<GameObject> objects = new();
        
        private void Start()
        {
            GvrEvents.GeneralSceneUnloaded.AddListener(PreWorldCreate);
            GvrEvents.GeneralSceneLoaded.AddListener(PostWorldCreate);

            // Unity demands CullingGroups to be created in Awake() or Start() earliest.
            soundCullingGroup = new();
        }

        public void PreWorldCreate()
        {
            soundCullingGroup.Dispose();
            soundCullingGroup = new();
            objects.Clear();
        }
        
        private void SoundChanged(CullingGroupEvent evt)
        {
            // Ignore Frustum and Occlusion culling.
            if (evt.previousDistance == evt.currentDistance)
                return;

            var inAudibleRange = evt.previousDistance > evt.currentDistance;

            objects[evt.index].SetActive(inAudibleRange);
        }
        
        
        /// <summary>
        /// Fill CullingGroups with GOs based on size (radius) and position
        /// </summary>
        public void PrepareSoundCulling([ItemCanBeNull] List<GameObject> gameObjects)
        {
            if (!FeatureFlags.I.enableSoundCulling)
                return;
            
            var spheres = new List<BoundingSphere>();

            foreach (var go in gameObjects.Where(i => i != null))
            {
                if (go == null)
                    continue;
                
                objects.Add(go);
                var sphere = new BoundingSphere(go.transform.position, go.GetComponent<AudioSource>().maxDistance);
                spheres.Add(sphere);
            }

            // Disable sounds if we're leaving the area and therefore last audible location.
            // Hint: As there are non spatial sounds (always same volume wherever we are),
            // we need to disable the sounds at exactly the spot we are.
            soundCullingGroup.SetBoundingDistances(new[]{0f});
            soundCullingGroup.onStateChanged = SoundChanged;
            soundCullingGroup.SetBoundingSpheres(spheres.ToArray());
        }
        
        /// <summary>
        /// Set main camera once world is loaded fully.
        /// Doesn't work at loading time as we change scenes etc.
        /// </summary>
        public void PostWorldCreate()
        {
            var mainCamera = Camera.main!;
            soundCullingGroup.targetCamera = mainCamera;
            soundCullingGroup.SetDistanceReferencePoint(mainCamera.transform);
        }
        
        private void OnDestroy()
        {
            soundCullingGroup.Dispose();
        }
    }
}
