using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Interface;
using System;
using System.Runtime.InteropServices;
using GothicVR.Scripts.Items;
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
            var soundObject = new GameObject(vobSound.soundName);
            soundObject.SetParent(parent);

            var audioSource = CreateAndAddAudioSource(soundObject, vobSound.soundName, vobSound);

            if (!audioSource)
                return soundObject;

            if (vobSound.initiallyPlaying)
                audioSource.Play();

            return soundObject;
        }

        /// <summary>
        /// Creating AudioSource from PxVobSoundDaytimeData is very similar to PxVobSoundData one.
        /// There are only two differences:
        ///     1. This one has two AudioSources
        ///     2. The sources will be toggled during gameplay when start/end time is reached.
        /// </summary>
        public GameObject Create(PxVobSoundDaytimeData vobSoundDaytime, GameObject parent = null)
        {
            var soundObject = new GameObject($"{vobSoundDaytime.soundName}-{vobSoundDaytime.soundName2}");
            soundObject.SetParent(parent);

            var audioSource1 = CreateAndAddAudioSource(soundObject, vobSoundDaytime.soundName, vobSoundDaytime);
            var audioSource2 = CreateAndAddAudioSource(soundObject, vobSoundDaytime.soundName2, vobSoundDaytime);

            if (!audioSource1 || !audioSource2)
                return soundObject;
            
            var audioDaytimeComp = soundObject.AddComponent<SoundDaytime>();
            
            audioDaytimeComp.SetAudioTimeSwitch(vobSoundDaytime.startTime, vobSoundDaytime.endTime, audioSource1, audioSource2);
            
            return soundObject;
        }

        private AudioSource CreateAndAddAudioSource(GameObject soundObject, string soundName, PxVobSoundData soundData)
        {
            byte[] wavFile;
            // Bugfix - Normally the data is to get C_SFX_DEF entries from VM. But sometimes there might be the real .wav file stored.
            if (soundName.ToLower().EndsWith(".wav"))
            {
                wavFile = assetCache.TryGetSound(soundData.soundName);
            }
            else
            {
                var sfxData = assetCache.TryGetSfxData(soundData.soundName);
                wavFile = assetCache.TryGetSound(sfxData.file);
            }

            if (wavFile == null)
            {
                Debug.LogError($"No .wav data returned for {soundData.soundName}");
                return null;
            }
            
            AudioSource source = soundObject.AddComponent<AudioSource>();
            source.clip = SoundConverter.ToAudioClip(wavFile);
            

            // Both need to be set, that Audio can be heard only within defined range.
            // https://answers.unity.com/questions/1316535/how-to-have-audio-only-be-heard-in-a-certain-radiu.html
            source.rolloffMode = AudioRolloffMode.Linear;
            source.spatialBlend = 1f;

            source.maxDistance = soundData.radius / 100; // Gothic's values are in cm, Unity's in m.

            // Some data added for testing purposes. More properties are still to add.
            source.loop = soundData.mode == PxWorld.PxVobSoundMode.PxVobSoundModeLoop;

            return source;
        }
    }
}