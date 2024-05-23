using GVR.Manager.Culling;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GVR.OXR.Components.Vobs
{
    [RequireComponent(typeof(Rigidbody))]
    public class OXRItemGrabInteractable : MonoBehaviour
    {
        public Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void SelectEntered(SelectEnterEventArgs args)
        {
            VobMeshCullingManager.I.StartTrackVobPositionUpdates(gameObject);
        }

        /// <summary>
        /// Activate physics on object immediately after it's stopped being grabbed
        /// </summary>
        public void SelectExited(SelectExitEventArgs args)
        {
            if (rb.isKinematic)
                rb.isKinematic = false;
            VobMeshCullingManager.I.StopTrackVobPositionUpdates(gameObject);
        }
    }
}
