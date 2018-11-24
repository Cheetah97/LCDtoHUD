namespace EemRdx.SessionModules
{
    public interface ISessionModule { }
    public interface InitializableModule
    {
        void Init();
    }
    public interface UpdatableModule
    {
        void Update();
    }
    public interface UnloadableModule
    {
        void UnloadData();
    }
    public interface SaveableModule
    {
        void Save();
    }
    public interface LoadableModule
    {
        void LoadData();
    }

    public abstract class SessionModuleBase<TKernel> : ISessionModule where TKernel: ISessionKernel
    {
        public TKernel MySessionKernel { get; private set; }
        public SessionModuleBase(TKernel MySessionKernel)
        {
            this.MySessionKernel = MySessionKernel;
        }

        protected abstract string DebugModuleName { get; }
        protected void WriteToDebugLog(string caller, string message, bool showOnHud = false, int duration = Helpers.Constants.DefaultLocalMessageDisplayTime, string color = VRage.Game.MyFontEnum.Green, string DefaultDebugNameOverride = null)
        {
            if (DefaultDebugNameOverride == null) DefaultDebugNameOverride = DebugModuleName;
            string qualifiedCaller = string.Format("{0}.{1}", DefaultDebugNameOverride, caller);
            EEMSessionKernel.Static.Log.DebugLog?.WriteToLog(qualifiedCaller, message, showOnHud, duration, color);
        }

        protected void LogErrorInDebugLog(string source, string message, System.Exception Scrap, string DefaultDebugNameOverride = null)
        {
            if (DefaultDebugNameOverride == null) DefaultDebugNameOverride = DebugModuleName;
            string qualifiedSource = string.Format("{0}.{1}", DefaultDebugNameOverride, source);
            EEMSessionKernel.Static.Log.DebugLog?.LogError(qualifiedSource, message, Scrap);
        }
    }
}
