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

        // STITCHME This probably ought to return a new InGameState after using the runway (or null if not possible)
        /// <summary>
        /// Returns whether this runway is usable according to the provided parameters
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="times">The number of consecutive times that this should be checked for usability. Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="comingIn">If true, evaluates usability while coming into the room. If false, evaluates usability when already in the room.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <returns></returns>
        public bool IsUsable(SuperMetroidModel model, InGameState inGameState, bool comingIn, int times = 1, bool usePreviousRoom = false)
        {
            return (UsableComingIn || !comingIn) && Strats.Any(s => s.IsFulfilled(model, inGameState, times: times, usePreviousRoom: usePreviousRoom));
        }
    }
}
