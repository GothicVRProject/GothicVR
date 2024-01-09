using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Globals;
using GVR.GothicVR.Scripts.Manager;
using UnityEngine;
using ZenKit.Daedalus;

namespace GVR.Npc
{
    public class Dialog: BasePlayerBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            DialogHelper.StartDialog(properties);
        }
    }
}
