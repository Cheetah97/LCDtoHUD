using Sandbox.ModAPI;

namespace EemRdx.Networking
{
    public class Sync<T> : IRegistrableSync<T>
    {
        public T Data { get; protected set; }
        public string DataDescription { get; protected set; }
        protected string SenderName;
        public static implicit operator T(Sync<T> Object)
        {
            return Object.Data;
        }

        /// <summary>
        /// Don't forget to Ask() to get actual value from server after registration.
        /// </summary>
        public Sync(string DataDescription)
        {
            this.DataDescription = DataDescription;
            SenderName = $"GenericSyncer {DataDescription}";
        }

        /// <summary>
        /// Registers a handler. Don't forget to initialize Networker beforehand.
        /// </summary>
        public void Register()
        {
            Networker.RegisterHandler(SenderName, Handler);
        }

        /// <summary>
        /// Unregisters a handler.
        /// </summary>
        public void Unregister()
        {
            Networker.UnregisterHandler(SenderName, Handler);
        }

        protected void Handler(NetworkerMessage message)
        {
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                if (message.DataDescription == "UpdateRequest")
                {
                    Data = Deserialize(message.Data);
                }
                else if (message.DataDescription == "Get")
                {
                    Networker.SendTo(message.SenderID, SenderName, "Update", Serialize(Data));
                }
            }
            else
            {
                if (message.DataDescription == "Update" && message.SenderID == MyAPIGateway.Multiplayer.ServerId)
                {
                    Data = Deserialize(message.Data);
                }
            }
        }

        protected static byte[] Serialize(T Object)
        {
            return MyAPIGateway.Utilities.SerializeToBinary(Object);
        }

        protected static T Deserialize(byte[] Raw)
        {
            return MyAPIGateway.Utilities.SerializeFromBinary<T>(Raw);
        }

        /// <summary>
        /// Updates a variable or sends a request to server (if called clientside).
        /// </summary>
        public void Set(T New)
        {
            if (MyAPIGateway.Multiplayer.IsServer && MyAPIGateway.Multiplayer.MultiplayerActive)
            {
                Networker.SendToAll(SenderName, "Update", Serialize(New));
                Data = New;
            }
            else
            {
                Networker.SendToServer(SenderName, "UpdateRequest", Serialize(New));
            }
        }

        /// <summary>
        /// Asks the server for actual value.
        /// </summary>
        public void Ask()
        {
            Networker.SendToServer(SenderName, "Get", null);
        }
    }

    public class EntitySync<T> : Sync<T>
    {
        public EntitySync(VRage.ModAPI.IMyEntity Entity, string DataDescription) : base(DataDescription)
        {
            SenderName = $"EntitySyncer {DataDescription} for {Entity.EntityId}";
        }
    }
}
