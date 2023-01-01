using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Strings
{
    /// <summary>
    /// A logical element which requires Samus to have reached the current node by a strat with a specific strat property.
    /// </summary>
    public class PreviousStratProperty : AbstractObjectLogicalElementWithString
    {
        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            // A strat property is a free-form string so we have nothing to initialize
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Returns whether the provided InGameState fulfills this PreviousStratProperty element.
        /// </summary>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="previousRoomCount">The number of rooms to go back by. 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid.</param>
        /// <returns></returns>
        public bool IsFulfilled(InGameState inGameState, int previousRoomCount = 0)
        {
            return inGameState.GetLastStrat(previousRoomCount)?.StratProperties?.Contains(Value) == true;
        }

        public override ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            if (IsFulfilled(inGameState, previousRoomCount))
            {
                // Clone the InGameState to fulfill method contract
                return new ExecutionResult(inGameState.Clone());
            }
            else
            {
                return null;
            }
        }
    }
}
