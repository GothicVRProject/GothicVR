using UnityEngine;
using UnityEngine.InputSystem;
using GVR.Manager;

public class ControllerManager : MonoBehaviour
{
    public GameObject raycastLeft;
    public GameObject raycastRight;
    public GameObject directLeft;
    public GameObject directRight;
    public GameObject MenuGameObject;
    public GameObject MapObject;

    private InputAction leftPrimaryButtonAction;
    private InputAction leftSecondaryButtonAction;

    private InputAction rightPrimaryButtonAction;
    private InputAction rightSecondaryButtonAction;

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

        rightPrimaryButtonAction.started += ctx => ShowMap();
        rightSecondaryButtonAction.started += ctx => ShowMainMenu();

        rightPrimaryButtonAction.Enable();
        rightSecondaryButtonAction.Enable();
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

    public void ShowMainMenu()
    {
        if (!MenuGameObject.activeSelf)
            MenuGameObject.SetActive(true);
        else
            MenuGameObject.SetActive(false);
    }

    public void ShowMap()
    {
        if (!MapObject.activeSelf)
            MapObject.SetActive(true);
        else
            MapObject.SetActive(false);
    }

    public void ShowInventory()
    {
    }
}
