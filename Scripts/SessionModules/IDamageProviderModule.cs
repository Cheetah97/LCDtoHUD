using VRage.Game.ModAPI;

namespace EemRdx.SessionModules
{
    public interface IDamageProviderModule : ISessionModule
    {
        void AddDamageHandler(IMyCubeGrid grid, DamageProviderModule.OnDamageTaken handler);
        void AddDamageHandler(long gridId, DamageProviderModule.OnDamageTaken handler);
        bool HasDamageHandler(IMyCubeGrid grid);
        bool HasDamageHandler(long gridId);
        void RemoveDamageHandler(IMyCubeGrid grid);
        void RemoveDamageHandler(long gridId);
    }
}