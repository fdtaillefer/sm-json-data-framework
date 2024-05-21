using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Navigation;
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
    /// <summary>
    /// Represents a way that Samus can leave the room through this node with a number of frames remaining on a charged shinespark, or with an active shinespark.
    /// A CanLeaveCharged is not needed to implicitly use a door's runway to achieve a shine charge, and it describes other kinds of scenarios.
    /// </summary>
    public class CanLeaveCharged : AbstractModelElement<UnfinalizedCanLeaveCharged, CanLeaveCharged>, IRunway, IExecutable
    {
        /// <summary>
        /// Number of tiles the player is expected to be able save if stutter is possible on a runway, as per applied logical options.
        /// </summary>
        public decimal TilesSavedWithStutter => AppliedLogicalOptions.TilesSavedWithStutter;

        /// <summary>
        /// Smallest number of tiles the player is expected to be able to obtain a shine charge with (before applying stutter), as per applied logical options.
        /// </summary>
        public decimal TilesToShineCharge => AppliedLogicalOptions.TilesToShineCharge;

        private UnfinalizedCanLeaveCharged InnerElement { get; set; }

        public CanLeaveCharged(UnfinalizedCanLeaveCharged innerElement, Action<CanLeaveCharged> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            InitiateRemotely = innerElement.InitiateRemotely?.Finalize(mappings);
            Strats = innerElement.Strats.Values.Select(strat => strat.Finalize(mappings)).ToDictionary(strat => strat.Name).AsReadOnly();
            Node = innerElement.Node.Finalize(mappings);
        }

        public int Length => InnerElement.Length;

        public int EndingUpTiles => InnerElement.EndingUpTiles;

        /// <summary>
        /// The number of tiles available to obtain a shine charge.
        /// </summary>
        public int UsedTiles => InnerElement.UsedTiles;

        /// <summary>
        /// The number of frames remaining on the shine charge when exiting the room.
        /// </summary>
        public int FramesRemaining => InnerElement.FramesRemaining;

        /// <summary>
        /// The number of frames that Samus spends shinesparking while executing this.
        /// Anything more than 0 implies leaving via a shinespark, so <see cref="FramesRemaining"/> should be 0.
        /// </summary>
        public int ShinesparkFrames => InnerElement.ShinesparkFrames;

        /// <summary>
        /// Indicates whether this CanLeavecharged involves executing a shinespark.
        /// </summary>
        public bool MustShinespark => InnerElement.MustShinespark;

        /// <summary>
        /// Whether the player is logically expected to know how to shinespark.
        /// </summary>
        public bool CanShinespark => AppliedLogicalOptions.CanShinespark;

        /// <summary>
        /// If present, declares this CanLeaveCharged as one that is initiated remotely and contains details regarding that context.
        /// A remote CanLeaveCharged is one that is executed at a different node than the door and follows a specific path to the door.
        /// In practice using this with a <see cref="GameNavigator"/>, you still start it at the exit node but then the navigator checks retroactively if you respected the path.
        /// </summary>
        public InitiateRemotely InitiateRemotely { get; }

        /// <summary>
        /// Returns whether this CanLeaveCharged is initiated at a different node than the door it exits through.
        /// </summary>
        public bool IsInitiatedRemotely => InnerElement.IsInitiatedRemotely;

        /// <summary>
        /// The strats that can be used to execute this CanLeaveCharged, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Strat> Strats { get; }

        public int OpenEnds => InnerElement.OpenEnds;

        public int GentleUpTiles => InnerElement.GentleUpTiles;

        public int GentleDownTiles => InnerElement.GentleDownTiles;

        public int SteepUpTiles => InnerElement.SteepUpTiles;

        public int SteepDownTiles => InnerElement.SteepDownTiles;

        public int StartingDownTiles => InnerElement.StartingDownTiles;

        /// <summary>
        /// The node in which this CanLeaveCharged is.
        /// </summary>
        public RoomNode Node { get; }

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // There are many things to check...
            // If logical options have rendered this CanLeaveCharged unusable, it can't be executed
            if (UselessByLogicalOptions)
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

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            bool useless = false;
            if (InitiateRemotely != null)
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
                if (!strat.UselessByLogicalOptions)
                {
                    allStratsUseless = false;
                }
            }
            if (allStratsUseless)
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

        protected override void UpdateLogicalProperties()
        {
            base.UpdateLogicalProperties();
            LogicallyNever = CalculateLogicallyNever();
        }

        public override bool CalculateLogicallyRelevant()
        {
            // If a CanLeaveCharged is impossible to execute, it may as well not exist
            return !CalculateLogicallyNever();
        }

        /// <summary>
        /// If true, then this CanLeaveCharged is impossible to execute given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <returns></returns>
        protected bool CalculateLogicallyNever()
        {
            // There's three separate things that can make a CanLeaveCharged impossible to execute:
            // - It must be initiated remotely, but that remote initiation is impossible
            // - It has no possible strats
            // - It requires a shinespark but shinesparks are logically disabled
            return (InitiateRemotely?.LogicallyNever is true) || !Strats.Values.Any(strat => strat.LogicallyRelevant) || (MustShinespark && !CanShinespark);
        }
    }

    public class UnfinalizedCanLeaveCharged : AbstractUnfinalizedModelElement<UnfinalizedCanLeaveCharged, CanLeaveCharged>, InitializablePostDeserializeInNode, IRunway
    {
        public int Length { get => UsedTiles; }

        public int EndingUpTiles { get => 0; }

        public int UsedTiles { get; set; }

        public int FramesRemaining { get; set; }

        public int ShinesparkFrames { get; set; }

        /// <summary>
        /// Indicates whether this CanLeavecharged involves executing a shinespark.
        /// </summary>
        public bool MustShinespark => ShinesparkFrames > 0;

        public UnfinalizedInitiateRemotely InitiateRemotely {get;set;}

        /// <summary>
        /// Returns whether this CanLeaveCharged is initiated at a different node than the door it exits through.
        /// </summary>
        public bool IsInitiatedRemotely => InitiateRemotely != null;

        /// <summary>
        /// The strats that can be used to execute this CanLeaveCharged, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedStrat> Strats { get; set; } = new Dictionary<string, UnfinalizedStrat>();

        public int OpenEnds { get; set; } = 0;

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoomNode)"/> has been called.</para>
        /// <para>The node in which this CanLeaveCharged is.</para>
        /// </summary>
        public UnfinalizedRoomNode Node { get; set; }

        public UnfinalizedCanLeaveCharged()
        {

        }

        public UnfinalizedCanLeaveCharged(RawCanLeaveCharged rawCanLeaveCharged, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            UsedTiles = rawCanLeaveCharged.UsedTiles;
            FramesRemaining = rawCanLeaveCharged.FramesRemaining;
            ShinesparkFrames = rawCanLeaveCharged.ShinesparkFrames;
            if (rawCanLeaveCharged.InitiateRemotely != null)
            {
                InitiateRemotely = new UnfinalizedInitiateRemotely(rawCanLeaveCharged.InitiateRemotely);
            }
            Strats = rawCanLeaveCharged.Strats.Select(rawStrat => new UnfinalizedStrat(rawStrat, knowledgeBase)).ToDictionary(strat => strat.Name);
            OpenEnds = rawCanLeaveCharged.OpenEnd;
            GentleUpTiles = rawCanLeaveCharged.GentleUpTiles;
            GentleDownTiles = rawCanLeaveCharged.GentleDownTiles;
            SteepUpTiles = rawCanLeaveCharged.SteepUpTiles;
            SteepDownTiles = rawCanLeaveCharged.SteepDownTiles;
            StartingDownTiles = rawCanLeaveCharged.StartingDownTiles;
        }

        protected override CanLeaveCharged CreateFinalizedElement(UnfinalizedCanLeaveCharged sourceElement, Action<CanLeaveCharged> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new CanLeaveCharged(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room, UnfinalizedRoomNode node)
        {
            Node = node;

            foreach (UnfinalizedStrat strat in Strats.Values)
            {
                strat.InitializeProperties(model, node.Room);
            }

            InitiateRemotely?.InitializeProperties(model, room, node, this);
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room, UnfinalizedRoomNode node)
        {
            List<string> unhandled = new List<string>();

            foreach(UnfinalizedStrat strat in Strats.Values)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            if (InitiateRemotely != null)
            {
                unhandled.AddRange(InitiateRemotely.InitializeReferencedLogicalElementProperties(model, room, node, this));
            }

            return unhandled.Distinct();
        }
    }
}
