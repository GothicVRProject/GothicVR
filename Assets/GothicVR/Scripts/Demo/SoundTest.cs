using UnityEngine;
using PxCs.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using System;
using System.Runtime.InteropServices;

namespace GVR.Demo
{

    public class SoundTest : SingletonBehaviour<SoundTest>
    {
        public void Create(IntPtr vdfPtr, string name)
        {

            if (!name.Contains(".WAV"))
            {
                name += ".WAV";
            }
            var vdfEntrySound = PxVdf.pxVdfGetEntryByName(vdfPtr, name);

            if (vdfEntrySound == IntPtr.Zero)
            {
                Debug.Log("Sound not found");
                return;
            }

            var wavSound = PxVdf.pxVdfEntryOpenBuffer(vdfEntrySound);

            if (wavSound == IntPtr.Zero)
            {
                Debug.Log("Sound could not be loaded");
                return;
            }

            ulong size = PxBuffer.pxBufferSize(wavSound);

            var array = IntPtr.Zero;

            array = PxBuffer.pxBufferLoadArray(wavSound);

            byte[] wavFile = new byte[size];

            Marshal.Copy(array, wavFile, 0, (int)size);
            var SoundObject = new GameObject(string.Format("Sound"));
            AudioSource source = SoundObject.AddComponent<AudioSource>();
            source.clip = SoundConverter.ToAudioClip(wavFile);
            // source.loop = true;
            source.Play();

        }
    }
}