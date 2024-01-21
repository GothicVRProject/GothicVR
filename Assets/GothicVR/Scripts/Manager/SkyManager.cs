using System;
using System.Collections.Generic;
using GVR.Caches;
using GVR.Data;
using GVR.Extensions;
using GVR.Globals;
using GVR.Util;
using GVR.World;
using UnityEngine;
using UnityEngine.Rendering;

namespace GVR.GothicVR.Scripts.Manager
{
    public class SkyManager : SingletonBehaviour<SkyManager>
    {
        private float masterTime;
        private bool noSky = true;
        [SerializeField] private List<SkyState> stateList = new List<SkyState>();

        private void Start()
        {
            GvrEvents.GameTimeSecondChangeCallback.AddListener(Interpolate);
        }

        public void InitSky()
        {
            stateList.AddRange(new[]
            {
                CreatePresetState(new SkyState(), (state) => state.PresetDay1()),
                CreatePresetState(new SkyState(), (state) => state.PresetDay2()),
                CreatePresetState(new SkyState(), (state) => state.PresetEvening()),
                CreatePresetState(new SkyState(), (state) => state.PresetNight0()),
                CreatePresetState(new SkyState(), (state) => state.PresetNight1()),
                CreatePresetState(new SkyState(), (state) => state.PresetNight2()),
                CreatePresetState(new SkyState(), (state) => state.PresetDawn()),
                CreatePresetState(new SkyState(), (state) => state.PresetDay0())
            });

            if (SettingsManager.GameSettings.GothicINISettings.ContainsKey("SKY_OUTDOOR"))
            {
                foreach (var state in stateList)
                {
                    if (state.time < 0.35 || state.time > 0.65)
                    {
                        var currentDay = GameTime.I.GetDay();
                        var day = (currentDay + 1);

                        // hacky way to use the proper color for the current day until animTex is implemented
                        var colorValues =
                            SettingsManager.GameSettings.GothicINISettings["SKY_OUTDOOR"]["zDayColor" + day % 2]
                                .Split(' ').Select(float.Parse).ToArray();

                        state.fogColor = new Vector3(colorValues[0], colorValues[1], colorValues[2]);
                    }
                }
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.ambientMode = AmbientMode.Flat;
            Interpolate();
        }

        private void Interpolate(DateTime _)
        {
            Interpolate();
        }

        private void Interpolate()
        {
            masterTime = GameTime.I.GetSkyTime(); // Current time

            var (previousIndex, nextIndex) = FindNextStateIndex();

            var lastState = stateList[previousIndex];
            var newState = stateList[nextIndex];

            float lerpDuration = 0.05f; // Duration over which to lerp
            float startTime = newState.time; // Starting time

            // Calculate how far we are between the two ticks (0.0 to 1.0)
            float endTime = startTime + lerpDuration;
            float lerpFraction = (masterTime - startTime) / (endTime - startTime);

            lerpFraction = Mathf.Clamp01(lerpFraction);

            if (lerpFraction >= 1 && noSky == false)
            {
                noSky = false;
                return; // finished blending
            }

            var oldPolyColor = lastState.polyColor.ToUnityColor(255) / 255f;
            var newPolyColor = newState.polyColor.ToUnityColor(255) / 255f;

            var oldDomeColor = lastState.domeColor0.ToUnityColor(255) / 255f;
            var newDomeColor = newState.domeColor0.ToUnityColor(255) / 255f;

            var oldFogColor = lastState.fogColor.ToUnityColor(255) / 255f;
            var newFogColor = newState.fogColor.ToUnityColor(255) / 255f;


            RenderSettings.ambientLight = Color.Lerp(oldPolyColor, newPolyColor, lerpFraction);
            RenderSettings.fogColor = Color.Lerp(oldFogColor, newFogColor, lerpFraction);

            if (lastState.layer[0].texName != "")
                RenderSettings.skybox.SetTexture("_Sky1", AssetCache.TryGetTexture(lastState.layer[0].texName));
            RenderSettings.skybox.SetVector("_Vector1", lastState.layer[0].texSpeed);
            RenderSettings.skybox.SetFloat("_Alpha1", lastState.layer[0].texAlpha / 255f);

            if (newState.layer[0].texName != "")
                RenderSettings.skybox.SetTexture("_Sky3", AssetCache.TryGetTexture(newState.layer[0].texName));
            RenderSettings.skybox.SetVector("_Vector3", newState.layer[0].texSpeed);
            RenderSettings.skybox.SetFloat("_Alpha3", newState.layer[0].texAlpha / 255f);

            if (lastState.layer[0].texName != "")
                RenderSettings.skybox.SetTexture("_Sky2", AssetCache.TryGetTexture(lastState.layer[1].texName));
            RenderSettings.skybox.SetVector("_Vector2", lastState.layer[1].texSpeed);
            RenderSettings.skybox.SetFloat("_Alpha2", lastState.layer[1].texAlpha / 255f);

            if (newState.layer[1].texName != "")
                RenderSettings.skybox.SetTexture("_Sky4", AssetCache.TryGetTexture(newState.layer[1].texName));
            RenderSettings.skybox.SetVector("_Vector4", newState.layer[1].texSpeed);
            RenderSettings.skybox.SetFloat("_Alpha4", newState.layer[1].texAlpha / 255f);

            RenderSettings.skybox.SetColor("_Color", oldDomeColor);
            RenderSettings.skybox.SetColor("_Color2", newDomeColor);

            RenderSettings.skybox.SetColor("_FogColor", oldFogColor);
            RenderSettings.skybox.SetColor("_FogColor2", newFogColor);

            RenderSettings.skybox.SetFloat("_Blend", lerpFraction);
            DynamicGI.UpdateEnvironment();
        }

        /// <summary>
        /// Find the previous and next state indices based on the current master time.
        /// </summary>
        private (int previousIndex, int nextIndex) FindNextStateIndex()
        {
            var nextIndex = stateList.FindLastIndex(x => x.time < masterTime);

            if (nextIndex == -1)
            {
                nextIndex = 0;
            }

            var previousIndex = nextIndex - 1;
            if (previousIndex < 0)
                previousIndex = stateList.Count - 1;

            return (previousIndex, nextIndex);
        }

        private SkyState CreatePresetState(SkyState skyState, Action<SkyState> applyPreset)
        {
            applyPreset(skyState);
            return skyState;
        }
    }
}