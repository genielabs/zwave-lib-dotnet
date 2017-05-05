using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZWaveLib.Values
{
    public enum CentralScenePressType
    {
        SingleClick = 0x0,
        Released = 0x1,
        HoldDown = 0x2,
        DoubleClick = 0x3
    }

    public class CentralSceneValue
    {
        public EventParameter EventType = EventParameter.CentralScene;
        public byte ButtonNumber; // byte 4
        public byte Level; // byte 2 - up from 0x75
        public CentralScenePressType PressType; // byte 3

        public static CentralSceneValue Parse(byte[] message)
        {
            return new CentralSceneValue()
            {
                Level = message[2],
                ButtonNumber = message[4],
                PressType = (CentralScenePressType) message[3]
            };
        }

        public override string ToString()
        {
            return $"ButtonNumber: {ButtonNumber}, Level: {Level}, PressType: {PressType}";
        }
    }
}
