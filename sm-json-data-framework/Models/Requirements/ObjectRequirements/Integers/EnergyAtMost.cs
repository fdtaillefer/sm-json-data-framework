using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
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
        public EnergyAtMost()
        {

        }

        public EnergyAtMost(int energy) : base(energy)
        {

        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int currentRegularEnergy = inGameState.Resources.GetAmount(Items.RechargeableResourceEnum.RegularEnergy);
            // Don't take damage if we've already reached the threshold
            return Math.Max(0, currentRegularEnergy - Value);
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            // This brings energy down to a specific level, and has no cares for damage reduction items
            return new Item[] { };
        }
    }
}
