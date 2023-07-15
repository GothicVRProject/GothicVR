using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using GVR.Phoenix.Interface;

public class LoadFont : MonoBehaviour
{
    bool fontloaded = false;

    void Update()
    {
        // FIXME - Need to change to Event based approach once it's implemented
        // https://github.com/GothicVRProject/GothicVR/issues/80

        if (!fontloaded)
            StartCoroutine(LoadGothicFont());
    }
    public IEnumerator LoadGothicFont()
    {
        fontloaded = true;
        yield return new WaitForEndOfFrame();
        var textMesh = transform.GetComponent<TMP_Text>();

        if (PhoenixBridge.GothicSubtitleFont)
        {
            textMesh.font = PhoenixBridge.GothicSubtitleFont;
            //textMesh.fontMaterial.color = Color.black;
        }
    }
}
