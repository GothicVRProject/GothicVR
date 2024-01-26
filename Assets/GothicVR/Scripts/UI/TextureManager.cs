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
    public Material mapmaterial;
    public Material lettermaterial;

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

        // These elements get their material assigned in Editor already.
        // TODO: remove the middleman materials and use these for settings menu
        // backgroundmaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        // buttonmaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Transparent);
        // slidermaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Transparent);
        // sliderpositionmaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Transparent);
        // arrowmaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Transparent);
        // fillermaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Transparent);

        loadingSphereMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        loadingSphereMaterial.color = new Color(.25f, .25f, .25f, 1f); // dark gray
    }

    public void LoadLoadingDefaultTextures()
    {
        mainMenuImageBackgroundMaterial.mainTexture = AssetCache.TryGetTexture("STARTSCREEN.TGA");
        mainMenuBackgroundMaterial.mainTexture = AssetCache.TryGetTexture("MENU_INGAME.TGA");
        mainMenuTextImageMaterial.mainTexture = AssetCache.TryGetTexture("MENU_GOTHIC.TGA");
        gothicLoadingMenuMaterial.mainTexture = AssetCache.TryGetTexture("LOADING.TGA");
        loadingBarBackgroundMaterial.mainTexture = AssetCache.TryGetTexture("PROGRESS.TGA");
        loadingBarMaterial.mainTexture = AssetCache.TryGetTexture("PROGRESS_BAR.TGA");
        backgroundMaterial.mainTexture = AssetCache.TryGetTexture("LOG_PAPER.TGA");
        buttonMaterial.mainTexture = AssetCache.TryGetTexture("INV_SLOT.TGA");
        sliderMaterial.mainTexture = AssetCache.TryGetTexture("MENU_SLIDER_BACK.TGA");
        sliderPositionMaterial.mainTexture = AssetCache.TryGetTexture("MENU_SLIDER_POS.TGA");
        fillerMaterial.mainTexture = AssetCache.TryGetTexture("MENU_BUTTONBACK.TGA");
        arrowMaterial.mainTexture = AssetCache.TryGetTexture("U.TGA");

        var maptexture = AssetCache.TryGetTexture("MAP_WORLD_ORC-C.TEX");
        mapmaterial.mainTexture = maptexture;

        var lettertexture = AssetCache.TryGetTexture("LETTERS-C.TEX");
        lettermaterial.mainTexture = lettertexture;
    }

    public void SetTexture(string texture, Material material)
    {
        material.mainTexture = AssetCache.TryGetTexture(texture);
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
