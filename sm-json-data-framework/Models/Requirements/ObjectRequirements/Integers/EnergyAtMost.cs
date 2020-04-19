using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to take damage down to a fixed amount. It can always be fulfilled, but the cost can vary.
    /// </summary>
    public class EnergyAtMost : AbstractDamageNumericalValueLogicalElement
    {
        // Overridden because this can always be fulfilled, so no need for any checks. Only the cost is variable.
        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            // While this may reduce energy, a lack of energy will never prevent it from being fulfilled
            return true;
        }

        public override int CalculateDamage(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            int currentRegularEnergy = inGameState.GetCurrentAmount(Items.RechargeableResourceEnum.RegularEnergy);
            // Don't take damage if we've already reached the threshold
            return Math.Max(0, currentRegularEnergy - Value);
        }
    }
}
