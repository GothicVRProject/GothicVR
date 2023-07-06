using System;
using UnityEngine;
using UnityEngine.InputSystem;

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

        private void Start()
        {
            XRDirectClimbInteractor.ClimbHandActivated += HandActivated;
            XRDirectClimbInteractor.ClimbHandDeactivated += HandDeactivated;
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

            ClimbActive?.Invoke();
        }
    
        private void HandDeactivated(string controllerName)
        {

            if (rightActive && controllerName == "RightHand Controller")
            {
                rightActive = false;
                ClimbInActive?.Invoke();
            }
            else if (leftActive && controllerName == "LeftHandBaseController")
            {
                leftActive = false;
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

        private void Climb()
        {
            Vector3 velocity = leftActive ? velocityLeft.action.ReadValue<Vector3>() : velocityRight.action.ReadValue<Vector3>();

            characterController.Move(characterController.transform.rotation * -velocity * Time.fixedDeltaTime);
        }
    }
}