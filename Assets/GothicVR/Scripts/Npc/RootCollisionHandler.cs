using GVR.Extensions;
using GVR.Properties;
using UnityEngine;

namespace GVR.Npc
{
    public class RootCollisionHandler : BasePlayerBehaviour
    {
        private void OnCollisionEnter(Collision coll)
        {
            properties.currentAction?.OnCollisionEnter(coll);
        }

        private void OnTriggerEnter(Collider coll)
        {
            properties.currentAction?.OnTriggerEnter(coll);
        }

        private void OnCollisionExit(Collision coll)
        {
            properties.currentAction?.OnCollisionExit(coll);

            // If NPC walks out of a FreePoint, it gets freed.
            if (!coll.gameObject.name.StartsWithIgnoreCase("FP_"))
                return;

            coll.gameObject.GetComponent<VobSpotProperties>().fp.IsLocked = false;
        }

        /// <summary>
        /// Sometimes a currentAnimation needs this information. Sometimes it's just for a FreePoint to clear up.
        /// </summary>
        private void OnTriggerExit(Collider coll)
        {
            properties.currentAction?.OnTriggerExit(coll);

            // If NPC walks out of a FreePoint, it gets freed.
            if (!coll.gameObject.name.StartsWithIgnoreCase("FP_"))
                return;

            coll.gameObject.GetComponent<VobSpotProperties>().fp.IsLocked = false;
        }

    }
}
