using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    public class DraygonElectricityFrames : AbstractObjectLogicalElementWithNumericalIntegerValue
    {
        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, bool usePreviousRoom = false)
        {
            int damage = model.Rules.CalculateElectricityGrappleDamage(inGameState, Value);
            return inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, damage);
        }
    }
}
