using GVR.Manager.Culling;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Manipulation;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GVR.Vob
{
    [RequireComponent(typeof(Rigidbody))]
    public class ItemGrabInteractable : UxrGrabbableObjectComponent<ItemGrabInteractable>
	{
        public GameObject attachPoint1;
        public GameObject attachPoint2;

        public Rigidbody rb;

		protected override void Start()
        {
            base.Start();
            rb = GetComponent<Rigidbody>();
        }

		protected override void OnObjectGrabbed(UxrManipulationEventArgs e)
		{
			Debug.Log($"Object {e.GrabbableObject.name} was grabbed by avatar {e.Grabber.Avatar.name}");
			VobMeshCullingManager.I.StartTrackVobPositionUpdates(gameObject);
		}

		protected override void OnObjectReleased(UxrManipulationEventArgs e)
		{
			Debug.Log($"Object {e.GrabbableObject.name} was released by avatar {e.Grabber.Avatar.name}");
			VobMeshCullingManager.I.StopTrackVobPositionUpdates(gameObject);
		}


        //public void SelectEntered(SelectEnterEventArgs args)
        //{
        //    VobMeshCullingManager.I.StartTrackVobPositionUpdates(gameObject);
        //}

        ///// <summary>
        ///// Activate physics on object immediately after it's stopped being grabbed
        ///// </summary>
        //public void SelectExited(SelectExitEventArgs args)
        //{
        //    if (rb.isKinematic)
        //        rb.isKinematic = false;
        //    VobMeshCullingManager.I.StopTrackVobPositionUpdates(gameObject);
        //}
    }
}
