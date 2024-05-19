using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to take damage down to a fixed amount. It can always be fulfilled, but the cost can vary.
    /// </summary>
    public class EnergyAtMost : AbstractDamageNumericalValueLogicalElement<UnfinalizedEnergyAtMost, EnergyAtMost>
    {
        public EnergyAtMost(UnfinalizedEnergyAtMost innerElement, Action<EnergyAtMost> mappingsInsertionCallback) 
            : base(innerElement, mappingsInsertionCallback)
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

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing in logical options can alter this
            return false;
        }

        protected override bool CalculateLogicallyNever()
        {
            // This is always possible, by definition
            return false;
        }

        protected override bool CalculateLogicallyAlways()
        {
            // This is always possible, by definition - though it's not always free
            return true;
        }
    }

    public class UnfinalizedEnergyAtMost : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedEnergyAtMost, EnergyAtMost>
    {
        public UnfinalizedEnergyAtMost()
        {

        }

        public UnfinalizedEnergyAtMost(int energy) : base(energy)
        {

        }

        protected override EnergyAtMost CreateFinalizedElement(UnfinalizedEnergyAtMost sourceElement, Action<EnergyAtMost> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new EnergyAtMost(sourceElement, mappingsInsertionCallback);
        }
    }
}
