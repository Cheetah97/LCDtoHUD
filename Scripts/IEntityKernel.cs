using EemRdx.EntityModules;
using VRage.ModAPI;

namespace EemRdx
{
    public interface IEntityKernel
    {
        IMyEntity Entity { get; }
        string DebugFullName { get; }
        string DebugKernelName { get; }

        T GetModule<T>() where T : IEntityModule;
        void Shutdown();
    }

}
