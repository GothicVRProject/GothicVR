using UnityEngine;
using DMCs.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using System;
using System.IO;

namespace GVR.Demo
{
    public class MusicTest : SingletonBehaviour<MusicTest>
    {

        private IntPtr mixer;
        private IntPtr music;
        private IntPtr directmusic;

        private short[] shortBuffer;
        public int bufferSize = 2048;

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

        public void Create(string path)
        {
            var fullPath = Path.GetFullPath(Path.Join(path, "/_work/DATA/Music/"));

            //create a new mixer
            mixer = DMMixer.DMusicInitMixer();
            music = DMMusic.DMusicInitMusic();
            directmusic = DMDirectMusic.DMusicInitDirectMusic();

            DMDirectMusic.DMusicAddPath(directmusic, Path.GetFullPath(Path.Join(fullPath, "dungeon")));
            DMDirectMusic.DMusicAddPath(directmusic, Path.GetFullPath(Path.Join(fullPath, "menu_men")));
            DMDirectMusic.DMusicAddPath(directmusic, Path.GetFullPath(Path.Join(fullPath, "orchestra")));


            var fileToLoad = "NCI_Day_Std.sgt";

            var pattern = DMDirectMusic.DMusicLoadFile(directmusic, fileToLoad, fileToLoad.Length);

            DMMusic.DMusicAddPattern(music, pattern);

            DMMixer.DMusicSetMusic(mixer, music, 0);

            shortBuffer = new short[bufferSize * 2];

            var buffer = new float[bufferSize * 4];

            var soundObject = new GameObject(string.Format("BACKROUND MUSIC"));
            AudioSource source = soundObject.AddComponent<AudioSource>();
            AudioClip audioClip = AudioClip.Create("Sound", bufferSize * 4, 2, 44100, true, PrepareData);
            audioClip.SetData(buffer, 0);
            source.clip = audioClip;
            source.loop = true;
            source.Play();
        }

        private void PrepareData(float[] data)
        {
            DMMixer.DMusicMix(mixer, shortBuffer, (uint)data.Length / 2);

            byte[] byteArray = new byte[data.Length * 2];
            Buffer.BlockCopy(shortBuffer, 0, byteArray, 0, byteArray.Length);
            var buffer = Convert16BitByteArrayToFloatArray(byteArray, 0, byteArray.Length);
            Array.Copy(buffer, data, buffer.Length);
        }

    }
}