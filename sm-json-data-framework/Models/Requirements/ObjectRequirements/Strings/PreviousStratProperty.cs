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
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// <returns></returns>
        public bool IsFulfilled(InGameState inGameState, bool usePreviousRoom)
        {
            return inGameState.GetLastStrat(usePreviousRoom)?.StratProperties?.Contains(Value) == true;
        }

        public override InGameState AttemptFulfill(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            if (IsFulfilled(inGameState, usePreviousRoom))
            {
                // Clone the InGameState to fulfill method contract
                return inGameState.Clone();
            }
            else
            {
                return null;
            }
        }
    }
}
