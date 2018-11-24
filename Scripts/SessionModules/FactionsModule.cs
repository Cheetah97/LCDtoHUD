using EemRdx.Helpers;
using Sandbox.ModAPI;

namespace EemRdx.SessionModules
{
    public class FactionsModule : SessionModuleBase<ISessionKernel>, InitializableModule, UpdatableModule, UnloadableModule
    {
        private ulong Ticker = 0;
        protected override string DebugModuleName { get; } = "FactionsModule";
        public FactionsModule(ISessionKernel MySessionKernel) : base(MySessionKernel) { }

        public void Init()
        {
            Factions.Factions.Initialize();
            Factions.Factions.Register();
        }

        public void Update()
        {
            Ticker++;
            if (!MyAPIGateway.Multiplayer.IsServer) return;

            if (MyAPIGateway.Multiplayer.Players.Count > 0 && !Factions.Factions.PlayerFactionInitComplete) { Factions.Factions.PlayerInitFactions(); }
            if (Ticker % Constants.WarAssessmentCounterLimit == 0) Factions.Factions.AssessFactionWar();
            if (Ticker % (Constants.TicksPerSecond * 30) == 0) Factions.Factions.FactionAssessment();
        }

        public void UnloadData()
        {
            Factions.Factions.Unload();
        }
    }
}
