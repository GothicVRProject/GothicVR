using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GVR.Phoenix.Interface;
using GVR.Manager;

public class TurnSettingDropdownController : MonoBehaviour
{
    public GameObject locomotionsystem;
    public ActionBasedSnapTurnProvider snapTurn;
    public ActionBasedContinuousTurnProvider continuousTurn;


    void Awake()
    {
        // FIXME - If we're on Loading scene, there is no locomotionSystem. We should switch it to something like "isLoadingState".
        if (locomotionsystem == null)
            return;

        var dropdown = transform.GetComponent<TMP_Dropdown>();
        dropdown.options.Clear();

        List<string> items = new List<string>();
        items.Add("Snap turn");
        items.Add("Continuous turn");

        foreach (var item in items)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData() { text = item });
        }
        // We use an empty font so we can force TMP to use spriteAsset for each character
        dropdown.itemText.font = GameData.I.EmptyFont;
        dropdown.onValueChanged.AddListener(DropdownItemSelected);
        snapTurn = locomotionsystem.GetComponent<ActionBasedSnapTurnProvider>();
        continuousTurn = locomotionsystem.GetComponent<ActionBasedContinuousTurnProvider>();

        dropdown.value = PlayerPrefs.GetInt(ConstantsManager.I.turnSettingPlayerPref);
        DropdownItemSelected(dropdown.value);
    }

    void DropdownItemSelected(int value)
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
        PlayerPrefs.SetInt(ConstantsManager.I.turnSettingPlayerPref, 0);
    }

    void EnableContinuousTurn()
    {
        snapTurn.enabled = false;
        continuousTurn.enabled = true;
        PlayerPrefs.SetInt(ConstantsManager.I.turnSettingPlayerPref, 1);
    }
}
