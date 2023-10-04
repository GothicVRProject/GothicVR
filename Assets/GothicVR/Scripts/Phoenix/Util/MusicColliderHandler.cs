using System.Collections.Generic;
using GVR.Creator;
using GVR.Debugging;
using GVR.Manager;
using UnityEngine;

namespace GVR.Phoenix.Util
{
    public class MusicCollisionHandler : MonoBehaviour
    {
        private static List<string> musicZones = new();

        private void OnTriggerEnter(Collider other)
        {
            if (!FeatureFlags.I.EnableMusic)
                return;

            if (!other.CompareTag(ConstantsManager.PlayerTag))
                return;

            musicZones.Add(gameObject.name);

            MusicCreator.I.SetMusic(gameObject.name, MusicCreator.Tags.Std);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!FeatureFlags.I.EnableMusic)
                return;
            
            if (!other.CompareTag(ConstantsManager.PlayerTag))
                return;

            musicZones.Remove(gameObject.name);

            // Other music will play now.
            if (musicZones.Count > 0)
                return;

            // Play default music.
            MusicCreator.I.SetMusic("MUSICZONE_DEF", MusicCreator.Tags.Std);
        }
    }
}
