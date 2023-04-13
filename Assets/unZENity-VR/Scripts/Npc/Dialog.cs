using UnityEngine;

namespace UZVR.Npc
{
    public class Dialog: BasePlayerBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Player"))
                return;

            Debug.Log("Player collission");

            // TODO Call dialog
        }
    }
}
