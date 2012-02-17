using System;

namespace xbee
{
    public class XBeeATCommandFrame
    {
        public XBeeATCommandFrame(ulong destinationAddress, string command, byte parameter = (byte)0)
        {
            _destinationAddress = destinationAddress;
            _command = command;
            Parameter = parameter;
        }

        private readonly ulong _destinationAddress;
        private readonly string _command;
        public byte Parameter { get; set; }

        public XBeeFrame GetFrame()
        {
            var destination = GetBytes(_destinationAddress);

            return new XBeeFrame(new byte[]
                {
                    0x17, // Frame Type - remote AT command
                    0x00, // No response requested

                    destination[0], // 64-bit destination address
                    destination[1], //
                    destination[2], //
                    destination[3], //
                    destination[4], //
                    destination[5], //
                    destination[6], //
                    destination[7], //

                    0xFF, // 16-bit destination address (unknown)
                    0xFE, //

                    0x02, // Apply changes immediately

                    (byte)_command[0], // AT command
                    (byte)_command[1], //

                    Parameter
                });
        }

        private byte[] GetBytes(ulong value)
        {
            var bytes = new byte[8];

            for (var i = 0; i < 8; i++)
            {
                bytes[7 - i] = (byte)value;
                value = value >> 8;
            }

            return bytes;
        }
    }
}