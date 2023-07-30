using GVR.Phoenix.Util;
using GVR.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GVR.Manager
{
    public class LoadingManager : SingletonBehaviour<LoadingManager>
    {
        public enum LoadingProgressType
        {
            WorldMesh,
            VOb,
            NPC
        }
        private GameObject bar;

        private Scene loadingScene;

        private const string loadingSceneName = "Loading";

        private Dictionary<LoadingProgressType, float> progressByType = new Dictionary<LoadingProgressType, float>();

        private void Start()
        {
            // Initializing the Dictionary with the default progress (which is 0) for each type
            foreach (LoadingProgressType progressType in Enum.GetValues(typeof(LoadingProgressType)))
            {
                if (!progressByType.ContainsKey(progressType))
                {
                    progressByType.Add(progressType, 0f);
                }
            }
        }

        public void ResetProgress()
        {
            foreach (LoadingProgressType progressType in progressByType.Keys.ToList())
            {
                progressByType[progressType] = 0f;
            }
        }

        public void SetBarFromScene(Scene scene)
        {
            var sphere = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "LoadingSphere");
            this.bar = sphere.FindChildRecursively("ProgressBar");
        }

        private float CalculateOverallProgress()
        {
            float totalProgress = 0f;
            int numTypes = progressByType.Count;

            foreach (var progressPair in progressByType)
            {
                totalProgress += progressPair.Value / numTypes;
            }

            return totalProgress;
        }

        private void UpdateLoadingBar()
        {
            // Calculate the overall progress based on individual progress values
            float overallProgress = CalculateOverallProgress();

            // Update the loading bar with the overall progress
            bar.GetComponent<Image>().fillAmount = overallProgress;
        }

        public void SetProgress(LoadingProgressType progressType, float progress)
        {
            progressByType[progressType] = progress;
            if (bar != null)
                UpdateLoadingBar();
        }

        public void AddProgress(LoadingProgressType progressType, float progress)
        {
            float newProgress = progressByType[progressType] + progress;
            progressByType[progressType] = newProgress;
            if (bar != null)
                UpdateLoadingBar();
        }

        public void SetMaterialForLoading(Scene scene)
        {
            var sphere = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "LoadingSphere");
            sphere.GetComponent<MeshRenderer>().material = TextureManager.I.LoadingSphereMaterial;
            sphere.FindChildRecursively("LoadingImage").GetComponent<Image>().material = TextureManager.I.GothicLoadingMenuMaterial;
            sphere.FindChildRecursively("ProgressBackground").gameObject.GetComponent<Image>().material = TextureManager.I.LoadingBarBackgroundMaterial;
            sphere.FindChildRecursively("ProgressBar").gameObject.GetComponent<Image>().material = TextureManager.I.LoadingBarMaterial;
        }
    }
}