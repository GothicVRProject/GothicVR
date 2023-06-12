using UnityEngine;
using PxCs.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using System;
using System.Runtime.InteropServices;
using PxCs.Data.Sound;

namespace GVR.Demo
{

    public class SoundTest : SingletonBehaviour<SoundTest>
    {
        public void Create(IntPtr vdfPtr, string name)
        {

            // Bugfix - Normally the data is to get C_SFX_DEF entries from VM. But sometimes there might be the real .wav file stored.
            var wavFile = PxSound.GetSoundArrayFromVDF<float>(vdfPtr, $"{name}.wav");
            
            var soundObject = new GameObject(string.Format("Sound"));
            AudioSource source = soundObject.AddComponent<AudioSource>();
            source.clip = SoundConverter.ToAudioClip(wavFile.sound);
            // // source.loop = true;
            source.Play();

        }
    }
}