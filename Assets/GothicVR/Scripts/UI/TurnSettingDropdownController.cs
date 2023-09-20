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

        var dropdown = transform.GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(DropdownItemSelected);

        dropdown.value = PlayerPrefs.GetInt(ConstantsManager.I.turnSettingPlayerPref);
        Debug.Log(PlayerPrefs.GetInt(ConstantsManager.I.turnSettingPlayerPref));
        DropdownItemSelected(dropdown.value);

        // FIXME - If we're on Loading scene, there is no locomotionSystem. We should switch it to something like "isLoadingState".
        if (locomotionsystem == null)
            return;
        snapTurn = locomotionsystem.GetComponent<ActionBasedSnapTurnProvider>();
        continuousTurn = locomotionsystem.GetComponent<ActionBasedContinuousTurnProvider>();
    }

    public void DropdownItemSelected(int value)
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
        PlayerPrefs.SetInt(ConstantsManager.I.turnSettingPlayerPref, 0);

        if (!locomotionsystem)
            return;

        snapTurn.enabled = true;
        continuousTurn.enabled = false;
    }

    void EnableContinuousTurn()
    {
        PlayerPrefs.SetInt(ConstantsManager.I.turnSettingPlayerPref, 1);

        if (!locomotionsystem)
            return;

        snapTurn.enabled = false;
        continuousTurn.enabled = true;
    }
}
