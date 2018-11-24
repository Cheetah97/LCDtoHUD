using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

//using Cheetah.AI;

namespace EemRdx.EntityModules
{
    public abstract class EntityModuleBase<TKernel> : IEntityModule where TKernel: IEntityKernel
    {
        public TKernel MyKernel { get; private set; }
        protected IMyEntity MyEntity => MyKernel.Entity;

        public EntityModuleBase(TKernel MyKernel)
        {
            this.MyKernel = MyKernel;
        }

        public string DebugFullName => string.Format("{0}[{1}]", DebugModuleName, (MyEntity != null ? MyEntity.DisplayName : "null"));
        public abstract string DebugModuleName { get; }

        #region Debug writing
        protected void WriteToDebugLog(string caller, string message, bool showOnHud = false, int duration = Helpers.Constants.DefaultLocalMessageDisplayTime, string color = VRage.Game.MyFontEnum.Green, string DefaultDebugNameOverride = null)
        {
            if (DefaultDebugNameOverride == null) DefaultDebugNameOverride = DebugFullName;
            string qualifiedCaller = !string.IsNullOrWhiteSpace(caller) ? string.Format("{0}.{1}", DefaultDebugNameOverride, caller) : DefaultDebugNameOverride;
            EEMSessionKernel.Static.Log.DebugLog?.WriteToLog(qualifiedCaller, message, showOnHud, duration, color);
        }
        protected void WriteToGeneralLog(string caller, string message, bool showOnHud = false, int duration = Helpers.Constants.DefaultLocalMessageDisplayTime, string color = VRage.Game.MyFontEnum.Green, string DefaultDebugNameOverride = null)
        {
            if (DefaultDebugNameOverride == null) DefaultDebugNameOverride = DebugFullName;
            string qualifiedCaller = !string.IsNullOrWhiteSpace(caller) ? string.Format("{0}.{1}", DefaultDebugNameOverride, caller) : DefaultDebugNameOverride;
            EEMSessionKernel.Static.Log.DebugLog?.WriteToLog(qualifiedCaller, message, showOnHud, duration, color);
        }

        protected void LogErrorInDebugLog(string source, string message, System.Exception Scrap, string DefaultDebugNameOverride = null)
        {
            if (DefaultDebugNameOverride == null) DefaultDebugNameOverride = DebugFullName;
            string qualifiedSource = string.Format("{0}.{1}", DefaultDebugNameOverride, source);
            EEMSessionKernel.Static.Log.DebugLog?.LogError(qualifiedSource, message, Scrap);
        }
        #endregion
    }

    public interface InitializableModule
    {
        bool Inited { get; }
        void Init();
    }

    public interface ClosableModule
    {
        void Close();
    }

    public interface UpdatableModule
    {
        /// <summary>
        /// Determines whether the module requires the bot grid to be operable
        /// in order to perform updates.
        /// </summary>
        bool RequiresOperable { get; }
        VRage.ModAPI.MyEntityUpdateEnum UpdateFrequency { get; }
        void Update();
    }
}
