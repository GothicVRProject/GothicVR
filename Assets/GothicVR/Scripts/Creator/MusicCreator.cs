using UnityEngine;
using System;
using System.IO;
using GVR.Util;
using GVR.Phoenix.Interface;
using DMCs.Interface;
using PxCs.Interface;
using PxCs.Data.Vm;

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

        public Tags pendingTags = Tags.Day;
        public Tags currentTags = Tags.Day;

        private bool hasPending = false;
        private bool reloadTheme = false;

        private PxVmMusicData pendingTheme;

        public int bufferSize = 2048;
        private short[] shortBuffer;

        public void Create(string G1Dir)
        {
            // Combine paths using Path.Combine instead of Path.Join
            var fullPath = Path.Combine(G1Dir, "_work", "DATA", "Music");

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

            // Set initial music
            setMusic("SYS_Menu");

            // Initialize audio source and clip
            var soundObject = CreateSoundObject();
            var audioClip = CreateAudioClip();

            // Set audio source properties and play music
            SetAudioSourceProperties(soundObject, audioClip);
        }

        private void AddMusicPath(string fullPath, string path)
        {
            fullPath = Path.Combine(fullPath, path);
            DMDirectMusic.DMusicAddPath(directmusic, fullPath);
        }

        private GameObject CreateSoundObject()
        {
            return new GameObject("Background Music");
        }

        private AudioClip CreateAudioClip()
        {
            return AudioClip.Create("Music", bufferSize * 4, 2, 44100, true, PrepareData);
        }

        private void SetAudioSourceProperties(GameObject soundObject, AudioClip audioClip)
        {
            var source = soundObject.AddComponent<AudioSource>();
            source.priority = 0;
            source.clip = audioClip;
            source.loop = true;
            source.Play();
        }

        private void PrepareData(float[] data)
        {
            UpdateMusic();

            shortBuffer = new short[bufferSize * 2];

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

            var theme = PxVm.InitializeMusic(PhoenixBridge.VmMusicPtr, name);

            reloadTheme = pendingTheme.file != theme.file;
            pendingTheme = theme;
            pendingTags = tags;
            hasPending = true;
        }

        public void setMusic(string name)
        {
            var theme = PxVm.InitializeMusic(PhoenixBridge.VmMusicPtr, name);
            reloadTheme = true;
            pendingTheme = theme;
            hasPending = true;
        }
    }
}