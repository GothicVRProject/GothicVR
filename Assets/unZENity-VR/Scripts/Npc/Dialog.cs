using UnityEngine;
using UZVR.Phoenix.Bridge.Vm.Gothic;

namespace UZVR.Npc
{
    public class Dialog: BasePlayerBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Player"))
                return;

            Debug.Log("Player collission");

            var dialogs = DialogBridge.GetSortedDialogsForNpc(Properties.DaedalusSymbolId);

            // Call dialog
        }
    }
}
