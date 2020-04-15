using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    public class HelperLogicalElement : AbstractStringLogicalElement
    {
        private Helper Helper { get; set; }

        public HelperLogicalElement(Helper helper)
        {
            Helper = helper;
        }

        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return Helper.Requires.IsFulfilled(model, inGameState, times: times);
        }
    }
}
