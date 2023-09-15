using System;
using System.IO;
using DMCs.Interface;
using GVR.Debugging;
using GVR.Manager.Settings;
using GVR.Phoenix.Interface;
using GVR.Util;
using PxCs.Data.Vm;
using PxCs.Interface;
using UnityEngine;

namespace GVR.Creator
{
    public class MusicCreator : SingletonBehaviour<MusicCreator>
    {
        private IntPtr mixer;
        private IntPtr music;
        private IntPtr directmusic;

        public enum Tags : byte
        {
            Day = 0,
            Ngt = 1 << 0,

            Std = 0,
            Fgt = 1 << 1,
            Thr = 1 << 2
        }

        private Tags pendingTags = Tags.Day;
        private Tags currentTags = Tags.Day;

        private bool hasPending = false;
        private bool reloadTheme = false;

        private PxVmMusicData pendingTheme;

        private int bufferSize = 2048;
        private short[] shortBuffer;

        private static AudioSource musicSource;

        private static GameObject backgroundMusic;

        private float musicUpdateInterval = 5f; // Update music every 5 seconds
        private float musicUpdateTimer = 0f;

        // This multiplier is used to increase the buffer size and reduce the number times PrepareData is called
        // also affects the delay of the music, it doesn't sound so harsh when switching
        private int bufferSizeMultiplier = 16;

        void Start()
        {
            if (!FeatureFlags.I.EnableMusic)
                return;
            backgroundMusic = GameObject.Find("BackgroundMusic");
            musicSource = backgroundMusic.AddComponent<AudioSource>();
        }

        /// <summary>
        /// Called once every 0.20 seconds to update the music once the timer is bigger than the updateInterval
        /// </summary>
        void FixedUpdate()
        {
            if (!FeatureFlags.I.EnableMusic)
                return;

            if (!WorldCreator.I.IsWorldLoaded)
            {
                UpdateMusic();
                return;
            }
            musicUpdateTimer += Time.fixedDeltaTime;
            if (musicUpdateTimer >= musicUpdateInterval)
            {
                musicUpdateTimer = 0f;
                UpdateMusic();
            }
        }

        public void Create()
        {
            if (!FeatureFlags.I.EnableMusic)
                return;

            var g1Dir = SettingsManager.I.GameSettings.GothicIPath;

            // Combine paths using Path.Combine instead of Path.Join
            var fullPath = Path.Combine(g1Dir, "_work", "DATA", "Music");

            // Initialize DirectMusic components
            mixer = DMMixer.DMusicInitMixer();
            music = DMMusic.DMusicInitMusic();
            directmusic = DMDirectMusic.DMusicInitDirectMusic();

            // Add paths for G1
            AddMusicPath(fullPath, "dungeon");
            AddMusicPath(fullPath, "menu_men");
            AddMusicPath(fullPath, "orchestra");

            // Add paths for G2
            AddMusicPath(fullPath, "newworld");
            AddMusicPath(fullPath, "AddonWorld");

            // Create audio clip with, 4 times the bufferSize so we have enough room, 2 channels and 44100Hz
            var audioClip = AudioClip.Create("Music", bufferSize * 4 * bufferSizeMultiplier, 2, 44100, true, PrepareData);

            musicSource.priority = 0;
            musicSource.clip = audioClip;
            musicSource.loop = true;
        }

        private void AddMusicPath(string fullPath, string path)
        {
            fullPath = Path.Combine(fullPath, path);
            DMDirectMusic.DMusicAddPath(directmusic, fullPath);
        }

        private void PrepareData(float[] data)
        {
            shortBuffer = new short[bufferSize * 2 * bufferSizeMultiplier];

            DMMixer.DMusicMix(mixer, shortBuffer, (uint)data.Length / 2);

            byte[] byteArray = new byte[data.Length * 2];
            Buffer.BlockCopy(shortBuffer, 0, byteArray, 0, byteArray.Length);

            float[] floatArray = Convert16BitByteArrayToFloatArray(byteArray, 0, byteArray.Length);
            Array.Copy(floatArray, data, floatArray.Length);
        }

        private void UpdateMusic()
        {
            if (!hasPending)
            {
                return;
            }

            hasPending = false;
            PxVmMusicData theme = pendingTheme;
            Tags tags = pendingTags;

            DMMixer.DMusicSetMusicVolume(mixer, pendingTheme.vol);

            if (!reloadTheme)
            {
                return;
            }

            var pattern = DMDirectMusic.DMusicLoadFile(directmusic, theme.file, theme.file.Length);

            DMMusic.DMusicAddPattern(music, pattern);

            Tags cur = currentTags & (Tags.Std | Tags.Fgt | Tags.Thr);
            Tags next = tags & (Tags.Std | Tags.Fgt | Tags.Thr);

            int em = 8; // end

            if (next == Tags.Std)
            {
                if (cur != Tags.Std)
                {
                    em = 2; // break
                    Debug.Log("break");
                }
            }
            else if (next == Tags.Fgt)
            {
                if (cur == Tags.Thr)
                {
                    em = 1; // fill
                    Debug.Log("fill");
                }
            }
            else if (next == Tags.Thr)
            {
                if (cur == Tags.Fgt)
                {
                    em = 0; // normal
                    Debug.Log("normal");
                }
            }
            DMMixer.DMusicSetMusic(mixer, music, em);
            currentTags = tags;
        }

        private static float[] Convert16BitByteArrayToFloatArray(byte[] source, int headerOffset, int dataSize)
        {
            int bytesPerSample = sizeof(Int16); // block size = 2
            int sampleCount = source.Length / bytesPerSample;

            float[] data = new float[sampleCount];

            Int16 maxValue = Int16.MaxValue;

            for (int i = 0; i < sampleCount; i++)
            {
                int offset = i * bytesPerSample;
                Int16 sample = BitConverter.ToInt16(source, offset);
                float floatSample = (float)sample / maxValue;
                data[i] = floatSample;
            }

            return data;
        }

        public void SetMusic(string zone, Tags tags)
        {
            bool isDay = (tags & Tags.Ngt) == 0;

            string result = zone.Substring(zone.IndexOf("_") + 1);

            var musicTag = "STD";

            if ((tags & Tags.Fgt) != 0)
                musicTag = "FGT";

            if ((tags & Tags.Thr) != 0)
                musicTag = "THR";

            string name = result + "_" + (isDay ? "DAY" : "NGT") + "_" + musicTag;

            var theme = PxVm.InitializeMusic(GameData.I.VmMusicPtr, name);

            reloadTheme = pendingTheme.file != theme.file;
            pendingTheme = theme;
            pendingTags = tags;
            hasPending = true;
        }

        public void setMusic(string name)
        {
            var theme = PxVm.InitializeMusic(GameData.I.VmMusicPtr, name);
            reloadTheme = true;
            pendingTheme = theme;
            hasPending = true;
        }

        private void StopMusic()
        {
            if (musicSource.isPlaying)
                musicSource.Pause();

            // reinitialize music
            DMMusic.DMusicFreeMusic(music);
            music = DMMusic.DMusicInitMusic();

            DMMixer.DMusicSetMusic(mixer, music);
        }

        private void RestartMusic()
        {
            hasPending = true;
            reloadTheme = true;
        }

        public void setEnabled(bool enable)
        {
            var isPlaying = musicSource.isPlaying;
            if (isPlaying == enable)
                return;

            if (enable)
            {
                RestartMusic();
                musicSource.Play();
            }
            else
            {
                StopMusic();
            }
        }
    }
}
