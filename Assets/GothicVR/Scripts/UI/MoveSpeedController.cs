using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class MoveSpeedController : MonoBehaviour
{
    public ActionBasedContinuousMoveProvider movecontroller;
    private const string moveSpeedPlayerPref = "MoveSpeed";
    void Start()
    {
        Slider speedslider = transform.GetComponent<Slider>();
        speedslider.onValueChanged.AddListener(ChangeMoveSpeed);
        speedslider.value = PlayerPrefs.GetFloat(moveSpeedPlayerPref, 8f);
    }

    void ChangeMoveSpeed(float movespeed)
    {
        movecontroller.moveSpeed = movespeed;
        PlayerPrefs.SetFloat(moveSpeedPlayerPref, movespeed);
    }
}
