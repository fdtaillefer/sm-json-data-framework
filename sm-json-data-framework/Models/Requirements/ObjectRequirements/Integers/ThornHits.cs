using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to take a number of hits from the weaker spikes (mostly found in Brinstar).
    /// </summary>
    public class ThornHits : AbstractDamageNumericalValueLogicalElement
    {
        public override int CalculateDamage(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return model.Rules.CalculateEnvironmentalDamage(inGameState, model.Rules.ThornDamage) * Value * times;
        }
    }
}
