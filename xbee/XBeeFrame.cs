using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace xbee
{
    public class XBeeFrame
    {
        public XBeeFrame(IList<byte> frameData)
        {
            FrameData = (frameData is ReadOnlyCollection<byte>) ? frameData : new ReadOnlyCollection<byte>(frameData);
        }

        public IList<byte> FrameData { get; private set; }

        public XBeeFrameType FrameType
        {
            get { return (XBeeFrameType)FrameData[0]; }
        }

        public ulong DataValue(int startIndex, int numBytes)
        {
            ulong result = 0;

            for (var i = 0; i < numBytes; i++)
                result = (result << 8) | FrameData[startIndex + i];

            return result;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", FrameType, BitConverter.ToString(FrameData.ToArray()));
        }
    }
}