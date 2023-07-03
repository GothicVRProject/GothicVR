using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using GVR.Phoenix.Interface;


public class DropdownController : MonoBehaviour
{
    bool fontloaded = false;
    public GameObject textAsset;
    public GameObject locomotionsystem;
    public ActionBasedSnapTurnProvider snapTurn;
    public ActionBasedContinuousTurnProvider continuousTurn;
    // Start is called before the first frame update
    void Start()
    {

        var dropdown = transform.GetComponent<TMP_Dropdown>();
        dropdown.options.Clear();

        List<string> items = new List<string>();
        items.Add("No turning");
        items.Add("Snap turn");
        items.Add("Continuous turn");

        foreach (var item in items)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData() { text = item });
        }

        dropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown); });
        snapTurn = locomotionsystem.GetComponent<ActionBasedSnapTurnProvider>();
        continuousTurn = locomotionsystem.GetComponent<ActionBasedContinuousTurnProvider>();

        

    }

    void DropdownItemSelected(TMP_Dropdown dropdown)
    {

        if (dropdown.value == 0) 
        {
            snapTurn.enabled = false;
            continuousTurn.enabled = false; 
        }
        if (dropdown.value == 1)
        {
            snapTurn.enabled = true;
            continuousTurn.enabled = false;
        }
        if (dropdown.value == 2)
        {
            snapTurn.enabled = false;
            continuousTurn.enabled = true;
        }

    }

    // Update is called once per frame
    void Update()
    {
        // FIXME - Need to change to Event based approach once it's implemented
        // https://github.com/GothicVRProject/GothicVR/issues/80

        if (!fontloaded)
        LoadGothicFont();
    }

    public void LoadGothicFont()
    {
       
        var textMesh = textAsset.transform.GetComponent<TMP_Text>();

        if (PhoenixBridge.GothicMenuFont)
        {
            textMesh.font = PhoenixBridge.GothicSubtitleFont;
            //textMesh.fontMaterial.color = Color.black;
            fontloaded = true;

        }
    }


}
