using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes.V2;
using GVR.Debugging;
using GVR.Extensions;
using GVR.Util;
using UnityEngine;

namespace GVR.GothicVR.Scripts.Manager
{
    public class BarrierManager : SingletonBehaviour<BarrierManager>
    {
        private GameObject barrier;

        private Material material1;
        private Material material2;
        private bool materialsCached;

        private bool barrierVisible;

        private bool showThunder;
        private bool[] activeThunder = { false, false, false, false };
        private float[] thunderDelay = { 8, 6, 14, 2 }; // https://ataulien.github.io/Inside-Gothic/barrier/#thunder
        private float[] thunderTimer = { 0, 0, 0, 0 };

        private AudioSource[] thunderSoundSources = new AudioSource[4];

        private bool barrierFadeIn;
        private bool fadeIn = true;
        private bool fadeOut;
        private float fadeState;
        private float fadeTime;
        private float timeUpdatedFade;

        private float nextActivation = 8f;

        private const float BarrierMinOpacity = 0f;
        private const float BarrierMaxOpacity = 120f;

        // all these values are representing the time in seconds
        private const float TimeToStayVisible = 25;
        private const float TimeToStayHidden = 1200;
        private const float TimeStepToUpdateFade = 0.001f;


        public void CreateBarrier()
        {
            var barrierMesh = AssetCache.TryGetMsh("MAGICFRONTIER_OUT.MSH");
            barrier = MeshFactory.CreateBarrier("Barrier", barrierMesh)
                .GetAllDirectChildren()[0];

            if (!FeatureFlags.I.enableSounds)
                return;
            
            for (var i = 0; i < thunderSoundSources.Length; i++)
            {
                thunderSoundSources[i] = barrier.AddComponent<AudioSource>();
                // AddThunder(i);
            }
        }

        public void FixedUpdate()
        {
            RenderBarrier();
        }

        /// <summary>
        /// Controls when and how much the barrier is visible
        /// </summary>
        private void RenderBarrier()
        {
            if (barrier == null) return;

            CacheMaterials();

            nextActivation -= Time.deltaTime;

            if (nextActivation <= 0)
            {
                barrierVisible = !barrierVisible;
                nextActivation = TimeToStayHidden + Random.Range(-(5f * 60f), 5f * 60f);
                barrierFadeIn = true;
            }

            if (!barrierFadeIn)
            {
                if (FeatureFlags.I.showBarrierLogs)
                    Debug.Log("Next Activation: " + nextActivation);

                return;
            }

            UpdateFadeState();

            if (showThunder && FeatureFlags.I.enableSounds)
            {
                var sound = VobHelper.GetSoundClip("MFX_BARRIERE_AMBIENT");
                
                for (var i = 0; i < 4; i++)
                {
                    if (!activeThunder[i] && !thunderSoundSources[i].isPlaying &&
                        (Time.time - thunderTimer[i]) > thunderDelay[i])
                    {
                        thunderTimer[i] = Time.time;
                        thunderSoundSources[i].PlayOneShot(sound);
                    }
                }
            }
        }

        private void UpdateFadeState()
        {
            if (fadeIn)
            {
                ApplyFadeToMaterials();

                if (Time.time - timeUpdatedFade > TimeStepToUpdateFade)
                {
                    fadeState++;
                    timeUpdatedFade = Time.time;
                }

                if (fadeState >= BarrierMaxOpacity)
                {
                    fadeState = BarrierMaxOpacity;
                    fadeIn = false;
                    fadeTime = Time.time;
                    showThunder = true;
                }
            }
            else
            {
                // Check if it's time to fade out
                if (Time.time - fadeTime > TimeToStayVisible)
                {
                    fadeTime = Time.time;
                    fadeOut = true;
                    showThunder = false;
                }

                if (!fadeOut)
                    return;

                ApplyFadeToMaterials();

                if (Time.time - timeUpdatedFade > TimeStepToUpdateFade)
                {
                    fadeState--;
                    timeUpdatedFade = Time.time;
                }

                if (fadeState <= BarrierMinOpacity)
                {
                    fadeState = BarrierMinOpacity;
                    fadeIn = true;
                    fadeOut = false;
                    barrierFadeIn = false;
                }
            }
        }

        private void ApplyFadeToMaterials()
        {
            float blendValue = fadeState / 255f;
            material1.SetFloat("_Blend", blendValue);
            material2.SetFloat("_Blend", blendValue);
        }

        private void CacheMaterials()
        {
            if (barrier != null && !materialsCached)
            {
                var materials = barrier.GetComponent<Renderer>().materials;
                material1 = materials[0];
                material2 = materials[1];
                materialsCached = true;
            }
        }

        private void AddThunder(int i)
        {
            var thunderStrip = new GameObject("ThunderStrip");
            thunderStrip.transform.SetParent(barrier.transform);
            thunderStrip.transform.localPosition = new Vector3(-50, 400, -56);
            thunderStrip.transform.localRotation = Quaternion.identity * Quaternion.Euler(0, i * 90, -90);
            MeshFactory.CreatePolyStrip(thunderStrip, 11, Vector3.zero, new(0, 320, 100));
        }
    }
}
