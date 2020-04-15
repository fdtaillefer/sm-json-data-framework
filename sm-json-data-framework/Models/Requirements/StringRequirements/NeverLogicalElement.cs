using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    public class NeverLogicalElement : AbstractStringLogicalElement
    {
        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return false;
        }
    }
}
