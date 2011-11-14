using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KinectLibrary.Audio
{
    class StreamFilter : Stream
    {
        private readonly Stream _baseStream;
        private int _index = 0;
        private readonly double[] _energy = new double[500];
        private readonly object _syncRoot = new object();
        private const int SamplesPerPixel = 10;
        private int _sampleCount = 0;
        private double _avgSample = 0;

        public StreamFilter(Stream stream)
        {
            _baseStream = stream;
        }

        public override bool CanRead
        {
            get { return _baseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _baseStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _baseStream.CanWrite; }
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override long Length
        {
            get { return _baseStream.Length; }
        }

        public override long Position
        {
            get { return _baseStream.Position; }
            set { _baseStream.Position = value; }
        }

        public void GetEnergy(double[] energyBuffer)
        {
            lock (_syncRoot)
            {
                int energyIndex = _index;
                for (int i = 0; i < _energy.Length; i++)
                {
                    energyBuffer[i] = _energy[energyIndex];
                    energyIndex++;
                    if (energyIndex >= _energy.Length)
                        energyIndex = 0;

                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int retVal = _baseStream.Read(buffer, offset, count);
            double a = 0.3;
            lock (_syncRoot)
            {
                for (int i = 0; i < retVal; i += 2)
                {

                    short sample = BitConverter.ToInt16(buffer, i + offset);
                    _avgSample += sample * sample;
                    _sampleCount++;

                    if (_sampleCount == SamplesPerPixel)
                    {
                        _avgSample /= SamplesPerPixel;

                        _energy[_index] = .2 + (_avgSample * 11) / (int.MaxValue / 2); //2^30 = (2^15)^2
                        _energy[_index] = _energy[_index] > 10 ? 10 : _energy[_index];

                        if (_index > 0)
                            _energy[_index] = _energy[_index] * a + (1 - a) * _energy[_index - 1];

                        _index++;
                        if (_index >= _energy.Length)
                            _index = 0;
                        _avgSample = 0;
                        _sampleCount = 0;
                    }

                }
            }

            return retVal;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Write(buffer, offset, count);
        }
    }
}
