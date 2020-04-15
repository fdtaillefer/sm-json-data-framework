using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements
{
    public class And : AbstractObjectLogicalElementWithSubRequirements
    {
        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return LogicalRequirements.IsFulfilled(model, inGameState, times: times, usePreviousRoom: usePreviousRoom);
        }
    }
}
