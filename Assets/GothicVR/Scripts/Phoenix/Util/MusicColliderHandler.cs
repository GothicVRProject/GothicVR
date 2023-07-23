using UnityEngine;
using GVR.Util;
using GVR.Creator;
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

            MusicCreator.I.SetMusic(gameObject.name, MusicCreator.Tags.Std);
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            musicZones.Remove(gameObject.name);

            if (musicZones.Count > 0) return;

            MusicCreator.I.SetMusic("MUSICZONE_DEF", MusicCreator.Tags.Std);
        }
    }
}
