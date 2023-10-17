using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TeleportationController : MonoBehaviour
{
    public GameObject baseControllerGameObject;
    public GameObject teleportationGameObject;
    public GameObject player;
    public InputActionReference teleportActivationReference;
    [Space]
    public UnityEvent onTeleportActivate;
    public UnityEvent onTeleportCanceled;

    private void Start()
    {
        teleportActivationReference.action.performed += TeleportModeActivate;
        teleportActivationReference.action.canceled += TeleportModeCancel;
    }
    
    private void TeleportModeActivate(InputAction.CallbackContext obj)
    {
        onTeleportActivate.Invoke();
    }
    
    void DeactivateTeleporter()
    {
        onTeleportCanceled.Invoke();
    }
    
    private void TeleportModeCancel(InputAction.CallbackContext obj)
    {
        Invoke(nameof(DeactivateTeleporter),.1f);
    }

    private void OnDestroy()
    {
        teleportActivationReference.action.performed -= TeleportModeActivate;
        teleportActivationReference.action.canceled -= TeleportModeCancel;
    }
}