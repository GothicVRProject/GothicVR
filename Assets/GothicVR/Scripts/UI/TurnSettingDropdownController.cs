using GVR.Globals;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TurnSettingDropdownController : MonoBehaviour
{
    public GameObject locomotionsystem;
    public ActionBasedSnapTurnProvider snapTurn;
    public ActionBasedContinuousTurnProvider continuousTurn;

    void Awake()
    {

        var dropdown = transform.GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(DropdownItemSelected);

        dropdown.value = PlayerPrefs.GetInt(Constants.turnSettingPlayerPref);
        Debug.Log(PlayerPrefs.GetInt(Constants.turnSettingPlayerPref));
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
        PlayerPrefs.SetInt(Constants.turnSettingPlayerPref, 0);

        if (!locomotionsystem)
            return;

        snapTurn.enabled = true;
        continuousTurn.enabled = false;
    }

    void EnableContinuousTurn()
    {
        PlayerPrefs.SetInt(Constants.turnSettingPlayerPref, 1);

        if (!locomotionsystem)
            return;

        snapTurn.enabled = false;
        continuousTurn.enabled = true;
    }
}
