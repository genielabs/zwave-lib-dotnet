using System;
using System.Linq;
using NUnit.Framework;
using ZWaveLib.CommandClasses;
using ZWaveLib.CommandClasses.Irrigation;
using ZWaveLib.Values;

namespace ZWaveLib.Tests;

public class CapabilityAndAlarmTests
{
    private readonly ZWaveNode node = new ZWaveNode();

    private NodeEvent Receive(params byte[] command)
    {
        return CommandClassFactory.GetCommandClass(command[0]).GetEvent(node, command);
    }

    [Test]
    public void MeterCapabilitiesIncludeExtendedScalesAndRateTypes()
    {
        var evt = Receive(0x32, 4, 0xe1, 0x85, 2, 1, 2);
        Assert.That(evt.Parameter, Is.EqualTo(EventParameter.MeterSupportedReport));
        var info = (MeterSupportedReport)evt.Value;
        Assert.That(info.MeterType, Is.EqualTo(1));
        Assert.That(info.SupportsReset, Is.True);
        Assert.That(info.RateTypeMask, Is.EqualTo(3));
        Assert.That(info.Scales, Is.EqualTo(new[] { 0, 2, 7, 16 }));
    }

    [Test]
    public void VersionTwoMeterIgnoresReservedBits()
    {
        node.CommandClasses.Add(new NodeCommandClass(0x32, 2));
        var info = (MeterSupportedReport)Receive(0x32, 4, 0xe1, 0xf5).Value;
        Assert.That(info.RateTypeMask, Is.Zero);
        Assert.That(info.Scales, Is.EqualTo(new[] { 0, 2 }));
    }

    [Test]
    public void NotificationDiscoveryUsesWireBitNumbers()
    {
        var info = (NotificationSupportedReport)Receive(0x71, 8, 0x82, 0x83, 2).Value;
        Assert.That(info.SupportsLegacyAlarm, Is.True);
        Assert.That(info.Types, Is.EqualTo(new[] { 1, 7, 9 }));
        var events = (NotificationEventSupportedReport)Receive(0x71, 2, 7, 2, 8, 1).Value;
        Assert.That(events.Type, Is.EqualTo(7));
        Assert.That(events.Events, Is.EqualTo(new[] { 3, 8 }));
        Assert.That(((NotificationEventSupportedReport)Receive(0x71, 2, 7, 0).Value).Events, Is.Empty);
    }

    [TestCase(7, 8, EventParameter.SensorMotion)]
    [TestCase(7, 3, EventParameter.AlarmTampered)]
    [TestCase(5, 2, EventParameter.AlarmFlood)]
    [TestCase(1, 2, EventParameter.AlarmSmoke)]
    [TestCase(6, 0x16, EventParameter.AlarmDoorWindow)]
    public void SpecificIdleClearsTheSameModuleField(int type, int eventId, EventParameter field)
    {
        var active = Receive(0x71, 5, 0, 0, 0, 0, (byte)type, (byte)eventId, 0);
        var idle = Receive(0x71, 5, 0, 0, 0, 0xff, (byte)type, 0, 0x81, (byte)eventId, 9);
        Assert.That(active.Parameter, Is.EqualTo(field));
        Assert.That(active.Value, Is.Not.EqualTo((byte)0));
        Assert.That(idle.Parameter, Is.EqualTo(field));
        Assert.That(idle.Value, Is.EqualTo((byte)0));
        Assert.That(idle.NestedEvent, Is.Null);
    }

    [Test]
    public void EncapsulatedIdleKeepsAllResetEventsAndTheFollowingCommand()
    {
        var root = Receive(0x8f, 1, 2, 13, 0x60, 0x0d, 2, 0, 0x71, 5, 0, 0, 0, 0xff, 7, 0, 0,
            5, 0x32, 2, 1, 0x11, 42);
        foreach (var parameter in new[] { EventParameter.AlarmGeneric, EventParameter.SensorMotion, EventParameter.AlarmTampered })
        {
            Assert.That(root.Instance, Is.EqualTo(2));
            Assert.That(root.Parameter, Is.EqualTo(parameter));
            Assert.That(root.Value, Is.EqualTo((byte)0));
            root = root.NestedEvent!;
        }
        Assert.That(root.Parameter, Is.EqualTo(EventParameter.MeterWatt));
        Assert.That(root.Value, Is.EqualTo(42));
    }

    [Test]
    public void LegacyAlarmAndSensorAlarmPreserveTheirLevels()
    {
        Assert.That(Receive(0x71, 5, 1, 23).Value, Is.EqualTo((byte)23));
        Assert.That(Receive(0x9c, 2, 0, 1, 42).Value, Is.EqualTo((byte)42));
        Assert.That(Receive(0x71, 5, 0, 0, 0, 0xff, 6, 0x17).Value, Is.EqualTo((byte)0));
    }

    [Test]
    public void IncompleteReportsDoNotPublishReadingsOrCapabilities()
    {
        Assert.That(Receive(0x32, 4, 1, 0x80, 2, 1), Is.Null);
        Assert.That(Receive(0x32, 2, 1, 0x14, 42), Is.Null);
        Assert.That(Receive(0x71, 8, 2, 0x80), Is.Null);
        Assert.That(Receive(0x71, 2, 7, 2, 8), Is.Null);
        Assert.That(Receive(0x71, 5, 0, 0, 0, 0xff, 7, 0, 1), Is.Null);
        Assert.That(Receive(0x71, 5, 0, 0, 0, 0xff, 7, 0, 0x81, 8), Is.Null);
    }

    [Test]
    public void GetCommandsEncodeTheRequestedReport()
    {
        using var controller = new ZWaveController();
        var target = new ZWaveNode(controller, 2);
        static byte[] Payload(ZWaveMessage msg) => msg.RawData.Skip(6).Take(msg.RawData[5]).ToArray();
        Assert.That(Payload(Irrigation.SystemConfigGet(target)), Is.EqualTo(new byte[] { 0x6b, 6 }));
        Assert.That(Payload(Irrigation.SystemStatusGet(target)), Is.EqualTo(new byte[] { 0x6b, 3 }));
        Assert.That(Payload(Meter.GetSupported(target)), Is.EqualTo(new byte[] { 0x32, 3 }));
        Assert.That(Payload(Alarm.GetSupported(target)), Is.EqualTo(new byte[] { 0x71, 7 }));
        Assert.That(Payload(Alarm.GetSupportedEvents(target, 7)), Is.EqualTo(new byte[] { 0x71, 1, 7 }));
    }
}
