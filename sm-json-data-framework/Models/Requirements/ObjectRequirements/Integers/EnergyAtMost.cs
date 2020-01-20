using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    public class EnergyAtMost : AbstractObjectLogicalElementWithNumericalIntegerValue
    {
        public override bool IsFulfilled(InGameState inGameState, bool usePreviousRoom = false)
        {
            // While this may reduce energy, a lack of energy will never prevent it from being fulfilled
            return true;
        }
    }
}
