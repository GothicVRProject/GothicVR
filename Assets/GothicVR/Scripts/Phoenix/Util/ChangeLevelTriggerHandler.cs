using UnityEngine;
using GVR.Util;
using GVR.Phoenix.Interface;
using GVR.Importer;

namespace GVR.Phoenix.Util
{
    public class ChangeLevelTriggerHandler : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            Debug.Log("Triggered " + gameObject.name + " teleporting to " + gameObject.name.Split('_')[2]);

            SingletonBehaviour<PhoenixImporter>.GetOrCreate().LoadWorld(PhoenixBridge.VdfsPtr, gameObject.name.Split('_')[2].ToLower());
            

        }

    }
}
