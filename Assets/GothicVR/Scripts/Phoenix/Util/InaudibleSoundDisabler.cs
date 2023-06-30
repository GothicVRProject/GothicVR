using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GVR.Phoenix.Util
{
    public class InaudibleSoundDisabler : MonoBehaviour
    {
        private AudioListener audioListener;
        private AudioSource audioSource;
        private float distanceFromPlayer;

        void Start()
        {
            // Finds the Audio Listener and the Audio Source on the object
            audioListener = Camera.main.GetComponent<AudioListener>();
            audioSource = gameObject.GetComponent<AudioSource>();
        }

        void Update()
        {
            distanceFromPlayer = Vector3.Distance(transform.position, audioListener.transform.position);

            if (distanceFromPlayer <= audioSource.maxDistance)
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