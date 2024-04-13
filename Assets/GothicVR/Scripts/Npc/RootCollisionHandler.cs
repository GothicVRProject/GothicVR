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

            // FIXME - As we handle FreePoint locking via Colliders, we can't just say "free" whenever collider is left as a rotation can create this state already.
            // FIXME - Instead we need to handle unlocking via game logic. E.g. whenever a new state starts, clear our NPCs lock setting.
            // // If NPC walks out of a FreePoint, it gets freed.
            // if (coll.gameObject.name.StartsWithIgnoreCase("FP_") &&
            //     coll.gameObject.TryGetComponent<VobSpotProperties>(out var vobSpotProperties))
            //     vobSpotProperties.fp.IsLocked = false;
        }

        /// <summary>
        /// Sometimes a currentAnimation needs this information. Sometimes it's just for a FreePoint to clear up.
        /// </summary>
        private void OnTriggerExit(Collider coll)
        {
            properties.currentAction?.OnTriggerExit(coll);

            // FIXME - As we handle FreePoint locking via Colliders, we can't just say "free" whenever collider is left as a rotation can create this state already.
            // FIXME - Instead we need to handle unlocking via game logic. E.g. whenever a new state starts, clear our NPCs lock setting.
            // // If NPC walks out of a FreePoint, it gets freed.
            // if (coll.gameObject.name.StartsWithIgnoreCase("FP_") &&
            //     coll.gameObject.TryGetComponent<VobSpotProperties>(out var vobSpotProperties))
            //     vobSpotProperties.fp.IsLocked = false;
        }

        private void Update()
        {
            // As we use legacy animations, we can't use RootMotion. We therefore need to rebuild it.

            /*
             * NPC GO hierarchy:
             *
             * root
             *  /BIP01/ <- animation root
             *    /RootCollisionHandler <- Moved with animation as inside BIP01, but physics are applied and merged to root
             *    /... <- animation bones
             */

            var collisionTransform = transform;

            // Apply physics based position change to root.
            npcRoot.transform.localPosition += collisionTransform.localPosition;

            // Empty physics based diff. Next frame physics will be recalculated.
            collisionTransform.localPosition = Vector3.zero;
        }
    }
}
