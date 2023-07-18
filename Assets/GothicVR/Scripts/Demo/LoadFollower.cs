using UnityEngine;

public class LogoFollower : MonoBehaviour
{
    public Transform target; // Reference to the main camera
    public float smoothness = 0.1f; // Smoothing factor for the logo movement
    public float followDelay = 0.5f; // Delay between the logo and the main camera

    private Vector3 initialOffset; // Initial offset between the logo and the camera
    private Vector3 velocity = Vector3.zero; // Velocity for smoothing the movement

    private void Start()
    {
        // Calculate and store the initial offset between the logo and the camera
        initialOffset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        // Calculate the target position with a delay
        Vector3 targetPosition = target.position + (target.forward * followDelay);

        // Calculate the center position of the camera's view
        Vector3 cameraCenterPosition = targetPosition;

        // Calculate the final position of the logo based on the center position and the initial offset
        Vector3 logoPosition = cameraCenterPosition + Quaternion.Euler(target.eulerAngles.x, target.eulerAngles.y, 0f) * initialOffset;

        // Smoothly move the logo towards the final position
        transform.position = Vector3.SmoothDamp(transform.position, logoPosition, ref velocity, smoothness);

        // Rotate the logo to face the camera
        transform.LookAt(target);
        transform.Rotate(0f, 0f, 180f);
    }
}
