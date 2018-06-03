using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZWaveLib.Values
{
    public enum CentralScenePressType
    {
        Pressed1Time = 0x00,
        Released = 0x01,
        HeldDown = 0x02,
        Pressed2Times = 0x03,
        Pressed3Times = 0x04,
        Pressed4Times = 0x05,
        Pressed5Times = 0x06,
        Reserved = 0x07,
    }
    
    public class CentralSceneValue
    {
        // Layout
        // 1 = MessageType
        // 2 = Level- up from 0x75
        // 3 = KeyAttribute
        // 4 = Scene ID
        public EventParameter EventType = EventParameter.CentralSceneNotification;
        public byte SceneId; // byte 4
        public byte Level; // byte 2 
        public CentralScenePressType PressType; // byte 3

        public static CentralSceneValue Parse(byte[] message)
        {
            return new CentralSceneValue()
            {
                Level = message[2],
                SceneId = message[4],
                PressType = (CentralScenePressType) (message[3] & 0x07)
            };

        }

        public override string ToString()
        {
            return $"SceneId: {SceneId}, Level: {Level}, PressType: {PressType}";
        }
    }
}
