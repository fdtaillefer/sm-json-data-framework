using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class CanLeaveCharged : InitializablePostDeserializeInNode, IRunway, IExecutable
    {
        [JsonIgnore]
        public int Length { get => UsedTiles; }

        [JsonIgnore]
        public int EndingUpTiles { get => 0; }

        public int UsedTiles { get; set; }

        public int FramesRemaining { get; set; }

        public int ShinesparkFrames { get; set; }

        public InitiateRemotely InitiateRemotely {get;set;}

        /// <summary>
        /// Returns whether this CanLeaveCharged is initiated at a different node than the door it exits through.
        /// </summary>
        [JsonIgnore]
        public bool IsInitiatedRemotely { get { return InitiateRemotely != null; } }

        public IDictionary<string, Strat> Strats { get; set; } = new Dictionary<string, Strat>();

        [JsonPropertyName("openEnd")]
        public int OpenEnds { get; set; } = 0;

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The effective runway length for the runway of this CanLeaveCharged, based on initial rules and logical options.
        /// Because this is not considered reversible, there is only one value.</para>
        /// </summary>
        [JsonIgnore]
        public decimal EffectiveRunwayLength { get; protected set; } = 0;

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, RoomNode)"/> has been called.</para>
        /// <para>The node in which this CanLeaveCharged is.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode Node { get; set; }

        public void Initialize(SuperMetroidModel model, Room room, RoomNode node)
        {
            Node = node;

            // Eliminate disabled strats
            Strats = Strats.WhereEnabled(model);

            foreach (Strat strat in Strats.Values)
            {
                strat.Initialize(model, node.Room);
            }

            EffectiveRunwayLength = model.Rules.CalculateEffectiveRunwayLength(this, model.LogicalOptions.TilesSavedWithStutter);

            InitiateRemotely?.Initialize(model, room, node, this);
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            List<string> unhandled = new List<string>();

            foreach(Strat strat in Strats.Values)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            if (InitiateRemotely != null)
            {
                unhandled.AddRange(InitiateRemotely.InitializeReferencedLogicalElementProperties(model, room, node, this));
            }

            return unhandled.Distinct();
        }

        public ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // There are many things to check...

            // If we don't have SpeedBooster, this is not usable
            if (!inGameState.Inventory.HasSpeedBooster())
            {
                return null;
            }

            // If we need a shinespark and the tech is turned off, this is not usable
            bool mustShinespark = ShinesparkFrames > 0;
            if (mustShinespark && !model.CanShinespark())
            {
                return null;
            }

            // If the player is unable to charge a shinespark with the available runway, this is not usable
            if (EffectiveRunwayLength < model.LogicalOptions.TilesToShineCharge)
            {
                return null;
            }

            // STITCHME Is there any remote initiation check anywhere? If this is remote it should only be executable if the path followed in the room matches the remote config

            // Figure out how much energy we will need to have for the shinespark
            int energyNeededForShinespark = model.Rules.CalculateEnergyNeededForShinespark(ShinesparkFrames, times: times);
            int shinesparkEnergyToSpend = model.Rules.CalculateShinesparkDamage(inGameState, ShinesparkFrames, times: times);

            // Try to execute all strats, 
            // obtaining the result of whichever spends the lowest amount of resources while retaining enough for the shinespark
            (Strat bestStrat, ExecutionResult result) = model.ExecuteBest(Strats.Values, inGameState, times: times, previousRoomCount: previousRoomCount,
                // Not calling IsResourceAvailable() because Samus only needs to have that much energy, not necessarily spend all of it
                acceptationCondition: igs => igs.Resources.GetAmount(ConsumableResourceEnum.ENERGY) >= energyNeededForShinespark);

            // If we couldn't find a successful strat, give up
            if (result == null)
            {
                return null;
            }

            // Add a record of the canLeaveCharged being executed
            result.AddExecutedCanLeaveCharged(this, bestStrat);

            // Finally, spend the energy for executing a shinespark if needed (we already asked to check that the state has enough)
            result.ResultingState.ApplyConsumeResource(model, ConsumableResourceEnum.ENERGY, shinesparkEnergyToSpend);
            
            return result;
        }
    }
}
