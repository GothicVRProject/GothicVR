using System.Collections.Generic;
using GVR.Debugging;
using GVR.Extensions;
using GVR.Globals;
using GVR.Manager;
using UnityEngine;

namespace GVR.Vob
{
    public class MusicCollisionHandler : MonoBehaviour
    {
        private static Stack<string> musicZones = new();

        private void OnTriggerEnter(Collider other)
        {
            if (!FeatureFlags.I.enableMusic)
                return;

            if (!other.CompareTag(Constants.PlayerTag))
                return;

            // We are already playing this segment.
            if (!musicZones.IsEmpty() && musicZones.Peek() == gameObject.name)
                return;

            musicZones.Push(gameObject.name);

            MusicManager.Play(gameObject.name, MusicManager.SegmentTags.Std);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!FeatureFlags.I.enableMusic)
                return;
            
            if (!other.CompareTag(Constants.PlayerTag))
                return;

            if (musicZones.IsEmpty() || musicZones.Peek() != gameObject.name)
                return;

            musicZones.Pop();

            // Play default music
            if (musicZones.IsEmpty())
            {
                MusicManager.Play("MUSICZONE_DEF", MusicManager.SegmentTags.Std);
            }
            else
            {
                MusicManager.Play(musicZones.Peek(), MusicManager.SegmentTags.Std);
            }
        }
    }
}
