using System;
using System.IO;
using DMCs.Interface;
using GVR.Caches;
using GVR.Debugging;
using GVR.Manager.Settings;
using GVR.Util;
using PxCs.Data.Vm;
using UnityEngine;

namespace GVR.Manager
{
    public class MusicManager : SingletonBehaviour<MusicManager>
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

        private bool hasPending;
        private bool reloadTheme;

        private PxVmMusicData pendingTheme;

        private int bufferSize = 2048;
        private short[] shortBuffer;

        private static AudioSource musicSource;


        // This multiplier is used to increase the buffer size and reduce the number times PrepareData is called
        // also affects the delay of the music, it doesn't sound so harsh when switching
        // It also controls how fast/slow the music is updated 
        // (since we are updating music when we don't have any more music data to parse)
        private int bufferSizeMultiplier = 16;

        private void Start()
        {
            var backgroundMusic = GameObject.Find("BackgroundMusic");
            backgroundMusic.TryGetComponent<AudioSource>(out musicSource);
        }


        public void Create()
        {
            var g1Dir = SettingsManager.GameSettings.GothicIPath;

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

            UpdateMusic();
        }

        private void UpdateMusic()
        {
            if (!hasPending)
                return;

            hasPending = false;
            PxVmMusicData theme = pendingTheme;
            Tags tags = pendingTags;

            DMMixer.DMusicSetMusicVolume(mixer, pendingTheme.vol);

            if (!reloadTheme)
                return;

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
            int bytesPerSample = sizeof(short); // block size = 2
            int sampleCount = source.Length / bytesPerSample;

            float[] data = new float[sampleCount];

            short maxValue = short.MaxValue;

            for (int i = 0; i < sampleCount; i++)
            {
                int offset = i * bytesPerSample;
                short sample = BitConverter.ToInt16(source, offset);
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

            var musicName = $"{result}_{(isDay ? "DAY" : "NGT")}_{musicTag}";

            var theme = AssetCache.TryGetMusic(musicName);

            reloadTheme = pendingTheme.file != theme.file;
            pendingTheme = theme;
            pendingTags = tags;
            hasPending = true;

            if (FeatureFlags.I.ShowMusicLogs)
                Debug.Log($"Playing music: theme >{musicName}< from file >{theme.file}<");
        }

        public void SetMusic(string musicName)
        {
            var theme = AssetCache.TryGetMusic(musicName);
            reloadTheme = true;
            pendingTheme = theme;
            hasPending = true;

            if (FeatureFlags.I.ShowMusicLogs)
                Debug.Log($"[Music] Playing music: theme >{musicName}< from file >{theme.file}<");
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

        public void SetEnabled(bool enable)
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
