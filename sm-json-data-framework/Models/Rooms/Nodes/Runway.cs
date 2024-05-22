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
    /// <summary>
    /// Represents a sequence of contiguous tiles directly connected to a door, which Samus can use to accumulate speed when running out of or (possibly) into a room.
    /// </summary>
    public class Runway : AbstractModelElement<UnfinalizedRunway, Runway>, IRunway
    {
        private UnfinalizedRunway InnerElement {get;set;}

        public Runway(UnfinalizedRunway innerElement, Action<Runway> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            Strats = InnerElement.Strats.Values.Select(strat => strat.Finalize(mappings)).ToDictionary(strat => strat.Name).AsReadOnly();
            Node = InnerElement.Node.Finalize(mappings);
        }

        /// <summary>
        /// The name of the runway. This is unique across the entire model.
        /// </summary>
        public string Name => InnerElement.Name;

        public int Length => InnerElement.Length;

        public int GentleUpTiles => InnerElement.GentleUpTiles;

        public int GentleDownTiles => InnerElement.GentleDownTiles;

        public int SteepUpTiles => InnerElement.SteepUpTiles;

        public int SteepDownTiles => InnerElement.SteepDownTiles;

        public int StartingDownTiles => InnerElement.StartingDownTiles;

        public int EndingUpTiles => InnerElement.EndingUpTiles;
        
        /// <summary>
        /// The strats that can be executed to use this Runway, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Strat> Strats { get; }

        /// <summary>
        /// Indicates whether this Runway can be used to continue gaining momentum after entering the room with some momentum.
        /// </summary>
        public bool UsableComingIn => InnerElement.UsableComingIn;

        /// <summary>
        /// The node to which this runway is tied.
        /// </summary>
        public RoomNode Node { get; }

        public int OpenEnds => InnerElement.OpenEnds;

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
            (Strat bestStrat, ExecutionResult result) = model.ExecuteBest(Strats.Values, inGameState, times: times, previousRoomCount: previousRoomCount);
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

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            foreach (Strat strat in Strats.Values)
            {
                strat.ApplyLogicalOptions(logicalOptions);
            }
        }

        protected override void UpdateLogicalProperties()
        {
            base.UpdateLogicalProperties();
            LogicallyNever = CalculateLogicallyNever();

            // We could pre-calculate an effective runway length here if we had the rules.
        }

        public override bool CalculateLogicallyRelevant()
        {
            // If a runway cannot be used, it may as well not exist
            return !CalculateLogicallyNever();
        }

        /// <summary>
        /// If true, then this runway is impossible to use given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <returns></returns>
        protected bool CalculateLogicallyNever()
        {
            // A runway is impossible to use if it has no strats that can be executed
            return !Strats.Values.WhereLogicallyRelevant().Any();
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

    public class UnfinalizedRunway : AbstractUnfinalizedModelElement<UnfinalizedRunway, Runway>, InitializablePostDeserializeInNode, IRunway
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
