using TMPro;
using UnityEngine;
using GVR.Debugging;
using GVR.Manager;

public class TurnSettingDropdownController_v2 : MonoBehaviour
{
    private RuntimeSettings runtimeSettings;

    void Awake()
    {

        runtimeSettings.turntype = RuntimeSettings.TurnType.ContinuousTurn;
        Debug.Log(runtimeSettings.turntype.ToString());

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
                runtimeSettings.turntype = RuntimeSettings.TurnType.ContinuousTurn;
                break;
            case 0:
            default:
                runtimeSettings.turntype = RuntimeSettings.TurnType.SnapTurn;
                break;
        }
    }
}
