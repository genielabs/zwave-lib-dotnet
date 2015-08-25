using System;

namespace ZWaveLib
{

    public enum QueryStage
    {
        NotSet,
        WaitAck,
        SendDataReady,
        Complete,
        Error
    }

    public enum FrameHeader : byte
    {
        SOF = 0x01,
        ACK = 0x06,
        NAK = 0x15,
        CAN = 0x18
    }

    public enum MessageDirection : byte
    {
        Inbound = 0x00,
        Outbound = 0x01
    }

    public enum MessageType : byte
    {
        Request = 0x00,
        Response = 0x01,
        NotSet = 0xFF
    }

}

