using GVR.Manager;
using UnityEngine;

namespace GVR.Phoenix.Util
{
    public class ChangeLevelTriggerHandler : MonoBehaviour
    {
        public string levelName;
        public string startVob;
        
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;
            
            GvrSceneManager.I.LoadWorld(levelName.Split(".")[0], startVob.Trim());
        }

    }
}
