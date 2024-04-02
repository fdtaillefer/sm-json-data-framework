using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// A logical element that is fulfilled by fulfilling the requirements of an inner Helper.
    /// </summary>
    public class HelperLogicalElement : AbstractStringLogicalElement
    {
        private Helper Helper { get; set; }

        public HelperLogicalElement(Helper helper)
        {
            Helper = helper;
        }

        public override ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return Helper.Requires.Execute(model, inGameState, times: times * model.LogicalOptions.NumberOfTries(Helper), previousRoomCount: previousRoomCount);
        }
    }
}
