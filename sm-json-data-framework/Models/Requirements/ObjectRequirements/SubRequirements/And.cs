using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements
{
    /// <summary>
    /// A logical element that is fulfilled by fulfilling all of its inner logical elements.
    /// </summary>
    public class And : AbstractObjectLogicalElementWithSubRequirements
    {
        public And()
        {

        }

        public And(LogicalRequirements logicalRequirements): base(logicalRequirements)
        {
            
        }

        public override bool IsNever()
        {
            return LogicalRequirements.IsNever();
        }

        public override ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return LogicalRequirements.Execute(model, inGameState, times: times, previousRoomCount: previousRoomCount);
        }
    }
}
