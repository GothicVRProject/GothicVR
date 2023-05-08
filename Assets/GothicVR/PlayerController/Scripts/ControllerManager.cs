using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class ControllerManager : MonoBehaviour
{ 
    public GameObject raycastLeft;
    public GameObject raycastRight;
    public GameObject directLeft;
    public GameObject directRight;
    //public GameObject healthBar;
    //public GameObject manaBar;
    //public GameObject inventoryBag; was removed from game
    //public GameObject quickAccessSlots;
    //public GameObject mainMenue;
    //public GameObject inventory;
   // private InventoryClose inventoryCloseScript;
    public bool raycastActive = false;
    private InputDeviceCharacteristics leftControllerCharacteristic = InputDeviceCharacteristics.Left;
    private InputDeviceCharacteristics rightControllerCharacteristic = InputDeviceCharacteristics.Right;

    private InputDevice leftController;
    private InputDevice rightController;


    private void Start()
    {
        TryInitialize();
        // inventoryCloseScript =  inventory.gameObject.GetComponent<InventoryClose>();
    }

    private void Update()
    {
        if (!leftController.isValid && !rightController.isValid)
        {
            //Try initializing controllers until they are valid
            TryInitialize();
        }
        else
        {
            //Use the first configuration set of controller buttons
            ControllerManager_v1();
        }


    }

    void ControllerManager_v1()
    {
        leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool leftprimarybuttonvalue);
        leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool leftsecondarybuttonvalue);
        rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool rightprimarybuttonvalue);
        rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool rightsecondarybuttonvalue);

        //Left Controller - Primary button
        if (leftprimarybuttonvalue == true)
        {
            ShowRayCasts();
        }
        else
        {
            HideRayCasts();
        }

        //Left Controller - Secondary button
        if (leftsecondarybuttonvalue == true)
        {
            ShowUI();
        }
        else
        {
            HideUI();
        }

        //Right Controller - Primary button
        if (rightprimarybuttonvalue == true)
        {
            ShowInventory();
        }
        else
        {

        }

        //Right Controller - Secondary button
        if (rightsecondarybuttonvalue == true)
        {
            ShowMainMenue();
        }
        else
        {

        }
    }

    public void ShowUI()
    {
        //manaBar.SetActive(true);
        //healthBar.SetActive(true);
        //if (quickAccessSlots)
        //quickAccessSlots.SetActive(true);

    }

    public void HideUI()
    {
        //if (healthBar.gameObject.GetComponent<HealthBarFollowPlayer>().currentlyEating == false)
        //{
        //    healthBar.SetActive(false);
        //}

        //if (manaBar.gameObject.GetComponent<HealthBarFollowPlayer>().currentlyEating == false)
        //{
        //    manaBar.SetActive(false);
        //}
        //if (quickAccessSlots)
        //quickAccessSlots.SetActive(false);
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

    public void ShowMainMenue()
    {
       // mainMenue.SetActive(true);
    }

    public void HideMainMenue()
    {
       // mainMenue.SetActive(false);
    }

    public void ShowInventory()
    {
        //if (!inventory.activeSelf)
        //{
        //    inventory.gameObject.transform.parent = null;
        //    inventory.SetActive(true);
        //}
        //else
        //{
        //    // Show and hide are toggeling too fast when pressing the controller button
        //   // HideInventory();
        //}
 
    }

    public void HideInventory()
    {
        //if (inventoryCloseScript != null)
        //{
        //    inventoryCloseScript.closeFunction();
        //}
    }

  

      //  IEnumerator (float delay)
    void TryInitialize()
    {
        List<InputDevice> leftDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(leftControllerCharacteristic, leftDevices);
        List<InputDevice> rightDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristic, rightDevices);

        if (leftDevices.Count > 0)
        {
            leftController = leftDevices[0];
        }

        if (rightDevices.Count > 0)
        {
            rightController = rightDevices[0];
        }
    }



}

