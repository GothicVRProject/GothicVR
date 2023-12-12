using System;
using UnityEngine;

namespace GVR.Npc
{
    public class Dialog: BasePlayerBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            Debug.Log("Player collission");

            // TODO Call dialog

        }
    }
}
