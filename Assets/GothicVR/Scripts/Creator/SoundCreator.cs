using GothicVR.Vob;
using GVR.Caches;
using GVR.Debugging;
using GVR.Demo;
using GVR.Manager;
using GVR.Phoenix.Interface.Vm;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Sound;
using PxCs.Data.Struct;
using PxCs.Data.Vob;
using PxCs.Interface;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;


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
            SetPosAndRot(soundObject, vobSound.position, vobSound.rotation!.Value);


            var audioSource = CreateAndAddAudioSource(soundObject, vobSound.soundName, vobSound);

            if (!audioSource)
                return soundObject;

            if (vobSound.initiallyPlaying)
                audioSource.Play();

            // Deactivate the gameobject to prevent audio from being played and CPU usage
            soundObject.SetActive(false);

            return soundObject;
        }

        public void Create(PxVobZoneMusicData vobSound, GameObject parent = null)
        {
            var soundObject = new GameObject(vobSound.vobName);
            soundObject.SetParent(parent);

            var soundObjectCollider = soundObject.AddComponent<BoxCollider>();

            var min = vobSound.boundingBox.min.ToUnityVector();
            var max = vobSound.boundingBox.max.ToUnityVector();

            soundObject.transform.position = (min + max) / 2f;
            soundObject.transform.localScale = (max - min);
            soundObjectCollider.isTrigger = true;

            if (FeatureFlags.I.EnableMusic)
                soundObject.AddComponent<MusicCollisionHandler>();
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

            SetPosAndRot(soundObject, vobSoundDaytime.position, vobSoundDaytime.rotation!.Value);

            // TODO - Is it right to have two AudioSources on one GO? Or would it be more Unity like to have two separate sub-GOs with one Source each?
            var audioSource1 = CreateAndAddAudioSource(soundObject, vobSoundDaytime.soundName, vobSoundDaytime);
            var audioSource2 = CreateAndAddAudioSource(soundObject, vobSoundDaytime.soundName2, vobSoundDaytime);

            if (!audioSource1 || !audioSource2)
                return soundObject;

            var audioDaytimeComp = soundObject.AddComponent<SoundDaytime>();

            audioDaytimeComp.SetAudioTimeSwitch(vobSoundDaytime.startTime, vobSoundDaytime.endTime, audioSource1, audioSource2);

            // Deactivate the gameobject to prevent audio from being played and CPU usage
            soundObject.SetActive(false);

            return soundObject;
        }

        private AudioSource CreateAndAddAudioSource(GameObject soundObject, string soundName, PxVobSoundData soundData)
        {
            PxSoundData<float> wavFile;

            if (soundName.ToLower() == "nosound.wav")
            {
                //instead of decoding nosound.wav which might be decoded incorrectly, just return null
                return null;
            }
            // Bugfix - Normally the data is to get C_SFX_DEF entries from VM. But sometimes there might be the real .wav file stored.
            if (soundName.ToLower().EndsWith(".wav"))
            {
                wavFile = assetCache.TryGetSound(soundData.soundName);
            }
            else
            {
                var sfxData = assetCache.TryGetSfxData(soundData.soundName);

                if (sfxData == null)
                {
                    Debug.LogError($"No sfx data returned for {soundData.soundName}");
                    return null;
                }

                wavFile = assetCache.TryGetSound(sfxData.file);
            }

            if (wavFile == null)
            {
                Debug.LogError($"No .wav data returned for {soundData.soundName}");
                return null;
            }

            AudioSource source = soundObject.AddComponent<AudioSource>();
            source.clip = SoundConverter.ToAudioClip(wavFile.sound);

            AudioSourceManager.I.AddAudioSource(soundObject, source);

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

        private void SetPosAndRot(GameObject obj, Vector3 position, PxMatrix3x3Data rotation)
        {
            SetPosAndRot(obj, position.ToUnityVector(), rotation.ToUnityMatrix().rotation);
        }

        private void SetPosAndRot(GameObject obj, UnityEngine.Vector3 position, Quaternion rotation)
        {
            obj.transform.localRotation = rotation;
            obj.transform.localPosition = position;
        }
    }
}