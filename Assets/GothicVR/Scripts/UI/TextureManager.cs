using System;
using UnityEngine;
using GVR.Caches;
using GVR.Util;
using GVR.Phoenix.Interface;
using TMPro;
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
        }
    }

    public void LoadCustomTextures()
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

}
