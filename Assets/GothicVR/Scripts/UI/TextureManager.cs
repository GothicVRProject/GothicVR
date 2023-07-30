using System;
using System.Collections;
using GVR.Caches;
using GVR.Phoenix.Interface;
using UnityEngine;

public class TextureManager : MonoBehaviour
{
    private Texture2D backgroundtexture;
    public Material backgroundmaterial;
    private Texture2D buttontexture;
    public Material buttonmaterial;
    private Texture2D slidertexture;
    public Material slidermaterial;
    private Texture2D sliderpositiontexture;
    public Material sliderpositionmaterial;
    private Texture2D arrowtexture;
    public Material arrowmaterial;
    private Texture2D fillertexture;
    public Material fillermaterial;
    private Texture2D skytexture;
    public Material skymaterial;

    private bool textureloaded = false;

    void Update()
    {
        // FIXME - We should register to a "BootstrapDone" event rather than checking every frame.
        if (!textureloaded && GameData.I.VdfsPtr != IntPtr.Zero)
        {
            LoadCustomTextures();
            
            // Set Skybox one frame later.
            StartCoroutine(SetSkyBox());
        }
    }

    private void LoadCustomTextures()
    {
        textureloaded = true;
        backgroundtexture = AssetCache.I.TryGetTexture("LOG_PAPER.TGA");
        backgroundmaterial.mainTexture = backgroundtexture;

        buttontexture = AssetCache.I.TryGetTexture("INV_SLOT.TGA");
        buttonmaterial.mainTexture = buttontexture;

        slidertexture = AssetCache.I.TryGetTexture("MENU_SLIDER_BACK.TGA");
        slidermaterial.mainTexture = slidertexture;

        sliderpositiontexture = AssetCache.I.TryGetTexture("MENU_SLIDER_POS.TGA");
        sliderpositionmaterial.mainTexture = sliderpositiontexture;

        fillertexture = AssetCache.I.TryGetTexture("MENU_BUTTONBACK-C.TEX");
        fillermaterial.mainTexture = fillertexture;

        arrowtexture = AssetCache.I.TryGetTexture("U.TGA");
        arrowmaterial.mainTexture = arrowtexture;

        skytexture = AssetCache.I.TryGetTexture("SKYDAY_LAYER0_A0-C.TEX");
        skymaterial.mainTexture = skytexture;
    }

    /// <summary>
    /// For some reason Skybox resets itself to default.
    /// It might be, as we have no real material set initially. Therefore we set this one now.
    /// One frame after we set the material.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SetSkyBox()
    {
        yield return null; // Skip 1 frame
        RenderSettings.skybox = skymaterial;
    }
}
