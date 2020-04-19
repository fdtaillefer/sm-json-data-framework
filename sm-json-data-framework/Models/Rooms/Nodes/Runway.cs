using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Node
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
        /// Attempts to fulfill the requirements for using this Runway by the provided in-game state. If successful, returns a new InGameState instance to
        /// represent the in-game state after using the runway. If unsuccessful, returns null.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="comingIn">If true, tries to use the runway while coming into the room. If false, tries to use it when already in the room.</param>
        /// <param name="times">The number of consecutive times that this runway should be used. Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <returns>A new InGameState representing the state after fulfillment if successful, or null otherwise</returns>
        public InGameState AttemptUse(SuperMetroidModel model, InGameState inGameState, bool comingIn, int times = 1, bool usePreviousRoom = false)
        {
            // If we're coming in, this must be usable coming in
            if(!UsableComingIn && comingIn)
            {
                return null;
            }

            // Try to execute all strats, returning the resulting state of whichever spends the lowest amount of resources
            return model.ApplyOr(inGameState, Strats, (s, igs) => s.AttemptFulfill(model, igs, times: times, usePreviousRoom: usePreviousRoom));
        }
    }
}
