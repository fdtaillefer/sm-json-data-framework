using sm_json_data_framework.InGameStates;
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
    /// Represents a sequence of contiguous tiles directly connected to a door, which Samus can use to accumulate speed when running out of or (possibly) into a room.
    /// </summary>
    public class Runway : AbstractModelElement<UnfinalizedRunway, Runway>, IRunway, ILogicalExecutionPreProcessable
    {
        /// <summary>
        /// Number of tiles the player is expected to be able save if stutter is possible on a runway, as per applied logical options.
        /// </summary>
        public decimal TilesSavedWithStutter => AppliedLogicalOptions.TilesSavedWithStutter;

        /// <summary>
        /// Smallest number of tiles the player is expected to be able to obtain a shine charge with (before applying stutter), as per applied logical options.
        /// </summary>
        public decimal TilesToShineCharge => AppliedLogicalOptions.TilesToShineCharge;

        public Runway(UnfinalizedRunway sourceElement, Action<Runway> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Name = sourceElement.Name;
            Length = sourceElement.Length;
            GentleUpTiles = sourceElement.GentleUpTiles;
            GentleDownTiles = sourceElement.GentleDownTiles;
            SteepUpTiles = sourceElement.SteepUpTiles;
            SteepDownTiles = sourceElement.SteepDownTiles;
            StartingDownTiles = sourceElement.StartingDownTiles;
            EndingUpTiles = sourceElement.EndingUpTiles;
            UsableComingIn = sourceElement.UsableComingIn;
            OpenEnds = sourceElement.OpenEnds;
            Strats = sourceElement.Strats.Values.Select(strat => strat.Finalize(mappings)).ToDictionary(strat => strat.Name).AsReadOnly();
            Node = sourceElement.Node.Finalize(mappings);
        }

        /// <summary>
        /// The name of the runway. This is unique across the entire model.
        /// </summary>
        public string Name { get; }

        public int Length { get; }

        public int GentleUpTiles { get; }

        public int GentleDownTiles { get; }

        public int SteepUpTiles { get; }

        public int SteepDownTiles { get; }

        public int StartingDownTiles { get; }

        public int EndingUpTiles { get; }
        
        /// <summary>
        /// The strats that can be executed to use this Runway, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Strat> Strats { get; }

        /// <summary>
        /// Indicates whether this Runway can be used to continue gaining momentum after entering the room with some momentum.
        /// </summary>
        public bool UsableComingIn { get; }

        /// <summary>
        /// The node to which this runway is tied.
        /// </summary>
        public RoomNode Node { get; }

        public int OpenEnds { get; }

        /// <summary>
        /// Attempts to use this runway based on the provided in-game state (which will not be altered), 
        /// by fulfilling its execution requirements.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to use for execution. This will NOT be altered by this method.</param>
        /// <param name="comingIn">If true, tries to use the runway while coming into the room. If false, tries to use it when already in the room.</param>
        /// <param name="times">The number of consecutive times that this runway should be used.
        /// Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (whenever in-room state is relevant). 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns>An ExecutionResult describing the execution if successful, or null otherwise.
        /// The in-game state in that ExecutionResult will never be the same instance as the provided one.</returns>
        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, bool comingIn, int times = 1, int previousRoomCount = 0)
        {
            if (LogicallyNever)
            {
                return null;
            }

            // If we're coming in, this must be usable coming in
            if (!UsableComingIn && comingIn)
            {
                return null;
            }

            // Return the result of the best strat execution
            (Strat bestStrat, ExecutionResult result) = Strats.Values.ExecuteBest(model, inGameState, times: times, previousRoomCount: previousRoomCount);
            if (result == null)
            {
                return null;
            }
            else
            {
                // Add a record of the runway being used
                result.AddUsedRunway(this, bestStrat);
                return result;
            }
        }

        /// <summary>
        /// Creates and returns an executable version of this runway.
        /// </summary>
        /// <param name="comingIn">Indicates whether the executable should consider that the player is coming in the room or not.</param>
        /// <returns></returns>
        public ExecutableRunway AsExecutable(bool comingIn)
        {
            return new ExecutableRunway(this, comingIn);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            foreach (Strat strat in Strats.Values)
            {
                strat.ApplyLogicalOptions(logicalOptions, rules);
            }
        }

        protected override void UpdateLogicalProperties(SuperMetroidRules rules)
        {
            LogicalEffectiveRunwayLength = rules.CalculateEffectiveRunwayLength(this, TilesSavedWithStutter);
            LogicalEffectiveReversibleRunwayLength = rules.CalculateEffectiveReversibleRunwayLength(this, TilesSavedWithStutter);
            LogicalEffectiveRunwayLengthNoCharge = rules.CalculateEffectiveRunwayLength(this, tilesSavedWithStutter: 0);
            base.UpdateLogicalProperties(rules);
            LogicallyNever = CalculateLogicallyNever(rules);
            LogicallyAlways = CalculateLogicallyAlways(rules);
            LogicallyFree = CalculateLogicallyFree(rules);
        }

        /// <summary>
        /// The effective runway length of this Runway, given the current logical options.
        /// </summary>
        public decimal LogicalEffectiveRunwayLength { get; private set; }

        /// <summary>
        /// The effective runway length of this Runway if considered reversible, given the current logical options.
        /// </summary>
        public decimal LogicalEffectiveReversibleRunwayLength { get; private set; }

        /// <summary>
        /// The effective runway length of this Runway when used just to run and not to shine charge, given the current logical options.
        /// </summary>
        public decimal LogicalEffectiveRunwayLengthNoCharge { get; private set; }

        public override bool CalculateLogicallyRelevant(SuperMetroidRules rules)
        {
            // If a runway cannot be used, it may as well not exist
            return !CalculateLogicallyNever(rules);
        }

        /// <summary>
        /// If true, then this runway is impossible to use given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            // A runway is impossible to use if it has no strats that can be executed
            return !Strats.Values.WhereLogicallyRelevant().Any();
        }

        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            // We only need one always possible strat in order to always be possible
            return Strats.Values.WhereLogicallyAlways().Any();
        }

        public bool LogicallyFree { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyFree"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyFree(SuperMetroidRules rules)
        {
            // We only need one free strat in order to be free
            return Strats.Values.WhereLogicallyFree().Any();
        }
    }

    /// <summary>
    /// An IExecutable wrapper around Runway, with handling for whether the execution is done coming in the room or not.
    /// </summary>
    public class ExecutableRunway : IExecutable
    {
        public ExecutableRunway(Runway runway, bool comingIn)
        {
            Runway = runway;
            ComingIn = comingIn;
        }

        public Runway Runway { get; private set; }

        public bool ComingIn { get; private set; }

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return Runway.Execute(model, inGameState, ComingIn, times: times, previousRoomCount: previousRoomCount);
        }
    }

    public class UnfinalizedRunway : AbstractUnfinalizedModelElement<UnfinalizedRunway, Runway>, InitializablePostDeserializeInNode
    {
        public string Name { get; set; }

        public int Length { get; set; }

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        public int EndingUpTiles { get; set; } = 0;

        /// <summary>
        /// The strats that can be executed to use this Runway, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedStrat> Strats { get; set; } = new Dictionary<string, UnfinalizedStrat>();

        public bool UsableComingIn { get; set; } = true;

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom, UnfinalizedRoomNode)"/> has been called.</para>
        /// <para>The node to which this runway is tied.</para>
        /// </summary>
        public UnfinalizedRoomNode Node { get; set; }

        public int OpenEnds { get; set; } = 0;

        public UnfinalizedRunway()
        {

        }

        public UnfinalizedRunway(RawRunway rawRunway, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Name = rawRunway.Name;
            Length = rawRunway.Length;
            GentleUpTiles = rawRunway.GentleUpTiles;
            GentleDownTiles = rawRunway.GentleDownTiles;
            SteepUpTiles = rawRunway.SteepUpTiles;
            SteepDownTiles = rawRunway.SteepDownTiles;
            StartingDownTiles = rawRunway.StartingDownTiles;
            EndingUpTiles = rawRunway.EndingUpTiles;
            Strats = rawRunway.Strats.Select(rawStrat => new UnfinalizedStrat(rawStrat, knowledgeBase)).ToDictionary(strat => strat.Name);
            UsableComingIn = rawRunway.UsableComingIn;
            OpenEnds = rawRunway.OpenEnd;
        }

        protected override Runway CreateFinalizedElement(UnfinalizedRunway sourceElement, Action<Runway> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Runway(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room, UnfinalizedRoomNode node)
        {
            Node = node;

            foreach (UnfinalizedStrat strat in Strats.Values)
            {
                strat.InitializeProperties(model, room);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room, UnfinalizedRoomNode node)
        {
            List<string> unhandled = new List<string>();

            foreach(UnfinalizedStrat strat in Strats.Values)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
