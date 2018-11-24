using System;
using System.Collections.Generic;
using Sandbox.ModAPI;

namespace EemRdx.Networking
{
    public static class Networker
    {
        public const ushort CommChannel = 7207;
        public static bool Inited { get; private set; }
        // We won't go beyond 4KKK of workshop creations quickly, right?
        public static uint ModID { get; private set; }

        private static Dictionary<string, HashSet<Action<NetworkerMessage>>> MessageHandlers = new Dictionary<string, HashSet<Action<NetworkerMessage>>>();

        public static void Init(uint ModWorkshopID)
        {
            if (Inited) return;
            ModID = ModWorkshopID;
            MyAPIGateway.Multiplayer.RegisterMessageHandler(CommChannel, Handler);
            Inited = true;
        }

        public static void Close()
        {
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(CommChannel, Handler);
        }

        static void Handler(byte[] rawmessage)
        {
            NetworkerMessage message = MyAPIGateway.Utilities.SerializeFromBinary<NetworkerMessage>(rawmessage);
            if (message == null || message.ModID != ModID) return;
            // TODO: <Cheetah Comment> Add logging
            if (MessageHandlers.ContainsKey(message.DataTag))
            {
                foreach (Action<NetworkerMessage> handler in MessageHandlers[message.DataTag])
                {
                    try
                    {
                        if (handler != null) handler(message);
                    }
                    catch { }
                }
            }
        }

        public static bool RegisterHandler(string SenderName, Action<NetworkerMessage> handler)
        {
            if (handler == null) return false;
            if (MessageHandlers.ContainsKey(SenderName)) return MessageHandlers[SenderName].Add(handler);
            else
            {
                MessageHandlers.Add(SenderName, new HashSet<Action<NetworkerMessage>> { handler });
                return true;
            }
        }

        public static bool UnregisterHandler(string SenderName, Action<NetworkerMessage> handler)
        {
            if (handler == null) return false;
            if (!MessageHandlers.ContainsKey(SenderName)) return false;
            else return MessageHandlers[SenderName].Remove(handler);
        }

        private static byte[] GenerateMessage(string senderName, string dataDescription, byte[] data)
        {
            NetworkerMessage message = new NetworkerMessage
            {
                ModID = ModID,
                SenderID = MyAPIGateway.Multiplayer.MyId,
                DataTag = senderName,
                DataDescription = dataDescription,
                Data = data
            };
            return MyAPIGateway.Utilities.SerializeToBinary(message);
        }

        public static void SendToAll(string SenderName, string DataDescription, byte[] Data)
        {
            byte[] Raw = GenerateMessage(SenderName, DataDescription, Data);
            MyAPIGateway.Multiplayer.SendMessageToOthers(CommChannel, Raw);
        }

        public static void SendTo(ulong ID, string SenderName, string DataDescription, byte[] Data)
        {
            byte[] Raw = GenerateMessage(SenderName, DataDescription, Data);
            MyAPIGateway.Multiplayer.SendMessageTo(CommChannel, Raw, ID);
        }

        public static void SendToServer(string SenderName, string DataDescription, byte[] Data)
        {
            byte[] Raw = GenerateMessage(SenderName, DataDescription, Data);
            MyAPIGateway.Multiplayer.SendMessageToServer(CommChannel, Raw);
        }
    }
}
