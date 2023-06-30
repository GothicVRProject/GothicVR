using GothicVR.Vob;
using GVR.Caches;
using GVR.Phoenix.Interface.Vm;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Sound;
using PxCs.Data.Vob;
using PxCs.Interface;
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
        public GameObject Create(PxVobZoneMusicData vobSound, GameObject parent = null)
        {
            var soundObject = new GameObject(vobSound.vobName);
            soundObject.SetParent(parent);

            var soundObjectCollider = soundObject.AddComponent<BoxCollider>();

            var min = vobSound.boundingBox.min.ToUnityVector();
            var max = vobSound.boundingBox.max.ToUnityVector();

            soundObject.transform.position = (min + max) / 2f;
            soundObject.transform.localScale = (max - min);
            soundObjectCollider.isTrigger = true;

            var musicCollisionHandler = soundObject.AddComponent<MusicCollisionHandler>();

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
            PxSoundData<float> wavFile;
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
            source.clip = SoundConverter.ToAudioClip(wavFile.sound);

            soundObject.AddComponent<InaudibleSoundDisabler>();

            // Both need to be set, that Audio can be heard only within defined range.
            // https://answers.unity.com/questions/1316535/how-to-have-audio-only-be-heard-in-a-certain-radiu.html
            source.rolloffMode = AudioRolloffMode.Linear;
            source.spatialBlend = 1f;

            source.maxDistance = soundData.radius / 100; // Gothic's values are in cm, Unity's in m.
            source.volume = soundData.volume / 100; // Gothic's volume is 0...100, Unity's is 0...1. 

            source.loop = soundData.mode == PxWorld.PxVobSoundMode.PxVobSoundModeLoop;

            // FIXME - Random play isn't implemented yet.

            return source;
        }
    }
}