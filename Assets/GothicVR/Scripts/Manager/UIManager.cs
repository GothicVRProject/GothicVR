using System.Linq;
using GVR.Creator;
using GVR.Phoenix.Interface;
using GVR.Util;
using GVR.Caches;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

namespace GVR.Manager
{
    public class UIManager : SingletonBehaviour<UIManager>
    {
        public Material GothicLoadingMenuMaterial;
        public Material LoadingBarBackgroundMaterial;
        public Material LoadingBarMaterial;
        public void LoadDefaultTextures()
        {
            var loadingBackgroundTexture = AssetCache.I.TryGetTexture("LOADING.TGA");
            GothicLoadingMenuMaterial.mainTexture = loadingBackgroundTexture;

            var progressBackgroundTexture = AssetCache.I.TryGetTexture("PROGRESS.TGA");
            LoadingBarBackgroundMaterial.mainTexture = progressBackgroundTexture;

            var progressTexture = AssetCache.I.TryGetTexture("PROGRESS_BAR.TGA");
            LoadingBarMaterial.mainTexture = progressTexture;
        }

        public void setTexture(string texture,Material material)
        {
            material.mainTexture = AssetCache.I.TryGetTexture(texture);

        }

    }
}