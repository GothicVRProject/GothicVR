using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Creator.Sounds;
using GVR.Data;
using GVR.Debugging;
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
        public Transform SunDirection;
        [Tooltip("Changes will be reflected in Editor Runtime mode for testing purposes.")]
        public Color SunColor;
        [Tooltip("Changes will be reflected in Editor Runtime mode for testing purposes.")]
        public Color AmbientColor;
        [Tooltip("Changes will be reflected in Editor Runtime mode for testing purposes.")]
        [Range(0, 1)]
        public float PointLightIntensity = 1f;
        public bool IsRaining;

        private float masterTime;
        private bool noSky = true;
        private List<SkyState> stateList = new();

        private static readonly int SunDirectionShaderId = Shader.PropertyToID("_SunDirection");
        private static readonly int SunColorShaderId = Shader.PropertyToID("_SunColor");
        private static readonly int AmbientShaderId = Shader.PropertyToID("_AmbientColor");
        private static readonly int PointLightIntensityShaderId = Shader.PropertyToID("_PointLightIntensity");

        private SkyStateRain rainState = new();
        private ParticleSystem rainParticleSystem;
        private AudioSource rainParticleSound;
        private float rainWeightAndVolume;

        private const int MAX_PARTICLE_COUNT = 700;

        private static readonly int SkyTex1ShaderId = Shader.PropertyToID("_Sky1");
        private static readonly int SkyTex2ShaderId = Shader.PropertyToID("_Sky2");
        private static readonly int SkyTex3ShaderId = Shader.PropertyToID("_Sky3");
        private static readonly int SkyTex4ShaderId = Shader.PropertyToID("_Sky4");
        private static readonly int SkyMovement1ShaderId = Shader.PropertyToID("_Vector1");
        private static readonly int SkyMovement2ShaderId = Shader.PropertyToID("_Vector2");
        private static readonly int SkyMovement3ShaderId = Shader.PropertyToID("_Vector3");
        private static readonly int SkyMovement4ShaderId = Shader.PropertyToID("_Vector4");
        private static readonly int Sky1OpacityShaderId = Shader.PropertyToID("_Sky1Opacity");
        private static readonly int Sky2OpacityShaderId = Shader.PropertyToID("_Sky2Opacity");
        private static readonly int Sky3OpacityShaderId = Shader.PropertyToID("_Sky3Opacity");
        private static readonly int Sky4OpacityShaderId = Shader.PropertyToID("_Sky4Opacity");
        private static readonly int LayersBlendShaderId = Shader.PropertyToID("_LayerBlend");
        private static readonly int FogColor1ShaderId = Shader.PropertyToID("_FogColor");
        private static readonly int FogColor2ShaderId = Shader.PropertyToID("_FogColor2");
        private static readonly int DomeColor1ShaderId = Shader.PropertyToID("_DomeColor1");
        private static readonly int DomeColor2ShaderId = Shader.PropertyToID("_DomeColor2");

        private void Start()
        {
            GvrEvents.GameTimeSecondChangeCallback.AddListener(Interpolate);
            GvrEvents.GameTimeHourChangeCallback.AddListener(UpdateRainTime);
            GvrEvents.GeneralSceneLoaded.AddListener(GeneralSceneLoaded);
        }

        private void OnValidate()
        {
            SetShaderProperties();
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

        private void UpdateStateTexAndFog()
        {
            if (SettingsManager.GameSettings.GothicINISettings.ContainsKey("SKY_OUTDOOR"))
            {
                var currentDay = GameTime.I.GetDay();
                var day = (currentDay + 1);

                float[] colorValues;

                try
                {
                    // hacky way to use the proper color for the current day until animTex is implemented
                    // % 2 is used as there are only 2 textures for the sky, consistent between G1 and G2 
                    colorValues = SettingsManager.GameSettings.GothicINISettings["SKY_OUTDOOR"]["zDayColor" + day % 2]
                        .Split(' ').Select(float.Parse).ToArray();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return;
                }

                foreach (var state in stateList)
                {
                    // all states that contain day sky layer dawn, evening and day 0 to 3
                    if (state.time < 0.35 || state.time > 0.65)
                    {
                        state.layer[0].texName = "SKYDAY_LAYER0_A" + day % 2 + ".TGA";
                        // day states that contain sky cloud layer day 0 to 3
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

            UpdateStateTexAndFog();

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
            float lerpFraction = Mathf.Clamp01((masterTime - startTime) / lerpDuration);

            if (lerpFraction >= 1 && noSky == false)
            {
                return; // finished blending
            }

            Color oldPolyColor = lastState.polyColor.ToUnityColor(255) / 255f;
            Color newPolyColor = newState.polyColor.ToUnityColor(255) / 255f;

            Color oldDomeColor = lastState.domeColor0.ToUnityColor(255) / 255f;
            Color newDomeColor = newState.domeColor0.ToUnityColor(255) / 255f;

            Color oldFogColor = lastState.fogColor.ToUnityColor(255) / 255f;
            Color newFogColor = newState.fogColor.ToUnityColor(255) / 255f;

            RenderSettings.ambientLight = Color.Lerp(oldPolyColor, newPolyColor, lerpFraction);
            RenderSettings.fogColor = Color.Lerp(oldFogColor, newFogColor, lerpFraction);

            // Old sky layer 1.
            if (!string.IsNullOrEmpty(lastState.layer[0].texName))
            {
                RenderSettings.skybox.SetTexture(SkyTex1ShaderId, TextureCache.TryGetTexture(lastState.layer[0].texName));
            }
            RenderSettings.skybox.SetVector(SkyMovement1ShaderId, lastState.layer[0].texSpeed);
            RenderSettings.skybox.SetFloat(Sky1OpacityShaderId, lastState.layer[0].texAlpha / 255f);

            // Old sky layer 2.
            if (!string.IsNullOrEmpty(lastState.layer[1].texName))
            {
                RenderSettings.skybox.SetTexture(SkyTex2ShaderId, TextureCache.TryGetTexture(lastState.layer[1].texName));
            }
            RenderSettings.skybox.SetVector(SkyMovement2ShaderId, lastState.layer[1].texSpeed);
            RenderSettings.skybox.SetFloat(Sky2OpacityShaderId, lastState.layer[1].texAlpha / 255f);

            // New sky layer 1.
            if (!string.IsNullOrEmpty(newState.layer[0].texName))
            {
                RenderSettings.skybox.SetTexture(SkyTex3ShaderId, TextureCache.TryGetTexture(newState.layer[0].texName));
            }
            RenderSettings.skybox.SetVector(SkyMovement3ShaderId, newState.layer[0].texSpeed);
            RenderSettings.skybox.SetFloat(Sky3OpacityShaderId, newState.layer[0].texAlpha / 255f);

            // New sky layer 2.
            if (!string.IsNullOrEmpty(newState.layer[1].texName))
            {
                RenderSettings.skybox.SetTexture(SkyTex4ShaderId, TextureCache.TryGetTexture(newState.layer[1].texName));
            }
            RenderSettings.skybox.SetVector(SkyMovement4ShaderId, newState.layer[1].texSpeed);
            RenderSettings.skybox.SetFloat(Sky4OpacityShaderId, newState.layer[1].texAlpha / 255f);

            // Fog and dome color.
            RenderSettings.skybox.SetColor(FogColor1ShaderId, oldFogColor);
            RenderSettings.skybox.SetColor(FogColor2ShaderId, newFogColor);
            RenderSettings.skybox.SetColor(DomeColor1ShaderId, oldDomeColor);
            RenderSettings.skybox.SetColor(DomeColor2ShaderId, newDomeColor);

            RenderSettings.skybox.SetFloat(LayersBlendShaderId, lerpFraction);
            SetShaderProperties();
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

        private void SetShaderProperties()
        {
            if (SunDirection)
            {
                Shader.SetGlobalVector(SunDirectionShaderId, SunDirection.forward);
            }
            Shader.SetGlobalColor(SunColorShaderId, SunColor);
            Shader.SetGlobalColor(AmbientShaderId, AmbientColor);
            Shader.SetGlobalFloat(PointLightIntensityShaderId, PointLightIntensity);
        }

        private void GeneralSceneLoaded()
        {
            RenderSettings.skybox = Instantiate(TextureManager.I.skyMaterial);

            InitRainGO();
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
            if (masterTime > 0.02f || // This function is called every hour but is run only once a day at 12:00 pm
                GameTime.I.GetDay() == 1) // Dont update if it is the first day 
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

            if (!rainParticleSound.isPlaying && FeatureFlags.I.enableSounds)
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
