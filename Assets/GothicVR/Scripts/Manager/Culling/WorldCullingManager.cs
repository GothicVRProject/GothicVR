using System.Collections.Generic;
using System.Linq;
using GVR.Debugging;
using GVR.Util;
using JetBrains.Annotations;
using UnityEngine;

namespace GVR.Manager.Culling
{
    public class WorldCullingManager : SingletonBehaviour<WorldCullingManager>
    {
        // Stored for resetting after world switch
        private CullingGroup cullingGroup;

        // Stored for later index mapping SphereIndex => GOIndex
        private readonly List<GameObject> objects = new();
        
        private void Start()
        {
            // FIXME - reactivate when event is merged and existing in Main
            // GvrSceneManager.I.sceneGeneralUnloaded.AddListener(PreWorldCreate);
            // GvrSceneManager.I.sceneGeneralLoaded.AddListener(PostWorldCreate);

            // Unity demands CullingGroups to be created in Awake() or Start() earliest.
            cullingGroup = new();
        }

        public void PreWorldCreate()
        {
            cullingGroup.Dispose();
            cullingGroup = new();
            objects.Clear();
        }
        
        private void OnStateChanged(CullingGroupEvent evt)
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
        public void PrepareWorldCulling([ItemCanBeNull] List<GameObject> gameObjects)
        {
            if (!FeatureFlags.I.enableWorldCulling)
                return;
            
            var spheres = new List<BoundingSphere>();

            foreach (var go in gameObjects.Where(i => i != null))
            {
                if (go == null)
                    continue;
                
                objects.Add(go);
                var sphere = GetSphere(go);
                spheres.Add(sphere);
            }

            // Disable if we're 1m away from last possible audible location.
            cullingGroup.SetBoundingDistances(new[]{250f});
            cullingGroup.onStateChanged = OnStateChanged;
            cullingGroup.SetBoundingSpheres(spheres.ToArray());
        }
        
        private BoundingSphere GetSphere(GameObject go)
        {
            var mesh = go.GetComponent<MeshFilter>().mesh;
            var bboxSize = mesh.bounds.size;
                
            var maxDimension = Mathf.Max(bboxSize.x, bboxSize.y, bboxSize.z); // Get biggest dim for calculation of object size group.
            var sphere = new BoundingSphere(mesh.bounds.center, maxDimension / 2); // Use center of bbox as location.

            return sphere;
        }
        
        /// <summary>
        /// Set main camera once world is loaded fully.
        /// Doesn't work at loading time as we change scenes etc.
        /// </summary>
        public void PostWorldCreate()
        {
            var mainCamera = Camera.main!;
            cullingGroup.targetCamera = mainCamera;
            cullingGroup.SetDistanceReferencePoint(mainCamera.transform);
        }
        
        private void OnDestroy()
        {
            cullingGroup.Dispose();
        }
    }
}