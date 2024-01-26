using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace GVR.Creator.Sounds
{
    public static class IMAADPCMDecoder
    {
        private enum WAVE_FORMAT
        {
            UNKNOWN,
            PCM,
            ADPCM,
            IMA_ADPCM = 0x11,
        }
        
        public static byte[] CreatePCMHeader(int sampleRate, int channels, int bitsPerSample, int dataSize)
        {
            var header = new List<byte>();

            header.AddRange(Encoding.ASCII.GetBytes("RIFF"));
            int fileSize = dataSize + 36;
            header.AddRange(BitConverter.GetBytes(fileSize));
            header.AddRange(Encoding.ASCII.GetBytes("WAVE"));
            header.AddRange(Encoding.ASCII.GetBytes("fmt "));
            header.AddRange(BitConverter.GetBytes(16));
            header.AddRange(BitConverter.GetBytes((short)1));
            header.AddRange(BitConverter.GetBytes((short)channels));
            header.AddRange(BitConverter.GetBytes(sampleRate));
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            header.AddRange(BitConverter.GetBytes(byteRate));
            short blockAlign = (short)(channels * bitsPerSample / 8);
            header.AddRange(BitConverter.GetBytes(blockAlign));
            header.AddRange(BitConverter.GetBytes((short)bitsPerSample));
            header.AddRange(Encoding.ASCII.GetBytes("data"));
            header.AddRange(BitConverter.GetBytes(dataSize));
            return header.ToArray();
        }

        private static int ReadHeader(Stream stream, bool forDecode, bool forceMonoEncode)
        {
            var riffLength = 0;
            if (ReadId(stream) != "RIFF")
                throw new ApplicationException("Invalid RIFF header");
            riffLength = ReadInt32(stream);
            if (ReadId(stream) != "WAVE")
                throw new ApplicationException("Wave type is expected");
            int fmtSize = 0;
            dataSize = 0;
            while (stream.Position < stream.Length)
            {
                switch (ReadId(stream))
                {
                    case "fmt ":
                        fmtSize = ReadInt32(stream);
                        if (forDecode)
                        {
                            if (ReadUInt16(stream) != (ushort)WAVE_FORMAT.IMA_ADPCM)
                                throw new ApplicationException("Not IMA ADPCM");
                        }
                        else
                        {
                            if (ReadUInt16(stream) != (ushort)WAVE_FORMAT.PCM)
                                throw new ApplicationException("Not PCM");
                        }
                        inChannels = ReadUInt16(stream);
                        samplesPerSecond = ReadInt32(stream);
                        ReadInt32(stream);
                        blockAlign = ReadUInt16(stream);
                        if (forDecode)
                        {
                            if (ReadUInt16(stream) != 4)
                                throw new ApplicationException("Not 4-bit format");
                        }
                        else
                        {
                            if (ReadUInt16(stream) != 16)
                                throw new ApplicationException("Not 16-bit format");
                        }
                        ReadBytes(stream, fmtSize - 16);
                        break;
                    case "data":
                        dataSize = ReadInt32(stream);
                        offset = (int)stream.Position;
                        stream.Position += dataSize;
                        break;
                    default:
                        int size = ReadInt32(stream);
                        stream.Position += size;
                        break;
                }
            }
            if (fmtSize == 0)
                throw new ApplicationException("No format information");
            else if (dataSize == 0)
                throw new ApplicationException("No data");

            int blocks = (int)(dataSize / blockAlign);
            int blocklen;
            int dataLength;
            int bytesPerSecond;

            outChannels = (forceMonoEncode && !forDecode ? (ushort)1 : inChannels);

            if (forDecode)
            {
                blocklen = ((blockAlign - (inChannels * 4)) * 4) + (inChannels * 2); // 4=bits 2 = 16bit (2 bytes)  - How much to pull from source stream
                dataLength = blocks * blocklen;
                bytesPerSecond = samplesPerSecond * outChannels * 2;
            }
            else
            {
                imaBlockAlign = 36 * outChannels;

                // compressed data without header (4 is header per channel)
                int imaDataOnly = (imaBlockAlign - (outChannels * 4));

                //(How many uncompressed samples fit in a block) + (how many headers)
                dataLength = ((dataSize / (imaDataOnly * 4)) * imaDataOnly) + ((dataSize / (imaDataOnly * 4)) * (outChannels * 4));

                // crop off any decimal points.  Each channel will shrink by 1 quarter + 4 bytes per block + channel
                bytesPerSecond = (int)(samplesPerSecond * 0.5625 * outChannels);

                predictedValues = new short[outChannels];
                stepIndexes = new int[outChannels];
            }
            
            if (inChannels > outChannels)
                dataLength /= 2;

            length = dataLength + (!forDecode ? 48 : 44);

            header = CreatePCMHeader(samplesPerSecond, outChannels, 16, dataLength);

            return header.Length;
        }

        public static byte[] Decode(byte[] srcBytes)
        {
            using (MemoryStream s = new MemoryStream(srcBytes))
            {
                ReadHeader(s, true, false);
                List<byte> decodedData = new List<byte>(header.Length + dataSize);

                decodedData.AddRange(header);

                int blocks = dataSize / blockAlign;

                for (int i = 0; i < blocks; i++)
                {
                    byte[] block = DecodeBlock(s, i);
                    decodedData.AddRange(block);
                }

                header = null;
                return decodedData.ToArray();
            }

            // Clean up resources here if needed
        }

        [CanBeNull]
        private static byte[] DecodeBlock(Stream stream, int source)
        {
            if (source >= dataSize / blockAlign)
                return null;
            if (cacheNo == source)
                return cache;

            int position = offset + (source * blockAlign); //4 = compression ratio
            if (position >= stream.Length)
                return null;

            stream.Position = position;
            byte[] data = ReadBytes(stream, blockAlign);

            using (MemoryStream memStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memStream))
                {
                    SampleValue[] values = new SampleValue[outChannels];
                    for (int channel = 0; channel < outChannels; channel++)
                    {
                        values[channel] = new SampleValue(data, channel * 4);
                    }
                    int bytesPerChannelBlock = outChannels * 4;

                    if (outChannels == 1) //mono
                    {
                        for (int i = bytesPerChannelBlock; i < blockAlign; i++)
                        {
                            writer.Write(values[0].DecodeNext(data[i] & 0xf));
                            writer.Write(values[0].DecodeNext(data[i] >> 4));
                        }
                    }
                    else
                    {
                        for (int i = bytesPerChannelBlock; i < blockAlign; i += bytesPerChannelBlock)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                for (int channel = 0; channel < outChannels; channel++)
                                    writer.Write(values[channel].DecodeNext(data[i + j + channel * 4] & 0xf));
                                for (int channel = 0; channel < outChannels; channel++)
                                    writer.Write(values[channel].DecodeNext(data[i + j + channel * 4] >> 4));
                            }
                        }
                    }
                    cacheNo = source;
                    cache = memStream.ToArray();
                }
            }
            return cache;
        }

        private struct SampleValue
        {
            public short PredictedValue;
            public int StepIndex;

            public SampleValue(short predictedValue, int stepIndex)
            {
                this.PredictedValue = predictedValue;
                this.StepIndex = stepIndex;
            }

            public SampleValue(byte[] value, int stepIndex)
            {
                PredictedValue = BitConverter.ToInt16(value, stepIndex);
                StepIndex = value[stepIndex + 2];
            }

            private static readonly int[] StepTable = new[]
            {
                7, 8, 9, 10, 11, 12, 13, 14,
                16, 17, 19, 21, 23, 25, 28, 31,
                34, 37, 41, 45, 50, 55, 60, 66,
                73, 80, 88, 97, 107, 118, 130, 143,
                157, 173, 190, 209, 230, 253, 279, 307,
                337, 371, 408, 449, 494, 544, 598, 658,
                724, 796, 876, 963, 1060, 1166, 1282, 1411,
                1552, 1707, 1878, 2066, 2272, 2499, 2749, 3024,
                3327, 3660, 4026, 4428, 4871, 5358, 5894, 6484,
                7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
                15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794,
                32767
            };

            private static readonly int[] IndexTable = new[]
            {
                -1, -1, -1, -1, 2, 4, 6, 8,
                -1, -1, -1, -1, 2, 4, 6, 8
            };

            public short DecodeNext(int adpcm)
            {
                int step = StepTable[StepIndex];
                int diff = ((((adpcm & 7) << 1) + 1) * step) >> 3;

                if ((adpcm & 8) != 0)
                    diff = -diff;
                int predictedValue = ((int)PredictedValue) + diff;

                PredictedValue = (short)Math.Clamp(predictedValue, short.MinValue, short.MaxValue);


                int idx = StepIndex + IndexTable[adpcm];
                if (idx >= StepTable.Length) idx = StepTable.Length - 1;
                if (idx < 0) idx = 0;

                StepIndex = idx;
                return PredictedValue;
            }
        }

        private static byte[] ReadBytes(Stream s, int length)
        {
            byte[] ret = new byte[length];
            if (length > 0) s.Read(ret, 0, length);
            return ret;
        }

        private static string ReadId(Stream s) { return Encoding.UTF8.GetString(ReadBytes(s, 4), 0, 4); }
        private static int ReadInt32(Stream s) { return BitConverter.ToInt32(ReadBytes(s, 4), 0); }
        private static ushort ReadUInt16(Stream s) { return BitConverter.ToUInt16(ReadBytes(s, 2), 0); }

        private static int length;
        private static ushort inChannels;
        private static ushort outChannels;
        private static int samplesPerSecond;
        private static ushort blockAlign;
        private static int offset;
        private static int dataSize;
        [CanBeNull] private static byte[] header;
        private static int cacheNo = -1;
        private static byte[] cache;

        private static int imaBlockAlign;
        private static short[] predictedValues;
        private static int[] stepIndexes;
    }
}
