using GVR.Caches;
using GVR.Extensions;
using GVR.Util;
using UnityEngine;

public class TextureManager : SingletonBehaviour<TextureManager>
{
    public Material MainMenuImageBackgroundMaterial;
    public Material MainMenuBackgroundMaterial;
    public Material MainMenuTextImageMaterial;

    public Material backgroundmaterial;
    public Material buttonmaterial;
    public Material slidermaterial;
    public Material sliderpositionmaterial;
    public Material arrowmaterial;
    public Material fillermaterial;
    public Material skymaterial;
    public Material mapmaterial;

    public Material GothicLoadingMenuMaterial;
    public Material LoadingBarBackgroundMaterial;
    public Material LoadingBarMaterial;
    public Material LoadingSphereMaterial;

    private const string defaultShader = "Universal Render Pipeline/Unlit"; // "Unlit/Transparent Cutout";

    private void Start()
    {

        MainMenuImageBackgroundMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        MainMenuBackgroundMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        MainMenuTextImageMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Transparent);

        GothicLoadingMenuMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        LoadingBarBackgroundMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        LoadingBarMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);

        // TODO: remove the middleman materials and use these for settings menu
        // backgroundmaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        // buttonmaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Transparent);
        // slidermaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Transparent);
        // sliderpositionmaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Transparent);
        // arrowmaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Transparent);
        // fillermaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Transparent);

        LoadingSphereMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        LoadingSphereMaterial.color = new Color(.25f, .25f, .25f, 1f); // dark gray
    }

    public void LoadLoadingDefaultTextures()
    {

        var mainMenuImageBackgroundTexture = AssetCache.TryGetTexture("STARTSCREEN.TGA");
        MainMenuImageBackgroundMaterial.mainTexture = mainMenuImageBackgroundTexture;

        var mainMenuImageTexture = AssetCache.TryGetTexture("MENU_INGAME.TGA");
        MainMenuBackgroundMaterial.mainTexture = mainMenuImageTexture;

        var mainMenuTextImageTexture = AssetCache.TryGetTexture("MENU_GOTHIC.TGA");
        MainMenuTextImageMaterial.mainTexture = mainMenuTextImageTexture;

        var loadingBackgroundTexture = AssetCache.TryGetTexture("LOADING.TGA");
        GothicLoadingMenuMaterial.mainTexture = loadingBackgroundTexture;

        var progressBackgroundTexture = AssetCache.TryGetTexture("PROGRESS.TGA");
        LoadingBarBackgroundMaterial.mainTexture = progressBackgroundTexture;

        var progressTexture = AssetCache.TryGetTexture("PROGRESS_BAR.TGA");
        LoadingBarMaterial.mainTexture = progressTexture;

        var backgroundtexture = AssetCache.TryGetTexture("LOG_PAPER.TGA");
        backgroundmaterial.mainTexture = backgroundtexture;

        var buttontexture = AssetCache.TryGetTexture("INV_SLOT.TGA");
        buttonmaterial.mainTexture = buttontexture;

        var slidertexture = AssetCache.TryGetTexture("MENU_SLIDER_BACK.TGA");
        slidermaterial.mainTexture = slidertexture;

        var sliderpositiontexture = AssetCache.TryGetTexture("MENU_SLIDER_POS.TGA");
        sliderpositionmaterial.mainTexture = sliderpositiontexture;

        var fillertexture = AssetCache.TryGetTexture("MENU_BUTTONBACK-C.TEX");
        fillermaterial.mainTexture = fillertexture;

        var arrowtexture = AssetCache.TryGetTexture("U.TGA");
        arrowmaterial.mainTexture = arrowtexture;

        var skytexture = AssetCache.TryGetTexture("SKYDAY_LAYER1_A0-C.TEX");
        skymaterial.mainTexture = skytexture;

        var maptexture = AssetCache.TryGetTexture("MAP_WORLD_ORC-C.TEX");
        mapmaterial.mainTexture = maptexture;
    }

    public void SetTexture(string texture, Material material)
    {
        material.mainTexture = AssetCache.TryGetTexture(texture);
    }

    private Material GetEmptyMaterial(MaterialExtension.BlendMode blendMode = MaterialExtension.BlendMode.Opaque)
    {
        var standardShader = Shader.Find(defaultShader);
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
