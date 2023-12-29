using GVR.Globals;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class MoveSpeedController : MonoBehaviour
{
    public ActionBasedContinuousMoveProvider movecontroller;
    void Start()
    {
        Slider speedslider = transform.GetComponent<Slider>();
        speedslider.onValueChanged.AddListener(ChangeMoveSpeed);
        speedslider.value = PlayerPrefs.GetFloat(Constants.moveSpeedPlayerPref, Constants.moveSpeed);
    }

    public void ChangeMoveSpeed(float moveSpeed)
    {
        PlayerPrefs.SetFloat(Constants.moveSpeedPlayerPref, moveSpeed);
        Constants.moveSpeed = moveSpeed;

        if (!movecontroller)
            return;

        movecontroller.moveSpeed = moveSpeed;
    }
}
