using GVR.Context.Controls;
using GVR.Globals;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GVR.OXR
{
    public class OXRClimbingAdapter : IClimbingAdapter
    {
        public void AddClimbingComponent(GameObject go)
        {
            // We will set some default values for collider and grabbing now.
            // Adding it now is easier than putting it on a prefab and updating it at runtime (as grabbing didn't work this way out-of-the-box).
            // e.g. grabComp's colliders aren't recalculated if we have the XRGrabInteractable set in Prefab.
            var grabComp = go.AddComponent<XRGrabInteractable>();
            var rigidbodyComp = go.GetComponent<Rigidbody>();
            var meshColliderComp = go.GetComponentInChildren<MeshCollider>();

            meshColliderComp.convex = true; // We need to set it to overcome Physics.ClosestPoint warnings.
            go.tag = Constants.ClimbableTag;
            rigidbodyComp.isKinematic = true;
            grabComp.throwOnDetach = false; // Throws errors and isn't needed as we don't want to move the kinematic ladder when released.
            grabComp.trackPosition = false;
            grabComp.trackRotation = false;
            grabComp.selectMode = InteractableSelectMode.Multiple; // With this, we can grab with both hands!
        }
    }
}
