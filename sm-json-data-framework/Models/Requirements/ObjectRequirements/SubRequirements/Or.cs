using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements
{
    public class Or : AbstractObjectLogicalElementWithSubRequirements
    {
        public override bool IsFulfilled(InGameState inGameState, bool usePreviousRoom = false)
        {
            return LogicalRequirements.IsFulfilled(inGameState, and: false, usePreviousRoom: usePreviousRoom);
        }
    }
}
