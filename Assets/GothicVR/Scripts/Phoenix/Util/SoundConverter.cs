using UnityEngine;
using System.Text;
using System;

namespace GVR.Phoenix.Util
{
    public class SoundConverter
    {
        const int BlockSize_16Bit = 2;

        public static AudioClip ToAudioClip(byte[] fileBytes, int offsetSamples = 0, string name = "wav")
        {
            string riff = Encoding.ASCII.GetString(fileBytes, 0, 4);
            string wave = Encoding.ASCII.GetString(fileBytes, 8, 4);
            int subchunk1 = BitConverter.ToInt32(fileBytes, 16);
            UInt16 audioFormat = BitConverter.ToUInt16(fileBytes, 20);

            float[] data;


            // NB: Only uncompressed PCM wav files are supported.
            string formatCode = FormatCode(audioFormat);

            Debug.AssertFormat(audioFormat == 1 || audioFormat == 17 || audioFormat == 65534, "Detected format code '{0}' {1}, but only PCM and WaveFormatExtensable uncompressed formats are currently supported.", audioFormat, formatCode);

            UInt16 channels = BitConverter.ToUInt16(fileBytes, 22);
            int sampleRate = BitConverter.ToInt32(fileBytes, 24);
            int byteRate = BitConverter.ToInt32(fileBytes, 28);
            UInt16 blockAlign = BitConverter.ToUInt16(fileBytes, 32);
            UInt16 bitDepth = BitConverter.ToUInt16(fileBytes, 34);

            int headerOffset = 16 + 4 + subchunk1 + 4;
            int subchunk2 = BitConverter.ToInt32(fileBytes, headerOffset);

            if (audioFormat == 17)
            {
                return ToAudioClip(AddWavHeader(ImaAdpcmDecoder.Decode(fileBytes, fileBytes.Length * 2), sampleRate, 16, (short)channels), offsetSamples, name);
            }
            Debug.LogFormat("riff={0} wave={1} subchunk1={2} format={3} channels={4} sampleRate={5} byteRate={6} blockAlign={7} bitDepth={8} headerOffset={9} subchunk2={10} filesize={11}", riff, wave, subchunk1, formatCode, channels, sampleRate, byteRate, blockAlign, bitDepth, headerOffset, subchunk2, fileBytes.Length);
            switch (bitDepth)
            {
                case 8:
                    data = Convert8BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);
                    break;
                case 16:
                    data = Convert16BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);
                    break;
                default:
                    throw new Exception(bitDepth + " bit depth is not supported.");
            }


