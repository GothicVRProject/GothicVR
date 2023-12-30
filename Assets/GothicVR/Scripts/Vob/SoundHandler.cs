using System.Collections;
using UnityEngine;
using ZenKit.Vobs;
using Random = UnityEngine.Random;

namespace GothicVR.Vob
{
    public class SoundHandler : MonoBehaviour
    {
        public AudioSource audioSource;
        public VobSoundProperties properties;
        
        // We need to avoid to start the Coroutine twice.
        private bool isCoroutineRunning;
        
        
        private void OnEnable()
        {
            StartCoroutine();
        }

        private void OnDisable()
        {
            // Coroutines are stopped when GameObject gets disabled. But we need to restart during OnEnable() manually.
            isCoroutineRunning = false;
        }

        /// <summary>
        /// This will be called during VobCreation time. OnEnable() is too early on to check, if we really need the Coroutine
        /// as properties.soundData will be set at a later state (it's expected to be before calling this method tbh).
        /// Now we can check starting the Coroutine.
        /// </summary>
        public void PrepareSoundHandling()
        {
            if (properties.soundData == null)
            {
                Debug.LogError("VobSoundProperties.soundData not set. Can't register random sound play!");
                return;
            }

            if (gameObject.activeSelf)
                StartCoroutine();
        }

        private void StartCoroutine()
        {
            // Either it's not yet initialized (no clip) or it's no random loop
            if (audioSource.clip == null || properties.soundData.Mode != SoundMode.Random)
                return;
            
            if (isCoroutineRunning)
                return;

            StartCoroutine(ReplayRandomSound());
            isCoroutineRunning = true;
        }

        private IEnumerator ReplayRandomSound()
        {
            while (true)
            {
                var nextRandomPlayTime = properties.soundData.RandomDelay
                                         + Random.Range(0.0f, properties.soundData.RandomDelayVar);
                yield return new WaitForSeconds(nextRandomPlayTime);

                audioSource.Play();
                yield return new WaitForSeconds(audioSource.clip.length);
            }
        }
    }
}
