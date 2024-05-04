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

            Debug.Log("Changing - Add " + gameObject.name);

            // FIXME - Seems different GOs with same name aren't added to the Set. Fix it.
            // FIXME - We need to load the currently active music when spawned. Currently we need to walk 1cm to trigger collider.
            MusicManager.MusicZones.Add(gameObject);

            MusicManager.Play(MusicManager.SegmentTags.Std);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!FeatureFlags.I.enableMusic)
                return;
            
            if (!other.CompareTag(Constants.PlayerTag))
                return;

            Debug.Log("Changing - Remove " + gameObject.name);

            MusicManager.MusicZones.Remove(gameObject);

            MusicManager.Play(MusicManager.SegmentTags.Std);
        }
    }
}
