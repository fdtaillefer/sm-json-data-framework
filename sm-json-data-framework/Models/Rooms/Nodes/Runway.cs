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
    public class Runway : AbstractModelElement, InitializablePostDeserializeInNode, IRunway
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
        public IDictionary<string, Strat> Strats { get; set; } = new Dictionary<string, Strat>();

        public bool UsableComingIn = true;

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room, RoomNode)"/> has been called.</para>
        /// <para>The node to which this runway is tied.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode Node { get; set; }

        [JsonPropertyName("openEnd")]
        public int OpenEnds { get; set; } = 0;

        public Runway()
        {

        }

        public Runway(RawRunway rawRunway, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Name = rawRunway.Name;
            Length = rawRunway.Length;
            GentleUpTiles = rawRunway.GentleUpTiles;
            GentleDownTiles = rawRunway.GentleDownTiles;
            SteepUpTiles = rawRunway.SteepUpTiles;
            SteepDownTiles = rawRunway.SteepDownTiles;
            StartingDownTiles = rawRunway.StartingDownTiles;
            EndingUpTiles = rawRunway.EndingUpTiles;
            Strats = rawRunway.Strats.Select(rawStrat => new Strat(rawStrat, knowledgeBase)).ToDictionary(strat => strat.Name);
            UsableComingIn = rawRunway.UsableComingIn;
            OpenEnds = rawRunway.OpenEnd;
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            bool noUsefulStrat = true;
            foreach(Strat strat in Strats.Values)
            {
                strat.ApplyLogicalOptions(logicalOptions);
                if(!strat.UselessByLogicalOptions)
                {
                    noUsefulStrat = false;
                }
            }

            // We could pre-calculate an effective runway length here if we had the rules.

            // A runway becomes useless if its strats are impossible
            return noUsefulStrat;
        }

        public void InitializeProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            Node = node;

            foreach (Strat strat in Strats.Values)
            {
                strat.InitializeProperties(model, room);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            List<string> unhandled = new List<string>();

            foreach(Strat strat in Strats.Values)
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
        /// <param name="previousRoomCount">The number of playable rooms to go back by (whenever in-room state is relevant). 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns>An ExecutionResult describing the execution if successful, or null otherwise.
        /// The in-game state in that ExecutionResult will never be the same instance as the provided one.</returns>
        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, bool comingIn, int times = 1, int previousRoomCount = 0)
        {
            if(UselessByLogicalOptions)
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
}
