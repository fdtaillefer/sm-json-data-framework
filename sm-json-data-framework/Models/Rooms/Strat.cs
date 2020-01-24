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
        /// Evaluates whether the requirements of this strat are met by the provided in-game state.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <returns></returns>
        public bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, bool usePreviousRoom = false)
        {
            // STITCHME This needs to worry about obstacles too...

            return Requires.IsFulfilled(model, inGameState, usePreviousRoom: usePreviousRoom);
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
