using UnityEngine;
using UnityEngine.InputSystem;
using GVR.Manager;

public class ControllerManager : MonoBehaviour
{
    public GameObject raycastLeft;
    public GameObject raycastRight;
    public GameObject directLeft;
    public GameObject directRight;
    public GameObject settingsMenue;
    public GameObject teleportMenu;

    public bool uiFollowsMe;

    private InputAction leftPrimaryButtonAction;
    private InputAction leftSecondaryButtonAction;

    private InputAction rightPrimaryButtonAction;
    private InputAction rightSecondaryButtonAction;

    private CloseMenueScript settingsMenuCloseScript;
    private CloseMenueScript teleportMenuCloseScript;

    private void Awake()
    {
        leftPrimaryButtonAction = new InputAction("primaryButton", binding: "<XRController>{LeftHand}/primaryButton");
        leftSecondaryButtonAction = new InputAction("secondaryButton", binding: "<XRController>{LeftHand}/secondaryButton");

        leftPrimaryButtonAction.started += ctx => ShowRayCasts();
        leftPrimaryButtonAction.canceled += ctx => HideRayCasts();

        leftPrimaryButtonAction.Enable();
        leftSecondaryButtonAction.Enable();

        rightPrimaryButtonAction = new InputAction("primaryButton", binding: "<XRController>{RightHand}/primaryButton");
        rightSecondaryButtonAction = new InputAction("secondaryButton", binding: "<XRController>{RightHand}/secondaryButton");

        rightPrimaryButtonAction.started += ctx => ShowSettingsMenu();

        rightSecondaryButtonAction.started += ctx => ShowTeleportMenu();

        rightPrimaryButtonAction.Enable();
        rightSecondaryButtonAction.Enable();

        settingsMenuCloseScript = (CloseMenueScript)settingsMenue.GetComponent(typeof(CloseMenueScript));
        teleportMenuCloseScript = (CloseMenueScript)teleportMenu.GetComponent(typeof(CloseMenueScript));
    }

    private void OnDestroy()
    {
        leftPrimaryButtonAction.Disable();
        leftSecondaryButtonAction.Disable();

        rightPrimaryButtonAction.Disable();
        rightSecondaryButtonAction.Disable();
    }

    public void ShowRayCasts()
    {
        raycastLeft.SetActive(true);
        raycastRight.SetActive(true);
        directLeft.SetActive(false);
        directRight.SetActive(false);
    }

    public void HideRayCasts()
    {
        raycastLeft.SetActive(false);
        raycastRight.SetActive(false);
        directLeft.SetActive(true);
        directRight.SetActive(true);
    }

    public void ShowSettingsMenu()
    {
        if (!settingsMenue.activeSelf)
        {
            if (!uiFollowsMe)
            {
                settingsMenue.transform.parent = null;
            }
            settingsMenue.SetActive(true);
            FontManager.I.ChangeFont();

            if (teleportMenu.activeSelf)
            {
                if (!uiFollowsMe)
                {
                    teleportMenuCloseScript.CloseFunction();
                }
                else
                {
                    teleportMenu.SetActive(false);
                }
            }
        }
        else
        {
            if (!uiFollowsMe)
            {
                settingsMenuCloseScript.CloseFunction();
            }
            else
            {
                settingsMenue.SetActive(false);
            }
        }
    }

    public void ShowTeleportMenu()
    {
        if (!teleportMenu.activeSelf)
        {
            if (!uiFollowsMe)
            {
                teleportMenu.transform.parent = null;
            }
            teleportMenu.SetActive(true);
            FontManager.I.ChangeFont();

            if (settingsMenue.activeSelf)
            {
                if (!uiFollowsMe)
                {
                    settingsMenuCloseScript.CloseFunction();
                }
                else
                {
                    settingsMenue.SetActive(false);
                }
            }
        }
        else
        {
            if (!uiFollowsMe)
            {
                teleportMenuCloseScript.CloseFunction();
            }
            else
            {
                teleportMenu.SetActive(false);
            }
        }
    }
    public void ShowUI()
    {
    }

    public void HideUI()
    {
    }

    public void ShowMainMenue()
    {
    }

    public void HideMainMenue()
    {
    }

    public void ShowInventory()
    {
    }

    public void HideInventory()
    {
    }
}
