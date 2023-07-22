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
    void Start()
    {
        Slider speedslider = transform.GetComponent<Slider>();
        speedslider.onValueChanged.AddListener(ChangeMoveSpeed);
        speedslider.value = PlayerPrefs.GetFloat("MoveSpeed", 8f);
    }

    void ChangeMoveSpeed(float movespeed)
    {
        movecontroller.moveSpeed = movespeed;
        PlayerPrefs.SetFloat("MoveSpeed", movespeed);
    }
}
