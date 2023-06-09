using System.IO;
using System;
using System.Text;

namespace GVR.Phoenix.Util
{
    internal enum WAVE_FORMAT
    {
        IMA_ADPCM = 0x11,
    }

    public class ImaAdpcmDecoder
    {

        private int readHeader(Stream stream, bool forDecode, bool forceMonoEncode)
        {
            if (ReadID(stream) != "RIFF")
                throw new ApplicationException("Invalid RIFF header");

            int riffLen = ReadInt32(stream);

            if (ReadID(stream) != "WAVE")
                throw new ApplicationException("Wave type is expected");

            int fmtsize = 0;
            _dataSize = 0;

            while (stream.Position < stream.Length)
            {
                var id = ReadID(stream);
                var size = ReadInt32(stream);

                switch (id)
                {
                    case "fmt ":
                        fmtsize = size;
                        if (ReadUInt16(stream) != (ushort)WAVE_FORMAT.IMA_ADPCM)
                            throw new ApplicationException("Not IMA ADPCM");
                        _inChannels = ReadUInt16(stream);
                        _samplesPerSec = ReadInt32(stream);
                        ReadInt32(stream);
                        _blockAlign = ReadUInt16(stream);
                        if (ReadUInt16(stream) != 4)
                            throw new ApplicationException("Not 4-bit format");
                        ReadBytes(stream, fmtsize - 16);
                        break;

                    case "data":
                        _dataSize = size;
                        _offset = (int)stream.Position;
                        stream.Position += _dataSize;
                        break;

                    default:
                        stream.Position += size;
                        break;
                }
            }

            if (fmtsize == 0 || _dataSize == 0)
                throw new ApplicationException("No format information or data");

            int blocks = (int)(_dataSize / _blockAlign);
            int blocklen = ((_blockAlign - (_inChannels * 4)) * 4) + (_inChannels * 2); //4=bits 2 = 16bit (2 bytes)  - How much to pull from source stream
            int datalen = blocks * blocklen;
            int bytesPerSec = _samplesPerSec * (forceMonoEncode ? (ushort)1 : _inChannels) * 2;
            if (_inChannels > _outChannels)
                datalen /= 2;

            _length = datalen + 44;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(Encoding.UTF8.GetBytes("RIFF"));
                bw.Write(_length - 8);
                bw.Write(Encoding.UTF8.GetBytes("WAVE"));
                bw.Write(Encoding.UTF8.GetBytes("fmt "));
                bw.Write(16);
                bw.Write((ushort)(WAVE_FORMAT.IMA_ADPCM)); // FormatTag
                bw.Write(_outChannels);
                bw.Write(_samplesPerSec);
                bw.Write(bytesPerSec); // AvgBytesPerSec
                bw.Write((ushort)(forDecode ? (_outChannels * 2) : _imaBlockAlign)); // BlockAlign
                bw.Write((ushort)(forDecode ? 16 : 4)); // BitsPerSample
                bw.Write(Encoding.UTF8.GetBytes("data"));
                bw.Write(datalen);
                _header = ms.ToArray();
            }

            return _header.Length;
        }


        public byte[] Decode(byte[] srcData)
        {
            using (var s = new MemoryStream(srcData))
            {
                readHeader(s, true, false);
                using (var fs = new MemoryStream())
                {
                    fs.Write(_header, 0, _header.Length);
                    int blocks = _dataSize / _blockAlign;
                    for (int i = 0; i < blocks; i++)
                    {
                        byte[] block = DecodeBlock(s, i);
                        fs.Write(block, 0, block.Length);
                    }
                    _header = null;
                    return fs.ToArray();
                }
            }
        }

        private byte[] DecodeBlock(Stream s, int src)
        {
            if (src >= _dataSize / _blockAlign) return null;
            if (_cacheNo == src) return _cache;

            int pos = _offset + (src * _blockAlign); //4 = compression ratio
            if (pos >= s.Length) return null;

            s.Position = pos;
            byte[] data = ReadBytes(s, _blockAlign);

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    SampleValue[] v = new SampleValue[_outChannels];
                    for (int ch = 0; ch < _outChannels; ch++)
                    {
                        v[ch] = new SampleValue(data, ch * 4);
                    }
                    int ch4 = _outChannels * 4;

                    if (_outChannels == 1) //mono
                    {
                        for (int i = ch4; i < _blockAlign; i++)
                        {
                            bw.Write(v[0].DecodeNext(data[i] & 0xf));
                            bw.Write(v[0].DecodeNext(data[i] >> 4));
                        }
                    }
                    else
                    {
                        for (int i = ch4; i < _blockAlign; i += ch4)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                for (int ch = 0; ch < _outChannels; ch++)
                                    bw.Write(v[ch].DecodeNext(data[i + j + ch * 4] & 0xf));
                                for (int ch = 0; ch < _outChannels; ch++)
                                    bw.Write(v[ch].DecodeNext(data[i + j + ch * 4] >> 4));
                            }
                        }
                    }
                    _cacheNo = src;
                    _cache = ms.ToArray();
                }
            }
            return _cache;
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

        private byte[] ReadBytes(Stream s, int length)
        {
            byte[] ret = new byte[length];
            if (length > 0) s.Read(ret, 0, length);
            return ret;
        }

        private string ReadID(Stream s) { return Encoding.UTF8.GetString(ReadBytes(s, 4), 0, 4); }
        private int ReadInt32(Stream s) { return BitConverter.ToInt32(ReadBytes(s, 4), 0); }
        private ushort ReadUInt16(Stream s) { return BitConverter.ToUInt16(ReadBytes(s, 2), 0); }

        private int _length;
        private ushort _inChannels;
        private ushort _outChannels;
        private int _samplesPerSec;
        private ushort _blockAlign;
        private int _offset;
        private int _dataSize;
        private byte[] _header;
        private int _cacheNo = -1;
        private byte[] _cache;

        private int _imaBlockAlign;
    }
}