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
            float oldVolume = PlayerPrefs.GetFloat(volumePlayerPrefName, 1f);
            audioVolumeSlider.value = oldVolume;
        }

        public void SliderUpdate(float value)
        {
            PlayerPrefs.SetFloat(volumePlayerPrefName, value);
            // Volume and loudness are not the same, volume can be linear but loudness is logarithmic
            // https://www.msdmanuals.com/home/multimedia/table/measurement-of-loudness
            audioMixer.audioMixer.SetFloat(volumePlayerPrefName, Mathf.Log10(value) * 20);
        }
    }
}