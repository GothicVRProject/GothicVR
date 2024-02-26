using GVR.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GVR.Extensions;
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
        public int Index { get; private set; }

        public static readonly List<StationaryLight> Lights = new List<StationaryLight>();

        public static readonly int GlobalStationaryLightPositionsAndAttenuationShaderId = Shader.PropertyToID("_GlobalStationaryLightPositionsAndAttenuation");
        public static readonly int GlobalStationaryLightColorIndicesShaderId = Shader.PropertyToID("_GlobalStationaryLightColorIndices");
        public static readonly int GlobalStationaryLightColorsShaderId = Shader.PropertyToID("_GlobalStationaryLightColors");
        public static readonly int StationaryLightIndicesShaderId = Shader.PropertyToID("_StationaryLightIndices");
        public static readonly int StationaryLightIndices2ShaderId = Shader.PropertyToID("_StationaryLightIndices2");
        public static readonly int StationaryLightCountShaderId = Shader.PropertyToID("_StationaryLightCount");

        private static Coroutine _updateDirtiedMeshesRoutine;

        private List<MeshRenderer> _affectedRenderers = new List<MeshRenderer>();
        private Light _unityLight;
        private static readonly List<(Vector3 Position, float Range)> ThreadSafeLightData = new();
        internal int CurrentColorIndex;
        internal float ColorAnimationFps; // change color every 1/ColorAnimationFps seconds
        internal List<Color> ColorAnimationList = new();
        public static Vector4[] LightColors;

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, Range);
        }

        private void Awake()
        {
            Lights.Add(this);
            ThreadSafeLightData.Add((transform.position, Range));
        }

        private void OnDestroy()
        {
            try
            {
                Lights.Remove(this);
            }
            catch (Exception)
            {
                Debug.LogError($"[{nameof(StationaryLight)}] Light collection unexpectedly does not contain light {name} on destroy.");
            }
        }

        private void OnEnable()
        {
            Profiler.BeginSample("Stationary light enabled");
            StartCoroutine(ChangeColor(this));
            for (int i = 0; i < _affectedRenderers.Count; i++)
            {
                StationaryLightsManager.AddLightOnRenderer(this, _affectedRenderers[i]);
            }
            Profiler.EndSample();
        }

        private void OnDisable()
        {
            Profiler.BeginSample("Stationary light disable");
            StopCoroutine(ChangeColor(this));
            for (int i = 0; i < _affectedRenderers.Count; i++)
            {
                StationaryLightsManager.RemoveLightOnRenderer(this, _affectedRenderers[i]);
            }
            Profiler.EndSample();
        }

        public static void InitStationaryLights()
        {
            Debug.Log($"[{nameof(StationaryLight)}] Total stationary light count: {Lights.Count}");

            // e.g. if we disabled Vob loading within FeatureFlags.
            if (Lights.IsEmpty())
                return;
            List<Vector4> _lightColorsList = new List<Vector4>();
            foreach (var light in Lights)
            {
                _lightColorsList.Add(light.Color.linear);
                _lightColorsList.AddRange(light.ColorAnimationList.Select(t => t.linear).Select(dummy => (Vector4)dummy));
            }
            Vector4[] _lightPositionsAndAttenuation = new Vector4[Lights.Count];
            float[] _lightColorIndices = new float[Lights.Count];
            LightColors = _lightColorsList.Distinct().ToArray();
            for (int i = 0; i < Lights.Count; i++)
            {
                Lights[i].Index = i;
                Lights[i].GatherRenderers();
                _lightPositionsAndAttenuation[i] = new Vector4(Lights[i].transform.position.x, Lights[i].transform.position.y, Lights[i].transform.position.z, 1f / (Lights[i].Range * Lights[i].Range));
                int index = Array.IndexOf(LightColors, Lights[i].Color.linear);
                _lightColorIndices[i] = index;
                Lights[i].gameObject.SetActive(false);
                Lights[i].gameObject.SetActive(true);
            }

            Shader.SetGlobalVectorArray(GlobalStationaryLightPositionsAndAttenuationShaderId, _lightPositionsAndAttenuation);
            Shader.SetGlobalFloatArray(GlobalStationaryLightColorIndicesShaderId, _lightColorIndices);
            Shader.SetGlobalVectorArray(GlobalStationaryLightColorsShaderId, LightColors);
            ThreadSafeLightData.Clear(); // Clear the thread safe data as it is no longer needed.
        }
        
        private static IEnumerator ChangeColor(StationaryLight light)
        {
            yield return new WaitForSeconds(1 / light.ColorAnimationFps);
            while (true)
            {
                yield return new WaitForSeconds(1 / light.ColorAnimationFps);
                if (light.ColorAnimationList.Count == 0) continue;
                light.CurrentColorIndex++;
                if (light.CurrentColorIndex >= light.ColorAnimationList.Count)
                    light.CurrentColorIndex = 0;

                light.Color = light.ColorAnimationList[light.CurrentColorIndex];
            }
        }

        public static int CountLightsInBounds(Bounds bounds)
        {
            int count = 0;
            for (int i = 0; i < Lights.Count; i++)
            {
                Bounds lightBounds = new Bounds(ThreadSafeLightData[i].Position, Vector3.one * ThreadSafeLightData[i].Range * 2);
                if (bounds.Intersects(lightBounds))
                {
                    count++;
                }
            }

            return count;
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
