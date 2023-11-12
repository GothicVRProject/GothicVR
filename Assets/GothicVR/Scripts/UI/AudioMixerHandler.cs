using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace GVR.GothicVR.Scripts.UI
{
    public class AudioMixerHandler : MonoBehaviour
    {
        [SerializeField] private AudioMixerGroup audioMixer;
        [SerializeField] private Slider audioVolumeSlider;
        [SerializeField] private string volumePlayerPrefName;

        void Awake()
        {
            float oldVolume = PlayerPrefs.GetFloat(volumePlayerPrefName);
            audioVolumeSlider.value = oldVolume;
        }

        public void SliderUpdate(float value)
        {
            PlayerPrefs.SetFloat(volumePlayerPrefName, value);
            audioMixer.audioMixer.SetFloat(volumePlayerPrefName, Mathf.Log10(value) * 20);
        }
    }
}