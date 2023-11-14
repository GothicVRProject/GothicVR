using GVR.Manager.Culling;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GVR.Vob
{
    [RequireComponent(typeof(Rigidbody))]
    public class ItemGrabInteractable : MonoBehaviour
    {
        public GameObject attachPoint1;
        public GameObject attachPoint2;
        
        private bool alreadyGrabbedOnce;
        
        public void SelectEntered(SelectEnterEventArgs args)
        {
            VobMeshCullingManager.I.StartTrackVobPositionUpdates(gameObject);
        }
        
        /// <summary>
        /// Activate physics on object immediately after it's stopped being grabbed for the first time.
        /// </summary>
        public void SelectExited(SelectExitEventArgs args)
        {
            VobMeshCullingManager.I.StopTrackVobPositionUpdates(gameObject);
            
            if (alreadyGrabbedOnce)
                return;

            GetComponent<Rigidbody>().isKinematic = false;
            alreadyGrabbedOnce = true;
        }
    }
}
