using System.Collections.Generic;
using TMPro;
using UnityEngine;
using GVR.Phoenix.Interface;
using GVR.Manager;

public class UIBehaviourDropdownController : MonoBehaviour
{
    public GameObject controllerManager;
    public GameObject settingsMenu;
    public GameObject teleportMenu;
    ControllerManager controllerManagerScript;
    CloseMenueScript settingsMenuCloseScript;
    CloseMenueScript teleportMenuCloseScript;
    void Awake()
    {
        var dropdown = transform.GetComponent<TMP_Dropdown>();
        dropdown.options.Clear();

        List<string> items = new List<string>();
        items.Add("UI stays in world");
        items.Add("UI follows me");

        foreach (var item in items)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData() { text = item });
        }
        dropdown.itemText.font = GameData.I.GothicSubtitleFont;
        dropdown.onValueChanged.AddListener(DropdownItemSelected);

        settingsMenuCloseScript = (CloseMenueScript)settingsMenu.GetComponent(typeof(CloseMenueScript));
        teleportMenuCloseScript = (CloseMenueScript)teleportMenu.GetComponent(typeof(CloseMenueScript));

        dropdown.value = PlayerPrefs.GetInt(ConstantsManager.I.uiBehaviourPlayerPref);
        DropdownItemSelected(dropdown.value);
       
    }

    void DropdownItemSelected(int value)
    { 
        switch (value)
        {
            case 1:
                if (controllerManager != null)
                settingsMenuCloseScript.CloseFunction();
                teleportMenuCloseScript.CloseFunction();
                    controllerManagerScript = (ControllerManager)controllerManager.GetComponent(typeof(ControllerManager));
                    controllerManagerScript.uiFollowsMe = true;
                break;
            case 0:
            default:
                if (controllerManager != null)
                settingsMenuCloseScript.CloseFunction();
                teleportMenuCloseScript.CloseFunction();
                    controllerManagerScript = (ControllerManager)controllerManager.GetComponent(typeof(ControllerManager));
                    controllerManagerScript.uiFollowsMe = false;
                break;
        }
    }
}
