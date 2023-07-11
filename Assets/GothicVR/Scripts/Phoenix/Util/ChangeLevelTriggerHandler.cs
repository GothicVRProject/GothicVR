using UnityEngine;
using GVR.Util;
using GVR.Phoenix.Interface;
using GVR.Creator;

namespace GVR.Phoenix.Util
{
    public class ChangeLevelTriggerHandler : MonoBehaviour
    {
        public string levelName;
        public string startVob;
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            SingletonBehaviour<WorldCreator>.GetOrCreate().LoadWorld(PhoenixBridge.VdfsPtr, levelName.Split(".")[0], startVob.Trim(' '));
        }

    }
}
