using UnityEngine;

namespace UZVR.Npc
{
    public abstract class BasePlayerBehaviour: MonoBehaviour
    {

        protected Properties Properties;

        private void Start()
        {
            Properties = GetComponent<Properties>();
        }
    }
}
