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
            this.bar = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "LoadingSphere").transform.Find("LoadingCanvas/LoadingImage/ProgressBackground/ProgressBar").gameObject;
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
            UpdateLoadingBar();
        }

        public void AddProgress(LoadingProgressType progressType, float progress)
        {
            float newProgress = progressByType[progressType] + progress;
            progressByType[progressType] = newProgress;
            UpdateLoadingBar();
        }

        public void SetMaterialForLoading(Scene scene)
        {
            scene.GetRootGameObjects().FirstOrDefault(go => go.name == "LoadingSphere").transform.Find("LoadingCanvas/LoadingImage/ProgressBackground/ProgressBar").gameObject.GetComponent<Image>().material = UIManager.I.LoadingBarMaterial;
            scene.GetRootGameObjects().FirstOrDefault(go => go.name == "LoadingSphere").transform.Find("LoadingCanvas/LoadingImage/ProgressBackground").gameObject.GetComponent<Image>().material = UIManager.I.LoadingBarBackgroundMaterial;
            scene.GetRootGameObjects().FirstOrDefault(go => go.name == "LoadingSphere").transform.Find("LoadingCanvas/LoadingImage").gameObject.GetComponent<Image>().material = UIManager.I.GothicLoadingMenuMaterial;
            scene.GetRootGameObjects().FirstOrDefault(go => go.name == "LoadingSphere").GetComponent<MeshRenderer>().material = UIManager.I.LoadingSphereMaterial;
        }
    }
}