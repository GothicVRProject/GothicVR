using GVR.Caches;
using GVR.Extensions;
using GVR.Globals;
using GVR.Util;
using UnityEngine;

public class TextureManager : SingletonBehaviour<TextureManager>
{
    public Material mainMenuImageBackgroundMaterial;
    public Material mainMenuBackgroundMaterial;
    public Material mainMenuTextImageMaterial;

    public Material backgroundMaterial;
    public Material buttonMaterial;
    public Material sliderMaterial;
    public Material sliderPositionMaterial;
    public Material arrowMaterial;
    public Material fillerMaterial;
    public Material skyMaterial;
    public Material mapMaterial;

    public Material gothicLoadingMenuMaterial;
    public Material loadingBarBackgroundMaterial;
    public Material loadingBarMaterial;
    public Material loadingSphereMaterial;

    private void Start()
    {
        mainMenuImageBackgroundMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        mainMenuBackgroundMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        mainMenuTextImageMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Transparent);

        gothicLoadingMenuMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        loadingBarBackgroundMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        loadingBarMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);

        loadingSphereMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        loadingSphereMaterial.color = new Color(.25f, .25f, .25f, 1f); // dark gray
    }

    public void LoadLoadingDefaultTextures()
    {
        mainMenuImageBackgroundMaterial.mainTexture = TextureCache.TryGetTexture("STARTSCREEN.TGA");
        mainMenuBackgroundMaterial.mainTexture = TextureCache.TryGetTexture("MENU_INGAME.TGA");
        mainMenuTextImageMaterial.mainTexture = TextureCache.TryGetTexture("MENU_GOTHIC.TGA");
        gothicLoadingMenuMaterial.mainTexture = TextureCache.TryGetTexture("LOADING.TGA");
        loadingBarBackgroundMaterial.mainTexture = TextureCache.TryGetTexture("PROGRESS.TGA");
        loadingBarMaterial.mainTexture = TextureCache.TryGetTexture("PROGRESS_BAR.TGA");
        backgroundMaterial.mainTexture = TextureCache.TryGetTexture("LOG_PAPER.TGA");
        buttonMaterial.mainTexture = TextureCache.TryGetTexture("INV_SLOT.TGA");
        sliderMaterial.mainTexture = TextureCache.TryGetTexture("MENU_SLIDER_BACK.TGA");
        sliderPositionMaterial.mainTexture = TextureCache.TryGetTexture("MENU_SLIDER_POS.TGA");
        fillerMaterial.mainTexture = TextureCache.TryGetTexture("MENU_BUTTONBACK.TGA");
        arrowMaterial.mainTexture = TextureCache.TryGetTexture("U.TGA");
        mapMaterial.mainTexture = TextureCache.TryGetTexture("MAP_WORLD_ORC.TGA");
    }

    public void SetTexture(string texture, Material material)
    {
        material.mainTexture = TextureCache.TryGetTexture(texture);
    }

    private Material GetEmptyMaterial(MaterialExtension.BlendMode blendMode)
    {
        var standardShader = Constants.ShaderUnlit;
        var material = new Material(standardShader);

        switch (blendMode)
        {
            case MaterialExtension.BlendMode.Opaque:
                material.ToOpaqueMode();
                break;
            case MaterialExtension.BlendMode.Transparent:
                material.ToTransparentMode();
                break;
        }
        // Enable clipping of alpha values.
        material.EnableKeyword("_ALPHATEST_ON");

        return material;
    }
}
