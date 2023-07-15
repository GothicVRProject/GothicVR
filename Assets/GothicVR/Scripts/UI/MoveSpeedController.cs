using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public class MoveSpeedController : MonoBehaviour
{
    public ActionBasedContinuousMoveProvider movecontroller;

    // Start is called before the first frame update
    void Start()
    {
        Slider speedslider = transform.GetComponent<Slider>();
        speedslider.onValueChanged.AddListener(ChangeMoveSpeed);
 
    }

    // Update is called once per frame
    void ChangeMoveSpeed(float movespeed)
    {
        
        movecontroller.moveSpeed = movespeed;
    }
}
