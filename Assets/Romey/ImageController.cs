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
   // public TMP_FontAsset textMesh;

        
            
    // Update is called once per frame
    void Update()
    {
        if (PhoenixBridge.VdfsPtr != IntPtr.Zero)
        {
            LoadBackground();
        }
    }

    public void LoadBackground()
    {
        backgroundtexture = SingletonBehaviour<AssetCache>.GetOrCreate().TryGetTexture("LOG_PAPER.TGA");
        backgroundmaterial.mainTexture = backgroundtexture;

        buttontexture = SingletonBehaviour<AssetCache>.GetOrCreate().TryGetTexture("INV_SLOT.TGA");
        buttonmaterial.mainTexture = buttontexture;

        //textMesh = PhoenixBridge.GothicSubtitleFont;

    }
}
