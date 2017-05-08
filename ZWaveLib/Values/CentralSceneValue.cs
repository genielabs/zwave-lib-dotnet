using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZWaveLib.Values
{
    public enum CentralScenePressType
    {
        Pressed1times = 0x0,
        Released = 0x1,
        HeldDown = 0x2,
        Pressed2times = 0x3,
        Pressed3times = 0x04,
        Pressed4times = 0x05,
        Pressed5times = 0x06,
        Reserved = 0x07,
    }

    public enum CentralSceneValueID
    {
        SceneCount = 0x00,
        Scene_KeyAttribute = 0x01,
        SceneID = 0x02,
        Button = 0x03,
        Scenes_Identical = 0x04,
        Supported_KeyAttributes_All_Scenes = 0x05,
        Supported_KeyAttributes_Scene_1 = 0x06,
        Supported_KeyAttributes_Scene_2 = 0x07,
        Supported_KeyAttributes_Scene_3 = 0x08,
        SceneNumber = 0x80,
    }

    public class CentralSceneValue
    {
        // Layout
        // 1 = MessageType
        // 2 = Level- up from 0x75
        // 3 = KeyAttribute
        // 4 = Scene ID
        public EventParameter EventType = EventParameter.CentralScene;
        public byte SceneId; // byte 4
        public byte Level; // byte 2 
        public CentralScenePressType PressType; // byte 3

        public static CentralSceneValue Parse(byte[] message)
        {
            int keyAttribute = message[3];

            // Hack for Fibaro Keyfob
            if (keyAttribute > 0x80)
                keyAttribute -= 0x80;

            return new CentralSceneValue()
            {
                Level = message[2],
                SceneId = message[4],
                PressType = (CentralScenePressType) keyAttribute
            };
            
        }

        public override string ToString()
        {
            return $"SceneId: {SceneId}, Level: {Level}, PressType: {PressType}";
        }
    }
}
