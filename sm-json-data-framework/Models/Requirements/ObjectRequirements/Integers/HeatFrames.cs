using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    public class HeatFrames : AbstractObjectLogicalElementWithNumericalIntegerValue
    {
        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            int damage = model.Rules.CalculateHeatDamage(inGameState, Value) * times;
            return inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, damage);
        }
    }
}
