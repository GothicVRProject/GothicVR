using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GVR.Phoenix.Util;

public class RayTeleportClimb : MonoBehaviour
{
    [SerializeField] private XRInteractorLineVisual lineVisual;
    [SerializeField] private XRBaseInteractor interactor;
    [SerializeField] private GameObject player;

    private bool isHittingObject = false;
    private GameObject hitObject;
    private Vector3 zsPos0Position;
    private Vector3 zsPos1Position;
    private float hitTime;
    private float teleportDelay = 0.5f; // Adjust the delay duration as needed

    private void Start()
    {
        interactor.selectEntered.AddListener(OnRaycastHit);
        interactor.selectExited.AddListener(OnRaycastExit);
    }

    private void OnRaycastHit(SelectEnterEventArgs args)
    {
        // Check if the interactable GameObject has the tag "Climbable"
        if (args.interactableObject != null && args.interactableObject.transform.CompareTag("Climbable"))
        {
            // Show a message in the logs
            hitObject = args.interactableObject.transform.gameObject;

            // Get the zs_pos0 and zs_pos1 positions
            zsPos0Position = hitObject.FindChildRecursively("ZS_POS0").transform.position;
            zsPos1Position = hitObject.FindChildRecursively("ZS_POS1").transform.position;

            isHittingObject = true;

            // Record the hit time
            hitTime = Time.time;
        }
    }

    private void OnRaycastExit(SelectExitEventArgs args)
    {
        isHittingObject = false;
    }

    private void Update()
    {
        if (isHittingObject)
        {
            // Check if the delay duration has passed since the hit
            if (Time.time - hitTime >= teleportDelay)
            {
                PerformTeleport();
            }
        }
    }

    private void PerformTeleport()
    {
        // Get the player's position
        Vector3 playerPosition = player.transform.position;

        float yDifferenceToZsPos0 = Mathf.Abs(playerPosition.y - zsPos0Position.y);
        float yDifferenceToZsPos1 = Mathf.Abs(playerPosition.y - zsPos1Position.y);

        // Teleport the player to the closer zs_pos position based on y-level
        if (yDifferenceToZsPos0 < yDifferenceToZsPos1)
        {
            TeleportPlayer(zsPos1Position);
        }
        else
        {
            TeleportPlayer(zsPos0Position);
        }

        // Reset the state
        isHittingObject = false;

        // Deactivate the teleport ray
        interactor.enabled = false;
    }

    private void TeleportPlayer(Vector3 targetPosition)
    {
        // Teleport the player to the target position
        player.transform.position = targetPosition;
    }
}
