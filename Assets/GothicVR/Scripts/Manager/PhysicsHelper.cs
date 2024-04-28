using GVR.Properties;
using UnityEngine;

namespace GVR.Manager
{
    public static class PhysicsHelper
    {
        public static void DisablePhysicsForNpc(NpcProperties props)
        {
            props.colliderRootMotion.GetComponent<Rigidbody>().isKinematic = true;
        }

        public static void EnablePhysicsForNpc(NpcProperties props)
        {
            props.colliderRootMotion.GetComponent<Rigidbody>().isKinematic = false;
        }
    }
}
