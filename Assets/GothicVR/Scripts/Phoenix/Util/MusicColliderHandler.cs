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
            if (!FeatureFlags.I.enableMusic)
                return;

            if (!other.CompareTag(ConstantsManager.PlayerTag))
                return;

            musicZones.Add(gameObject.name);

            MusicManager.I.SetMusic(gameObject.name, MusicManager.Tags.Std);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!FeatureFlags.I.enableMusic)
                return;
            
            if (!other.CompareTag(ConstantsManager.PlayerTag))
                return;

            musicZones.Remove(gameObject.name);

            // Other music will play now.
            if (musicZones.Count > 0)
                return;

            // Play default music.
            MusicManager.I.SetMusic("MUSICZONE_DEF", MusicManager.Tags.Std);
        }
    }
}
