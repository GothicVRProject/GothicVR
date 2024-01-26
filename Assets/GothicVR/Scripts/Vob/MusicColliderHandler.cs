using System.Collections.Generic;
using GVR.Debugging;
using GVR.Globals;
using GVR.Manager;
using UnityEngine;

namespace GVR.Vob
{
    public class MusicCollisionHandler : MonoBehaviour
    {
        private static List<string> musicZones = new();

        private void OnTriggerEnter(Collider other)
        {
            if (!FeatureFlags.I.enableMusic)
                return;

            if (!other.CompareTag(Constants.PlayerTag))
                return;

            musicZones.Add(gameObject.name);

            MusicManager.I.SetMusic(gameObject.name, MusicManager.Tags.Std);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!FeatureFlags.I.enableMusic)
                return;
            
            if (!other.CompareTag(Constants.PlayerTag))
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
