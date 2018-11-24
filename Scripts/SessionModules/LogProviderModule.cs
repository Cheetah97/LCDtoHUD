using EemRdx.Helpers;
using EemRdx.Utilities;

namespace EemRdx.SessionModules
{
    public interface ILogProvider : ISessionModule
    {
        ILog DebugLog { get; }
        ILog ProfilingLog { get; }
        ILog GeneralLog { get; }
    }

    public class LogProviderModule : SessionModuleBase<ISessionKernel>, InitializableModule, UnloadableModule, ILogProvider
    {
        public ILog DebugLog { get; private set; }
        public ILog ProfilingLog { get; private set; }
        public ILog GeneralLog { get; private set; }

        protected override string DebugModuleName => "LogProviderModule";

        public void Init()
        {
            if (Constants.DebugMode) DebugLog = new Log(Constants.DebugLogName);
            if (Constants.EnableProfilingLog) ProfilingLog = new Log(Constants.ProfilingLogName);
            if (Constants.EnableGeneralLog) GeneralLog = new Log(Constants.GeneralLogName);
        }

        public void UnloadData()
        {
            if (Constants.DebugMode) (DebugLog as Log).Close();
            if (Constants.EnableProfilingLog) (ProfilingLog as Log).Close();
            if (Constants.EnableGeneralLog) (GeneralLog as Log).Close();
        }

        public LogProviderModule(ISessionKernel MySessionKernel) : base(MySessionKernel) { }
    }
}
