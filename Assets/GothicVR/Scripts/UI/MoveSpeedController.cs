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
        speedslider.value = PlayerPrefs.GetFloat(ConstantsManager.moveSpeedPlayerPref, ConstantsManager.moveSpeed);
    }

    public void ChangeMoveSpeed(float moveSpeed)
    {
        PlayerPrefs.SetFloat(ConstantsManager.moveSpeedPlayerPref, moveSpeed);
        ConstantsManager.moveSpeed = moveSpeed;

        if (!movecontroller)
            return;

        movecontroller.moveSpeed = moveSpeed;
    }
}
