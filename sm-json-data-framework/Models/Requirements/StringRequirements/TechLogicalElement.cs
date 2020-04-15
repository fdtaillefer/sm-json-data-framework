using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Techs;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    public class TechLogicalElement : AbstractStringLogicalElement
    {
        private Tech Tech { get; set; }

        public TechLogicalElement(Tech tech)
        {
            Tech = tech;
        }

        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return Tech.Requires.IsFulfilled(model, inGameState, times: times);
        }
    }
}
