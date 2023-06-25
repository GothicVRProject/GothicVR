using UnityEngine;
using GVR.Util;
using System.Collections.Generic;

namespace GVR.Phoenix.Util
{
    public class MusicCollisionHandler : MonoBehaviour
    {
        private static List<string> musicZones = new List<string>();

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            musicZones.Add(gameObject.name);

            SingletonBehaviour<MusicMixer>.GetOrCreate().SetMusic(gameObject.name, MusicMixer.Tags.Std);
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            musicZones.Remove(gameObject.name);

            if (musicZones.Count > 0) return;

            SingletonBehaviour<MusicMixer>.GetOrCreate().SetMusic("MUSICZONE_DEF", MusicMixer.Tags.Std);
        }
    }
}
