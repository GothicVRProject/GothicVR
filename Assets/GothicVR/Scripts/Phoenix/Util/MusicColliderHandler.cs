using System.Collections.Generic;
using GVR.Creator;
using GVR.Manager;
using UnityEngine;

namespace GVR.Phoenix.Util
{
    public class MusicCollisionHandler : MonoBehaviour
    {
        private static List<string> musicZones = new();

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(ConstantsManager.PlayerTag))
                return;

            musicZones.Add(gameObject.name);

            MusicCreator.I.SetMusic(gameObject.name, MusicCreator.Tags.Std);
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            musicZones.Remove(gameObject.name);

            if (musicZones.Count > 0)
                return;

            MusicCreator.I.SetMusic("MUSICZONE_DEF", MusicCreator.Tags.Std);
        }
    }
}
