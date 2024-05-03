using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Options;
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
        public Or()
        {

        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            LogicalRequirements.ApplyLogicalOptions(logicalOptions);

            // Since this is an Or, it only becomes impossible to fulfill if all of the inner logical elements is
            return LogicalRequirements.LogicalElements.All(element => element.UselessByLogicalOptions);
        }

        public Or(LogicalRequirements logicalRequirements) : base(logicalRequirements)
        {

        }

        public override bool IsNever()
        {
            // This is only never if all inner logical elements are never, since fulfilling this requires only one of the inner logical elements
            return LogicalRequirements.LogicalElements.All(element => element.IsNever());
        }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return LogicalRequirements.ExecuteOne(model, inGameState, times: times, previousRoomCount: previousRoomCount);
        }
    }
}
