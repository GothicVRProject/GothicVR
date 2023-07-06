using System;
using UnityEngine.XR.Interaction.Toolkit;

namespace GothicVR.Player
{
    /// <summary>
    /// @see https://medium.com/@dnwesdman/climbing-in-vr-with-the-xr-interaction-toolkit-164f6b381ed9
    /// @see https://fistfullofshrimp.com/unity-vr-climbing/
    /// </summary>
    public class XRDirectClimbInteractor : XRDirectInteractor
    {
        public static event Action<string> ClimbHandActivated;
        public static event Action<string> ClimbHandDeactivated;

        private string controllerName;

        protected override void Start()
        {
            base.Start();
            controllerName = gameObject.name;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (args.interactableObject.transform.gameObject.CompareTag("Climbable"))
            {
                ClimbHandActivated?.Invoke(controllerName);
            }
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            ClimbHandDeactivated?.Invoke(controllerName);
        }
    }
}