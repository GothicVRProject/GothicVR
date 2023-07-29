using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using GVR.Manager;

public class MoveSpeedController : MonoBehaviour
{
    public ActionBasedContinuousMoveProvider movecontroller;
    void Start()
    {
        Slider speedslider = transform.GetComponent<Slider>();
        speedslider.onValueChanged.AddListener(ChangeMoveSpeed);
        speedslider.value = PlayerPrefs.GetFloat(ConstantsManager.I.moveSpeedPlayerPref, 8f);
    }

    void ChangeMoveSpeed(float movespeed)
    {
        movecontroller.moveSpeed = movespeed;
        PlayerPrefs.SetFloat(ConstantsManager.I.moveSpeedPlayerPref, movespeed);
    }
}
