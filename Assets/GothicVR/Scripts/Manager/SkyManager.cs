using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Creator.Sounds;
using GVR.Data;
using GVR.Extensions;
using GVR.Globals;
using GVR.Manager.Settings;
using GVR.Util;
using GVR.World;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace GVR.GothicVR.Scripts.Manager
{
    public class SkyManager : SingletonBehaviour<SkyManager>
    {
        private float masterTime;
        private bool noSky = true;
        private List<SkyState> stateList = new();

        private SkyStateRain rainState = new();
        private ParticleSystem rainParticleSystem;
        private AudioSource rainParticleSound;
        private float rainWeightAndVolume;
        public bool IsRaining;

        private const int MAX_PARTICLE_COUNT = 700;

        private void Start()
        {
            GvrEvents.GameTimeSecondChangeCallback.AddListener(Interpolate);
            GvrEvents.GameTimeHourChangeCallback.AddListener(UpdateRainTime);
            GvrEvents.GeneralSceneLoaded.AddListener(InitRainGO);
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


            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.ambientMode = AmbientMode.Flat;
            InitRainState();
            noSky = true;

            Interpolate(new DateTime());
        }

        private void UpdateStateTexAndFog(SkyState currentState)
        {
            if (SettingsManager.GameSettings.GothicINISettings.ContainsKey("SKY_OUTDOOR"))
            {
                var currentDay = GameTime.I.GetDay();
                var day = (currentDay + 1);

                // hacky way to use the proper color for the current day until animTex is implemented
                var colorValues =
                    SettingsManager.GameSettings.GothicINISettings["SKY_OUTDOOR"]["zDayColor" + day % 2]
                        .Split(' ').Select(float.Parse).ToArray();

                foreach (var state in stateList)
                {
                    if (state.time < 0.35 || state.time > 0.65)
                    {
                        state.layer[0].texName = "SKYDAY_LAYER0_A" + day % 2 + ".TGA";
                        if (state.time < 0.3 || state.time > 0.7)
                        {
                            state.layer[1].texName = "SKYDAY_LAYER1_A" + day % 2 + ".TGA";
                            state.fogColor = new Vector3(colorValues[0], colorValues[1], colorValues[2]);
                            state.domeColor0 = new Vector3(colorValues[0], colorValues[1], colorValues[2]);
                        }
                    }
                }
            }
        }

        private void Interpolate(DateTime _)
        {
            masterTime = GameTime.I.GetSkyTime(); // Current time

            var (previousIndex, currentIndex) = FindNextStateIndex();

            var lastState = stateList[previousIndex];
            var currentState = stateList[currentIndex];
            IsRaining = masterTime > rainState.time && masterTime < rainState.endTime;

            UpdateStateTexAndFog(currentState);

            if (IsRaining)
            {
                UpdateRain(rainState.time);
                InterpolateSky(currentState, rainState, rainState.time, rainState.lerpDuration);
                return;
            }

            if (masterTime > rainState.time &&
                masterTime < rainState.endTime + rainState.lerpDuration) // when rain is ending
            {
                UpdateRain(rainState.endTime);
                InterpolateSky(rainState, currentState, rainState.endTime, rainState.lerpDuration);
                return;
            }

            UpdateRain(rainState.time);
            InterpolateSky(lastState, currentState, currentState.time, currentState.lerpDuration);
        }

        private void InterpolateSky(SkyState lastState, SkyState newState, float startTime, float lerpDuration = 0.05f)
        {
            // Calculate how far we are between the two ticks (0.0 to 1.0)
            float lerpFraction = (masterTime - startTime) / lerpDuration;

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

        private void InitRainState()
        {
            // values taken from the original game
            rainState.time = 0.187f; // 16:30
            rainState.endTime = 0.229f; // 17:30
            rainState.lerpDuration = 0.01f;
            rainState.polyColor = new Vector3(255.0f, 250.0f, 235.0f);
            rainState.fogColor = new Vector3(72.0f, 72.0f, 72.0f);
            rainState.domeColor0 = new Vector3(72.0f, 72.0f, 72.0f);
            rainState.layer[0].texName = "SKYRAINCLOUDS.TGA";
            rainState.layer[0].texAlpha = 255.0f;
        }

        private void InitRainGO()
        {
            // by default rainPFX is disabled so we need to find the parent and activate it
            var rainParticlesGameObject = GameObject.Find("Rain").FindChildRecursively("RainParticles");
            rainParticlesGameObject.SetActive(true);
            rainParticleSystem = rainParticlesGameObject.GetComponent<ParticleSystem>();
            rainParticleSystem.Stop();

            rainParticleSound = rainParticlesGameObject.GetComponentInChildren<AudioSource>();
            rainParticleSound.clip = SoundCreator.ToAudioClip(AssetCache.TryGetSound("RAIN_01.WAV"));
            rainParticleSound.volume = 0;
            rainParticleSound.Stop();
        }


        private void UpdateRainTime(DateTime _)
        {
            if (masterTime > 0.02f) // This function is called every hour but is run only once a day at 12:00 pm
                return;

            rainState.time = Random.Range(0f, 1f);

            if (0.96f < rainState.time)
            {
                rainState.time = 0.96f;
            }

            rainState.endTime = Random.Range(0f, 0.06f) + 0.04f + rainState.time;

            if (1.0f < rainState.endTime)
            {
                rainState.endTime = 1.0f;
            }
        }

        private void UpdateRain(float stateTime, float lerpDuration = 0.01f)
        {
            if (rainParticleSound == null || rainParticleSystem == null)
                return;

            if (masterTime < rainState.time ||
                masterTime > rainState.endTime + rainState.lerpDuration) // is not raining nor after rain
            {
                rainParticleSound.volume = 0;
                rainParticleSound.Stop();
                rainParticleSystem.Stop();
                return;
            }

            var lerpFraction = (masterTime - stateTime) / lerpDuration;

            lerpFraction = Mathf.Clamp01(lerpFraction);

            if (IsRaining)
            {
                rainWeightAndVolume = lerpFraction;
            }
            else if (masterTime > rainState.time && masterTime < rainState.endTime + rainState.lerpDuration)
            {
                rainWeightAndVolume = 1 - lerpFraction;
            }

            rainParticleSound.volume = rainWeightAndVolume;

            var module = rainParticleSystem.emission;
            module.rateOverTime = new ParticleSystem.MinMaxCurve(MAX_PARTICLE_COUNT * rainWeightAndVolume);

            if (!rainParticleSound.isPlaying)
            {
                rainParticleSound.Play();
            }

            if (!rainParticleSystem.isPlaying)
            {
                rainParticleSystem.Play();
            }
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

        private static SkyState CreatePresetState(SkyState skyState, Action<SkyState> applyPreset)
        {
            applyPreset(skyState);
            return skyState;
        }
    }
}