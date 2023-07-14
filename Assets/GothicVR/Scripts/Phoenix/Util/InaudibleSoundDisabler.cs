using UnityEngine;

namespace GVR.Phoenix.Util
{
    public class InaudibleSoundDisabler : MonoBehaviour
    {
        private AudioSource audioSource;
        private bool isAudioListenerObtained;
        private AudioListener audioListener;

        private bool IsAudioListenerObtained
        {
            get
            {
                if (!isAudioListenerObtained)
                {
                    isAudioListenerObtained = true;
                    audioListener = Camera.main?.GetComponent<AudioListener>();
                }
                return audioListener != null;
            }
        }

        private float DistanceFromPlayer => IsAudioListenerObtained ? Vector3.Distance(transform.position, audioListener.transform.position) : 0f;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (!IsAudioListenerObtained)
                return;

            if (DistanceFromPlayer <= audioSource.maxDistance)
            {
                ToggleAudioSource(true);
            }
            else
            {
                ToggleAudioSource(false);
            }
        }

        void ToggleAudioSource(bool isAudible)
        {
            if (audioSource == null)
                return;

            if (!isAudible && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
            else if (isAudible && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}
