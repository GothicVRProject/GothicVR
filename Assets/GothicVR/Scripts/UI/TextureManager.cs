using UnityEngine;
using GVR.Caches;
using GVR.Phoenix.Util;
using GVR.Util;

public class TextureManager : SingletonBehaviour<TextureManager>
{
    public Material backgroundmaterial;
    public Material buttonmaterial;
    public Material slidermaterial;
    public Material sliderpositionmaterial;
    public Material arrowmaterial;
    public Material fillermaterial;
    // public Material skymaterial;

    public Material GothicLoadingMenuMaterial;
    public Material LoadingBarBackgroundMaterial;
    public Material LoadingBarMaterial;
    public Material LoadingSphereMaterial;

    private const string defaultShader = "Universal Render Pipeline/Unlit"; // "Unlit/Transparent Cutout";

    private void Start()
    {
        GothicLoadingMenuMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        LoadingBarBackgroundMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);
        LoadingBarMaterial = GetEmptyMaterial(MaterialExtension.BlendMode.Opaque);

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
        var loadingBackgroundTexture = AssetCache.I.TryGetTexture("LOADING.TGA");
        GothicLoadingMenuMaterial.mainTexture = loadingBackgroundTexture;

        var progressBackgroundTexture = AssetCache.I.TryGetTexture("PROGRESS.TGA");
        LoadingBarBackgroundMaterial.mainTexture = progressBackgroundTexture;

        var progressTexture = AssetCache.I.TryGetTexture("PROGRESS_BAR.TGA");
        LoadingBarMaterial.mainTexture = progressTexture;

        var backgroundtexture = AssetCache.I.TryGetTexture("LOG_PAPER.TGA");
        backgroundmaterial.mainTexture = backgroundtexture;

        var buttontexture = AssetCache.I.TryGetTexture("INV_SLOT.TGA");
        buttonmaterial.mainTexture = buttontexture;

        var slidertexture = AssetCache.I.TryGetTexture("MENU_SLIDER_BACK.TGA");
        slidermaterial.mainTexture = slidertexture;

        var sliderpositiontexture = AssetCache.I.TryGetTexture("MENU_SLIDER_POS.TGA");
        sliderpositionmaterial.mainTexture = sliderpositiontexture;

        var fillertexture = AssetCache.I.TryGetTexture("MENU_BUTTONBACK-C.TEX");
        fillermaterial.mainTexture = fillertexture;

        var arrowtexture = AssetCache.I.TryGetTexture("U.TGA");
        arrowmaterial.mainTexture = arrowtexture;
    }

    public void SetTexture(string texture, Material material)
    {
        material.mainTexture = AssetCache.I.TryGetTexture(texture);
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
