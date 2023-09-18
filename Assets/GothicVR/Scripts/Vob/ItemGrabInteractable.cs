using GVR.Manager;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace GothicVR.Vob
{
    [RequireComponent(typeof(Rigidbody))]
    public class ItemGrabInteractable : MonoBehaviour
    {
        private bool alreadyGrabbedOnce;
        
        public void SelectEntered(SelectEnterEventArgs args)
        {
            // FIXME - We need to debug how we can get Left/Right controller information from the args.
            CullingGroupManager.I.StartTrackVobPositionUpdates(gameObject, InputDeviceCharacteristics.Left);
        }
        
        /// <summary>
        /// Activate physics on object immediately after it's stopped being grabbed for the first time.
        /// </summary>
        public void SelectExited(SelectExitEventArgs args)
        {
            CullingGroupManager.I.StopTrackVobPositionUpdates(gameObject);

            if (alreadyGrabbedOnce)
                return;

            GetComponent<Rigidbody>().isKinematic = false;
            alreadyGrabbedOnce = true;
            
        }
    }
}