using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace GothicVR.Player.Climb
{
    public class ClimbProvider : MonoBehaviour
    {
        public static event Action ClimbActive;
        public static event Action ClimbInActive;

        public CharacterController characterController;
        public InputActionProperty velocityRight;
        public InputActionProperty velocityLeft;

        private bool rightActive = false;
        private bool leftActive = false;

        private float originalMovementSpeed;
        private float originalTurnSpeed;

        private void Start()
        {
            XRDirectClimbInteractor.ClimbHandActivated += HandActivated;
            XRDirectClimbInteractor.ClimbHandDeactivated += HandDeactivated;

            originalMovementSpeed = transform.GetComponent<ActionBasedContinuousMoveProvider>().moveSpeed;

            originalTurnSpeed = transform.GetComponent<ContinuousTurnProviderBase>().turnSpeed;
        }

        private void OnDestroy()
        {
            XRDirectClimbInteractor.ClimbHandActivated -= HandActivated;
            XRDirectClimbInteractor.ClimbHandDeactivated -= HandDeactivated;
        }

        private void HandActivated(string controllerName)
        {
            if (controllerName == "LeftHandBaseController")
            {
                leftActive = true;
                rightActive = false;
            }
            else
            {
                leftActive = false;
                rightActive = true;
            }
            DeactivateMovement();
            ClimbActive?.Invoke();
        }

        private void HandDeactivated(string controllerName)
        {

            if (rightActive && controllerName == "RightHandBaseController")
            {
                rightActive = false;
                ActivateMovement();
                ClimbInActive?.Invoke();
            }
            else if (leftActive && controllerName == "LeftHandBaseController")
            {
                leftActive = false;
                ActivateMovement();
                ClimbInActive?.Invoke();
            }
        }

        private void FixedUpdate()
        {
            if (rightActive || leftActive)
            {
                Climb();
            }
        }

        /// <summary>
        /// Activates Gravity, movement and turn options
        /// </summary>
        private void ActivateMovement()
        {
            // Reactivate gravity and speed to original speed
            transform.GetComponent<ActionBasedContinuousMoveProvider>().useGravity = true;
            transform.GetComponent<ActionBasedContinuousMoveProvider>().moveSpeed = originalMovementSpeed;

            // In case of using Continuous turn, reactivate turn speed
            transform.GetComponent<ContinuousTurnProviderBase>().turnSpeed = originalTurnSpeed;

            // In case of using Snap Turn, reenable turn
            transform.GetComponent<SnapTurnProviderBase>().enableTurnLeftRight = true;
            transform.GetComponent<SnapTurnProviderBase>().enableTurnAround = true;
        }

        /// <summary>
        /// Deactivates Gravity, movement and turn options
        /// </summary>
        private void DeactivateMovement()
        {
            // Set gravity to false and speed to 0
            transform.GetComponent<ActionBasedContinuousMoveProvider>().useGravity = false;
            transform.GetComponent<ActionBasedContinuousMoveProvider>().moveSpeed = 0;

            // In case of using Continuous turn , set the turn speed to 0
            transform.GetComponent<ContinuousTurnProviderBase>().turnSpeed = 0;

            // In case of using Snap Turn, set turn to false
            transform.GetComponent<SnapTurnProviderBase>().enableTurnLeftRight = false;
            transform.GetComponent<SnapTurnProviderBase>().enableTurnAround = false;
        }

        private void Climb()
        {
            Vector3 velocity = leftActive ? velocityLeft.action.ReadValue<Vector3>() : velocityRight.action.ReadValue<Vector3>();

            characterController.Move(characterController.transform.rotation * -velocity * Time.fixedDeltaTime);
        }
    }
}