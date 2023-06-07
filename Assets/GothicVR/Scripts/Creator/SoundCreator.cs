using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Interface;
using System;
using System.Runtime.InteropServices;
using UnityEngine;



namespace GVR.Creator
{
    public class SoundCreator : SingletonBehaviour<SoundCreator>
    {
        void Start()
        {
            VmGothicBridge.PhoenixMdl_AI_OUTPUT.AddListener(AI_OUTPUT);
        }

        // ! NOT TESTED YET !
        /// <summary>
        /// Original Gothic uses this function to play dialogue.
        /// </summary>
        public static void AI_OUTPUT(string name)
        {

            // The body of this function can also be used to play sound effects, not just dialogue
            // According to this document `Sound effects are defined in multiple places, 
            // in .mds files as part of the animation EventBlocks, or in the SFX Daeduls scripts.`
            // and we have to check how to play sounds for animations
            // https://gothic-modding-community.github.io/gmc/zengin/sound/ 

            var vdfPtr = PhoenixBridge.VdfsPtr;

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

            // TODO: We create a new GameObject here but we need to attach it to the source of the sound
            // so Unity will handle the spatial side of things correctly 
            var SoundObject = new GameObject(string.Format(name));

            AudioSource source = SoundObject.AddComponent<AudioSource>();
            source.clip = SoundConverter.ToAudioClip(wavFile);

            source.Play();
        }
    }
}