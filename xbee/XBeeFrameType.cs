using System;

namespace xbee
{
    public enum XBeeFrameType : byte
    {
        ATCommandImmediate = 0x08,
        ATCommandQueued = 0x09,
        ATCommandResponse = 0x88,

        RemoteCommandRequest = 0x17,
        RemoteCommandResponse = 0x97,

        TXRequest = 0x10,
        TXResponse = 0x8B,

        RXReceived = 0x90,
        RXIOReceived = 0x92,

        ModemStatus = 0x8A,
        NodeIdentificationIndicator = 0x95
    }
}