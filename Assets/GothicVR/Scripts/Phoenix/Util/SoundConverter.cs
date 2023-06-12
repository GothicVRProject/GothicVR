using UnityEngine;

namespace GVR.Phoenix.Util
{
    public class SoundConverter
    {
        public static AudioClip ToAudioClip(float[] soundArray)
        {
            AudioClip audioClip = AudioClip.Create("Sound", soundArray.Length, 1, 44100, false);
            audioClip.SetData(soundArray, 0);
            return audioClip;
        }
    }
}