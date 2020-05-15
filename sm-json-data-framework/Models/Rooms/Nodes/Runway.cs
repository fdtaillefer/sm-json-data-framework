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
    public class Runway : InitializablePostDeserializeInNode, IRunway
    {
        public int Length { get; set; }

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        public int EndingUpTiles { get; set; } = 0;

        public IEnumerable<Strat> Strats { get; set; } = Enumerable.Empty<Strat>();

        public bool UsableComingIn = true;

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room, RoomNode)"/> has been called.</para>
        /// <para>The node to which this runway is tied.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode Node { get; set; }

        [JsonPropertyName("openEnd")]
        public int OpenEnds { get; set; } = 0;

        public void Initialize(SuperMetroidModel model, Room room, RoomNode node)
        {
            // Eliminate disabled strats
            Strats = Strats.WhereEnabled(model);

            foreach (Strat strat in Strats)
            {
                strat.Initialize(model, room);
            }

            Node = node;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            List<string> unhandled = new List<string>();

            foreach(Strat strat in Strats)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }

        /// <summary>
        /// Attempts to use this runway based on the provided in-game state (which will not be altered), 
        /// by fulfilling its execution requirements.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to use for execution. This will NOT be altered by this method.</param>
        /// <param name="comingIn">If true, tries to use the runway while coming into the room. If false, tries to use it when already in the room.</param>
        /// <param name="times">The number of consecutive times that this runway should be used.
        /// Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <returns>An ExecutionResult describing the execution if successful, or null otherwise.
        /// The in-game state in that ExecutionResult will never be the same instance as the provided one.</returns>
        public ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, bool comingIn, int times = 1, bool usePreviousRoom = false)
        {
            // If we're coming in, this must be usable coming in
            if (!UsableComingIn && comingIn)
            {
                return null;
            }

            // Return the result of the best strat execution
            (Strat bestStrat, ExecutionResult result) = model.ExecuteBest(Strats, inGameState, times: times, usePreviousRoom: usePreviousRoom);
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

        public ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return Runway.Execute(model, inGameState, ComingIn, times: times, usePreviousRoom: usePreviousRoom);
        }
    }
}
