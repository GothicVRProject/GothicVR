using GVR.Util;
using GVR.Caches;
using UnityEngine;

namespace GVR.Manager
{
    public class UIManager : SingletonBehaviour<UIManager>
    {
        public Material GothicLoadingMenuMaterial;
        public Material LoadingBarBackgroundMaterial;
        public Material LoadingBarMaterial;
        public Material LoadingSphereMaterial;

        private const string defaultShader = "Universal Render Pipeline/Unlit"; // "Unlit/Transparent Cutout";

        private void Start()
        {
            GothicLoadingMenuMaterial = GetEmptyMaterial();
            LoadingBarBackgroundMaterial = GetEmptyMaterial();
            LoadingBarMaterial = GetEmptyMaterial();

            LoadingSphereMaterial = GetEmptyMaterial();
            LoadingSphereMaterial.color = new Color(.25f, .25f, .25f, 1f); // dark gray
        }

        public void LoadLoadingDefaultTextures()
        {
            var loadingBackgroundTexture = AssetCache.I.TryGetTexture("LOADING.TGA");
            GothicLoadingMenuMaterial.mainTexture = loadingBackgroundTexture;

            var progressBackgroundTexture = AssetCache.I.TryGetTexture("PROGRESS.TGA");
            LoadingBarBackgroundMaterial.mainTexture = progressBackgroundTexture;

            var progressTexture = AssetCache.I.TryGetTexture("PROGRESS_BAR.TGA");
            LoadingBarMaterial.mainTexture = progressTexture;
        }

        public void SetTexture(string texture, Material material)
        {
            material.mainTexture = AssetCache.I.TryGetTexture(texture);
        }

        private Material GetEmptyMaterial()
        {
            var standardShader = Shader.Find(defaultShader);
            var material = new Material(standardShader);

            material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Enable clipping of alpha values.
            material.EnableKeyword("_ALPHATEST_ON");

            return material;
        }
    }
}