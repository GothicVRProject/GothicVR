using GVR.Properties;
using UnityEngine;

namespace GVR.Npc
{
    public abstract class BasePlayerBehaviour: MonoBehaviour
    {
        protected NpcProperties Properties;

        private void Start()
        {
            Properties = GetComponent<NpcProperties>();
        }
    }
}
