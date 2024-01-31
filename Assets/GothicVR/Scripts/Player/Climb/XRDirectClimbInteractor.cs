using GVR.Globals;
using UnityEngine;
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
        public static UnityEvent<string, GameObject> ClimbHandActivated = new();
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

            var objTransform = args.interactableObject.transform;

            if (objTransform.transform.CompareTag(Constants.ClimbableTag))
                ClimbHandActivated.Invoke(controllerName, objTransform.gameObject);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            if (args.interactableObject.transform.CompareTag(Constants.ClimbableTag))
                ClimbHandDeactivated.Invoke(controllerName);
        }
    }
}
