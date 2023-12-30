using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using GVR.Globals;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Properties;
using JetBrains.Annotations;
using PxCs.Data.Sound;
using UnityEngine;

namespace GVR.GothicVR.Scripts.Manager
{
    public static class VobHelper
    {
        private const float lookupDistance = 10f; // meter
        
        [CanBeNull]
        public static VobProperties GetFreeInteractableWithin10M(Vector3 position, string visualScheme)
        {
            return GameData.VobsInteractable
                .Where(i => Vector3.Distance(i.transform.position, position) < lookupDistance)
                .Where(i => i.visualScheme.EqualsIgnoreCase(visualScheme))
                .OrderBy(i => Vector3.Distance(i.transform.position, position))
                .FirstOrDefault();
        }

        [CanBeNull]
        public static GameObject GetNearestSlot(GameObject go, Vector3 position)
        {
            var goTransform = go.transform;

            if (goTransform.childCount == 0)
                return null;
            
            var zm = go.transform.GetChild(0);
            
            return zm.gameObject.GetAllDirectChildren()
                .Where(i => i.name.ContainsIgnoreCase("ZS"))
                .OrderBy(i => Vector3.Distance(i.transform.position, position))
                .FirstOrDefault();
        }
        
        public static AudioClip GetSoundClip(string soundName)
        {
            PxSoundData<float> wavFile;

            // FIXME - move to EqualsIgnoreCase()
            if (soundName.ToLower() == "nosound.wav")
            {
                //instead of decoding nosound.wav which might be decoded incorrectly, just return null
                return null;
            }
            
            // Bugfix - Normally the data is to get C_SFX_DEF entries from VM. But sometimes there might be the real .wav file stored.
            // FIXME - Move to EndsWithIgnoreCase()
            if (soundName.ToLower().EndsWith(".wav"))
            {
                wavFile = AssetCache.TryGetSound(soundName);
            }
            else
            {
                var sfxData = AssetCache.TryGetSfxData(soundName);

                if (sfxData == null)
                    return null;

                wavFile = AssetCache.TryGetSound(sfxData.File);
            }

            if (wavFile == null)
            {
                Debug.LogError($"No .wav data returned for {soundName}");
                return null;
            }
            
            return SoundConverter.ToAudioClip(wavFile);
        }
    }
}
