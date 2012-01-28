using System;

namespace xbee
{
    public class XBeeIOFrame
    {
        public XBeeIOFrame(XBeeFrame frame)
        {
            Frame = frame;

            SourceAddress = frame.DataValue(1, 8);
            SourceNetworkAddress = (ushort)frame.DataValue(9, 2);
            ReceiveOptions = frame.FrameData[11];
            NumberOfSamples = frame.FrameData[12];
            DigitalChannelMask = (ushort)frame.DataValue(13, 2);
            AnalogChannelMask = frame.FrameData[15];

            var nextIndex = 16;

            if (DigitalChannelMask > 0)
            {
                DigitalSamples = (ushort)frame.DataValue(nextIndex, 2);
                nextIndex += 2;
            }

            if ((AnalogChannelMask & 0x1) > 0)
            {
                Analog0Sample = (ushort)frame.DataValue(nextIndex, 2);
                nextIndex += 2;
            }

            if ((AnalogChannelMask & 0x2) > 0)
            {
                Analog1Sample = (ushort)frame.DataValue(nextIndex, 2);
                nextIndex += 2;
            }

            if ((AnalogChannelMask & 0x4) > 0)
            {
                Analog2Sample = (ushort)frame.DataValue(nextIndex, 2);
                nextIndex += 2;
            }

            if ((AnalogChannelMask & 0x8) > 0)
                Analog3Sample = (ushort)frame.DataValue(nextIndex, 2);
        }

        public XBeeFrame Frame { get; private set; }

        public ulong SourceAddress { get; private set; }
        public ushort SourceNetworkAddress { get; private set; }
        public byte ReceiveOptions { get; private set; }
        public byte NumberOfSamples { get; private set; }
        public ushort DigitalChannelMask { get; private set; }
        public byte AnalogChannelMask { get; private set; }
        public ushort DigitalSamples { get; private set; }
        public ushort Analog0Sample { get; private set; }
        public ushort Analog1Sample { get; private set; }
        public ushort Analog2Sample { get; private set; }
        public ushort Analog3Sample { get; private set; }
    }
}