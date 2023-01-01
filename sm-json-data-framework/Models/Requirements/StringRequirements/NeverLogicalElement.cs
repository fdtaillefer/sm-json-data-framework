using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// A logical element that is never ever fulfilled.
    /// </summary>
    public class NeverLogicalElement : AbstractStringLogicalElement
    {
        public override ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return null;
        }
    }
}
