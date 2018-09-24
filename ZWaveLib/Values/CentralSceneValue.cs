/*
  This file is part of ZWaveLib (https://github.com/genielabs/zwave-lib-dotnet)

  Copyright (2012-2018) G-Labs (https://github.com/genielabs)

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

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
            return $"{{\"SceneId\":{SceneId}, \"Level\":{Level}, \"PressType\":\"{PressType}\"}}";
        }
    }
}
