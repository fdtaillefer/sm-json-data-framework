using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Navigation;
using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
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
    public class CanLeaveCharged : AbstractModelElement<UnfinalizedCanLeaveCharged, CanLeaveCharged>, IRunway, IExecutable, ILogicalExecutionPreProcessable
    {
        /// <summary>
        /// Number of tiles the player is expected to be able save if stutter is possible on a runway, as per applied logical options.
        /// </summary>
        public decimal TilesSavedWithStutter => AppliedLogicalOptions.TilesSavedWithStutter;

        /// <summary>
        /// Smallest number of tiles the player is expected to be able to obtain a shine charge with (before applying stutter), as per applied logical options.
        /// </summary>
        public decimal TilesToShineCharge => AppliedLogicalOptions.TilesToShineCharge;

        public CanLeaveCharged(UnfinalizedCanLeaveCharged sourceElement, Action<CanLeaveCharged> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            UsedTiles = sourceElement.UsedTiles;
            FramesRemaining = sourceElement.FramesRemaining;
            ShinesparkFrames = sourceElement.ShinesparkFrames;
            OpenEnds = sourceElement.OpenEnds;
            GentleUpTiles = sourceElement.GentleUpTiles;
            GentleDownTiles = sourceElement.GentleDownTiles;
            SteepUpTiles = sourceElement.SteepUpTiles;
            SteepDownTiles = sourceElement.SteepDownTiles;
            StartingDownTiles = sourceElement.StartingDownTiles;
            InitiateRemotely = sourceElement.InitiateRemotely?.Finalize(mappings);
            Strats = sourceElement.Strats.Values.Select(strat => strat.Finalize(mappings)).ToDictionary(strat => strat.Name).AsReadOnly();
            Node = sourceElement.Node.Finalize(mappings);
        }

        public int Length => UsedTiles;

        public int EndingUpTiles { get => 0; }

        /// <summary>
        /// The number of tiles available to obtain a shine charge.
        /// </summary>
        public int UsedTiles { get; }

        /// <summary>
        /// The number of frames remaining on the shine charge when exiting the room.
        /// </summary>
        public int FramesRemaining { get; }

        /// <summary>
        /// The number of frames that Samus spends shinesparking while executing this.
        /// Anything more than 0 implies leaving via a shinespark, so <see cref="FramesRemaining"/> should be 0.
        /// </summary>
        public int ShinesparkFrames { get; }

        /// <summary>
        /// Indicates whether this CanLeavecharged involves executing a shinespark.
        /// </summary>
        public bool MustShinespark => ShinesparkFrames > 0;

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
        public bool IsInitiatedRemotely => InitiateRemotely != null;

        /// <summary>
        /// The strats that can be used to execute this CanLeaveCharged, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Strat> Strats { get; }

        public int OpenEnds { get; }

        public int GentleUpTiles { get; }

        public int GentleDownTiles { get; }

        public int SteepUpTiles { get; }

        public int SteepDownTiles { get; }

        public int StartingDownTiles { get; }

        /// <summary>
        /// The node in which this CanLeaveCharged is.
        /// </summary>
        public RoomNode Node { get; }

        /// <summary>
        /// Attempts to execute this CanLeaveCharged based on the provided in-game state (which will not be altered), 
        /// by fulfilling its execution requirements. Be aware that this does not do any checks with remote initiation, 
        /// as that part cannot be done while at the node of the CanLeaveCharged. Instead, this can be done retroactively. 
        /// It's seen as the responsibility of <see cref="CanComeInCharged.ExecuteUseful(SuperMetroidModel, ReadOnlyInGameState, int, int)"/>
        /// to combine the execution of a CanLeaveCharged with the retroactive validation of its remote initiation.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to use for execution. This will NOT be altered by this method.</param>
        /// <param name="times">The number of consecutive times that this should be executed.
        /// Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (whenever in-room state is relevant). 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns>An ExecutionResult describing the execution if successful, or null otherwise.
        /// The in-game state in that ExecutionResult will never be the same instance as the provided one.</returns>
        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // There are many things to check...
            // If logical options have rendered this CanLeaveCharged unusable, it can't be executed
            if (LogicallyNever)
            {
                return null;
            }

            // If we don't have SpeedBooster, this is not usable
            if (!inGameState.Inventory.HasSpeedBooster())
            {
                return null;
            }

            // If the player is unable to charge a shinespark with the available runway, this is not usable
            if (LogicalEffectiveRunwayLength < TilesToShineCharge)
            {
                return null;
            }

            // There is no check related to an in-room path here, because a CanLeaveCharged has no knowledge of retroactive use.
            // That's why such logic is in InGameState.GetRetroactiveCanLeaveChargeds()

            // Figure out how much energy we will need to have for the shinespark
            int energyNeededForShinespark = model.Rules.CalculateEnergyNeededForShinespark(inGameState, ShinesparkFrames, times: times);
            int shinesparkEnergyToSpend = model.Rules.CalculateShinesparkDamage(inGameState, ShinesparkFrames, times: times);

            // Try to execute all strats, 
            // obtaining the result of whichever spends the lowest amount of resources while retaining enough for the shinespark
            (Strat bestStrat, ExecutionResult result) = Strats.Values.ExecuteBest(model, inGameState, times: times, previousRoomCount: previousRoomCount,
                // Not calling IsResourceAvailable() because Samus only needs to have that much energy, not necessarily spend all of it
                acceptationCondition: igs => igs.Resources.GetAmount(ConsumableResourceEnum.Energy) >= energyNeededForShinespark);

            // If we couldn't find a successful strat, give up
            if (result == null)
            {
                return null;
            }

            // Add a record of the canLeaveCharged being executed
            result.AddExecutedCanLeaveCharged(this, bestStrat);

            // Add a record of SpeedBooster being used
            result.AddItemInvolved(model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME]);

            // Finally, spend the energy for executing a shinespark if needed (we already asked to check that the state has enough)
            result.ResultingState.ApplyConsumeResource(ConsumableResourceEnum.Energy, shinesparkEnergyToSpend);

            return result;
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            InitiateRemotely?.ApplyLogicalOptions(logicalOptions, rules);

            foreach (Strat strat in Strats.Values)
            {
                strat.ApplyLogicalOptions(logicalOptions, rules);
            }
        }

        protected override void UpdateLogicalProperties(SuperMetroidRules rules)
        {
            LogicalEffectiveRunwayLength = rules.CalculateEffectiveRunwayLength(this, TilesSavedWithStutter);
            base.UpdateLogicalProperties(rules);
            LogicallyNever = CalculateLogicallyNever(rules);
            LogicallyAlways = CalculateLogicallyAlways(rules);
            LogicallyFree = CalculateLogicallyFree(rules);
        }

        /// <summary>
        /// The effective runway length of this CanLeaveCharged, given the current logical options.
        /// </summary>
        public decimal LogicalEffectiveRunwayLength { get; private set; }

        public override bool CalculateLogicallyRelevant(SuperMetroidRules rules)
        {
            // If a CanLeaveCharged is impossible to execute, it may as well not exist
            return !CalculateLogicallyNever(rules);
        }

        /// <summary>
        /// If true, then this CanLeaveCharged is impossible to execute given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            int? maxEnergy = AppliedLogicalOptions.MaxPossibleAmount(ConsumableResourceEnum.Energy);
            // There's five separate things that can make a CanLeaveCharged impossible to execute:
            // - Speed Booster is removed from the game
            // - It must be initiated remotely, but that remote initiation is impossible
            // - It has no possible strats
            // - It requires a shinespark, but shinesparks are logically disabled OR the shinespark needs more energy than we can ever have
            // - It's too short to shine charge on given the logical options
            return !AppliedLogicalOptions.IsSpeedBoosterInGame()
                || (InitiateRemotely?.LogicallyNever is true) 
                || !Strats.Values.WhereLogicallyRelevant().Any() 
                || (MustShinespark && (!CanShinespark || (maxEnergy != null && maxEnergy.Value <= rules.CalculateBestCaseEnergyNeededForShinespark(ShinesparkFrames, AppliedLogicalOptions.RemovedItems))))
                || LogicalEffectiveRunwayLength < TilesToShineCharge;
        }

        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            // Besides not being never, there's five our separate conditions that must be met to always be possible
            // - Speed Booster is in-game and always available
            // - Remove initiation is unneeded OR always possible
            // - At least one strat is always possible
            // - Does not require a shinespark (if it does, there's energy costs that are not always possible)
            // - There is enough room logically to shine charge
            return !CalculateLogicallyNever(rules)
                && AppliedLogicalOptions.IsSpeedBoosterInGame() && AppliedLogicalOptions.StartConditions.StartingInventory.HasSpeedBooster()
                && (InitiateRemotely == null || InitiateRemotely.LogicallyAlways)
                && Strats.Values.WhereLogicallyAlways().Any()
                && !MustShinespark
                && LogicalEffectiveRunwayLength >= TilesToShineCharge;
        }

        public bool LogicallyFree { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyFree"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyFree(SuperMetroidRules rules)
        {
            // Besides not being never, there's five separate conditions that must be met to always be possible
            // - Speed Booster is in-game and always available
            // - Remove initiation is unneeded OR free
            // - At least one strat is free
            // - Does not require a shinespark (if it does, there's energy costs so it's not free)
            // - There is enough room logically to shine charge
            return !CalculateLogicallyNever(rules)
                && AppliedLogicalOptions.IsSpeedBoosterInGame() && AppliedLogicalOptions.StartConditions.StartingInventory.HasSpeedBooster()
                && (InitiateRemotely == null || InitiateRemotely.LogicallyFree)
                && Strats.Values.WhereLogicallyFree().Any()
                && !MustShinespark
                && LogicalEffectiveRunwayLength >= TilesToShineCharge;
        }
    }

    public class UnfinalizedCanLeaveCharged : AbstractUnfinalizedModelElement<UnfinalizedCanLeaveCharged, CanLeaveCharged>, InitializablePostDeserializeInNode
    {
        public int UsedTiles { get; set; }

        public int FramesRemaining { get; set; }

        public int ShinesparkFrames { get; set; }

        public UnfinalizedInitiateRemotely InitiateRemotely { get; set; }

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
