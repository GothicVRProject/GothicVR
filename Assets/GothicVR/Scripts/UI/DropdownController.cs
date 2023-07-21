using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GVR.Phoenix.Interface;
public class DropdownController : MonoBehaviour
{
    public GameObject textAsset;
    public GameObject locomotionsystem;
    public ActionBasedSnapTurnProvider snapTurn;
    public ActionBasedContinuousTurnProvider continuousTurn;
    // Start is called before the first frame update
    void Awake()
    {
        var dropdown = transform.GetComponent<TMP_Dropdown>();
        dropdown.options.Clear();

        List<string> items = new List<string>();
        items.Add("Snap turn");
        items.Add("Continuous turn");

        foreach (var item in items)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData() { text = item });

        }
        dropdown.itemText.font = GameData.I.GothicSubtitleFont;

        dropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown); });
        snapTurn = locomotionsystem.GetComponent<ActionBasedSnapTurnProvider>();
        continuousTurn = locomotionsystem.GetComponent<ActionBasedContinuousTurnProvider>();
    }

    void DropdownItemSelected(TMP_Dropdown dropdown)
    {
        switch (dropdown.value)
        {
            case 0:
                snapTurn.enabled = true;
                continuousTurn.enabled = false;
                break;
            case 1:
                snapTurn.enabled = false;
                continuousTurn.enabled = true;
                break;
            default:
                snapTurn.enabled = true;
                continuousTurn.enabled = false;
                break;
        }
    }
}
