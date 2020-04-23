using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements
{
    /// <summary>
    /// A logical element that is fulfilled by fulfilling any one of its inner logical elements.
    /// </summary>
    public class Or : AbstractObjectLogicalElementWithSubRequirements
    {
        public override InGameState AttemptFulfill(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return LogicalRequirements.AttemptFulfill(model, inGameState, times: times, usePreviousRoom: usePreviousRoom, and: false);
        }
    }
}
