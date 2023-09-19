using GVR.Manager;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GVR.Vob
{
    [RequireComponent(typeof(Rigidbody))]
    public class ItemGrabInteractable : MonoBehaviour
    {
        private bool alreadyGrabbedOnce;
        
        public void SelectEntered(SelectEnterEventArgs args)
        {
            CullingGroupManager.I.StartTrackVobPositionUpdates(gameObject);
        }
        
        /// <summary>
        /// Activate physics on object immediately after it's stopped being grabbed for the first time.
        /// </summary>
        public void SelectExited(SelectExitEventArgs args)
        {
            // FIXME - move this to another method which is checking "If grabbed before and now moveVector is 0, then stop positioning."
            CullingGroupManager.I.StopTrackVobPositionUpdates(gameObject);

            if (alreadyGrabbedOnce)
                return;

            GetComponent<Rigidbody>().isKinematic = false;
            alreadyGrabbedOnce = true;
            
        }
    }
}