            AudioClip audioClip = AudioClip.Create(name, data.Length, (int)channels, sampleRate, false);
            audioClip.SetData(data, 0);
            return audioClip;
        }

        #region wav file bytes to Unity AudioClip conversion methods

        private static float[] Convert8BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = BitConverter.ToInt32(source, headerOffset);
            headerOffset += sizeof(int);
            Debug.AssertFormat(wavSize > 0 && wavSize == dataSize, "Failed to get valid 8-bit wav size: {0} from data bytes: {1} at offset: {2}", wavSize, dataSize, headerOffset);

            float[] data = new float[wavSize];

            sbyte maxValue = sbyte.MaxValue;

            int i = 0;
            while (i < wavSize)
            {
                data[i] = (float)source[i] / maxValue;
                ++i;
            }

            return data;
        }

        private static float[] Convert16BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = BitConverter.ToInt32(source, headerOffset);
            headerOffset += sizeof(int);
            Debug.AssertFormat(wavSize > 0 && wavSize == dataSize, "Failed to get valid 16-bit wav size: {0} from data bytes: {1} at offset: {2}", wavSize, dataSize, headerOffset);

            int x = sizeof(Int16); // block size = 2
            int convertedSize = wavSize / x;

            float[] data = new float[convertedSize];

            Int16 maxValue = Int16.MaxValue;

            int offset = 0;
            int i = 0;
            while (i < convertedSize)
            {
                offset = i * x + headerOffset;
                data[i] = (float)BitConverter.ToInt16(source, offset) / maxValue;
                ++i;
            }

            Debug.AssertFormat(data.Length == convertedSize, "AudioClip .wav data is wrong size: {0} == {1}", data.Length, convertedSize);

            return data;
        }

        #endregion

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
                    Debug.LogWarning("Unknown wav code format:" + code);
                    return "";
            }
        }

        public static byte[] AddWavHeader(short[] pcmData, int sampleRate, short bitsPerSample, short channels)
        {
            byte[] header = new byte[44];

            // Chunk ID
            header[0] = (byte)'R';
            header[1] = (byte)'I';
            header[2] = (byte)'F';
            header[3] = (byte)'F';

            // Chunk size
            int chunkSize = pcmData.Length * (bitsPerSample / 8) * channels + 36;
            header[4] = (byte)(chunkSize & 0xff);
            header[5] = (byte)((chunkSize >> 8) & 0xff);
            header[6] = (byte)((chunkSize >> 16) & 0xff);
            header[7] = (byte)((chunkSize >> 24) & 0xff);

            // Format
            header[8] = (byte)'W';
            header[9] = (byte)'A';
            header[10] = (byte)'V';
            header[11] = (byte)'E';

            // Sub-chunk 1 ID
            header[12] = (byte)'f';
            header[13] = (byte)'m';
            header[14] = (byte)'t';
            header[15] = (byte)' ';

            // Sub-chunk 1 size
            int subchunk1Size = 16;
            header[16] = (byte)(subchunk1Size & 0xff);
            header[17] = (byte)((subchunk1Size >> 8) & 0xff);
            header[18] = (byte)((subchunk1Size >> 16) & 0xff);
            header[19] = (byte)((subchunk1Size >> 24) & 0xff);

            // Audio format
            short audioFormat = 1; // PCM
            header[20] = (byte)(audioFormat & 0xff);
            header[21] = (byte)((audioFormat >> 8) & 0xff);

            // Number of channels
            header[22] = (byte)(channels & 0xff);
            header[23] = (byte)((channels >> 8) & 0xff);

            // Sample rate
            header[24] = (byte)(sampleRate & 0xff);
            header[25] = (byte)((sampleRate >> 8) & 0xff);
            header[26] = (byte)((sampleRate >> 16) & 0xff);
            header[27] = (byte)((sampleRate >> 24) & 0xff);

            // Byte rate
            int byteRate = sampleRate * channels * (bitsPerSample / 8);
            header[28] = (byte)(byteRate & 0xff);
            header[29] = (byte)((byteRate >> 8) & 0xff);
            header[30] = (byte)((byteRate >> 16) & 0xff);
            header[31] = (byte)((byteRate >> 24) & 0xff);

            // Block align
            short blockAlign = (short)(channels * (bitsPerSample / 8));
            header[32] = (byte)(blockAlign & 0xff);
            header[33] = (byte)((blockAlign >> 8) & 0xff);

            // Bits per sample
            header[34] = (byte)(bitsPerSample & 0xff);
            header[35] = (byte)((bitsPerSample >> 8) & 0xff);

            // Sub-chunk 2 ID
            header[36] = (byte)'d';
            header[37] = (byte)'a';
            header[38] = (byte)'t';
            header[39] = (byte)'a';

            // Sub-chunk 2 size
            int subchunk2Size = pcmData.Length * (bitsPerSample / 8) * channels;
            header[40] = (byte)(subchunk2Size & 0xff);
            header[41] = (byte)((subchunk2Size >> 8) & 0xff);
            header[42] = (byte)((subchunk2Size >> 16) & 0xff);
            header[43] = (byte)((subchunk2Size >> 24) & 0xff);

            byte[] wavData = new byte[header.Length + pcmData.Length * (bitsPerSample / 8) * channels];

            Array.Copy(header, wavData, header.Length);

            int offset = header.Length;

            for (int i = 0; i < pcmData.Length; i++)
            {
                for (int j = 0; j < channels; j++)
                {
                    short sample = pcmData[i];
                    byte[] bytes = BitConverter.GetBytes(sample);
                    wavData[offset++] = bytes[0];
                    wavData[offset++] = bytes[1];
                }
            }

            return wavData;
        }
    }
}