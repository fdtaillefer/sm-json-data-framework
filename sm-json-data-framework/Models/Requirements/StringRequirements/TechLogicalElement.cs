using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Techs;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// A logical element that is fulfilled by fulfilling the requirements of an inner Tech.
    /// </summary>
    public class TechLogicalElement : AbstractStringLogicalElement
    {
        private Tech Tech { get; set; }

        public TechLogicalElement(Tech tech)
        {
            Tech = tech;
        }

        public override bool IsNever()
        {
            return false;
        }

        public override ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return Tech.Requires.Execute(model, inGameState, times: times * model.LogicalOptions.NumberOfTries(Tech), previousRoomCount: previousRoomCount);
        }
    }
}
