//using EemRdx.Scripts.BotModules;
//using Sandbox.ModAPI;

//namespace EemRdx.Scripts.Networking
//{
//    public class BotSyncModule<T> : BotModuleBase, InitializableModule, ClosableModule, ISync<T>
//    {
//        private EntitySync<T> Syncer;
//        public BotSyncModule(BotBase MyAI, string DataDescription) : base(MyAI)
//        {
//            Syncer = new EntitySync<T>(MyAI.Grid, DataDescription);
//        }

//        public T Data => Syncer.Data;

//        public string DataDescription => Syncer.DataDescription;

//        public void Ask() { Syncer.Ask(); }

//        public void Init()
//        {
//            Syncer.Register();
//        }

//        public void Close()
//        {
//            Syncer.Unregister();
//        }

//        public void Set(T New)
//        {
//            throw new System.NotImplementedException();
//        }
//    }
//}
