using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using GVR.Manager;
using GVR.Player;

public class MoveSpeedController : MonoBehaviour
{
    public ActionBasedContinuousMoveProvider movecontroller;
    void Start()
    {
        Slider speedslider = transform.GetComponent<Slider>();
        speedslider.onValueChanged.AddListener(ChangeMoveSpeed);
        speedslider.value = PlayerPrefs.GetFloat(ConstantsManager.I.moveSpeedPlayerPref, ConstantsManager.I.moveSpeed);
    }

    public void ChangeMoveSpeed(float moveSpeed)
    {
        PlayerPrefs.SetFloat(ConstantsManager.I.moveSpeedPlayerPref, moveSpeed);
        ConstantsManager.I.moveSpeed = moveSpeed;

        if (!movecontroller)
            return;

        movecontroller.moveSpeed = moveSpeed;
        MovementTypeController.UpdateSpeedVariable(movespeed);
    }
}
