using GVR.Debugging;
using GVR.Globals;
using GVR.Manager;
using UnityEngine;

namespace GVR.Vob
{
    public class MusicCollisionHandler : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!FeatureFlags.I.enableMusic)
                return;

            if (!other.CompareTag(Constants.PlayerTag))
                return;
            
            // FIXME - We need to load the currently active music when spawned. Currently we need to walk 1cm to trigger collider.
            MusicManager.AddMusicZone(gameObject);
            MusicManager.Play(MusicManager.SegmentTags.Std);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!FeatureFlags.I.enableMusic)
                return;
            
            if (!other.CompareTag(Constants.PlayerTag))
                return;

            MusicManager.RemoveMusicZone(gameObject);
            MusicManager.Play(MusicManager.SegmentTags.Std);
        }
    }
}
