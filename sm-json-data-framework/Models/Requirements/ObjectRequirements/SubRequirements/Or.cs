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
        public override bool IsNever()
        {
            // This is only never if all inner logical elements are never, since fulfilling this requires only one of the inner logical elements
            return LogicalRequirements.LogicalElements.All(element => element.IsNever());
        }

        public override ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return LogicalRequirements.ExecuteOne(model, inGameState, times: times, previousRoomCount: previousRoomCount);
        }
    }
}
