using System;
using UnityEngine;
using GVR.Caches;
using GVR.Util;
using GVR.Phoenix.Interface;
using TMPro;
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
