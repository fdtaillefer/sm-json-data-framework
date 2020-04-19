using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Rooms
{
    public class Strat : InitializablePostDeserializeInRoom
    {
        public string Name { get; set; }

        public bool Notable { get; set; }

        public LogicalRequirements Requires { get; set; }

        public IEnumerable<StratObstacle> Obstacles { get; set; } = Enumerable.Empty<StratObstacle>();

        public IEnumerable<StratFailure> Failures { get; set; } = Enumerable.Empty<StratFailure>();

        public IEnumerable<string> StratProperties { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Attempts to fulfill the requirements of this strat by the provided in-game state. If successful, returns a new InGameState instance to
        /// represent the in-game state after performing the strat. If unsuccessful, return nulls.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="times">The number of consecutive times that this should be fulfilled. Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <returns>A new InGameState representing the state after fulfillment if successful, or null otherwise</returns>
        public InGameState AttemptFulfill(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            // STITCHME This needs to worry about obstacles too...

            return Requires.AttemptFulfill(model, inGameState, times: times, usePreviousRoom: usePreviousRoom);
        }

        public void Initialize(SuperMetroidModel model, Room room)
        {
            foreach(StratFailure failure in Failures)
            {
                failure.Initialize(model, room);
            }

            foreach(StratObstacle obstacle in Obstacles)
            {
                obstacle.Initialize(model, room);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Requires.InitializeReferencedLogicalElementProperties(model, room));

            foreach(StratObstacle obstacle in Obstacles)
            {
                unhandled.AddRange(obstacle.InitializeReferencedLogicalElementProperties(model, room));
            }

            foreach(StratFailure failure in Failures)
            {
                unhandled.AddRange(failure.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
