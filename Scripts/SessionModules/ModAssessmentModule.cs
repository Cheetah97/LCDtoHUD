using EemRdx.Extensions;
using EemRdx.Helpers;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Collections;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace EemRdx.SessionModules
{
    public interface IModAssessmentModule : ISessionModule
    {
        bool EnergyShields { get; }
        bool DefenseShieldsModpack { get; }
        bool NanobotBuildAndRepair { get; }
    }

    public class ModAssessmentModule : SessionModuleBase<ISessionKernel>, InitializableModule, UpdatableModule, IModAssessmentModule
    {
        public ModAssessmentModule(ISessionKernel MySessionKernel) : base(MySessionKernel) { }
        public bool EnergyShields { get; private set; }
        public bool DefenseShieldsModpack { get; private set; }
        public bool NanobotBuildAndRepair { get; private set; }
        protected override string DebugModuleName { get; } = "ModAssessmentModule";
        private ulong Ticker = 0;
        /// <summary>
		/// Holds the collection of Nanobot Build and Repair System entities being tracked
		/// </summary>
		private static List<NanobotBuildAndRepairInfo> Nanobots = new List<NanobotBuildAndRepairInfo>();
        /// <summary>
		/// Struct to handle what's being tracked per Nanobot Build and Repair System entity
		/// </summary>
		private struct NanobotBuildAndRepairInfo
        {
            public readonly long NanobotId;
            public readonly long ReporterId;

            public NanobotBuildAndRepairInfo(long nanobotId, long reporterId)
            {
                ReporterId = reporterId;
                NanobotId = nanobotId;
            }
        }

        public void Init()
        {
            GrabModContent();
        }

        public void Update()
        {
            Ticker++;
            if (Ticker % Constants.ModAssessmentCounterLimit == 0) AssessMods();
        }

        /// <summary>
		/// Scans the Definition Manager for known Forbidden Tech definitions
		/// </summary>
		private void GrabModContent()
        {
            DictionaryValuesReader<MyDefinitionId, MyDefinitionBase> definitionBase = MyDefinitionManager.Static.GetAllDefinitions();

            foreach (MyDefinitionBase definition in definitionBase)
            {

                if (Constants.EnergyShieldDefinitions.Any(definition.Id.SubtypeId.ToString().Contains))
                {
                    Constants.EnergyShields = true;
                    EnergyShields = true;
                }

                if (Constants.DefenseShieldsModPackDefinitions.Any(definition.Id.SubtypeId.ToString().Contains))
                {
                    Constants.DefenseShieldsModPack = true;
                    DefenseShieldsModpack = true;
                }

                if (Constants.NanobotBuildAndRepairDefinitions.Any(definition.Id.SubtypeId.ToString().Contains))
                {
                    Constants.NanobotBuildAndRepair = true;
                    NanobotBuildAndRepair = true;
                }
            }

            if (NanobotBuildAndRepair && Constants.DebugMode)
                ShowIngameMessage.ShowOverrideMessage("Nanobot Build and Repair System - Detected");

            if (EnergyShields && Constants.DebugMode)
                ShowIngameMessage.ShowOverrideMessage("Energy Shield - Detected");

            if (DefenseShieldsModpack && Constants.DebugMode)
                ShowIngameMessage.ShowOverrideMessage("Defense Shields - Mod Pack - Detected");
        }

        /// <summary>
		/// Assesses all mods being monitored by the core
		/// </summary>
		private void AssessMods()
        {
            AssessNanobotLists();
        }

        /// <summary>
        /// This loops through the nanobot list to check on the status of all monitored tech
        /// </summary>
        private void AssessNanobotLists()
        {
            if (!NanobotBuildAndRepair) return;

            for (int counter = Nanobots.Count - 1; counter >= 0; counter--)
            { // Loop through the Nanobot struct backwards so we can RemoveAtFast and not affect the loop
                IMyShipWelder welder = MyAPIGateway.Entities.GetEntityById(Nanobots[counter].NanobotId) as IMyShipWelder;
                IMyEntity reporterEntity = MyAPIGateway.Entities.GetEntityById(Nanobots[counter].ReporterId);
                if (welder == null || reporterEntity == null)
                { // Block or reporter was destroyed
                    ShowIngameMessage.ShowOverrideMessage(PresetMessages.ForbiddenTechNeutral);
                    Nanobots.RemoveAtFast(counter);
                    continue;
                }
                if (Vector3D.DistanceSquared(welder.GetPosition(), reporterEntity.GetPosition()) > Constants.NanobotRangeToWatch.Squared())
                { // Block is outside of reporter danger zone
                    ShowIngameMessage.ShowOverrideMessage(PresetMessages.ForbiddenTechCleared);
                    Nanobots.RemoveAtFast(counter);
                    continue;
                }
                if (!welder.Enabled) continue; // Block is still disabled - if enabled, code below executes
                ShowIngameMessage.ShowOverrideMessage(PresetMessages.ForbiddenTechViolation);

                // <Cheetah Comment> This will blow up in your face if the player has no faction, is offline or
                // the welder is owned by nobody/factionless player (not the pilot of the ship with this stuff)
                //DeclareFullAiWar(MyAPIGateway.Players.GetPlayerById(welder.OwnerId).GetFaction());

                if (welder.OwnerId != 0)
                {
                    IMyPlayer player = MyAPIGateway.Players.GetPlayerById(welder.OwnerId);
                    IMyFaction playerFaction = player?.GetFaction();
                    if (playerFaction != null) Factions.Factions.DeclareFullAiWar(playerFaction);
                }
                // <Cheetah Comment> Yup, you may be surprised, but that's the only proper way to deal with modapi
                Nanobots.RemoveAtFast(counter);
            }
        }

        /// <summary>
        /// This accepts the items required to add a Nanobot Build and Repair System to the watch list.
        ///		This mod has been considered "Forbidden Technology" in EEM and is subject to certain sanctions if used near an EEM entity
        /// </summary>
        /// <param name="watchMe">EntityID of the Nanobot Build and Repair System to monitor</param>
        /// <param name="fromHere">Origin point of the original scan to see if the forbidden tech is still within the danger zone </param>
        /// <param name="byThisEntity">EEM Prefab that did the original scan; captured for future planning</param>
        public static void AddNanobotToWatchList(List<long> watchMe, long byThisEntity)
        {
            if (watchMe.Count == 0) return;             // Initial detection can return a blank list, so exit if this is the case
            List<long> playerList = new List<long>();   // Holds a list of players to make sure we're not spamming warning messages for the same tech
            foreach (long welderId in watchMe)
            { // loops through the welders that came in on the query to make sure we're accounting for all of the noticed forbidden tech
                IMyShipWelder welder = MyAPIGateway.Entities.GetEntityById(welderId) as IMyShipWelder;  // Cast the entityId as the known entity
                if (welder == null) continue;                                                           // Welder may have been destroyed or deleted, need to make sure it's still there
                NanobotBuildAndRepairInfo newNanobotCollection = new NanobotBuildAndRepairInfo(welderId, byThisEntity);         // Create the struct to be added to the list if it's a new item to watch
                int index = Nanobots.IndexOf(newNanobotCollection);                                     // Look to see if the struct is new or already exists
                if (index != -1)
                    continue;                                                                           // -1 says the struct is new, so if not -1, then skip to the next entity
                Nanobots.Add(newNanobotCollection);                                                     // This is a new item, add it to the list
                IMyPlayer player = MyAPIGateway.Players.GetPlayerById(welder.OwnerId);                  // Find the entity owner and warn them of the violation
                if (player == null) continue;                                                           // Player may be offline, someone else may be in their ship
                if (playerList.Contains(player.IdentityId)) continue;                                   // If the player has already been warned, skip to next entity
                playerList.Add(player.IdentityId);                                                      // Log the player so we don't spam them with messages
                ShowIngameMessage.ShowOverrideMessage(PresetMessages.ForbiddenTechWarning(player.DisplayName));
            }
        }
    }
}
