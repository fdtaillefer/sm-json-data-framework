using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    public class ThornHits : AbstractObjectLogicalElementWithNumericalIntegerValue
    {
        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, bool usePreviousRoom = false)
        {
            int damage = model.Rules.CalculateEnvironmentalDamage(inGameState, model.Rules.ThornDamage);
            return inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, damage);
        }
    }
}
