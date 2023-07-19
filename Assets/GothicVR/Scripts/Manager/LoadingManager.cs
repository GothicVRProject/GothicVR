using GVR.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

namespace GVR.Manager
{
    public class LoadingManager : SingletonBehaviour<LoadingManager>
    {
        private GameObject bar;

        private Scene loadingScene;

        private const string loadingSceneName = "Loading";

        private float progress = 0f;


        public void SetBarFromScene(Scene scene)
        {
            this.bar = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "LoadingSphere").transform.Find("LoadingCanvas/LoadingImage/ProgressBackground/ProgressBar").gameObject;
        }

        public void UpdateLoadingBar()
        {
            bar.GetComponent<Image>().fillAmount = this.progress;
        }
        public void SetProgress(float progress)
        {
            this.progress = progress;
            UpdateLoadingBar();
        }
        public void AddProgress(float progress)
        {
            this.progress += progress;
            UpdateLoadingBar();
        }
    }
}