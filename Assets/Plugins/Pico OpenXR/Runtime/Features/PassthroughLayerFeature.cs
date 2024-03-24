using System;
using UnityEngine;

namespace Unity.XR.OpenXR.Features.PICOSupport
{
    public class PassthroughLayerFeature : LayerBase
    {
        private  int id = 0;
        private Vector3[] vertices;
        private int[] triangles;
        private Mesh mesh;
        private bool isPassthroughSupported = false;
        private bool isCreateTriangleMesh = false;

        private void Awake()
        {
            base.Awake();
            id = ID;
            Debug.Log("AAA:"+this.gameObject.name+"   ID="+id);
        }

        private void Start()
        {
            MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                Debug.LogError("Passthrough GameObject does not have a mesh component.");
                return;
            }

            mesh = meshFilter.sharedMesh;
            vertices = mesh.vertices;
            triangles = mesh.triangles;
            isPassthroughSupported = PassthroughFeature.IsPassthroughSupported();
        }

        private void Update()
        {
            if (isPassthroughSupported && !isCreateTriangleMesh)
            {
                GeometryInstanceTransform Transform = new GeometryInstanceTransform();
                UpdateCoords();
                GetCurrentTransform(ref Transform);
                isCreateTriangleMesh = PassthroughFeature.CreateTriangleMesh(id, vertices, triangles, Transform);
            }
        }

        private void OnEnable()
        {
            Camera.onPostRender += OnPostRenderCallBack;
        }

        private void OnDisable()
        {
            Camera.onPostRender -= OnPostRenderCallBack;
        }


        private void OnPostRenderCallBack(Camera cam)
        {
            GeometryInstanceTransform Transform = new GeometryInstanceTransform();
            UpdateCoords();
            GetCurrentTransform(ref Transform);
            PassthroughFeature.UpdateMeshTransform(id, Transform);
        }

        void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                PassthroughFeature.PassthroughPause();
            }
            else
            {
                PassthroughFeature.PassthroughStart();
            }
        }

        
    }
}