using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ZWaveLib.Tests;

public class SerialFrameReaderTests
{
    // SERIAL_API_GET_INIT_DATA response with the standard 29-byte node mask.
    private static byte[] InitResponse()
    {
        byte[] frame = new byte[39];
        frame[0] = (byte)FrameHeader.SOF;
        frame[1] = (byte)(frame.Length - 2);
        frame[2] = (byte)MessageType.Response;
        frame[3] = (byte)ZWaveFunction.GetInitData;
        frame[4] = 5;
        frame[6] = 29;
        frame[7] = 0x07;
        frame[36] = 5;
        frame[37] = 1;
        return new ZWaveMessage(frame).RawData;
    }

    [TestCase(false)]
    [TestCase(true)]
    public void DeliversCompleteResponseInOneRead(bool withAck)
    {
        var frame = InitResponse();
        var wire = withAck ? new[] { (byte)FrameHeader.ACK }.Concat(frame).ToArray() : frame;
        var received = new List<byte[]>();
        new SerialFrameReader().Read(wire, received.Add);
        Assert.That(received, Is.EqualTo(new[] { frame }));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void DeliversTheSameResponseAtEveryReadBoundary(bool withAck)
    {
        var frame = InitResponse();
        var wire = withAck ? new[] { (byte)FrameHeader.ACK }.Concat(frame).ToArray() : frame;
        for (int split = 0; split <= wire.Length; split++)
        {
            var reader = new SerialFrameReader();
            var received = new List<byte[]>();
            if (split > 0) reader.Read(wire[..split], received.Add);
            Assert.That(received.Count, Is.EqualTo(split == wire.Length ? 1 : 0),
                $"Premature or missing delivery at split {split}");
            if (split < wire.Length) reader.Read(wire[split..], received.Add);
            Assert.That(received, Has.Count.EqualTo(1), $"Split {split}");
            Assert.That(received[0], Is.EqualTo(frame), $"Split {split}");
        }
    }

    [TestCase(1)]
    [TestCase(7)]
    [TestCase(100)]
    public void PreservesFrameOrderAcrossConsecutiveResponsesAndControls(int readSize)
    {
        var first = InitResponse();
        var second = new ZWaveMessage(new byte[] { 1, 4, 1, (byte)ZWaveFunction.GetControllerCapabilities, 0, 0 }).RawData;
        byte[] ack = { (byte)FrameHeader.ACK };
        byte[] can = { (byte)FrameHeader.CAN };
        byte[] nak = { (byte)FrameHeader.NAK };
        var wire = ack.Concat(first).Concat(second).Concat(can).Concat(nak).Concat(ack).ToArray();
        var reader = new SerialFrameReader();
        var received = new List<byte[]>();
        foreach (var chunk in wire.Chunk(readSize)) reader.Read(chunk, received.Add);
        Assert.That(received, Is.EqualTo(new[] { first, second, can, nak }));
    }
}
