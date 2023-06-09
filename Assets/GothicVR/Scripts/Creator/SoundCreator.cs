using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Interface;
using System;
using System.Runtime.InteropServices;
using GVR.Caches;
using JetBrains.Annotations;
using PxCs.Data.Vob;
using PxCs.Extensions;
using UnityEngine;



namespace GVR.Creator
{
    public class SoundCreator : SingletonBehaviour<SoundCreator>
    {
        private AssetCache assetCache;

        private void Start()
        {
            assetCache = SingletonBehaviour<AssetCache>.GetOrCreate();
            
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
            array = PxBuffer.pxBufferArray(wavSound);

            byte[] wavFile = new byte[size];

            Marshal.Copy(array, wavFile, 0, (int)size);

            // TODO: We create a new GameObject here but we need to attach it to the source of the sound
            // so Unity will handle the spatial side of things correctly 
            var SoundObject = new GameObject(string.Format(name));

            AudioSource source = SoundObject.AddComponent<AudioSource>();
            source.clip = SoundConverter.ToAudioClip(wavFile);

            source.Play();
        }
        
        // FIXME - add caching for audio file - 1) PxVmSfxData and/or 2) byte[]
        public GameObject Create(PxVobSoundData vobSound, GameObject parent = null)
        {
            // so Unity will handle the spatial side of things correctly 
            var soundObject = new GameObject(string.Format(name));
            soundObject.SetParent(parent);
            
            // FIXME - Load actual .wav file name from 
            // INSTANCE FIRE_SMALL (C_SFX_DEF) {file= "fire_small01.wav"; vol = 100; ...}
            var wavFile = assetCache.TryGetSound(vobSound.soundName);

            if (wavFile == null)
            {
                Debug.LogError($"No .wav data returned for {vobSound.soundName}");
                return null;
            }
            AudioSource source = soundObject.AddComponent<AudioSource>();
            source.clip = SoundConverter.ToAudioClip(wavFile);

            // Some data added for testing purposes. More properties are still to add.
            source.loop = vobSound.mode == PxWorld.PxVobSoundMode.PxVobSoundModeLoop;
            
            if (vobSound.initiallyPlaying)
                source.Play();

            return soundObject;
        }

    }
}