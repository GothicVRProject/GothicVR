﻿using System;
using System.Text;
using GVR.Data;
using GVR.Globals;
using JetBrains.Annotations;
using UnityEngine;

namespace GVR.Creator.Sounds
{
    public static class SoundCreator
    {
        private enum BitDepth
        {
            BIT8 = 8,
            BIT16 = 16
        }

        [CanBeNull]
        public static SoundData GetSoundArrayFromVfs(string name)
        {
            var node = GameData.Vfs.Find(name);
            if (node == null)
                return null;

            try
            {
                var wavFile = node.Buffer.Bytes;
                
                var channels = BitConverter.ToUInt16(wavFile, 22);
                var sampleRate = BitConverter.ToInt32(wavFile, 24);
                
                var floatArray = ConvertWavByteArrayToFloatArray(wavFile);
                var soundArray = new float[floatArray.Length];
                Array.Copy(floatArray, soundArray, floatArray.Length);
                return new SoundData
                {
                    sound = soundArray,
                    channels = channels,
                    sampleRate = sampleRate
                };
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        public static AudioClip ToAudioClip(SoundData wavFile)
        {
            var audioClip = AudioClip.Create("Sound", wavFile.sound.Length, wavFile.channels, wavFile.sampleRate, false);
            audioClip.SetData(wavFile.sound, 0);
            
            return audioClip;
        }
        
        private static float[] ConvertWavByteArrayToFloatArray(byte[] fileBytes)
        {
            var riff = Encoding.ASCII.GetString(fileBytes, 0, 4);
            var wave = Encoding.ASCII.GetString(fileBytes, 8, 4);
            var subchunk1 = BitConverter.ToInt32(fileBytes, 16);
            var audioFormat = BitConverter.ToUInt16(fileBytes, 20);

            float[] data;

            var formatCode = FormatCode(audioFormat);

            var channels = BitConverter.ToUInt16(fileBytes, 22);
            var sampleRate = BitConverter.ToInt32(fileBytes, 24);
            var byteRate = BitConverter.ToInt32(fileBytes, 28);
            var blockAlign = BitConverter.ToUInt16(fileBytes, 32);
            var bitDepth = BitConverter.ToUInt16(fileBytes, 34);

            var headerOffset = 16 + 4 + subchunk1 + 4;
            var subchunk2 = BitConverter.ToInt32(fileBytes, headerOffset);

            if (formatCode == "IMA ADPCM")
            {
                return ConvertWavByteArrayToFloatArray(IMAADPCMDecoder.Decode(fileBytes));
            }

            data = ConvertByteArrayToFloatArray(fileBytes, headerOffset, (BitDepth)bitDepth);
            return data;
        }


        private static float[] ConvertByteArrayToFloatArray(byte[] source, int headerOffset, BitDepth bit)
        {
            if (bit == BitDepth.BIT8)
            {
                var wavSize = BitConverter.ToInt32(source, headerOffset);
                headerOffset += sizeof(int);

                var data = new float[wavSize];

                var maxValue = sbyte.MaxValue;

                for (var i = 0; i < wavSize; i++)
                    data[i] = (float)source[i] / maxValue;

                return data;
            }
            else if (bit == BitDepth.BIT16)
            {
                var bytesPerSample = sizeof(Int16); // block size = 2
                var sampleCount = source.Length / bytesPerSample;

                var data = new float[sampleCount];

                var maxValue = Int16.MaxValue;

                for (int i = 0; i < sampleCount; i++)
                {
                    var offset = i * bytesPerSample;
                    var sample = BitConverter.ToInt16(source, offset);
                    var floatSample = (float)sample / maxValue;
                    data[i] = floatSample;
                }

                return data;
            }
            else
            {
                throw new Exception(bit + " bit depth is not supported.");
            }
        }

        private static string FormatCode(UInt16 code)
        {
            switch (code)
            {
                case 1:
                    return "PCM";
                case 2:
                    return "ADPCM";
                case 3:
                    return "IEEE";
                case 7:
                    return "μ-law";
                case 17:
                    return "IMA ADPCM";
                case 65534:
                    return "WaveFormatExtensable";
                default:
                    return "";
            }
        }
    }
}
