using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Collections;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace EemRdx.SessionModules
{
    public class KernelAssignerModule : SessionModuleBase<ISessionKernel>, InitializableModule, UpdatableModule, UnloadableModule
    {
        public KernelAssignerModule(ISessionKernel MySessionKernel) : base(MySessionKernel) { }
        protected override string DebugModuleName { get; } = "KernelAssignerModule";
        private static MyConcurrentQueue<IMyCubeGrid> NewlyAddedGrids = new MyConcurrentQueue<IMyCubeGrid>(20);
        private int Ticker = 0;
        private bool ReassignKernels = true;

        public void Init()
        {
            MyAPIGateway.Entities.OnEntityAdd += Entities_OnEntityAdd;
        }

        public void Update()
        {
            if (MySessionKernel.SessionState != SessionStateEnum.Running) return;
            Ticker++;
            if (Ticker > 5 && ReassignKernels)
            {
                RetroactiveKernelAssignment();
                ReassignKernels = false;
            }
            else
            {
                ProcessNewlyAddedGrids();
            }
        }

        public void UnloadData()
        {
            MyAPIGateway.Entities.OnEntityAdd -= Entities_OnEntityAdd;
        }

        private void ProcessNewlyAddedGrids()
        {
            List<IMyCubeGrid> newGrids = new List<IMyCubeGrid>();
            while (NewlyAddedGrids.Count > 0)
            {
                newGrids.Add(NewlyAddedGrids.Dequeue());
            }

            foreach (IMyCubeGrid grid in newGrids) BotFabric.HandleNewGrid(grid);
        }

        private void RetroactiveKernelAssignment()
        {
            HashSet<IMyEntity> gridentities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(gridentities, x => x is IMyCubeGrid);

            foreach (IMyCubeGrid grid in gridentities.Cast<IMyCubeGrid>())
            {
                if (!grid.Components.Has<BotKernel>())
                    NewlyAddedGrids.Enqueue(grid);
            }
        }

        private void Entities_OnEntityAdd(IMyEntity entity)
        {
            IMyCubeGrid grid = entity as IMyCubeGrid;
            if (grid != null)
            {
                NewlyAddedGrids.Enqueue(grid);
                WriteToDebugLog($"OnEntityAdd", $"Processed grid {grid.DisplayName}");
            }
        }
    }
}
