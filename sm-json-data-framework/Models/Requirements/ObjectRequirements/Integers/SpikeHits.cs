using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to take a number of hits from spikes.
    /// </summary>
    public class SpikeHits : AbstractDamageNumericalValueLogicalElement
    {
        public override int CalculateDamage(SuperMetroidModel model, InGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.Rules.CalculateEnvironmentalDamage(inGameState, model.Rules.SpikeDamage) * Value * times;
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            return model.Rules.GetEnvironmentalDamageReducingItems(model, inGameState);
        }
    }
}
