using UnityEngine;
using System;
using System.IO;
using GVR.Util;
using GVR.Phoenix.Interface;
using DMCs.Interface;
using PxCs.Interface;
using PxCs.Data.Vm;

namespace GVR.Phoenix.Util
{
    public class MusicMixer : SingletonBehaviour<MusicMixer>
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
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/Music/"));

            mixer = DMMixer.DMusicInitMixer();
            music = DMMusic.DMusicInitMusic();
            directmusic = DMDirectMusic.DMusicInitDirectMusic();

            DMDirectMusic.DMusicAddPath(directmusic, Path.GetFullPath(Path.Join(fullPath, "dungeon")));
            DMDirectMusic.DMusicAddPath(directmusic, Path.GetFullPath(Path.Join(fullPath, "menu_men")));
            DMDirectMusic.DMusicAddPath(directmusic, Path.GetFullPath(Path.Join(fullPath, "orchestra")));

            SetMenuMusic();

            var buffer = new float[bufferSize * 4];

            shortBuffer = new short[bufferSize * 2];

            var soundObject = new GameObject(string.Format("BACKROUND MUSIC"));

            AudioSource source = soundObject.AddComponent<AudioSource>();
            AudioClip audioClip = AudioClip.Create("Sound", bufferSize * 4, 2, 44100, true, PrepareData);

            source.clip = audioClip;
            source.Play();
        }

        private void PrepareData(float[] data)
        {
            UpdateMusic();

            DMMixer.DMusicMix(mixer, shortBuffer, (uint)data.Length / 2);

            byte[] byteArray = new byte[data.Length * 2];
            Buffer.BlockCopy(shortBuffer, 0, byteArray, 0, byteArray.Length);
            var buffer = Convert16BitByteArrayToFloatArray(byteArray, 0, byteArray.Length);
            Array.Copy(buffer, data, buffer.Length);
        }

        private void UpdateMusic()
        {
            PxVmMusicData theme = new PxVmMusicData();
            bool updateTheme = false;
            Tags tags = new Tags();

            if (hasPending)
            {
                hasPending = false;
                updateTheme = true;
                theme = pendingTheme;
                tags = pendingTags;
            }

            if (!updateTheme)
            {
                return;
            }

            hasPending = false;
            if (reloadTheme)
            {
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
                }
                DMMixer.DMusicSetMusic(mixer, music, em);
                currentTags = tags;
            }
            DMMixer.DMusicSetMusicVolume(mixer, pendingTheme.vol);
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
            string name = result + "_" + (isDay ? "DAY" : "NGT") + "_" + "STD";

            var theme = PxVm.InitializeMusic(PhoenixBridge.VmMusicPtr, name);

            reloadTheme = pendingTheme.file != theme.file;
            pendingTheme = theme;
            pendingTags = tags;

            hasPending = true;
        }

        public void SetMenuMusic()
        {
            var theme = PxVm.InitializeMusic(PhoenixBridge.VmMusicPtr, "SYS_Menu");
            reloadTheme = true;
            pendingTheme = theme;
            hasPending = true;
        }
    }
}