using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.ModAPI;

namespace EemRdx.SessionModules
{
    public class DamageProviderModule : SessionModuleBase<ISessionKernel>, InitializableModule, IDamageProviderModule
    {
        public DamageProviderModule(ISessionKernel MySessionKernel) : base(MySessionKernel) { }
        public delegate void OnDamageTaken(IMySlimBlock damagedBlock, MyDamageInformation damage);
        protected override string DebugModuleName { get; } = "DamageProviderModule";
        private readonly Dictionary<long, OnDamageTaken> DamageHandlers = new Dictionary<long, OnDamageTaken>();

        #region DictionaryAccessors
        public void AddDamageHandler(long gridId, OnDamageTaken handler)
        {
            DamageHandlers.Add(gridId, handler);
        }
        public void AddDamageHandler(IMyCubeGrid grid, OnDamageTaken handler)
        {
            AddDamageHandler(grid.GetTopMostParent().EntityId, handler);
        }
        public void RemoveDamageHandler(long gridId)
        {
            DamageHandlers.Remove(gridId);
        }
        public void RemoveDamageHandler(IMyCubeGrid grid)
        {
            RemoveDamageHandler(grid.GetTopMostParent().EntityId);
        }
        public bool HasDamageHandler(long gridId)
        {
            return DamageHandlers.ContainsKey(gridId);
        }
        public bool HasDamageHandler(IMyCubeGrid grid)
        {
            return HasDamageHandler(grid.GetTopMostParent().EntityId);
        }

        public void Init()
        {
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, DamageRefHandler);
            MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(0, GenericDamageHandler);
            MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, GenericDamageHandler);
        }

        private void DamageRefHandler(object damagedObject, ref MyDamageInformation damage)
        {
            GenericDamageHandler(damagedObject, damage);
        }

        private void GenericDamageHandler(object damagedObject, MyDamageInformation damage)
        {
            try
            {
                if (!(damagedObject is IMySlimBlock)) return;
                IMySlimBlock damagedBlock = (IMySlimBlock)damagedObject;
                IMyCubeGrid damagedGrid = damagedBlock.CubeGrid;
                long gridId = damagedGrid.EntityId;
                if (!DamageHandlers.ContainsKey(gridId)) return;
                try
                {
                    DamageHandlers[gridId].Invoke(damagedBlock, damage);
                }
                catch (Exception Scrap)
                {
                    LogErrorInDebugLog("GenericDamageHandler", "Grid damage handler threw: ", Scrap);
                }
            }
            catch (Exception Scrap)
            {
                LogErrorInDebugLog("GenericDamageHandler", "Damage handler procedure bugged out: ", Scrap);
            }
        }
        #endregion
    }
}
