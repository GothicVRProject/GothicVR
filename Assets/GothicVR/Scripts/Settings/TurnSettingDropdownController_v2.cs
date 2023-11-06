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

        dropdown.value = RuntimeSettings.LoadSettingsFromPlayerPrefs(ConstantsManager.turnSettingPlayerPref);
        DropdownItemSelected(dropdown.value);
    }

    public void DropdownItemSelected(int value)
    {
        switch (value)
        {
            case 1:
                RuntimeSettings.I.DropdownItemSelected(RuntimeSettings.TurnType.ContinuousTurn);
                break;
            case 0:
            default:
                RuntimeSettings.I.DropdownItemSelected(RuntimeSettings.TurnType.SnapTurn);
                break;
        }
    }
}
