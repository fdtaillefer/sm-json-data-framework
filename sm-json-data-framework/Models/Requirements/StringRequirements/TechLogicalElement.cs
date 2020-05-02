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

        public override ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return Tech.Requires.Execute(model, inGameState, times: times, usePreviousRoom: usePreviousRoom);
        }
    }
}
