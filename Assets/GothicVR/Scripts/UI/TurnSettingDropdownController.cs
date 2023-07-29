using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GVR.Phoenix.Interface;
public class TurnSettingDropdownController : MonoBehaviour
{
    public GameObject locomotionsystem;
    public ActionBasedSnapTurnProvider snapTurn;
    public ActionBasedContinuousTurnProvider continuousTurn;
    private const string turnSettingPlayerPref = "TurnSetting";

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
        dropdown.onValueChanged.AddListener(DropdownItemSelected);
        snapTurn = locomotionsystem.GetComponent<ActionBasedSnapTurnProvider>();
        continuousTurn = locomotionsystem.GetComponent<ActionBasedContinuousTurnProvider>();

        dropdown.value = PlayerPrefs.GetInt(turnSettingPlayerPref);
        DropdownItemSelected(dropdown);
    }

    void DropdownItemSelected(TMP_Dropdown dropdown)
    {
        switch (value)
        {
            case 1:
                EnableContinuousTurn();
                break;
            case 0:
            default:
                EnableSnapTurn();
                break;
        }
    }

    void EnableSnapTurn()
    {
        snapTurn.enabled = true;
        continuousTurn.enabled = false;
        PlayerPrefs.SetInt("TurnSetting", 0);
    }

    void EnableContinuousTurn()
    {
        snapTurn.enabled = false;
        continuousTurn.enabled = true;
        PlayerPrefs.SetInt("TurnSetting", 1);
    }
}
