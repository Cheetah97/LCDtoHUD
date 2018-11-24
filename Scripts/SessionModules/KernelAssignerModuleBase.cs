using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Collections;
using VRage.ModAPI;

namespace EemRdx.SessionModules
{
    public abstract class KernelAssignerModuleBase<TSessionKernel, TEntity> : SessionModuleBase<TSessionKernel>, InitializableModule, UpdatableModule, UnloadableModule where TSessionKernel: ISessionKernel where TEntity : IMyEntity
    {
        public KernelAssignerModuleBase(TSessionKernel MySessionKernel) : base(MySessionKernel) { }
        protected override string DebugModuleName { get; } = "KernelAssignerModuleBase";
        private static MyConcurrentQueue<TEntity> NewlyAddedEntities = new MyConcurrentQueue<TEntity>(20);
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
            List<TEntity> newEntities = new List<TEntity>();
            while (NewlyAddedEntities.Count > 0)
            {
                newEntities.Add(NewlyAddedEntities.Dequeue());
            }

            foreach (TEntity entity in newEntities) HandleEntity(entity);
        }

        private void RetroactiveKernelAssignment()
        {
            HashSet<IMyEntity> entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, x => x is TEntity);

            foreach (TEntity entity in entities.Cast<TEntity>())
            {
                HandleEntity(entity);
            }
        }

        protected abstract void HandleEntity(TEntity Entity);

        private void Entities_OnEntityAdd(IMyEntity NewEntity)
        {
            if (NewEntity is TEntity)
            {
                TEntity entity = (TEntity)NewEntity;
                NewlyAddedEntities.Enqueue(entity);
                WriteToDebugLog($"OnEntityAdd", $"Processed entity {entity.DisplayName}");
            }
        }
    }
}
