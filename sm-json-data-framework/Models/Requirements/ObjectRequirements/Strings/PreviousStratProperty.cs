using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Strings
{
    public class PreviousStratProperty : AbstractObjectLogicalElementWithString
    {
        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            // A strat property is a free-form string so we have nothing to initialize
            return Enumerable.Empty<string>();
        }

        public override bool IsFulfilled(InGameState inGameState, bool usePreviousRoom = false)
        {
            return inGameState.GetLastStrat(usePreviousRoom)?.StratProperties?.Contains(Value) == true;
        }
    }
}
