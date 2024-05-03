using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class CanLeaveCharged : AbstractModelElement, InitializablePostDeserializeInNode, IRunway, IExecutable
    {
        [JsonIgnore]
        public int Length { get => UsedTiles; }

        [JsonIgnore]
        public int EndingUpTiles { get => 0; }

        public int UsedTiles { get; set; }

        public int FramesRemaining { get; set; }

        public int ShinesparkFrames { get; set; }

        /// <summary>
        /// Indicates whether this CanLeavecharged involves executing a shinespark.
        /// </summary>
        public bool MustShinespark { get { return ShinesparkFrames > 0; } }

        private decimal TilesSavedWithStutter { get; set; } = LogicalOptions.DefaultTilesSavedWithStutter;

        private decimal TilesToShineCharge { get; set; } = LogicalOptions.DefaultTilesToShineCharge;

        public InitiateRemotely InitiateRemotely {get;set;}

        /// <summary>
        /// Returns whether this CanLeaveCharged is initiated at a different node than the door it exits through.
        /// </summary>
        [JsonIgnore]
        public bool IsInitiatedRemotely { get { return InitiateRemotely != null; } }

        /// <summary>
        /// The strats that can be used to execute this CanLeaveCharged, mapped by name.
        /// </summary>
        public IDictionary<string, Strat> Strats { get; set; } = new Dictionary<string, Strat>();

        [JsonPropertyName("openEnd")]
        public int OpenEnds { get; set; } = 0;

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, RoomNode)"/> has been called.</para>
        /// <para>The node in which this CanLeaveCharged is.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode Node { get; set; }

        public CanLeaveCharged()
        {

        }

        public CanLeaveCharged(RawCanLeaveCharged rawCanLeaveCharged, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            UsedTiles = rawCanLeaveCharged.UsedTiles;
            FramesRemaining = rawCanLeaveCharged.FramesRemaining;
            ShinesparkFrames = rawCanLeaveCharged.ShinesparkFrames;
            if (rawCanLeaveCharged.InitiateRemotely != null)
            {
                InitiateRemotely = new InitiateRemotely(rawCanLeaveCharged.InitiateRemotely);
            }
            Strats = rawCanLeaveCharged.Strats.Select(rawStrat => new Strat(rawStrat, knowledgeBase)).ToDictionary(strat => strat.Name);
            OpenEnds = rawCanLeaveCharged.OpenEnd;
            GentleUpTiles = rawCanLeaveCharged.GentleUpTiles;
            GentleDownTiles = rawCanLeaveCharged.GentleDownTiles;
            SteepUpTiles = rawCanLeaveCharged.SteepUpTiles;
            SteepDownTiles = rawCanLeaveCharged.SteepDownTiles;
            StartingDownTiles = rawCanLeaveCharged.StartingDownTiles;
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            TilesSavedWithStutter = logicalOptions?.TilesSavedWithStutter ?? LogicalOptions.DefaultTilesSavedWithStutter;
            TilesToShineCharge = logicalOptions?.TilesToShineCharge ?? LogicalOptions.DefaultTilesToShineCharge;

            bool useless = false;
            if(InitiateRemotely != null)
            {
                InitiateRemotely.ApplyLogicalOptions(logicalOptions);
                if (InitiateRemotely.UselessByLogicalOptions)
                {
                // This cannot be executed if it has a remote execution that has been made impossible
                    useless = true;
                }
            }

            // This cannot be executed if all its strats become impossible
            bool allStratsUseless = true;
            foreach (Strat strat in Strats.Values)
            {
                strat.ApplyLogicalOptions(logicalOptions);
                if(!strat.UselessByLogicalOptions)
                {
                    allStratsUseless = false;
                }
            }
            if(allStratsUseless)
            {
                useless = true;
            }

            // This cannot be executed if it requires a shinespark and those are disabled
            if (MustShinespark && !logicalOptions.CanShinespark)
            {
                useless = true;
            }

            // Since this is an in-room shine charge, its required nunmber of tiles is constant.
            // As such, we could check here whether the logical options make the shine too short to be possible.
            // However, this requires access to the game rules, which we don't have here.
            // Improve this if we decide to pass the rules here.

            // We could also pre-calculate an effective runway length if we had the rules

            return useless;
        }

        public void InitializeProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            Node = node;

            foreach (Strat strat in Strats.Values)
            {
                strat.InitializeProperties(model, node.Room);
            }

            InitiateRemotely?.InitializeProperties(model, room, node, this);
        }

        public bool CleanUpUselessValues(SuperMetroidModel model, Room room, RoomNode node)
        {
            // Cleanup Strats
            Strats = Strats.Where(kvp => kvp.Value.CleanUpUselessValues(model, room)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // A CanLeaveCharged doesn't need an InitiateRemotely, but if it has one it must remain useful.
            // Otherwise the CanLeaveCharged cannot be done and becomes useless.
            bool remoteInitiateUntouched = true;
            if(InitiateRemotely != null)
            {
                remoteInitiateUntouched = InitiateRemotely.CleanUpUselessValues(model, room, node, this);
            }
            return Strats.Any() && remoteInitiateUntouched;
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

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // There are many things to check...
            // If logical options have rendered this CanLeaveCharged unusable, it can't be executed
            if(UselessByLogicalOptions)
            {
                return null;
            }

            // If we don't have SpeedBooster, this is not usable
            if (!inGameState.Inventory.HasSpeedBooster())
            {
                return null;
            }

            // If the player is unable to charge a shinespark with the available runway, this is not usable
            if (model.Rules.CalculateEffectiveRunwayLength(this, TilesSavedWithStutter) < TilesToShineCharge)
            {
                return null;
            }

            // STITCHME Is there any remote initiation check anywhere? If this is remote it should only be executable if the path followed in the room matches the remote config
            // I think there were checks elsewhere though, I think it's in InGameState.GetRetroactiveCanLeaveChargdes().

            // Figure out how much energy we will need to have for the shinespark
            int energyNeededForShinespark = model.Rules.CalculateEnergyNeededForShinespark(ShinesparkFrames, times: times);
            int shinesparkEnergyToSpend = model.Rules.CalculateShinesparkDamage(inGameState, ShinesparkFrames, times: times);

            // Try to execute all strats, 
            // obtaining the result of whichever spends the lowest amount of resources while retaining enough for the shinespark
            (Strat bestStrat, ExecutionResult result) = model.ExecuteBest(Strats.Values, inGameState, times: times, previousRoomCount: previousRoomCount,
                // Not calling IsResourceAvailable() because Samus only needs to have that much energy, not necessarily spend all of it
                acceptationCondition: igs => igs.Resources.GetAmount(ConsumableResourceEnum.Energy) >= energyNeededForShinespark);

            // If we couldn't find a successful strat, give up
            if (result == null)
            {
                return null;
            }

            // Add a record of the canLeaveCharged being executed
            result.AddExecutedCanLeaveCharged(this, bestStrat);

            // Finally, spend the energy for executing a shinespark if needed (we already asked to check that the state has enough)
            result.ResultingState.ApplyConsumeResource(ConsumableResourceEnum.Energy, shinesparkEnergyToSpend);
            
            return result;
        }
    }
}
