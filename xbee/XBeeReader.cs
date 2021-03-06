using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;

namespace xbee
{
    public class XBeeReader
    {
        private const int _FRAME_START = 0x7E;
        private const int _CONTROL_CHARACTER = 0x7D;
        private const int _CONTROL_CHARACTER_MASK = 0x20;

        public XBeeReader(SerialPort port)
        {
            if (!port.IsOpen) port.Open();
            port.DiscardInBuffer();
            _port = port;
        }

        private readonly SerialPort _port;

        public bool FrameAvailable
        {
            get { return _port.BytesToRead > 0; }
        }

        public void WriteFrame(XBeeFrame frame)
        {
            var data = frame.FrameData.ToArray();
            _port.Write(new byte[] {_FRAME_START}, 0, 1);
            _port.Write(new[] {(byte)((data.Length & 0xFF00) >> 8), (byte)(data.Length & 0xFF)}, 0, 2);
            _port.Write(data, 0, data.Length);
            _port.Write(new[] {CalculateChecksum(data)}, 0, 1);
        }

        public XBeeFrame ReadFrame()
        {
            while (true)
            {
                int b;
                while (_FRAME_START != (b = ReadByte()))
                    Debug.WriteLine("Extra: " + b.ToString("X"));

                var length = (ReadByte() << 8) | ReadByte();

                var frameData = new byte[length];
                for (var i = 0; i < length; i++)
                    frameData[i] = ReadByte();

                var checksum = ReadByte();

                if (ValidateFrameData(frameData, checksum))
                    return new XBeeFrame(frameData);

                Debug.WriteLine(string.Format("Invalid Frame: {0},  Checksum: {1}", BitConverter.ToString(frameData), checksum));
            }
        }

        private byte ReadByte()
        {
            var b = _port.ReadByte();
            return (byte)(b != _CONTROL_CHARACTER ? b : _port.ReadByte() ^ _CONTROL_CHARACTER_MASK);
        }

        private bool ValidateFrameData(byte[] frameData, byte checksum)
        {
            var sum = frameData.Sum(x => (int)x) + checksum;
            return (sum & 0xFF) == 0xFF;
        }

        private byte CalculateChecksum(byte[] frameData)
        {
            var sum = frameData.Sum(x => (int)x);
            return (byte)(0xFF - sum);
        }
    }
}