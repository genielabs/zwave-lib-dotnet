/*
    This file is part of ZWaveLib Project source code.

    ZWaveLib is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ZWaveLib is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ZWaveLib.  If not, see <http://www.gnu.org/licenses/>.  
    
    Code inspired from Security CommandClass in OpenZWave
 
*/

/*
 *     Author: https://github.com/snagytx
 *     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ZWaveLib.Devices;
using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses
{
    
    public class SecutiryPayload
    {
        public byte[] message;
        public int length;
        public int part;
    }

    public class SecurityData
    {
        private byte[] encryptionKey = null;
        private byte[] controllerCurrentNonce = null;
        private byte[] privateNetworkKey = null;
        //private byte[] privateNetworkKey = new byte[] { 0x0F, 0x1E, 0x2D, 0x3C, 0x4B, 0x5A, 0x69, 0x78, 0x87, 0x96, 0xA5, 0xB4, 0xC3, 0xD2, 0xE1, 0xF0 };
        //private byte[] privateNetworkKey = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10 };
        private ZWaveNode parentNode = null;
    
        public byte[] DeviceCurrentNonce = null;

        public Stopwatch ControllerNonceTimer = new Stopwatch();
        public Stopwatch DeviceNonceTimer = new Stopwatch();

        public bool SchemeAgreed = false;
        public bool IsWaitingForNonce = false;
        public bool IsAddingNode = false;
        public bool IsNetworkKeySet = false;

        public SecurityData(ZWaveNode node)
        {
            parentNode = node;
        }

        public void SetPrivateNetworkKey(byte[] key)
        {
            privateNetworkKey = key;
        }

        public byte[] GetPrivateNetworkKey()
        {
            return privateNetworkKey;
        }

        public byte[] GeneratePrivateNetworkKey()
        {
            privateNetworkKey = new byte[16];
            Random rnd = new Random();
            rnd.NextBytes(privateNetworkKey);

            // notify the controller that the privateNetworkKey was generated so that it can save it
            NodeEvent keyGenEvent = new NodeEvent(parentNode, EventParameter.SecurityGeneratedKey, 0, 0);
            parentNode.OnNodeUpdated(keyGenEvent);

            return privateNetworkKey;
        }

        public void GenerateControllerCurrentNonce()
        {
            byte[] localControllerCurrentNonce = new byte[8];

            if (controllerCurrentNonce == null)
            {
                controllerCurrentNonce = new byte[8];
                // we needs to generate one and save it for the next call;
                Random rnd = new Random();
                rnd.NextBytes(controllerCurrentNonce);
            }

          Utility.logger.Debug("ControllerCurrentNonce: " + BitConverter.ToString(localControllerCurrentNonce));
            //return localControllerCurrentNonce;
        }

        public byte[] GetControllerCurrentNonce(bool clearNonce = false)
        {
            byte[] localControllerCurrentNonce = new byte[8];

            // safety check - don't try to copy if the source array is null
            if (controllerCurrentNonce == null)
            {
                Utility.logger.Error("ZWave Security Current Nonce is NULL");
                return controllerCurrentNonce;
            }

            Array.Copy(controllerCurrentNonce, localControllerCurrentNonce, 8);

            if (clearNonce)
            {
                // Ideally we should get in here ONLY when decrypting a message from the device.
                // This would cause the controllerCurrentNonce to be re-generated which would
                // give us the most secure communication, if we see issues we just don't pass
                // the clearNonce argument

                // since for now we don't regenerate it let's not set it to null
                //controllerCurrentNonce = null;
            }

            return localControllerCurrentNonce;
        }

        public byte[] EncryptionKey
        {
            get
            {
                // the EncryptionKey needs to be generated in two cases:
                // 1 - encryptionKey is null
                // 2 - once the PrivateNetworkKey was sent to the device

                // these three arrays seems to be this like this 
                byte[] initialNetworkKey = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };                
                byte[] encryptPassword = new byte[] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA };
                byte[] authPassword = new byte[] { 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55 };

                byte[] networkKey;

                if (IsAddingNode && !IsNetworkKeySet)
                {
                    networkKey = initialNetworkKey;
                  Utility.logger.Debug("In SetupNetworkKey  - in node inclusion mode.");
                }
                else
                {
                    if (privateNetworkKey == null)
                        GeneratePrivateNetworkKey();
                    networkKey = privateNetworkKey;
                }

                encryptionKey = AesWork.EncryptEcbMessage(networkKey, encryptPassword);
                AuthorizationKey = AesWork.EncryptEcbMessage(networkKey, authPassword);

                Utility.logger.Debug("      networkKey: " + BitConverter.ToString(networkKey));
                Utility.logger.Debug("   encryptionKey: " + BitConverter.ToString(encryptionKey));
                Utility.logger.Debug("AuthorizationKey: " + BitConverter.ToString(AuthorizationKey));

                return encryptionKey;
            }
        }

        public byte[] AuthorizationKey = null;

        public List<SecutiryPayload> SecurePayload = new List<SecutiryPayload>();
        public int SequenceCounter = 0;
    }

    public class Security : ICommandClass
    {

        //public byte[] PrivateNetworkKey = new byte[] { 0x0F, 0x1E, 0x2D, 0x3C, 0x4B, 0x5A, 0x69, 0x78, 0x87, 0x96, 0xA5, 0xB4, 0xC3, 0xD2, 0xE1, 0xF0 };
        //public static byte[] PrivateNetworkKey = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10 };

        public CommandClass GetClassId()
        {
            return CommandClass.Security;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            byte cmdType = message[1];
            byte[] decryptedMessage;
            NodeEvent nodeEvent = null;

            int startOffset = 1;
            switch (cmdType)
            {
            case (byte)SecurityCommand.SupportedReport:
                Utility.logger.Debug("Received COMMAND_SUPPORTED_REPORT for node: " + node.Id);
                /* this is a list of CommandClasses that should be Encrypted.
                 * and it might contain new command classes that were not present in the NodeInfoFrame
                 * so we have to run through, mark existing Command Classes as SetSecured (so SendMsg in the Driver
                 * class will route the unecrypted messages to our SendMsg) and for New Command
                 * Classes, create them, and of course, also do a SetSecured on them.
                 *
                 * This means we must do a SecurityCmd_SupportedGet request ASAP so we dont have
                 * Command Classes created after the Discovery Phase is completed!
                 */
                byte[] securedClasses = new byte[message.Length - 3];
                Array.Copy(message, 3, securedClasses, 0, message.Length - 3);
                nodeEvent = new NodeEvent(node, EventParameter.SecurityNodeInformationFrame, securedClasses, 0);
                break;

            case (byte)SecurityCommand.SchemeReport:
                Utility.logger.Debug("Received COMMAND_SCHEME_REPORT for node: " + node.Id + ", " + (startOffset + 1));
                int schemes = message[startOffset + 1];
                SecurityData nodeSecurityData = GetSecurityData(node);
                if (nodeSecurityData.SchemeAgreed)
                {
                    break;
                }
                else if (schemes == (byte)SecurityScheme.SchemeZero)
                {
                    SetNetworkKey(node);
                    nodeSecurityData.SchemeAgreed = true;
                }
                else
                {
                    Utility.logger.Debug("   No common security scheme.  The device will continue as an unsecured node.");
                }
                break;

            case (byte)SecurityCommand.NonceGet:
                Utility.logger.Debug("Received COMMAND_NONCE_GET for node: " + node.Id);

                /* the Device wants to send us a Encrypted Packet, and thus requesting for our latest NONCE */
                SendNonceReport(node);
                break;

            case (byte)SecurityCommand.MessageEncap:
                Utility.logger.Debug("Received COMMAND_MESSAGE_ENCAP for node: " + node.Id);

                /* We recieved a Encrypted single packet from the Device. Decrypt it. */
                decryptedMessage = DecryptMessage(node, message, startOffset);
                if (decryptedMessage != null)
                    nodeEvent = new NodeEvent(node, EventParameter.SecurityDecriptedMessage, decryptedMessage, 0);
                break;

            case (byte)SecurityCommand.NonceReport:
                Utility.logger.Debug("Received COMMAND_NONCE_REPORT for node: " + node.Id);

                /* we recieved a NONCE from a device, so assume that there is something in a queue to send out */
                ProcessNonceReport(node, message, startOffset);
                break;

            case (byte)SecurityCommand.MessageEncapNonceGet:
                Utility.logger.Debug("Received COMMAND_MESSAGE_ENCAP_NONCE_GET for node: " + node.Id);

                /* we recieved a encrypted packet from the device, and the device is also asking us to send a
                     * new NONCE to it, hence there must be multiple packets.*/
                decryptedMessage = DecryptMessage(node, message, startOffset);
                if (decryptedMessage != null)
                    nodeEvent = new NodeEvent(node, EventParameter.SecurityDecriptedMessage, decryptedMessage, 0);
                /* Regardless of the success/failure of Decrypting, send a new NONCE */
                SendNonceReport(node);
                break;

            case (byte)SecurityCommand.NetworkKeySet:
                Utility.logger.Debug("Received COMMAND_NETWORK_KEY_SET for node: " + node.Id + ", " + (startOffset + 1));

                /* we shouldn't get a NetworkKeySet from a node if we are the controller
                     * as we send it out to the Devices
                    */
                break;

            case (byte)SecurityCommand.NetworkKeyVerify:
                Utility.logger.Debug("Received COMMAND_NETWORK_KEY_VERIFY for node: " + node.Id + ", " + (startOffset + 1));

                /*
                     * if we can decrypt this packet, then we are assured that our NetworkKeySet is successfull
                     * and thus should set the Flag referenced in SecurityCmd_SchemeReport
                    */
                GetSupported(node);
                break;

            case (byte)SecurityCommand.SchemeInherit:
                Utility.logger.Debug("Received COMMAND_SCHEME_INHERIT for node: " + node.Id);
                /* only used in a Controller Replication Type enviroment. */
                break;

            }

            return nodeEvent;
        }

        #region Public Security Command Class methods

        public static void GetSupported(ZWaveNode node)
        {
            var message = ZWaveMessage.BuildSendDataRequest(node.Id, new byte[] { 
                (byte)CommandClass.Security,
                (byte)SecurityCommand.SupportedGet
            });
            SendMessage(node, message);
        }

        public static void GetScheme(ZWaveNode node)
        {
            node.SendDataRequest(new byte[] {
                (byte)CommandClass.Security,
                (byte)SecurityCommand.SchemeGet,
                0
            });
        }

        private static void SetNetworkKey(ZWaveNode node)
        {
            byte[] t_msg = new byte[18];
            t_msg[0] = (byte)CommandClass.Security;
            t_msg[1] = (byte)SecurityCommand.NetworkKeySet;
            var privateNetworkKey = GetSecurityData(node).GetPrivateNetworkKey();
            if (privateNetworkKey == null)
            {
                privateNetworkKey = GetSecurityData(node).GeneratePrivateNetworkKey();
            }
            Array.Copy(privateNetworkKey, 0, t_msg, 2, 16);
            byte[] f_msg = ZWaveMessage.BuildSendDataRequest(node.Id, t_msg);
            SendMessage(node, f_msg);
        }

        public static void SendMessage(ZWaveNode node, byte[] message)
        {
            int length = message[5];

            Utility.logger.Debug("In sendMsg - SecurityHandler");

            if (message.Length < 7)
            {
                Utility.logger.Error("Message too short");
            }
            if (message[3] != 0x13)
            {
                Utility.logger.Error("Invalid Message type");
            }

            if (length > 28)
            {
                SecutiryPayload t_payload = new SecutiryPayload();
                t_payload.length = 28;
                t_payload.part = 1;
                byte[] t_message = new byte[t_payload.length];
                System.Array.Copy(message, 6, t_message, 0, t_payload.length);
                t_payload.message = t_message;
                QueuePayload(node, t_payload);

                SecutiryPayload t_payload2 = new SecutiryPayload();
                t_payload2.length = length - 28;
                t_payload2.part = 2;
                byte[] t_message2 = new byte[t_payload.length];
                System.Array.Copy(message, 34, t_message2, 0, t_payload2.length);
                t_payload2.message = t_message2;
                QueuePayload(node, t_payload2);
            }
            else
            {
                SecutiryPayload t_payload = new SecutiryPayload();
                t_payload.length = length;
                t_payload.part = 0;
                byte[] t_message = new byte[t_payload.length];
                System.Array.Copy(message, 6, t_message, 0, t_payload.length);
                t_payload.message = t_message;
                QueuePayload(node, t_payload);
            }
        }

        public static SecurityData GetSecurityData(ZWaveNode node)
        {
            return (SecurityData)node.GetData("SecurityData", new SecurityData(node)).Value;
        }

        #endregion

        #region Private helper methods

        private static void QueuePayload(ZWaveNode node, SecutiryPayload payload)
        {
            var nodeSecurityData = GetSecurityData(node);

            lock (nodeSecurityData.SecurePayload)
            {
                nodeSecurityData.SecurePayload.Add(payload);
                if (nodeSecurityData.DeviceNonceTimer.ElapsedMilliseconds > 10000)
                    nodeSecurityData.IsWaitingForNonce = false;
                if (!nodeSecurityData.IsWaitingForNonce)
                {
                    RequestNonce(node);
                }
            }
        }

        private static bool RequestNonce(ZWaveNode node)
        {
            var nodeSecurityData = GetSecurityData(node);

            Utility.logger.Debug("In sendRequestNonce - SecurityHandler");

            if (nodeSecurityData.IsWaitingForNonce)
                return false;

            Utility.logger.Debug("In sendRequestNonce - not waiting for Nonce - SecurityHandler");
            nodeSecurityData.IsWaitingForNonce = true;

            node.SendDataRequest(new byte[] {
                (byte)CommandClass.Security,
                (byte)SecurityCommand.NonceGet
            });

            return true;
        }

        private static void SendNonceReport(ZWaveNode node)
        {
            byte[] message = new byte[10];

            message[0] = (byte)CommandClass.Security;
            message[1] = (byte)SecurityCommand.NonceReport;

            GetSecurityData(node).GenerateControllerCurrentNonce();

            Array.Copy(GetSecurityData(node).GetControllerCurrentNonce(), 0, message, 2, 8);

            node.SendDataRequest(message);

            GetSecurityData(node).ControllerNonceTimer.Reset();
        }

        private static void ProcessNonceReport(ZWaveNode node, byte[] message, int start)
        {
            SecurityData nodeSecurityData = GetSecurityData(node);
            nodeSecurityData.DeviceNonceTimer.Restart();

            if (nodeSecurityData.DeviceCurrentNonce == null)
                nodeSecurityData.DeviceCurrentNonce = new byte[8];
            Array.Copy(message, start + 1, nodeSecurityData.DeviceCurrentNonce, 0, 8);

            EncryptMessage(node, message);
            nodeSecurityData.IsWaitingForNonce = false;

            // if we still have items in the queue request a new nonce
            if (nodeSecurityData.SecurePayload.Count > 0)
            {
                RequestNonce(node);
            }
        }

        // IN the mesage to be Encrypted
        // OUT - true - message processed and sent - proceed to next one
        //     - false - we need to wait for the nonce report to come
        private static bool EncryptMessage(ZWaveNode node, byte[] message)
        {
            SecurityData nodeSecurityData = GetSecurityData(node);

            Utility.logger.Debug("In EncryptMessage - secure_payload [" + nodeSecurityData.SecurePayload.Count + "]  - " + nodeSecurityData.DeviceNonceTimer.ElapsedMilliseconds);

            // if we get true we need to wait for the new Nonce
            // if we get false we need to proceed
            //            if (sendRequestNonce())
            //                return false;
            if (nodeSecurityData.DeviceNonceTimer.ElapsedMilliseconds > 10000)
                return false;

            SecutiryPayload payload = null;
            lock (nodeSecurityData.SecurePayload)
            {
                if (nodeSecurityData.SecurePayload.Count > 0)
                {
                    payload = nodeSecurityData.SecurePayload.First();
                    nodeSecurityData.SecurePayload.Remove(payload);
                }
            }

            if (payload != null)
            {
                int len = payload.length + 20;

                byte[] t_message = new byte[len];

                int i = 0;

                t_message[i] = (byte)CommandClass.Security;
                i++;
                t_message[i] = (byte)SecurityCommand.MessageEncap;

                byte[] initializationVector = new byte[16];
                for (int a = 0; a < 8; a++)
                {
                    initializationVector[a] = (byte)0xAA;
                    i++;
                    t_message[i] = initializationVector[a];
                }

                Array.Copy(nodeSecurityData.DeviceCurrentNonce, 0, initializationVector, 8, 8);

                int sequence = 0;

                if (payload.part == 1)
                {
                    ++nodeSecurityData.SequenceCounter;
                    sequence = nodeSecurityData.SequenceCounter & (byte)0x0f;
                    sequence |= (byte)0x10;
                }
                else if (payload.part == 2)
                {
                    ++nodeSecurityData.SequenceCounter;
                    sequence = nodeSecurityData.SequenceCounter & (byte)0x0f;
                    sequence |= (byte)0x30;
                }

                byte[] plaintextmsg = new byte[payload.length + 1];
                plaintextmsg[0] = (byte)sequence;
                for (int a = 0; a < payload.length; a++)
                {
                    plaintextmsg[a + 1] = payload.message[a];
                }

                byte[] encryptedPayload = new byte[30];

                encryptedPayload = AesWork.EncryptOfbMessage(nodeSecurityData.EncryptionKey, initializationVector, plaintextmsg);

                Utility.logger.Debug("authKey " + BitConverter.ToString(nodeSecurityData.AuthorizationKey));
                Utility.logger.Debug("EncryptKey " + BitConverter.ToString(nodeSecurityData.EncryptionKey));
                Utility.logger.Debug("Input Packet: " + BitConverter.ToString(plaintextmsg));
                Utility.logger.Debug("IV " + BitConverter.ToString(initializationVector));
                Utility.logger.Debug("encryptedPayload " + BitConverter.ToString(encryptedPayload));

                for (int a = 0; a < payload.length + 1; ++a)
                {
                    i++;
                    t_message[i] = encryptedPayload[a];
                }

                i++;
                t_message[i] = nodeSecurityData.DeviceCurrentNonce[0];

                //GenerateAuthentication
                int start = 1;
                byte[] mac = GenerateAuthentication(nodeSecurityData.AuthorizationKey, t_message, start, t_message.Length + 2 - start - 1, 0x01, node.Id, initializationVector);
                for (int a = 0; a < 8; ++a)
                {
                    i++;
                    t_message[i] = mac[a];
                }

                node.SendDataRequest(t_message);
                Utility.logger.Debug("In EncryptMessage - message sent");

                if ((nodeSecurityData.IsNetworkKeySet == false) && payload.message[0] == (byte)CommandClass.Security && payload.message[1] == (byte)SecurityCommand.NetworkKeySet)
                {
                    nodeSecurityData.IsNetworkKeySet = true;
                    nodeSecurityData.IsAddingNode = false;
                }

                return true;
            }
            return true;

        }

        private static byte[] DecryptMessage(ZWaveNode node, byte[] message, int start)
        {
            SecurityData nodeSecurityData = GetSecurityData(node);

            Utility.logger.Debug("In DecryptMessage - SecurityHandler");
            if (nodeSecurityData.ControllerNonceTimer.ElapsedMilliseconds > 10000)
            {
                Utility.logger.Error("Received the nonce  too late'" + nodeSecurityData.ControllerNonceTimer.ElapsedMilliseconds + "' > 10000");
                return null;
            }

            Utility.logger.Debug("Message to be decrypted: " + BitConverter.ToString(message));

            // Get IV from inbound packet
            byte[] iv = new byte[16];
            Array.Copy(message, start + 1, iv, 0, 8);
            byte[] currentNonce = nodeSecurityData.GetControllerCurrentNonce(true);

            if (currentNonce == null)
            {
                Utility.logger.Error("currentNonce null");
                return null;
            }

            Utility.logger.Debug("currentNonce NOT null");

            Array.Copy(currentNonce, 0, iv, 8, 8);

            int _length = message.Length;
            int encryptedpackagesize = _length - 11 - 8; //19 + 11 + 8
            byte[] encryptedpacket = new byte[encryptedpackagesize];


            Array.Copy(message, 8 + start + 1, encryptedpacket, 0, encryptedpackagesize);

            byte[] decryptedpacket = AesWork.EncryptOfbMessage(nodeSecurityData.EncryptionKey, iv, encryptedpacket);
            Utility.logger.Debug("Message          " + BitConverter.ToString(message));
            Utility.logger.Debug("IV               " + BitConverter.ToString(iv));
            Utility.logger.Debug("Encrypted Packet " + BitConverter.ToString(encryptedpacket));
            Utility.logger.Debug("Decrypted Packet " + BitConverter.ToString(decryptedpacket));

            byte[] mac = GenerateAuthentication(nodeSecurityData.AuthorizationKey, message, start, _length, node.Id, 0x01, iv);

            byte[] e_mac = new byte[8];
            Array.Copy(message, start + 8 + encryptedpackagesize + 2, e_mac, 0, 8);
            if (!Enumerable.SequenceEqual(mac, e_mac))
            {
                Utility.logger.Debug("Computed mac " + BitConverter.ToString(mac) + " does not match the provider mac " + BitConverter.ToString(e_mac) + ". Dropping.");
                if (nodeSecurityData.SecurePayload.Count > 1)
                    RequestNonce(node);
                return null;
            }
            /*
            if (decryptedpacket[1] == (byte)CommandClass.Security && 1 == 0)
            {
                byte[] msg = new byte[decryptedpacket.Length - 1];
                Array.Copy(decryptedpacket, 1, msg, 0, msg.Length);

                Utility.logger.Debug("Processing Internally: " + BitConverter.ToString(msg));

            }
            else
            {
                */
            byte[] msg = new byte[decryptedpacket.Length - 2 + 8];
            Array.Clear(msg, 0, 7);
            Array.Copy(decryptedpacket, 1, msg, 7, msg.Length - 7);

            msg[6] = (byte)(msg.Length - 7);

            Utility.logger.Debug("Forwarding: " + BitConverter.ToString(msg));

            /* send to the Command Class for Proecssing */
            Utility.logger.Debug("Received External Command Class: " + BitConverter.ToString(new byte[] { decryptedpacket[1] }));
//                node.MessageRequestHandler(node.pController, msg);
            Utility.logger.Debug("In DecryptMessage - Finished");

            return msg;
        }

        private static byte[] GenerateAuthentication(byte[] authorizationKey, byte[] data, int start, int length, byte sendingNode, byte receivingNode, byte[] iv)
        {
            // data should stat at 4
            byte[] buffer = new byte[256];
            byte[] tmpauth = new byte[16];
            int ib = 0;
            buffer[ib] = data[start + 0];
            ib++;
            buffer[ib] = sendingNode;
            ib++;
            buffer[ib] = receivingNode;
            ib++;
            buffer[ib] = (byte)(length - 19);
            Array.Copy(data, start + 9, buffer, 4, buffer[3]);

            byte[] buff = new byte[length - 19 + 4];
            Array.Copy(buffer, buff, length - 19 + 4);
            //Utility.logger.Debug("Raw Auth (minus IV)" + BitConverter.ToString(buff));

            tmpauth = AesWork.EncryptEcbMessage(authorizationKey, iv);

            byte[] encpck = new byte[16];
            Array.Clear(encpck, 0, 16);

            int block = 0;

            for (int i = 0; i < buff.Length; i++)
            {
                encpck[block] = buff[i];
                block++;

                if (block == 16)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        tmpauth[j] = (byte)(encpck[j] ^ tmpauth[j]);
                        encpck[j] = 0;
                    }
                    block = 0;
                    tmpauth = AesWork.EncryptEcbMessage(authorizationKey, tmpauth);
                }
            }

            /* any left over data that isn't a full block size*/
            if (block > 0)
            {
                for (int i = 0; i < 16; i++)
                {
                    tmpauth[i] = (byte)(encpck[i] ^ tmpauth[i]);
                }
                tmpauth = AesWork.EncryptEcbMessage(authorizationKey, tmpauth);
            }

            byte[] auth = new byte[8];
            Array.Copy(tmpauth, auth, 8);

            //Utility.logger.Debug("Computed Auth " + BitConverter.ToString(auth));

            return auth;
        }

        #endregion

    }
}

