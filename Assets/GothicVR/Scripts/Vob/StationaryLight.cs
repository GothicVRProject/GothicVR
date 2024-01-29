using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace GVR
{
    [RequireComponent(typeof(Light))]
    public class StationaryLight : MonoBehaviour
    {
        public Color Color
        {
            get
            {
                if (!_unityLight)
                {
                    _unityLight = GetComponent<Light>();
                }
                return _unityLight.color;
            }
            set
            {
                if (!_unityLight)
                {
                    _unityLight = GetComponent<Light>();
                }
                _unityLight.color = value;
            }
        }

        public LightType Type
        {
            get
            {
                if (!_unityLight)
                {
                    _unityLight = GetComponent<Light>();
                }
                return _unityLight.type;
            }
            set
            {
                if (!_unityLight)
                {
                    _unityLight = GetComponent<Light>();
                }
                _unityLight.type = value;
            }
        }

        public float Intensity
        {
            get
            {
                if (!_unityLight)
                {
                    _unityLight = GetComponent<Light>();
                }
                return _unityLight.intensity;
            }
            set
            {
                if (!_unityLight)
                {
                    _unityLight = GetComponent<Light>();
                }
                _unityLight.intensity = value;
            }
        }

        public float Range
        {
            get
            {
                if (!_unityLight)
                {
                    _unityLight = GetComponent<Light>();
                }
                return _unityLight.range;
            }
            set
            {
                if (!_unityLight)
                {
                    _unityLight = GetComponent<Light>();
                }
                _unityLight.range = value;
            }
        }

        public float SpotAngle
        {
            get
            {
                if (!_unityLight)
                {
                    _unityLight = GetComponent<Light>();
                }
                return _unityLight.spotAngle;
            }
            set
            {
                if (!_unityLight)
                {
                    _unityLight = GetComponent<Light>();
                }
                _unityLight.spotAngle = value;
            }
        }

        private static List<StationaryLight> _lights = new List<StationaryLight>();

        private static readonly int GlobalStationaryLightPositionsAndAttenuationShaderId = Shader.PropertyToID("_GlobalStationaryLightPositionsAndAttenuation");
        private static readonly int GlobalStationaryLightColorsShaderId = Shader.PropertyToID("_GlobalStationaryLightColors");
        private static readonly int StationaryLightIndicesShaderId = Shader.PropertyToID("_StationaryLightIndices");
        private static readonly int StationaryLightIndices2ShaderId = Shader.PropertyToID("_StationaryLightIndices2");
        private static readonly int StationaryLightCountShaderId = Shader.PropertyToID("_StationaryLightCount");

        private static Dictionary<MeshRenderer, List<StationaryLight>> _lightsPerRenderer = new Dictionary<MeshRenderer, List<StationaryLight>>();
        private static HashSet<MeshRenderer> _dirtiedMeshes = new HashSet<MeshRenderer>();

        private List<MeshRenderer> _affectedRenderers = new List<MeshRenderer>();
        private Light _unityLight;
        private int _index;
        private List<Material> _nonAllocMaterials = new List<Material>();

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, Range);
        }

        private void Awake()
        {
            _lights.Add(this);
        }

        private void OnDestroy()
        {
            try
            {
                _lights.Remove(this);
            }
            catch (Exception)
            {
                Debug.LogError($"[{nameof(StationaryLight)}] Light collection unexpectedly does not contain light {name} on destroy.");
            }
        }

        private void OnEnable()
        {
            return;
            Profiler.BeginSample("Stationary light enabled");
            for (int i = 0; i < _affectedRenderers.Count; i++)
            {
                if (!_lightsPerRenderer.ContainsKey(_affectedRenderers[i]))
                {
                    _lightsPerRenderer.Add(_affectedRenderers[i], new List<StationaryLight>());
                }

                _lightsPerRenderer[_affectedRenderers[i]].Add(this);
                _dirtiedMeshes.Add(_affectedRenderers[i]);
            }
            Profiler.EndSample();
        }

        private void OnDisable()
        {
            return;
            Profiler.BeginSample("Stationary light disable");
            for (int i = 0; i < _affectedRenderers.Count; i++)
            {
                if (!_lightsPerRenderer.ContainsKey(_affectedRenderers[i]))
                {
                    continue;
                }

                try
                {
                    _lightsPerRenderer[_affectedRenderers[i]].Remove(this);
                    _dirtiedMeshes.Add(_affectedRenderers[i]);
                }
                catch (Exception)
                {
                    //Debug.LogError($"[{nameof(StationaryLight)}] Light {name} wasn't part of {_affectedRenderers[i].name}'s lights on disable. This is unexpected.");
                }
            }
            Profiler.EndSample();
        }

        //private void LateUpdate()
        //{
        //    // Update the renderer once for all updated lights.
        //    if (_dirtiedMeshes.Count > 0)
        //    {
        //        Profiler.BeginSample("Update stationary light renderers");
        //        foreach (MeshRenderer renderer in _dirtiedMeshes)
        //        {
        //            UpdateRenderer(renderer);
        //        }
        //        _dirtiedMeshes.Clear();
        //        Profiler.EndSample();
        //    }
        //}

        public static void InitStationaryLights()
        {
            Debug.Log($"[{nameof(StationaryLight)}] Total stationary light count: {_lights.Count}");
            Vector4[] _lightPositionsAndAttenuation = new Vector4[_lights.Count];
            Vector4[] _lightColors = new Vector4[_lights.Count];
            for (int i = 0; i < _lights.Count; i++)
            {
                _lights[i]._index = i;
                _lights[i].GatherRenderers();
                _lightPositionsAndAttenuation[i] = new Vector4(_lights[i].transform.position.x, _lights[i].transform.position.y, _lights[i].transform.position.z, 1f / (_lights[i].Range * _lights[i].Range));
                _lightColors[i] = _lights[i].Color.linear;
                _lights[i].gameObject.SetActive(true);
            }

            Shader.SetGlobalVectorArray(GlobalStationaryLightPositionsAndAttenuationShaderId, _lightPositionsAndAttenuation);
            Shader.SetGlobalVectorArray(GlobalStationaryLightColorsShaderId, _lightColors);
        }

        private void UpdateRenderer(MeshRenderer renderer)
        {
            if (!renderer)
            {
                return;
            }

            Matrix4x4 indicesMatrix = Matrix4x4.identity;
            renderer.GetSharedMaterials(_nonAllocMaterials);
            for (int i = 0; i < Mathf.Min(16, _lightsPerRenderer[renderer].Count); i++)
            {
                indicesMatrix[i / 4, i % 4] = _lightsPerRenderer[renderer][i]._index;
            }
            for (int i = 0; i < _nonAllocMaterials.Count; i++)
            {
                _nonAllocMaterials[i].SetMatrix(StationaryLightIndicesShaderId, indicesMatrix);
                _nonAllocMaterials[i].SetInt(StationaryLightCountShaderId, _lightsPerRenderer[renderer].Count);
            }

            if (_lightsPerRenderer[renderer].Count >= 16)
            {
                for (int i = 0; i < Mathf.Min(16, _lightsPerRenderer[renderer].Count - 16); i++)
                {
                    indicesMatrix[i / 4, i % 4] = _lightsPerRenderer[renderer][i + 16]._index;
                }
                for (int i = 0; i < _nonAllocMaterials.Count; i++)
                {
                    _nonAllocMaterials[i].SetMatrix(StationaryLightIndices2ShaderId, indicesMatrix);
                }
            }
        }

        private void GatherRenderers()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, Range);
            for (int i = 0; i < colliders.Length; i++)
            {
                MeshRenderer renderer = colliders[i].GetComponent<MeshRenderer>();
                if (renderer)
                {
                    _affectedRenderers.Add(renderer);
                }
            }
        }
    }
}
