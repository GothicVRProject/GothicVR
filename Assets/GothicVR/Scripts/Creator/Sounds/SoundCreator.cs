using System;
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
            if (GameData.Vfs == null)
                return null;

            var node = GameData.Vfs.Find(name);
            if (node == null)
                return null;

            try
            {
                var wavFile = node.Buffer.Bytes;

                var soundData = ConvertWavByteArrayToFloatArray(wavFile);
                return soundData;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        public static AudioClip ToAudioClip(SoundData wavFile)
        {
            AudioClip audioClip;

            try
            {
                audioClip =
                    AudioClip.Create("Sound", wavFile.sound.Length / wavFile.channels, wavFile.channels,
                        wavFile.sampleRate, false);
                audioClip.SetData(wavFile.sound, 0);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                audioClip = AudioClip.Create("Sound", 1, 1, 44100, false);
                audioClip.SetData(new float[] { 0 }, 0); // almost empty audio
            }

            return audioClip;
        }

        private static SoundData ConvertWavByteArrayToFloatArray(byte[] fileBytes)
        {
            var riff = Encoding.ASCII.GetString(fileBytes, 0, 4);
            var wave = Encoding.ASCII.GetString(fileBytes, 8, 4);
            var subchunk1 = BitConverter.ToInt32(fileBytes, 16);
            var audioFormat = BitConverter.ToUInt16(fileBytes, 20);

            var formatCode = FormatCode(audioFormat);

            var channels = BitConverter.ToUInt16(fileBytes, 22);
            var sampleRate = BitConverter.ToInt32(fileBytes, 24);
            var byteRate = BitConverter.ToInt32(fileBytes, 28);
            var blockAlign = BitConverter.ToUInt16(fileBytes, 32);
            var bitDepth = BitConverter.ToUInt16(fileBytes, 34);

            // Calculate header offset and data size
            var headerOffset = 20 + subchunk1;
            var dataSizeOffset = headerOffset + 4;
            if(dataSizeOffset + 4 > fileBytes.Length)
                throw new ArgumentException("Invalid WAV file structure.");

            var subchunk2 = BitConverter.ToInt32(fileBytes, dataSizeOffset);

            // Ensure that subchunk2 does not exceed fileBytes length
            var dataAvailable = fileBytes.Length - (dataSizeOffset + 4);
            if(subchunk2 > dataAvailable)
            {
                subchunk2 = dataAvailable;
            }
            
            if (formatCode == "IMA ADPCM")
            {
                return ConvertWavByteArrayToFloatArray(IMAADPCMDecoder.Decode(fileBytes));
            }

            // Copy WAV data section into a new array
            var data = new byte[subchunk2];
            Array.Copy(fileBytes, dataSizeOffset + 4, data, 0, subchunk2);

            return new SoundData
            {
                sound = ConvertByteArrayToFloatArray(data, 0, (BitDepth)bitDepth),
                channels = channels,
                sampleRate = sampleRate
            };
        }


        private static float[] ConvertByteArrayToFloatArray(byte[] source, int headerOffset, BitDepth bit)
        {
            switch (bit)
            {
                case BitDepth.BIT8:
                {
                    var wavSize = BitConverter.ToInt32(source, headerOffset);

                    var data = new float[wavSize];

                    var maxValue = sbyte.MaxValue;

                    for (var i = 0; i < wavSize; i++)
                        data[i] = (float)source[i] / maxValue;

                    return data;
                }
                case BitDepth.BIT16:
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
                default:
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
