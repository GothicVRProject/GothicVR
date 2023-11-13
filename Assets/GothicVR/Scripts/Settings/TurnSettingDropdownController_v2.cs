using TMPro;
using UnityEngine;
using GVR.Debugging;
using GVR.Manager;

public class TurnSettingDropdownController_v2 : MonoBehaviour
{
    void Awake()
    {
        var dropdown = transform.GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(DropdownItemSelected);

        dropdown.value = PlayerSettingsManager.LoadSettingsFromPlayerPrefs(ConstantsManager.turnSettingPlayerPref);
        DropdownItemSelected(dropdown.value);
    }

    public void DropdownItemSelected(int value)
    {
        switch (value)
        {
            case 1:
                PlayerSettingsManager.I.DropdownItemSelected(PlayerSettingsManager.TurnType.ContinuousTurn);
                break;
            case 0:
            default:
                PlayerSettingsManager.I.DropdownItemSelected(PlayerSettingsManager.TurnType.SnapTurn);
                break;
        }
    }
}
