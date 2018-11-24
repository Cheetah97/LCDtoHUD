using System.Collections.Generic;
using System.Linq;
using EemRdx.SessionModules;
using VRage.Game.Components;

namespace EemRdx
{
    //[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public abstract class SessionKernel : MySessionComponentBase, ISessionKernel
    {
        private bool Inited = false;
        protected List<ISessionModule> Modules = new List<ISessionModule>();

        /// <summary>
        /// The Mod ID of your workshop upload. You may use 0 for local testing, but be sure to change after upload.
        /// </summary>
        public abstract uint ModID { get; }
        public abstract System.Guid StorageGuid { get; }

        public ILogProvider Log => GetModule<LogProviderModule>();
        public INetworker Networker => GetModule<NetworkerModule>();
        public ISaveProvider SaveProvider => GetModule<SaveProvider>();
        public SessionStateEnum SessionState { get; private set; }

        public T GetModule<T>() where T: ISessionModule
        {// This is going to be called often, better go with no LINQ since it can be 2x slower
            foreach (ISessionModule module in Modules)
            {
                if (module is T) return (T)module;
            }
            return default(T);
        }

        public override void UpdateBeforeSimulation()
        {
            if (!Inited) Init();
            UpdateModules();
        }

        private void Init()
        {
            SessionState = SessionStateEnum.Running;
            CreateModules();
            InitModules();
            Inited = true;
        }

        public override void LoadData()
        {
            SessionState = SessionStateEnum.Loading;
            LoadModules();
        }

        public override void SaveData()
        {
            SaveModules();
        }

        protected override void UnloadData()
        {
            SessionState = SessionStateEnum.Unloading;
            UnloadModules();
        }

        /// <summary>
        /// Remember to call the base when overriding
        /// </summary>
        protected virtual void CreateModules()
        {
            Modules.Add(new LogProviderModule(this));
            Modules.Add(new NetworkerModule(this));
        }

        private void LoadModules()
        {
            foreach (LoadableModule module in Modules.OfType<LoadableModule>())
            {
                module.LoadData();
            }
        }

        private void InitModules()
        {
            foreach (InitializableModule module in Modules.OfType<InitializableModule>())
            {
                module.Init();
            }
        }

        private void UpdateModules()
        {
            foreach (UpdatableModule module in Modules.OfType<UpdatableModule>())
            {
                module.Update();
            }
        }

        private void UnloadModules()
        {
            foreach (UnloadableModule module in Modules.OfType<UnloadableModule>())
            {
                module.UnloadData();
            }
        }

        private void SaveModules()
        {
            foreach (SaveableModule module in Modules.OfType<SaveableModule>())
            {
                module.Save();
            }
        }
    }
}
