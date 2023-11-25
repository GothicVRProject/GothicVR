using PxCs.Data.Sound;
using UnityEngine;

namespace GVR.Phoenix.Util
{
    public static class SoundConverter
    {
        public static AudioClip ToAudioClip(PxSoundData<float> wavFile)
        {
            AudioClip audioClip = AudioClip.Create("Sound", wavFile.sound.Length, wavFile.channels, wavFile.sampleRate, false);
            audioClip.SetData(wavFile.sound, 0);
            return audioClip;
        }
    }
}