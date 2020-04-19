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

        // STITCHME Keep this when removing them
        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return inGameState.GetLastStrat(usePreviousRoom)?.StratProperties?.Contains(Value) == true;
        }

        public override InGameState AttemptFulfill(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            if (IsFulfilled(model, inGameState, times: times, usePreviousRoom: usePreviousRoom))
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
