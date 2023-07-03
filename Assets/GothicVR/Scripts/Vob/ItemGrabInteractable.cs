using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GothicVR.Vob
{
    [RequireComponent(typeof(Rigidbody))]
    public class ItemGrabInteractable : MonoBehaviour
    {
        private bool alreadyGrabbedOnce;
        
        /// <summary>
        /// Activate physics on object immediately after it's stopped being grabbed for the first time.
        /// </summary>
        public void SelectExited(SelectExitEventArgs args)
        {
            if (alreadyGrabbedOnce)
                return;

            GetComponent<Rigidbody>().isKinematic = false;
            alreadyGrabbedOnce = true;
        }
    }
}