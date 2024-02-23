using System;
using System.Collections;
using System.Collections.Generic;
using GVR.Util;
using UnityEngine;
using UnityEngine.Profiling;

namespace GVR.Manager
{
    public class StationaryLightsManager : SingletonBehaviour<StationaryLightsManager>
    {
        private static readonly HashSet<MeshRenderer> DirtiedMeshes = new();
        private static readonly Dictionary<MeshRenderer, List<StationaryLight>> LightsPerRenderer = new();
        private static readonly List<Material> NonAllocMaterials = new();

        private void LateUpdate()
        {
            // Update the renderer once for all updated lights.
            if (DirtiedMeshes.Count > 0)
            {
                Profiler.BeginSample("Update stationary light renderers");
                foreach (MeshRenderer renderer in DirtiedMeshes)
                {
                    UpdateRenderer(renderer);
                }
                DirtiedMeshes.Clear();
                Profiler.EndSample();
            }
        }

        public void StartCoroutineChangeColors()
        {
            foreach (var light in StationaryLight.Lights)
            {
                StartCoroutine(ChangeColor(light));
            }
        }

        private IEnumerator ChangeColor(StationaryLight light)
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
                UpdateShaderArray(light);
            }
        }

        private void UpdateShaderArray(StationaryLight light)
        {
            if (StationaryLight.LightColors == null || StationaryLight.LightColors.Length == 0)
                return;
            var colorIndices = Shader.GetGlobalFloatArray(StationaryLight.GlobalStationaryLightColorIndicesShaderId);
            colorIndices[light.Index] = Array.IndexOf(StationaryLight.LightColors,
                StationaryLight.Lights[light.Index].Color.linear);
            Shader.SetGlobalFloatArray(StationaryLight.GlobalStationaryLightColorIndicesShaderId, colorIndices);
        }

        public static void AddLightOnRenderer(StationaryLight light, MeshRenderer renderer)
        {
            if (!LightsPerRenderer.ContainsKey(renderer))
            {
                LightsPerRenderer.Add(renderer, new List<StationaryLight>());
            }

            LightsPerRenderer[renderer].Add(light);
            DirtiedMeshes.Add(renderer);
        }

        public static void RemoveLightOnRenderer(StationaryLight light, MeshRenderer renderer)
        {
            if (!LightsPerRenderer.ContainsKey(renderer))
            {
                return;
            }

            try
            {
                LightsPerRenderer[renderer].Remove(light);
                DirtiedMeshes.Add(renderer);
            }
            catch
            {
                //Debug.LogError($"[{nameof(StationaryLight)}] Light {name} wasn't part of {_affectedRenderers[i].name}'s lights on disable. This is unexpected.");
            }
        }

        private void UpdateRenderer(MeshRenderer renderer)
        {
            if (!renderer)
            {
                return;
            }

            Matrix4x4 indicesMatrix = Matrix4x4.identity;
            renderer.GetSharedMaterials(NonAllocMaterials);
            for (int i = 0; i < Mathf.Min(16, LightsPerRenderer[renderer].Count); i++)
            {
                indicesMatrix[i / 4, i % 4] = LightsPerRenderer[renderer][i].Index;
            }
            for (int i = 0; i < NonAllocMaterials.Count; i++)
            {
                if (NonAllocMaterials[i])
                {
                    NonAllocMaterials[i].SetMatrix(StationaryLight.StationaryLightIndicesShaderId, indicesMatrix);
                    NonAllocMaterials[i].SetInt(StationaryLight.StationaryLightCountShaderId, LightsPerRenderer[renderer].Count);
                }
            }

            if (LightsPerRenderer[renderer].Count >= 16)
            {
                for (int i = 0; i < Mathf.Min(16, LightsPerRenderer[renderer].Count - 16); i++)
                {
                    indicesMatrix[i / 4, i % 4] = LightsPerRenderer[renderer][i + 16].Index;
                }
                for (int i = 0; i < NonAllocMaterials.Count; i++)
                {
                    if (NonAllocMaterials[i])
                    {
                        NonAllocMaterials[i].SetMatrix(StationaryLight.StationaryLightIndices2ShaderId, indicesMatrix);
                    }
                }
            }
        }
    }
}
