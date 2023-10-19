using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace GVR.Player.Climb
{
    public class ClimbProvider : MonoBehaviour
    {
        public CharacterController characterController;
        public InputActionProperty velocityRight;
        public InputActionProperty velocityLeft;

        private bool rightActive;
        private bool leftActive;

        private float originalMovementSpeed;
        private float originalTurnSpeed;

        private void Start()
        {
            XRDirectClimbInteractor.ClimbHandActivated.AddListener(HandActivated);
            XRDirectClimbInteractor.ClimbHandDeactivated.AddListener(HandDeactivated);

            originalMovementSpeed = transform.GetComponent<ActionBasedContinuousMoveProvider>().moveSpeed;
            originalTurnSpeed = transform.GetComponent<ContinuousTurnProviderBase>().turnSpeed;
        }
        
        /// <summary>
        /// As this is called every 0.02 seconds - fixed. This offers a smoother movement than with variable fps.
        /// </summary>
        private void FixedUpdate()
        {
            if (rightActive || leftActive)
                Climb();
        }
        
        private void OnDestroy()
        {
            XRDirectClimbInteractor.ClimbHandActivated.RemoveListener(HandActivated);
            XRDirectClimbInteractor.ClimbHandDeactivated.RemoveListener(HandDeactivated);
        }
        
        private void Climb()
        {
            var velocity = Vector3.zero;
            velocity += leftActive ? velocityLeft.action.ReadValue<Vector3>() : Vector3.zero;
            velocity += rightActive ? velocityRight.action.ReadValue<Vector3>() : Vector3.zero;

            characterController.Move(characterController.transform.rotation * -velocity * Time.fixedDeltaTime);
        }
        
        /// <summary>
        /// If a hand starts grabbing a Ladder.
        /// </summary>
        private void HandActivated(string controllerName)
        {
            switch (controllerName)
            {
                case "LeftHandBaseController":
                    leftActive = true;
                    break;
                case "RightHandBaseController":
                    rightActive = true;
                    break;
                default:
                    return; // Do nothing.
            }
            
            DeactivateMovement();
        }

        /// <summary>
        /// If a hand stops grabbing a Ladder.
        /// </summary>
        private void HandDeactivated(string controllerName)
        {
            switch (controllerName)
            {
                case "LeftHandBaseController":
                    leftActive = false;
                    break;
                case "RightHandBaseController":
                    rightActive = false;
                    break;
                default:
                    return; // Do nothing.
            }

            if (leftActive || rightActive)
                return;
            
            ActivateMovement();
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
    }
}
