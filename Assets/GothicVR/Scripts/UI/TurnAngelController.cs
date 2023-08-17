using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using GVR.Manager;

public class TurnAngleController : MonoBehaviour
{
    public ActionBasedSnapTurnProvider snapturnprovider;
    public ActionBasedContinuousTurnProvider continuousturnprovider;
    void Start()
    {
        Slider turnangelslider = transform.GetComponent<Slider>();
        turnangelslider.onValueChanged.AddListener(ChangeTurnAngle);
        turnangelslider.value = PlayerPrefs.GetFloat(ConstantsManager.I.turnAnglePlayerPref, ConstantsManager.I.turnAngleDefault);
    }

    void ChangeTurnAngle(float turn_angel)
    {
        snapturnprovider.turnAmount = turn_angel;
        continuousturnprovider.turnSpeed = turn_angel;
        PlayerPrefs.SetFloat(ConstantsManager.I.turnAnglePlayerPref, turn_angel);
    }
}
