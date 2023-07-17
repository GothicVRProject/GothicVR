using System;
using UnityEngine;
using GVR.Caches;
using GVR.Util;
using GVR.Phoenix.Interface;
using TMPro;

public class ImageController : MonoBehaviour
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
    bool textureloaded = false;
    // public TMP_FontAsset textMesh;



    // Update is called once per frame
    void Update()
    {
        if ( !textureloaded && PhoenixBridge.VdfsPtr != IntPtr.Zero)
        {
            LoadCustomTextures();
        }
    }

    public void LoadCustomTextures()
    {
        textureloaded = true;
        backgroundtexture = SingletonBehaviour<AssetCache>.GetOrCreate().TryGetTexture("LOG_PAPER.TGA");
        backgroundmaterial.mainTexture = backgroundtexture;

        buttontexture = SingletonBehaviour<AssetCache>.GetOrCreate().TryGetTexture("INV_SLOT.TGA");
        buttonmaterial.mainTexture = buttontexture;

        slidertexture = SingletonBehaviour<AssetCache>.GetOrCreate().TryGetTexture("MENU_SLIDER_BACK.TGA");
        slidermaterial.mainTexture = slidertexture;

        sliderpositiontexture = SingletonBehaviour<AssetCache>.GetOrCreate().TryGetTexture("MENU_SLIDER_POS.TGA");
        sliderpositionmaterial.mainTexture = sliderpositiontexture;

        fillertexture = SingletonBehaviour<AssetCache>.GetOrCreate().TryGetTexture("MENU_BUTTONBACK-C.TEX");
        fillermaterial.mainTexture = fillertexture;

        arrowtexture = SingletonBehaviour<AssetCache>.GetOrCreate().TryGetTexture("U.TGA");
        arrowmaterial.mainTexture = arrowtexture;

        //textMesh = PhoenixBridge.GothicSubtitleFont;

    }

 
}
