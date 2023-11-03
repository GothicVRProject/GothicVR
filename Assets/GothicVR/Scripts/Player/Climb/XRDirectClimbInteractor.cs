using System;
using GVR.Manager;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace GVR.Player.Climb
{
    /// <summary>
    /// @see https://medium.com/@dnwesdman/climbing-in-vr-with-the-xr-interaction-toolkit-164f6b381ed9
    /// @see https://fistfullofshrimp.com/unity-vr-climbing/
    /// </summary>
    public class XRDirectClimbInteractor : XRDirectInteractor
    {
        public static UnityEvent<string> ClimbHandActivated = new();
        public static UnityEvent<string> ClimbHandDeactivated = new();

        private string controllerName;

        protected override void Start()
        {
            base.Start();
            controllerName = gameObject.name;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (args.interactableObject.transform.CompareTag(ConstantsManager.ClimbableTag))
                ClimbHandActivated.Invoke(controllerName);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            if (args.interactableObject.transform.CompareTag(ConstantsManager.ClimbableTag))
                ClimbHandDeactivated.Invoke(controllerName);
        }
    }
}
