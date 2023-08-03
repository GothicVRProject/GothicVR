using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using GVR.Manager;

namespace GVR.Player
{
    public class MovementTypeController : MonoBehaviour
    {
        //No UI icon for now 
        //public Canvas runningCanvas;
        public GameObject leftHand;
        public GameObject rightHand;
        public CharacterController character;

        public static float walking_speed;
        public static float running_speed;
        private bool _runClickHandled = false;

        private Vector3 previousPosLeft;
        private Vector3 previousPosRight;

        private const float MIN_VELOCITY_ARM_SWING_ACTIVATE = 0.02f;
        private bool _armSwingActive = false;

        private ActionBasedContinuousMoveProvider _locomotionProvider;

        private void Start()
        {
            _locomotionProvider = GetComponent<ActionBasedContinuousMoveProvider>();
            UpdateSpeedVariable(PlayerPrefs.GetFloat(ConstantsManager.I.moveSpeedPlayerPref, 8f));
           

            SetWalkingSpeed(walking_speed);
            SetPreviousArmPosition();
        }

        // TODO: WalkingSpeed after Thumbstick click is too fast.
        // TODO: Implement ease mechanism for hand velocity (Run icon is triggered permanently on/off if not eased).
        private void Update()
        {
            CalculateArmSwing();
            SetMovementSpeed();
        }

        public static void UpdateSpeedVariable(float speed)
        {
            walking_speed = speed;
            running_speed = speed * 2f;
        }


        // Credits: https://www.youtube.com/watch?v=Eipi6rNPz9U
        private void CalculateArmSwing()
        {
            var leftClicked = GetButtonClicked(InputDeviceCharacteristics.Left, CommonUsages.gripButton);
            var rightClicked = GetButtonClicked(InputDeviceCharacteristics.Right, CommonUsages.gripButton);
            var otherMovementClicked = GetButtonClicked(InputDeviceCharacteristics.Left, CommonUsages.primary2DAxis);

            // Exclude normal walking from ArmLocomotion.
            if (!leftClicked || !rightClicked || otherMovementClicked)
            {
                return;
            }

            Vector3 leftHandVelocity = leftHand.transform.position - previousPosLeft;
            Vector3 rightHandVelocity = rightHand.transform.position - previousPosRight;
            float totalVelocity = leftHandVelocity.magnitude + rightHandVelocity.magnitude;

            if (totalVelocity >= MIN_VELOCITY_ARM_SWING_ACTIVATE)
            {
                if (!_armSwingActive)
                {
                    SetRunningSpeed(running_speed);
                    _armSwingActive = true;
                }

                _armSwingActive = true;
                var direction = Camera.main.transform.forward;
                character.Move(running_speed * Time.deltaTime * Vector3.ProjectOnPlane(direction, Vector3.up));
            }
            else if (_armSwingActive)
            {
                SetWalkingSpeed(walking_speed);
                _armSwingActive = false;
            }

            SetPreviousArmPosition();
        }

        private void SetPreviousArmPosition()
        {
            previousPosLeft = leftHand.transform.position;
            previousPosRight = rightHand.transform.position;
        }

        // @see XR Input mappings https://docs.unity3d.com/Manual/xr_input.html
        private void SetMovementSpeed()
        {
            var clicked = GetButtonClicked(InputDeviceCharacteristics.Left, CommonUsages.primary2DAxisClick);

            // nothing's clicked?
            if (!clicked)
            {
                _runClickHandled = false;
                return;
            }

            // Click was already handled?
            if (_runClickHandled)
            {
                return;
            }

            // Toggle speed on every button press.

            if (_locomotionProvider.moveSpeed == walking_speed)
            {
                SetRunningSpeed(running_speed);
            }
            else
            {
                SetWalkingSpeed(walking_speed);
            }

            // Do not execute speed change for the duration of this click again.
            _runClickHandled = true;
        }

        private void SetWalkingSpeed(float speed)
        {
            _locomotionProvider.moveSpeed = speed;
            //runningCanvas.enabled = false;
        }

        private void SetRunningSpeed(float speed)
        {
            _locomotionProvider.moveSpeed = speed;
            //runningCanvas.enabled = true;
        }

        private InputDevice? GetController(InputDeviceCharacteristics characteristic)
        {
            var gameControllers = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(characteristic, gameControllers);

            if (!gameControllers.Any())
            {
                return null;
            }
            else
            {
                return gameControllers[0];
            }
        }

        private bool GetButtonClicked(InputDeviceCharacteristics characteristic, InputFeatureUsage<bool> button)
        {
            var controller = GetController(characteristic);

            if (!controller.HasValue)
            {
                return false;
            }
            else
            {
                return GetButtonClicked(controller.Value, button);
            }
        }

        private bool GetButtonClicked(InputDevice controller, InputFeatureUsage<bool> button)
        {
            bool clicked;
            controller.TryGetFeatureValue(button, out clicked);

            return clicked;
        }

        private bool GetButtonClicked(InputDeviceCharacteristics characteristic, InputFeatureUsage<Vector2> button)
        {
            var controller = GetController(characteristic);

            if (!controller.HasValue)
            {
                return false;
            }
            else
            {
                return GetButtonClicked(controller.Value, button);
            }
        }

        private bool GetButtonClicked(InputDevice controller, InputFeatureUsage<Vector2> button)
        {
            Vector2 clicked;
            controller.TryGetFeatureValue(button, out clicked);

            return !clicked.Equals(Vector2.zero);
        }
    }
}