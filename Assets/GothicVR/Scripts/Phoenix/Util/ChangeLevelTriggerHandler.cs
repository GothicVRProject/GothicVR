using UnityEngine;
using GVR.Util;
using GVR.Phoenix.Interface;
using GVR.Importer;

namespace GVR.Phoenix.Util
{
    public class ChangeLevelTriggerHandler : MonoBehaviour
    {
        public string levelName;
        public string startVob;
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            Debug.Log("Triggered " + gameObject.name + " teleporting to " + levelName.Split(".")[0] + " having level name " + levelName + " and start vob " + startVob);

            SingletonBehaviour<PhoenixImporter>.GetOrCreate().LoadWorld(PhoenixBridge.VdfsPtr, levelName.Split(".")[0], startVob.Trim(' '));


        }

    }
}
